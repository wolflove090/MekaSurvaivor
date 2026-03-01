# 新規武器追加 実装計画

## 実装方針
- 既存の武器システムは `WeaponService` が `WeaponUpgradeUiController.UpgradeCardType` をキーに `WeaponBase` を生成し、`PlayerController` が1本の武器チェーンとして `Tick` を回している。この構造を維持したまま、武器種の追加とカード提示ロジックの拡張を行う。
- 新規武器3種は、発動タイミングの組み立てを `Application` 層、Unity依存の移動・衝突・生成を `Infrastructure` 層へ分離する。既存の `IWeaponEffectExecutor` を拡張し、武器側はリクエストデータを渡すだけに留める。
- ドローンは「プレイヤー追従しながら自律攻撃する継続オブジェクト」を生成する武器として扱う。`WeaponBase` はドローン生成・再展開の周期だけを管理し、実際の周回移動と敵探索・射撃は専用 `DroneController` に持たせる。
- バウンドボールは既存 `ProjectileController` の直進弾では不足するため、反射回数と衝突法線による方向反射を持つ専用コントローラーを追加する。初期方向はワールド固定の右下とし、衝突時は `Collider.ClosestPoint` と接触点近傍ベクトルから簡易法線を導出して反射角を決める。
- 火炎瓶は「放物線で飛ぶ投擲本体」と「着地後に残る炎エリア」を分離する。飛翔中は非ダメージ、着地時にのみ炎エリアを生成するため、既存 `ThrowingBulletController` とは別系統の `FlameBottleProjectileController` を用意する。
- 武器強化UIは表示枠3枚を維持し、全武器候補から重複なしでランダムに3件を表示する。カードの押下結果を「UI上の表示候補」から「実際の `UpgradeCardType`」へ変換する責務を `WeaponUpgradeUiController` / `WeaponUpgradePresenter` 側へ追加する。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/UI/WeaponUpgrade/View/WeaponUpgradeUiController.cs`
  - `UpgradeCardType` に `Drone`、`BoundBall`、`FlameBottle` を追加する。
  - 3枚のカードへ表示中の武器候補を割り当てる仕組みを追加する。
  - ランダムに3件の一意な候補を選び、ボタン押下時に「カードIndex」ではなく「割り当て済み武器種別」を通知する。
- `Assets/Survaivor/UI/WeaponUpgrade/Presentation/WeaponUpgradePresenter.cs`
  - `OnUpgradeCardSelected` を実カード種別受け取りへ変更する。
  - レベルアップ時に UI を開く前に、候補一覧の生成をビューへ指示する初期化導線を追加する。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - 武器システム初期化で新規武器プレハブ/依存の注入経路を維持する。
  - 新規武器追加後も初期武器 (`Shooter`) の付与を維持しつつ、アップグレード適用が新規列挙値に対応できることを確認する。
- `Assets/Survaivor/Character/Combat/Application/WeaponService.cs`
  - 武器ビルダー辞書と型辞書へ新規3武器を追加する。
  - 武器候補一覧を外部へ返すAPI（例: `GetAvailableUpgradeTypes()`）を追加し、UIのランダム抽選元を一元化する。
- `Assets/Survaivor/Character/Combat/Application/IWeaponEffectExecutor.cs`
  - ドローン展開、バウンドボール発射、火炎瓶発射、炎エリア生成に対応する実行メソッドを追加する。
- `Assets/Survaivor/Character/Combat/Infrastructure/WeaponEffectExecutor.cs`
  - 新規武器用のプールを追加し、各リクエストを対応する `*Controller` 初期化へ変換する。
  - 炎エリアは既存 `DamageFieldController` の再利用可否に応じて、専用コントローラーか汎用初期化分岐を追加する。
- `Assets/Survaivor/Character/Combat/Infrastructure/Factories/BulletFactory.cs`
  - 新規武器用プレハブ参照（ドローン、バウンドボール、火炎瓶本体、炎エリア、必要ならドローン弾）を追加する。
  - 火炎瓶用の地面基準取得に必要な地面オブジェクト参照、またはそこへ到達する参照を保持する。
- `Assets/Survaivor/Game/Infrastructure/GameBootstrapper.cs`
  - `BulletFactory` へ既存の敵レジストリ・破壊可能オブジェクトスポナー注入を継続し、新規武器が必要とする参照もシーン初期化で満たせることを確認する。
- `Assets/Survaivor/Character/Combat/Infrastructure/Projectiles/ProjectileController.cs`
  - バウンド処理や非ダメージ飛翔体とは責務が異なるため、共通化可能な初期化点だけを整理し、必要なら拡張ポイント（衝突時フック）を追加する。
- `Assets/Survaivor/Character/Combat/Infrastructure/DamageFields/DamageFieldController.cs`
  - 破壊可能オブジェクトへの継続ダメージを許可する。
  - 追従対象なし・その場固定のエリア運用を明示的に扱えるようにする。

### 新規作成候補
- `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs`
  - ドローン展開周期とレベルアップ時の発射間隔短縮を管理する。
- `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs`
  - バウンドボールの発射周期と初期方向決定を管理する。
- `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs`
  - 火炎瓶の投擲周期とレベルアップ時の火炎性能強化を管理する。
- `Assets/Survaivor/Character/Combat/Application/DroneSpawnRequest.cs`
  - ドローン生成位置、追従対象、攻撃力、発射間隔などを運ぶ。
- `Assets/Survaivor/Character/Combat/Application/BoundBallFireRequest.cs`
  - バウンドボールの発射位置、初期方向、攻撃力、最大バウンド回数を運ぶ。
- `Assets/Survaivor/Character/Combat/Application/FlameBottleFireRequest.cs`
  - 火炎瓶の発射位置、初速、向き、攻撃力、地面基準情報を運ぶ。
- `Assets/Survaivor/Character/Combat/Application/FlameAreaSpawnRequest.cs`
  - 着地地点、持続時間、範囲、継続ダメージ情報を運ぶ。
- `Assets/Survaivor/Character/Combat/Infrastructure/Drones/DroneController.cs`
  - プレイヤー周回、敵探索、一定周期射撃を管理する。
- `Assets/Survaivor/Character/Combat/Infrastructure/Projectiles/BoundBallController.cs`
  - 最大3回のバウンドと衝突法線ベースの反射、ダメージ適用を管理する。
- `Assets/Survaivor/Character/Combat/Infrastructure/Projectiles/FlameBottleProjectileController.cs`
  - 放物線飛翔、地面Y到達判定、着地時の炎エリア生成要求を管理する。
- `Assets/Survaivor/Character/Combat/Infrastructure/DamageFields/FlameAreaController.cs`
  - その場固定で敵・破壊可能オブジェクトに継続ダメージを与える炎エリアを管理する。

### テスト追加・改修対象
- `Assets/Survaivor/Tests/EditMode/Combat/WeaponServiceTests.cs`
  - 新規武器のビルダー登録、候補一覧取得、既存武器との共存を検証する。
- `Assets/Survaivor/Tests/EditMode/Enemy/EnemyRegistryTests.cs`
  - 必要に応じてドローンのターゲット探索前提となる近傍探索の継続利用を確認する。
- `Assets/Survaivor/Tests/EditMode/Combat/` 配下に新規テスト
  - バウンド回数上限、火炎エリア継続ダメージ、カード候補抽選ロジックの単体テストを追加する。

## データフロー / 処理フロー

### 1. 武器強化UIのランダム候補表示
1. `GameMessageBus.PlayerLevelUp` を `WeaponUpgradePresenter` が受信する。
2. `WeaponUpgradePresenter` が `PlayerController` または `WeaponService` から全武器候補一覧を取得する。
3. `WeaponUpgradeUiController` が候補一覧から重複なしで3件をランダム抽選し、各カードへ武器名を割り当てる。
4. ユーザーがカードを押すと、UIは「表示中の `UpgradeCardType`」を `OnUpgradeCardSelected` で通知する。
5. `WeaponUpgradePresenter` がその武器種別を `PlayerController.ApplyWeaponUpgrade()` へ渡す。

### 2. ドローン武器の発動
1. `DroneWeapon.Tick()` がクールダウン満了時に `DroneSpawnRequest` を生成する。
2. `WeaponEffectExecutor` が `DroneController` をプールから取得し、プレイヤー追従対象、周回半径、発射間隔、攻撃力を設定して有効化する。
3. `DroneController.Update()` がプレイヤー周囲を周回し、内部の射撃タイマーが満了したら `EnemyRegistry.FindNearestEnemy()` で対象を探す。
4. 対象が見つかった場合のみ、ドローン位置から弾を発射する。弾の実体は既存 `BulletController` を流用するか、同等の初期化を行う。
5. ドローン武器レベルアップ時は、ドローンの発射間隔設定値を短縮する。

### 3. バウンドボールの発動
1. `BoundBallWeapon.Fire()` がワールド固定の右下ベクトルを初期方向として `BoundBallFireRequest` を生成する。
2. `WeaponEffectExecutor` が `BoundBallController` を取得し、初期位置・方向・攻撃力・最大バウンド回数3を設定する。
3. `BoundBallController` は毎フレーム移動し、敵または破壊可能オブジェクトへ衝突するとダメージを与える。
4. 衝突時に、接触点と衝突対象中心との差分から法線近似を求め、`Vector3.Reflect` で次の進行方向を計算する。
5. バウンド回数が3に達したらオブジェクトを返却する。

### 4. 火炎瓶の発動
1. `FlameBottleWeapon.Fire()` がプレイヤーの向きに応じた水平初速と上方向成分を含む `FlameBottleFireRequest` を生成する。
2. `WeaponEffectExecutor` が `FlameBottleProjectileController` を取得し、発射位置、初速、攻撃力、地面基準 `y` を設定する。
3. `FlameBottleProjectileController` は放物線運動で移動し、飛翔中はダメージ判定を持たない。
4. 射出物の `y` が地面オブジェクトから取得した基準 `y` 以下になったタイミングで着地し、`FlameAreaSpawnRequest` を発行するか、Executor経由で炎エリアを生成する。
5. `FlameAreaController` が着地点で一定時間残留し、敵と破壊可能オブジェクトへ一定間隔でダメージを与え、時間経過後に返却される。

## 実装ステップ

### Phase 1: 武器種追加とアップグレードUI拡張
- `UpgradeCardType` に新規3武器を追加する。
- `WeaponUpgradeUiController` のイベントシグネチャを `Action<UpgradeCardType>` に変更する。
- カード表示名のマッピングと、全武器候補から3件ランダム抽選するロジックを追加する。
- 既存の3ボタン構造を変えず、表示中の候補と押下結果が一致するようにする。

### Phase 2: Application層の武器定義追加
- `DroneWeapon`、`BoundBallWeapon`、`FlameBottleWeapon` と各リクエスト型を追加する。
- `WeaponService` のビルダー辞書・型辞書を拡張する。
- 武器候補一覧取得APIを `WeaponService` に追加し、UI側が列挙値のハードコードを持たないようにする。
- レベルアップ時の強化内容を各武器で定義する。

### Phase 3: Infrastructure層の実体追加
- `BulletFactory` に新規プレハブ参照を追加する。
- `WeaponEffectExecutor` に新規プールと初期化処理を追加する。
- `DroneController` を実装し、プレイヤー追従、周回、内部射撃を構築する。
- `BoundBallController` を実装し、衝突ダメージ、法線ベース反射、最大3回バウンドを実装する。
- `FlameBottleProjectileController` と `FlameAreaController` を実装し、放物線飛翔、地面Y判定、継続ダメージを構築する。

### Phase 4: 既存コンポーネントとの接続
- `PlayerController.InitializeWeaponSystems()` の依存注入が新規プレハブ参照を含んでも破綻しないことを確認する。
- `GameBootstrapper` と `BulletFactory` 経由で敵レジストリ、破壊可能オブジェクトスポナー、地面オブジェクト参照を接続する。
- `DamageFieldController` を拡張または `FlameAreaController` を分離し、破壊可能オブジェクトへの継続ダメージを有効にする。

### Phase 5: テストと動作確認
- EditModeテストを追加し、武器追加の管理ロジックと抽選ロジックを検証する。
- バウンド回数上限、火炎瓶の飛翔中非ダメージ、炎エリアの持続ダメージを確認する。
- PlayModeまたは手動確認で、武器強化UI、プレハブ参照、実際の見た目挙動を確認する。

## リスクと対策
- リスク: `WeaponUpgradeUiController` のイベント型変更で既存のUI選択処理が壊れる。
  - 対策: `WeaponUpgradePresenter` まで同一ターンで更新し、カードIndex依存の箇所を完全に撤去する。
- リスク: 武器候補のランダム抽選が毎回同じ結果、または重複を返す。
  - 対策: 候補一覧をコピーしてシャッフルし、先頭3件を採用する単純な一意抽選に固定する。
- リスク: ドローンが `WeaponBase` の都度発動で複数生成され続け、意図せず増殖する。
  - 対策: 初期実装は1武器につき1機維持とし、未展開時のみ生成、展開済みならステータス更新のみにする。
- リスク: バウンドボールの法線計算が不安定で、対象内部にめり込んで連続ヒットする。
  - 対策: 衝突後に法線方向へわずかに押し戻してから進行を再開し、同フレーム再衝突を防ぐ。
- リスク: 火炎瓶の地面基準 `y` がシーンごとにズレると着地タイミングが狂う。
  - 対策: 地面オブジェクト参照を `BulletFactory` など一箇所で解決し、そこからワールド座標を取得する。
- リスク: 破壊可能オブジェクトへの継続ダメージ追加で既存 `DamageFieldWeapon` の仕様も変わる可能性がある。
  - 対策: 既存 `DamageFieldController` を共用する場合は明示的なフラグで対象種別を制御し、影響範囲を限定する。

## 検証方針
- EditModeテスト
  - `WeaponService` が新規武器を生成できること。
  - 武器候補一覧から3件の一意なランダム候補を生成できること。
  - バウンド回数が3回で停止すること。
  - 火炎エリアが敵と破壊可能オブジェクトに継続ダメージを与えること。
- 手動確認
  - レベルアップ時に3枚のカードへ異なる武器候補が表示されること。
  - ドローンがプレイヤー周囲を周回し、敵がいる時のみ弾を撃つこと。
  - バウンドボールが右下へ飛び、最大3回反射してから消えること。
  - 火炎瓶がプレイヤーの向きに応じて飛び、地面到達で炎エリアを残すこと。
  - 炎エリアが時間経過で消え、破壊可能オブジェクトに触れると即座に破壊されること。
- Unity確認
  - `u console get -l E` で参照不足や NullReference が発生していないこと。
  - 必要に応じて `u play` で Main シーンを起動し、プレハブ設定漏れがないこと。

## コードスニペット（主要変更イメージ）
```csharp
public class WeaponUpgradeUiController : MonoBehaviour
{
    WeaponUpgradeUiController.UpgradeCardType[] _displayedTypes = new WeaponUpgradeUiController.UpgradeCardType[3];

    public event Action<UpgradeCardType> OnUpgradeCardSelected;

    public void SetUpgradeCandidates(IReadOnlyList<UpgradeCardType> candidates)
    {
        for (int i = 0; i < _displayedTypes.Length; i++)
        {
            _displayedTypes[i] = candidates[i];
            // ボタンラベル更新
        }
    }

    void OnCardClicked(int cardIndex)
    {
        if (!_isUpgradeUiOpen)
        {
            return;
        }

        OnUpgradeCardSelected?.Invoke(_displayedTypes[cardIndex]);
        CloseUpgradeUi();
    }
}
```

```csharp
public class WeaponService
{
    public IReadOnlyList<WeaponUpgradeUiController.UpgradeCardType> GetAvailableUpgradeTypes()
    {
        return new[]
        {
            WeaponUpgradeUiController.UpgradeCardType.Shooter,
            WeaponUpgradeUiController.UpgradeCardType.Throwing,
            WeaponUpgradeUiController.UpgradeCardType.DamageField,
            WeaponUpgradeUiController.UpgradeCardType.Drone,
            WeaponUpgradeUiController.UpgradeCardType.BoundBall,
            WeaponUpgradeUiController.UpgradeCardType.FlameBottle
        };
    }
}
```

```csharp
public class BoundBallController : ProjectileController
{
    int _remainingBounceCount;

    protected override void OnTriggerEnter(Collider other)
    {
        if (!IsDamageTarget(other))
        {
            return;
        }

        ApplyDamage(other);
        _remainingBounceCount--;

        if (_remainingBounceCount <= 0)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 normal = (transform.position - hitPoint).normalized;
        _direction = Vector3.Reflect(_direction, normal);
    }
}
```

```csharp
public class FlameBottleProjectileController : MonoBehaviour
{
    float _groundY;
    Vector3 _velocity;

    void Update()
    {
        _velocity += Physics.gravity * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        if (transform.position.y <= _groundY)
        {
            _effectExecutor.SpawnFlameArea(new FlameAreaSpawnRequest(
                new Vector3(transform.position.x, _groundY, transform.position.z),
                _sourcePow,
                _duration,
                _radius));
            ReturnToPoolOrDestroy();
        }
    }
}
```
