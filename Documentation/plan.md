# カウガール攻撃間隔短縮 設計書

## 要件確認結果
- `Documentation/要件書/cowgirl-style-attack-interval-requirements.md` を確認し、実装前提として解釈が揺れやすい点はありませんでした。
- 要件の中核は「攻撃間隔倍率 `0.75` を `PlayerStyleType.Cowgirl` 選択中だけ保持し、既存武器と新規取得武器の両方へ即時反映すること」です。
- `DroneWeapon` は武器本体のクールダウンが再展開間隔、要件上短縮対象はドローン本体の `ShotInterval` であるため、他武器と同じ適用方法にはしません。

## 実装方針
- 攻撃間隔倍率の状態は `PlayerState` に追加し、スタイル効果の責務は既存方針どおり `PlayerProgressionService` と `IPlayerStyleEffect` 系に集約する。
- `CowgirlStyleEffect.ApplyParameters()` は `PlayerState.AttackIntervalMultiplier` を `0.75f` に設定し、`PlayerProgressionService.ResetStyleParameters()` は他倍率と同様に `1.0f` へ戻す。
- 武器側は「基準攻撃間隔」と「現在の有効攻撃間隔」を分離する。基準値は各武器クラスの既存フィールド (`_shootInterval`, `_throwInterval`, `_spawnInterval`, `_droneShotInterval`) に保持し、有効値は `PlayerState` の倍率を参照して都度算出する。
- `WeaponService` から各武器へ `Func<float>` の攻撃間隔倍率参照デリゲートを注入し、新規取得武器も生成直後から現在スタイルの倍率を参照できるようにする。
- 既存武器の残りクールダウン即時補正は `PlayerController.ChangeStyle()` 完了後に武器チェーン全体へ「現在の有効間隔へ clamp し直す」再計算処理を流す。
- `DroneWeapon` は再展開間隔 `_deployInterval` を維持しつつ、`DroneSpawnRequest.ShotInterval` にだけ倍率を反映する。これで要件の「ドローン本体の攻撃間隔のみ短縮」を満たす。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Player/Domain/PlayerState.cs`
  - `AttackIntervalMultiplier` を追加する。
  - `SetAttackIntervalMultiplier()` と `ResetAttackIntervalMultiplier()` を追加する。
- `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs`
  - `ResetStyleParameters()` に攻撃間隔倍率のリセットを追加する。
  - 既存のスタイル切替順序は維持し、`ChangeStyle()` で新スタイル適用前に基準値へ戻す構造を継続する。
- `Assets/Survaivor/Character/Player/Application/StyleEffects/CowgirlStyleEffect.cs`
  - `ApplyParameters()` で `0.75f` を設定する実装へ置き換える。
- `Assets/Survaivor/Character/Combat/Application/WeaponBase.cs`
  - 攻撃間隔倍率参照デリゲート保持を追加する。
  - 基準間隔へ倍率を掛ける共通ヘルパーと、武器チェーン全体のクールダウン再 clamp 用 public メソッドを追加する。
- `Assets/Survaivor/Character/Combat/Application/WeaponService.cs`
  - コンストラクタに `Func<float> attackIntervalMultiplierProvider` を追加し、対象武器生成時に渡す。
  - `RebuildWeapons()` を含む全生成経路で同じ provider を利用する。
- `Assets/Survaivor/Character/Combat/Application/BulletWeapon.cs`
  - `CooldownDuration` を「基準発射間隔 × 現在倍率」で返すよう変更する。
- `Assets/Survaivor/Character/Combat/Application/ThrowingWeapon.cs`
  - `CooldownDuration` を倍率込みで返すよう変更する。
- `Assets/Survaivor/Character/Combat/Application/DamageFieldWeapon.cs`
  - `CooldownDuration` を倍率込みで返すよう変更する。
- `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs`
  - `CooldownDuration` を倍率込みで返すよう変更する。
- `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs`
  - `CooldownDuration` を倍率込みで返すよう変更する。
- `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs`
  - `DroneSpawnRequest` へ渡す `ShotInterval` を倍率込みに変更する。
  - `_deployInterval` には倍率を掛けないことを明示する。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - `WeaponService` 初期化時に `PlayerState.AttackIntervalMultiplier` の provider を渡す。
  - `ChangeStyle()` 後に既存武器チェーンへクールダウン再 clamp を掛ける。

### テスト追加・更新対象
- `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs`
  - カウガール選択で攻撃間隔倍率 `0.75f` が設定されることを追加検証する。
  - カウガールから他スタイルへ切替時に `1.0f` へ戻ることを検証する。
- `Assets/Survaivor/Tests/EditMode/Combat/WeaponLevelTuningTests.cs`
  - 攻撃間隔倍率 provider を与えた場合に、対象武器の `CooldownDuration` または発行リクエスト値へ倍率が反映されることを追加検証する。
  - `DroneWeapon` だけは再展開間隔ではなく `DroneSpawnRequest.ShotInterval` にだけ反映されることを検証する。
- `Assets/Survaivor/Tests/EditMode/Combat/WeaponServiceTests.cs`
  - `WeaponService` が provider を各生成武器へ配線することを検証する。
- `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs`
  - スタイル変更後、既存武器を持った状態でも新倍率が即時有効になることを確認する統合寄りテスト追加を検討する。

## データフロー / 処理フロー

### 1. カウガール選択時
1. `PlayerController.ChangeStyle(PlayerStyleType.Cowgirl)` が呼ばれる。
2. `PlayerExperience.ChangeStyle()` 経由で `PlayerProgressionService.ChangeStyle()` が呼ばれる。
3. `PlayerProgressionService.ResetStyleParameters()` が既存スタイル由来の倍率をすべて `1.0f` に戻す。
4. `CowgirlStyleEffect.ApplyParameters()` が `PlayerState.AttackIntervalMultiplier = 0.75f` を設定する。
5. `PlayerController.ChangeStyle()` が `_weapon` に対してクールダウン再 clamp を実行する。
6. 次フレーム以降、既存武器は倍率込みの有効攻撃間隔で発動する。

### 2. 既存武器の攻撃間隔参照
1. `WeaponBase.Tick()` が各武器の `CooldownDuration` を参照する。
2. 対象武器は内部の基準間隔に対して `AttackIntervalMultiplier` を掛けた値を返す。
3. `WeaponService.Tick()` がその有効間隔でクールダウンを進行・再設定する。
4. スタイル変更直後に残りクールダウンが有効間隔を超えていれば、事前 clamp 済みのため待ち時間が不正に長く残らない。

### 3. カウガール選択中の新規武器取得
1. `PlayerController.ApplyWeaponUpgrade()` が `WeaponService.ApplyUpgrade()` を呼ぶ。
2. `WeaponService` は既存と同じ builder で武器を生成するが、追加した倍率 provider も同時に渡す。
3. 新規武器は生成直後から `PlayerState.AttackIntervalMultiplier` を参照できる。
4. そのため、カウガール中に取得した武器も最初の Tick から短縮後間隔で動作する。

### 4. ドローン武器の例外処理
1. `DroneWeapon.Tick()` 自体は `_deployInterval` をそのまま使う。
2. `DroneWeapon.Fire()` で `DroneSpawnRequest` を組み立てる際、`_droneShotInterval` にだけ `AttackIntervalMultiplier` を掛ける。
3. `WeaponEffectExecutor` と `DroneController` には既存どおり `ShotInterval` が渡るため、周回中ドローンの攻撃だけが短縮される。

### 5. 他スタイルへ戻した時
1. `PlayerProgressionService.ResetStyleParameters()` で `AttackIntervalMultiplier` を `1.0f` に戻す。
2. `PlayerController` が武器チェーンへ再 clamp を掛ける。
3. すべての武器は基準攻撃間隔へ戻り、多重適用は発生しない。

## 実装ステップ

### Phase 1: プレイヤー状態とスタイル効果を拡張する
- `PlayerState` に攻撃間隔倍率 API を追加する。
- `PlayerProgressionService.ResetStyleParameters()` に攻撃間隔倍率の reset を追加する。
- `CowgirlStyleEffect` を空実装から倍率設定実装へ更新する。
- `PlayerProgressionServiceTests` にカウガール倍率の設定・解除テストを追加する。

### Phase 2: 武器への倍率参照を配線する
- `WeaponBase` に倍率 provider と共通ヘルパーを追加する。
- `WeaponService` の builder シグネチャを拡張し、各武器コンストラクタへ provider を渡す。
- `BulletWeapon`、`ThrowingWeapon`、`DamageFieldWeapon`、`BoundBallWeapon`、`FlameBottleWeapon` の `CooldownDuration` を倍率込みへ変更する。
- `DroneWeapon` は `ShotInterval` へのみ倍率を反映するよう分岐する。

### Phase 3: 既存武器の即時反映を成立させる
- `WeaponBase` にクールダウン再 clamp を武器チェーンへ伝播する API を追加する。
- `PlayerController.ChangeStyle()` でスタイル変更直後にその API を呼ぶ。
- `PlayerController` の `WeaponService` 初期化で `PlayerState.AttackIntervalMultiplier` を参照する provider を渡す。

### Phase 4: テストと確認を整備する
- 攻撃間隔倍率の単体テストを武器系テストへ追加する。
- 既存武器保持中のスタイル切替で即時反映されることを統合寄りに確認する。
- ドローンの例外仕様をテストで固定化する。

## リスクと対策
- リスク: `PlayerProgressionService.ResetStyleParameters()` に攻撃間隔倍率 reset を入れ忘れると、他スタイルへ切り替えても効果が残る。
  - 対策: `PlayerProgressionServiceTests` にカウガール → 別スタイルの切替テストを追加する。
- リスク: `WeaponService` の一部 builder だけ provider を渡し忘れると、新規取得武器だけ倍率未反映になる。
  - 対策: `RebuildWeapons()` と `ApplyUpgrade()` の両経路を `WeaponServiceTests` で確認する。
- リスク: 既存クールダウン clamp が武器チェーンの先頭にしか効かないと、下位の ride weapon に旧間隔が残る。
  - 対策: `WeaponBase` 側で再帰的に処理する API を持たせる。
- リスク: `DroneWeapon` に共通ロジックをそのまま当てると、再展開間隔まで短縮されて要件逸脱する。
  - 対策: `DroneWeapon` は `CooldownDuration` を維持し、`ShotInterval` 反映だけ別処理にする。
- リスク: `PlayerState` 参照が null のタイミングで provider を組むと、初期化順によって null 参照になる。
  - 対策: `PlayerController.InitializeWeaponSystems()` では `PlayerState` 未解決時に `1.0f` を返す provider を使う。

## 検証方針
- EditMode テスト
  - `PlayerProgressionService.ChangeStyle(PlayerStyleType.Cowgirl)` 実行後に `AttackIntervalMultiplier == 0.75f` になること。
  - カウガールからメイドやセレブへ変更すると `AttackIntervalMultiplier == 1.0f` に戻ること。
  - `BulletWeapon`、`ThrowingWeapon`、`DamageFieldWeapon`、`BoundBallWeapon`、`FlameBottleWeapon` が倍率 `0.75f` のとき有効クールダウンへ反映されること。
  - `DroneWeapon` は `DroneSpawnRequest.ShotInterval` のみ `0.75f` 倍され、再展開間隔は維持されること。
  - 既存武器に対してスタイル切替後すぐクールダウン clamp が掛かること。
- 手動確認
  - プレイ中にカウガールへ変更した直後、既存所持武器の発射テンポが即時に上がること。
  - カウガール中に武器取得した場合も、最初の発動から短縮後テンポになること。
  - 他スタイルへ戻した直後、攻撃テンポが基準値へ戻ること。
- Unity コマンド確認
  - `u tests run edit`
  - `u console get -l E`

## コードスニペット（主要変更イメージ）
```csharp
public class PlayerState
{
    public float AttackIntervalMultiplier { get; private set; } = 1f;

    public void SetAttackIntervalMultiplier(float multiplier)
    {
        AttackIntervalMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetAttackIntervalMultiplier()
    {
        AttackIntervalMultiplier = 1f;
    }
}
```

```csharp
public class CowgirlStyleEffect : IPlayerStyleEffect
{
    const float ATTACK_INTERVAL_MULTIPLIER = 0.75f;

    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        context?.PlayerState?.SetAttackIntervalMultiplier(ATTACK_INTERVAL_MULTIPLIER);
    }
}
```

```csharp
public abstract class WeaponBase
{
    readonly Func<float> _attackIntervalMultiplierProvider;

    protected float ApplyAttackIntervalMultiplier(float baseInterval)
    {
        float multiplier = _attackIntervalMultiplierProvider != null
            ? _attackIntervalMultiplierProvider()
            : 1f;
        return Mathf.Max(0f, baseInterval * multiplier);
    }

    public void ClampCooldownToCurrentDurationRecursively()
    {
        _rideWeapon?.ClampCooldownToCurrentDurationRecursively();
        ClampCooldownTimerToDuration();
    }
}
```

```csharp
public class DroneWeapon : WeaponBase
{
    protected override float CooldownDuration => _deployInterval;

    protected override void Fire()
    {
        float effectiveShotInterval = ApplyAttackIntervalMultiplier(_droneShotInterval);
        _effectExecutor.DeployDrone(
            new DroneSpawnRequest(
                GetOriginPosition(),
                _originTransform,
                sourcePow,
                _orbitRadius,
                effectiveShotInterval,
                phaseOffsetDegrees));
    }
}
```
