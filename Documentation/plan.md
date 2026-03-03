# 武器レベル段階調整 実装計画

## 実装方針
- 既存の割合短縮ベース強化を廃止し、各武器クラス内で「レベルごとの固定性能テーブル」を持つ構成へ統一する。`LevelUp()` は現在値に倍率をかけるのではなく、現在レベルに対応する値を再設定する。
- 取得直後の武器はレベル1として扱われるため、コンストラクタ内の初期値もレベル1仕様へ合わせる。`RebuildWeapons()` は新規インスタンス生成後に `LevelUp()` を積む実装のため、初期値がレベル1になっていれば既存の再構築ロジックをそのまま活用できる。
- クールダウンを持つ武器は、固定値への更新後に `ClampCooldownTimerToDuration()` を呼んで既存の待機時間補正を維持する。
- ドローンのみ「攻撃間隔固定値化」に加えて、レベル5で2機展開という構造変更が必要である。現在の `WeaponEffectExecutor` は単一ドローン前提なので、複数アクティブドローン管理へ拡張する。
- ドローンの2機化は、位相差・再利用管理・周回位置維持の意図がコードだけでは追いづらいため、`DroneWeapon`、`WeaponEffectExecutor`、`DroneController` の要点には簡潔な説明コメントを残す。
- 火炎瓶の複数投射は `FlameBottleWeapon.Fire()` 内で1回の発動につき複数回 `FireFlameBottle()` を発行し、扇状に初速ベクトルを分配する。`WeaponEffectExecutor` と `FlameBottleProjectileController` は既存の単発投射処理を再利用する。
- 今回は武器レベル調整が対象であり、`PlayerController` の武器取得導線、`WeaponService.ApplyUpgrade()` / `RebuildWeapons()` のレベル管理フローは原則維持する。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Combat/Application/BulletWeapon.cs`
  - 発射間隔をレベル固定テーブル `1.5 / 1.0 / 0.8 / 0.4 / 0.2` に変更する。
  - 既存の割合短縮フィールド（`_intervalReductionPerLevel` など）を削減または未使用にし、レベルから直接 `ShootInterval` を算出する。
- `Assets/Survaivor/Character/Combat/Application/ThrowingWeapon.cs`
  - 発射間隔をレベル固定テーブル `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
  - 既存の割合短縮ロジックを固定値更新へ置き換える。
- `Assets/Survaivor/Character/Combat/Application/DamageFieldWeapon.cs`
  - サイズをレベル固定テーブル `3.0 / 3.5 / 4.0 / 4.5 / 5.0` に変更する。
  - 既存の `_areaScaleGrowthPerLevel` による線形加算を、テーブル参照へ置き換える。
- `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs`
  - ドローン攻撃間隔をレベル固定テーブル `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
  - レベル5で展開数を2機に切り替える状態を持たせる。
  - `Fire()` で必要機数ぶん `DeployDrone()` を要求し、各ドローンに位相差情報を渡す。
  - 2機化の分岐理由と位相割り当て意図が分かるコメントを残す。
- `Assets/Survaivor/Character/Combat/Application/DroneSpawnRequest.cs`
  - 2機の正反対配置を表現するため、ドローンごとの位相オフセットまたはインデックス情報を追加する。
- `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs`
  - 発射間隔をレベル固定テーブル `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
  - バウンド回数をレベルごとの固定値（Lv1=1, Lv2=1, Lv3=2, Lv4=2, Lv5=3）へ変更する。
  - 発射方向を右下固定から真下固定（`Vector3.back` 基準）へ変更する。
- `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs`
  - 投擲間隔をレベル固定テーブル `1.5 / 1.0 / 1.0 / 1.0 / 1.0` に変更する。
  - 投射物数をレベルごとの固定値（Lv1=1, Lv2=1, Lv3=2, Lv4=3, Lv5=4）へ変更する。
  - `Fire()` 内で扇状拡散になるよう、正面基準で左右へ角度分散した複数の初速ベクトルを組み立てる。
- `Assets/Survaivor/Character/Combat/Infrastructure/WeaponEffectExecutor.cs`
  - `_activeDroneController` / `DRONE_POOL_SIZE = 1` の単一前提をやめ、複数ドローンを同時維持できる構造に変更する。
  - ドローンインスタンスを識別して再利用できるコレクション管理へ変更する。
  - 位相ごとにインスタンスを維持する理由が分かるコメントを残す。
- `Assets/Survaivor/Character/Combat/Infrastructure/Drones/DroneController.cs`
  - 初期位相オフセットを受け取れるようにし、2機時に180度ずれた位置を維持する。
  - 同一半径・異なる位相で周回し続けるため、初期角度の設定処理を追加する。
  - 周回角度の初期化と更新意図が分かるコメントを残す。

### テスト追加・更新対象
- `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs`
  - 既存の `TrySetWeaponLevel()` / `RebuildWeapons()` を活用し、各武器レベルの再構築後に固定値が適用されることを確認するテストを追加または更新する。
- `Assets/Survaivor/Tests/EditMode/...` 配下の武器系テスト（新規作成を含む）
  - 各武器クラス単体のレベル境界値テストを追加する。
  - ドローン2機化、バウンド回数、火炎瓶投射数の特例を検証する。

### 変更不要想定
- `Assets/Survaivor/Character/Combat/Application/WeaponService.cs`
  - `ApplyUpgrade()` と `RebuildWeapons()` は、各武器の初期値と `LevelUp()` が正しければそのまま使える。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - 武器レベル適用経路は現状の `ApplyWeaponUpgrade()` / `TrySetWeaponLevel()` を継続利用できるため、今回の主変更対象ではない。

## データフロー / 処理フロー

### 1. 通常の武器取得
1. `PlayerController.ApplyWeaponUpgrade(type)` が呼ばれる。
2. `WeaponService.ApplyUpgrade()` が未所持なら該当 `WeaponBase` 派生クラスを新規生成する。
3. 新規生成された武器は、コンストラクタ直後の内部状態がレベル1テーブル値になる。
4. 以後 `WeaponBase.Tick()` により、そのレベル1性能で発動する。

### 2. 既存武器のレベルアップ
1. `PlayerController.ApplyWeaponUpgrade(type)` が所持済み武器に対して呼ばれる。
2. `WeaponService.ApplyUpgrade()` が既存インスタンスの `LevelUp()` を1回呼ぶ。
3. `LevelUp()` は `UpgradeLevel` を進める。
4. 武器クラスは、更新後レベルに対応する固定テーブル値を参照して、発射間隔・サイズ・バウンド回数・投射物数・展開数を再設定する。
5. クールダウン武器は `ClampCooldownTimerToDuration()` を呼び、残り時間を新しいクールダウン上限へ補正する。

### 3. `TrySetWeaponLevel()` による再構築
1. `PlayerController.TrySetWeaponLevel(type, targetLevel)` が呼ばれる。
2. `WeaponService.RebuildWeapons()` が各武器をレベル1状態で作り直す。
3. 目標レベルまで `LevelUp()` を繰り返す。
4. 各武器が固定テーブルで状態を上書きするため、段階をまたいでも累積誤差なく正しい値へ到達する。

### 4. ドローンのレベル5発動
1. `DroneWeapon.Fire()` が現在レベルを参照する。
2. レベル1-4では1回だけ `DeployDrone()` を呼ぶ。
3. レベル5では2回 `DeployDrone()` を呼び、片方は位相0度、もう片方は位相180度で要求する。
4. `WeaponEffectExecutor` が各位相に対応するドローンインスタンスを取得または再初期化する。
5. `DroneController` は共通半径で周回しつつ、初期位相差により常に円の正反対を維持する。

### 5. 火炎瓶の扇状複数投射
1. `FlameBottleWeapon.Fire()` が現在レベルに応じた投射物数を取得する。
2. プレイヤー前方ベクトルを中心軸とし、投射数に応じて左右へ対称な角度オフセットを計算する。
3. 各角度ごとに水平初速を回転させ、同一の上方向初速を加えた `initialVelocity` を作る。
4. 生成した本数ぶん `IWeaponEffectExecutor.FireFlameBottle()` を呼ぶ。
5. 各 `FlameBottleProjectileController` は既存どおり着地後に炎エリアを生成する。

## 実装ステップ

### Phase 1: 固定性能テーブル化
- `BulletWeapon`、`ThrowingWeapon`、`DamageFieldWeapon`、`BoundBallWeapon`、`FlameBottleWeapon`、`DroneWeapon` それぞれにレベル別固定値テーブルを定義する。
- コンストラクタ初期値をレベル1へ合わせる。
- `LevelUp()` を「現在レベルに応じた値を反映する処理」へ差し替える。
- レベル5以降で `LevelUp()` が呼ばれても、配列上限を超えないよう clamp する。

### Phase 2: バウンドボールと火炎瓶の挙動差分反映
- `BoundBallWeapon` の発射方向を真下へ変更する。
- `BoundBallWeapon` のレベルごとの最大バウンド回数を固定値で反映する。
- `FlameBottleWeapon` の投射物数制御を追加する。
- 火炎瓶の扇状拡散角度の決め方を実装する。左右対称の固定角度ステップを使い、レベルに応じて中心1本から最大4本まで広げる。

### Phase 3: ドローン複数展開対応
- `DroneSpawnRequest` に位相情報を追加する。
- `DroneWeapon` が現在レベルに応じて展開数と位相一覧を決定する。
- `WeaponEffectExecutor` を複数アクティブドローン管理へ変更し、最低2機を同時維持できるプールサイズへ増やす。
- `DroneController.Initialize()` に初期角度または位相オフセットを渡し、2機が正反対で周回するようにする。
- ドローン関連の複雑な分岐には、読み手が追跡しやすい最小限のコメントを追加する。

### Phase 4: テスト整備
- 各武器クラスのレベル1-5境界値を検証する EditMode テストを追加する。
- `TrySetWeaponLevel()` により任意レベルへ再構築した場合も、固定テーブル値が一致することを確認する。
- レベル5ドローンで2機化すること、2機が異なる位相を持つことを検証する。
- 火炎瓶のレベル3-5で投射本数が2・3・4になることを検証する。
- バウンドボールの方向とバウンド回数がレベルに応じて更新されることを検証する。

## リスクと対策
- リスク: 初期値をレベル1へ合わせ忘れると、未強化武器だけ仕様外のままになる。
  - 対策: コンストラクタ直後の値もテスト対象に含める。
- リスク: `RebuildWeapons()` はレベル1からの積み上げなので、`LevelUp()` が累積加算のままだと誤差や二重適用が残る。
  - 対策: `LevelUp()` 内で毎回「現在レベルから直接再計算」する実装に統一する。
- リスク: ドローンの複数化で既存の単一 `_activeDroneController` 参照が残ると、2機目生成時に1機目が上書きされる。
  - 対策: コレクション管理へ置き換え、位相単位で再利用対象を分離する。
- リスク: ドローンプール数を増やさないと、2機要求時に同一インスタンスが再利用されて重なる。
  - 対策: プールサイズを2以上へ増やし、同一フレーム内の複数取得を前提に確認する。
- リスク: 火炎瓶の扇状拡散角度が過大だと、前方投擲の意図から外れる。
  - 対策: 固定の小さめ角度ステップを採用し、左右対称に限定する。
- リスク: バウンドボールの真下方向定義を `Vector3.down` にすると3D移動面と軸がずれる。
  - 対策: 現行ゲーム面（XZ平面移動）に合わせて、前後軸の下方向として `z` マイナス方向を使う。

## 検証方針
- EditMode テスト
  - `BulletWeapon` がレベル1-5で `1.5 / 1.0 / 0.8 / 0.4 / 0.2` 秒になること。
  - `ThrowingWeapon` がレベル1-5で `2.0 / 1.5 / 1.0 / 0.8 / 0.4` 秒になること。
  - `DamageFieldWeapon` がレベル1-5で `3.0 / 3.5 / 4.0 / 4.5 / 5.0` のサイズになること。
  - `BoundBallWeapon` がレベルごとに正しい発射間隔とバウンド回数を持ち、方向が真下であること。
  - `FlameBottleWeapon` がレベルごとに正しい投擲間隔と投射物数を持つこと。
  - `DroneWeapon` がレベル5で2機要求を行い、位相0度/180度を使うこと。
  - `PlayerController.TrySetWeaponLevel()` 経由の再構築後も、同じ値に到達すること。
- 手動確認
  - ゲーム内で各武器をレベル1から5まで上げた際、体感挙動がテーブルどおり変化すること。
  - レベル5ドローンで2機が重ならず、円の正反対を周回すること。
  - レベル3-5火炎瓶で扇状に本数が増えること。
- Unity 確認
  - `u tests run edit` で EditMode テストが通ること。
  - `u console get -l E` で関連エラーが出ていないこと。

## コードスニペット（主要変更イメージ）
```csharp
public class BulletWeapon : WeaponBase
{
    static readonly float[] SHOOT_INTERVALS = { 1.5f, 1.0f, 0.8f, 0.4f, 0.2f };

    public BulletWeapon(...) : base(originTransform, rideWeapon, effectExecutor)
    {
        _shootInterval = SHOOT_INTERVALS[0];
    }

    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, SHOOT_INTERVALS.Length - 1);
        _shootInterval = SHOOT_INTERVALS[levelIndex];
        ClampCooldownTimerToDuration();
    }
}
```

```csharp
public class DroneWeapon : WeaponBase
{
    static readonly float[] SHOT_INTERVALS = { 2.0f, 1.5f, 1.0f, 0.8f, 0.4f };

    protected override void Fire()
    {
        int droneCount = UpgradeLevel >= 5 ? 2 : 1;

        for (int index = 0; index < droneCount; index++)
        {
            // Lv5 のみ 2 機展開し、2機目は 180 度ずらして重なりを避ける。
            float phaseDegrees = droneCount == 2 && index == 1 ? 180f : 0f;
            _effectExecutor.DeployDrone(
                new DroneSpawnRequest(
                    GetOriginPosition(),
                    _originTransform,
                    sourcePow,
                    _orbitRadius,
                    _droneShotInterval,
                    phaseDegrees));
        }
    }
}
```

```csharp
public class FlameBottleWeapon : WeaponBase
{
    static readonly int[] PROJECTILE_COUNTS = { 1, 1, 2, 3, 4 };

    protected override void Fire()
    {
        int projectileCount = PROJECTILE_COUNTS[Mathf.Clamp(UpgradeLevel - 1, 0, PROJECTILE_COUNTS.Length - 1)];

        foreach (float angleOffset in BuildFanAngles(projectileCount))
        {
            Vector3 horizontalDirection = Quaternion.Euler(0f, angleOffset, 0f) * baseDirection;
            Vector3 initialVelocity = horizontalDirection * _horizontalSpeed + Vector3.up * _upwardSpeed;
            _effectExecutor.FireFlameBottle(new FlameBottleFireRequest(origin, initialVelocity, sourcePow, origin.y, _flameDuration, _flameRadius));
        }
    }
}
```
