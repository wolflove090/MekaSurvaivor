# Weapon Application責務分離 リファクタ 実装計画

## 実装方針
- `WeaponBase`、`BulletWeapon`、`DamageFieldWeapon`、`ThrowingWeapon` を `Application` 層へ移せるように、Unity 依存処理を `Infrastructure` 側へ切り出す。
- 武器クラスは「いつ発動するか」「どの要求を出すか」のみを担当し、Prefab取得、プール操作、`Instantiate`、`GetComponent` は行わない。
- 発射や生成に必要な情報は、純C#の要求データとして定義し、`Application` 層から `Infrastructure` 層の実行ポートへ渡す。
- `PlayerController` と `WeaponService` の既存利用形は大きく崩さず、呼び出し元の変更を最小限に抑える。
- 既存挙動を維持するため、実装は「要求データの導入」→「生成アダプタの追加」→「`*Weapon` の純化」→「呼び出し側調整」の順で段階的に行う。

## 変更対象ファイル一覧

### 新規追加予定
- `Assets/Scripts/Application/Character/Combat/WeaponFireRequest.cs`
  - 武器発動要求の共通表現を定義する。
- `Assets/Scripts/Application/Character/Combat/BulletFireRequest.cs`
  - 弾発射に必要な位置、方向、攻撃力を保持する。
- `Assets/Scripts/Application/Character/Combat/ThrowingFireRequest.cs`
  - 投擲弾発射に必要な位置、方向、攻撃力を保持する。
- `Assets/Scripts/Application/Character/Combat/DamageFieldSpawnRequest.cs`
  - ダメージフィールド生成に必要な位置、追従対象、攻撃力、スケールを保持する。
- `Assets/Scripts/Application/Character/Combat/IWeaponEffectExecutor.cs`
  - 武器要求を実行する Application から Infrastructure へのポートを定義する。
- `Assets/Scripts/Infrastructure/Unity/Combat/WeaponEffectExecutor.cs`
  - 各要求を受けて Prefab 生成、プール取得、`*Controller` 初期化を行う。

### 既存ファイルの改修対象
- `Assets/Scripts/Wepon/WeaponBase.cs`
  - Unity 実装依存を排除し、発動要求の抽象化とクールダウン進行に責務を限定する。
- `Assets/Scripts/Wepon/BulletWeapon.cs`
  - ターゲット探索と `BulletFireRequest` 生成に責務を限定する。
- `Assets/Scripts/Wepon/DamageFieldWeapon.cs`
  - `DamageFieldSpawnRequest` 生成と強化値管理に責務を限定する。
- `Assets/Scripts/Wepon/ThrowingWeapon.cs`
  - `ThrowingFireRequest` 生成に責務を限定する。
- `Assets/Scripts/Application/Character/Combat/WeaponService.cs`
  - `*Weapon` 生成時に実行ポートを注入できるよう、ビルダー構築を見直す。
- `Assets/Scripts/Character/Player/PlayerController.cs`
  - `WeaponService` 初期化時に武器実行ポートを渡す。
- `Assets/Scripts/Wepon/Prefabs/BulletFactory.cs`
  - Prefab参照の提供責務を `WeaponEffectExecutor` から利用しやすい形へ調整する。
- `Assets/Tests/EditMode/WeaponServiceTests.cs`
  - 要求データ生成と強化動作に合わせてテストを更新する。

## データフロー / 処理フロー

### 1. 武器初期化
1. `PlayerController` が `BulletFactory` と発動基準 `Transform` を取得する。
2. `PlayerController` が `WeaponEffectExecutor` を生成または参照取得する。
3. `PlayerController` が `WeaponService` に対して、発動基準と実行ポートを渡す。
4. `WeaponService` が `BulletWeapon` / `ThrowingWeapon` / `DamageFieldWeapon` を生成する。

### 2. 毎フレームの武器更新
1. `PlayerController.Update` が現在の武器チェーンに対して `Tick(Time.deltaTime)` を呼ぶ。
2. `WeaponBase` が `WeaponService` を使ってクールダウンを進行し、発動可否を判定する。
3. 発動条件を満たした場合、各 `*Weapon` が現在の状況から発動要求を生成する。
4. `*Weapon` が `IWeaponEffectExecutor` に要求を渡す。
5. `WeaponEffectExecutor` が対応する Prefab を取得し、プールまたは `Instantiate` で実体を生成する。
6. `WeaponEffectExecutor` が `BulletController` / `ThrowingBulletController` / `DamageFieldController` を初期化する。

### 3. 武器強化
1. `WeaponUpgradeUiController` から選択結果が `PlayerController.ApplyWeaponUpgrade` に渡る。
2. `PlayerController` が `WeaponService.ApplyUpgrade` を呼ぶ。
3. 既存武器なら `LevelUp()` で内部パラメータを更新する。
4. 未取得武器なら `WeaponService` が同じ実行ポートを注入して新しい `*Weapon` を生成する。

## フェーズ別実装計画

### Phase 1: 要求データと実行ポートの定義
- 武器発動要求を表す純C#型を追加する。
- `IWeaponEffectExecutor` を追加し、武器クラスが依存する唯一の実行境界を定義する。
- 要求データは最小限の値のみを持ち、Unity コンポーネント参照の露出を抑える。

### Phase 2: Infrastructure 実行アダプタの追加
- `WeaponEffectExecutor` を追加し、`BulletFactory`、`ObjectPool<T>`、`*Controller` 初期化を集約する。
- 弾、投擲弾、ダメージフィールドごとに、要求データから実体生成する処理を実装する。
- 既存 `BulletFactory` は Prefab参照と探索先参照の提供責務に限定する。

### Phase 3: `*Weapon` の純化
- `WeaponBase` から `Transform` の直接依存を最小化し、発動基準位置取得と実行ポート呼び出しに必要な抽象だけを残す。
- `BulletWeapon` から `ObjectPool`、Prefab保持、`Instantiate` を削除し、ターゲット探索後に `BulletFireRequest` を組み立てる。
- `DamageFieldWeapon` から `ObjectPool`、Prefab保持、`Instantiate` を削除し、生成要求を組み立てる。
- `ThrowingWeapon` から `ObjectPool`、Prefab保持、`Instantiate` を削除し、投擲要求を組み立てる。

### Phase 4: 呼び出し側の接続変更
- `WeaponService` のビルダーに実行ポート注入を追加する。
- `PlayerController` で `WeaponEffectExecutor` を初期化し、武器生成経路に接続する。
- 既存の武器チェーン構造と `Tick` 呼び出しを維持したまま、新経路へ切り替える。

### Phase 5: テスト更新と回帰確認
- EditMode テストで、`*Weapon` が要求データを正しく生成することを検証する。
- `WeaponServiceTests` を更新し、新規武器生成時に適切な依存が注入されることを確認する。
- 手動確認で、3種の武器の発動、強化、クールダウン、ダメージ反映が従来どおり動作することを確認する。

## リスクと対策
- リスク: `*Weapon` から Unity 依存を外す過程で、発射位置や向きの取得経路が複雑化する。
  - 対策: まずは必要最小限として、位置・向き・攻撃力を呼び出し時点で値として組み立てる。
- リスク: `BulletFactory` と新しい `WeaponEffectExecutor` の責務が重複する。
  - 対策: `BulletFactory` は設定参照の提供、`WeaponEffectExecutor` は実行のみに明確に分離する。
- リスク: 新旧の生成経路が混在すると、二重生成や未初期化が発生する。
  - 対策: `*Weapon` の `Fire()` 切り替えは一括で行い、旧生成コードを残さない。
- リスク: `Application` 層に Unity 型依存が残り、中途半端な分離になる。
  - 対策: `GameObject.Instantiate`、`ObjectPool<T>`、`GetComponent` を `*Weapon` から排除することを完了条件に含める。
- リスク: 既存テストが実装詳細に依存していて壊れる。
  - 対策: テスト対象を「生成実行」ではなく「要求生成」と「クールダウン進行」に寄せる。

## 検証方針
- EditMode テスト
  - `BulletWeapon` が有効なターゲットを見つけた場合のみ発射要求を出す。
  - `ThrowingWeapon` がプレイヤー向きに基づく投擲要求を出す。
  - `DamageFieldWeapon` が強化レベルに応じたスケールで生成要求を出す。
  - `WeaponService.ApplyUpgrade` が既存武器強化と新規武器追加を従来どおり処理する。
  - `WeaponBase.Tick` がクールダウン条件に従って発動判定する。
- 手動確認
  - シューター武器が最寄り敵へ発射される。
  - ターゲット不在時に破壊可能オブジェクトへ発射される。
  - 投擲武器がプレイヤーの向きに従って飛ぶ。
  - ダメージフィールドがプレイヤー追従し、強化でサイズが拡大する。
  - 武器強化 UI からの選択で新規武器追加と既存武器強化が維持される。

## コードスニペット（主要変更イメージ）
```csharp
/// <summary>
/// 武器発動の実行ポート
/// </summary>
public interface IWeaponEffectExecutor
{
    void FireBullet(BulletFireRequest request);
    void FireThrowing(ThrowingFireRequest request);
    void SpawnDamageField(DamageFieldSpawnRequest request);
}

/// <summary>
/// 弾発射要求
/// </summary>
public sealed class BulletFireRequest
{
    public Vector3 Origin { get; }
    public Vector3 Direction { get; }
    public int SourcePow { get; }

    public BulletFireRequest(Vector3 origin, Vector3 direction, int sourcePow)
    {
        Origin = origin;
        Direction = direction;
        SourcePow = sourcePow;
    }
}

protected override void Fire()
{
    GameObject target = _targetResolver.FindNearestTarget(_originProvider.Position);
    if (target == null)
    {
        return;
    }

    Vector3 direction = (target.transform.position - _originProvider.Position).normalized;
    _effectExecutor.FireBullet(new BulletFireRequest(_originProvider.Position, direction, _powProvider.CurrentPow));
}
```
