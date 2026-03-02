# 新規武器追加 ToDo

## Phase 1: 武器強化UIの候補選択を拡張
- [x] `Assets/Survaivor/UI/WeaponUpgrade/View/WeaponUpgradeUiController.cs` の `UpgradeCardType` に `Drone`、`BoundBall`、`FlameBottle` を追加する。
- [x] `WeaponUpgradeUiController` の `OnUpgradeCardSelected` を `Action<UpgradeCardType>` に変更する。
- [x] 3枚のカードに表示中の武器候補を保持する配列またはリストを追加する。
- [x] 全武器候補から重複なしで3件をランダム抽選するロジックを追加する。
- [x] カードラベルの表示内容を選ばれた `UpgradeCardType` に応じて切り替えられるようにする。
- [x] ボタン押下時にカードIndexではなく、表示中の `UpgradeCardType` を通知する。
- [x] `Assets/Survaivor/UI/WeaponUpgrade/Presentation/WeaponUpgradePresenter.cs` を新しいイベント型へ対応させる。

## Phase 2: 武器生成管理のApplication層を拡張
- [x] `Assets/Survaivor/Character/Combat/Application/WeaponService.cs` のビルダー辞書へ新規3武器を追加する。
- [x] `WeaponService` の型辞書へ新規3武器を追加する。
- [x] UIが参照するための武器候補一覧取得APIを `WeaponService` に追加する。
- [x] `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs` を新規作成する。
- [x] `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs` を新規作成する。
- [x] `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs` を新規作成する。
- [x] ドローン用、バウンドボール用、火炎瓶用の各 `*Request` クラスを新規作成する。
- [x] ドローンのレベルアップで発射間隔が短くなる処理を実装する。
- [x] 火炎瓶のレベルアップ時に強化する値（持続時間、範囲、間隔のいずれか）を確定して実装する。

## Phase 3: 実行ポートとプレハブ参照を拡張
- [x] `Assets/Survaivor/Character/Combat/Application/IWeaponEffectExecutor.cs` に新規武器用メソッドを追加する。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/WeaponEffectExecutor.cs` にドローン、バウンドボール、火炎瓶、炎エリア用の実行処理を追加する。
- [x] `WeaponEffectExecutor` に新規プールを追加する。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/Factories/BulletFactory.cs` に新規武器プレハブ参照を追加する。
- [ ] `BulletFactory` で火炎瓶の着地判定に使う地面オブジェクト参照を保持できるようにする。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` の武器システム初期化が新規依存追加後も成立するよう確認し、必要なら調整する。

## Phase 4: ドローンのInfrastructure実装
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/Drones/DroneController.cs` を新規作成する。
- [x] ドローンがプレイヤーを追従しながら周回する移動処理を実装する。
- [x] ドローン内部の射撃タイマーを実装する。
- [x] `EnemyRegistry` を使った最寄り敵探索を実装する。
- [x] 敵がいる時だけ弾を発射し、いない時は待機する処理を実装する。
- [x] 初期実装では1武器につき1機だけ維持し、再展開時に増殖しない制御を入れる。

## Phase 5: バウンドボールのInfrastructure実装
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/Projectiles/BoundBallController.cs` を新規作成する。
- [x] 初期方向をワールド固定の右下にする処理を実装する。
- [x] 敵および破壊可能オブジェクトへのダメージ処理を実装する。
- [x] 衝突点と `Collider.ClosestPoint` を使った法線近似を実装する。
- [x] `Vector3.Reflect` による反射方向計算を実装する。
- [x] 反射後のめり込み防止の押し戻し処理を追加する。
- [x] 最大3回のヒット後にプール返却する処理を実装する。

## Phase 6: 火炎瓶と炎エリアのInfrastructure実装
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/Projectiles/FlameBottleProjectileController.cs` を新規作成する。
- [x] プレイヤーの向きに応じた水平移動と上方向成分を持つ放物線移動を実装する。
- [x] 飛翔中はダメージ判定を行わない構成にする。
- [x] 地面オブジェクトから取得した基準 `y` に到達したら着地する処理を実装する。
- [x] 着地時に炎エリア生成を行い、自身をプール返却する処理を実装する。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/DamageFields/FlameAreaController.cs` を新規作成する、または既存 `DamageFieldController` を拡張する。
- [x] 炎エリアが一定時間その場に残る処理を実装する。
- [x] 炎エリアが敵と破壊可能オブジェクトに継続ダメージを与える処理を実装する。
- [x] 炎エリアが時間経過で消滅する処理を実装する。

## Phase 7: テスト追加と回帰確認
- [x] `Assets/Survaivor/Tests/EditMode/Combat/WeaponServiceTests.cs` に新規武器登録の検証を追加する。
- [x] 武器候補抽選ロジックのユニットテストを追加する。
- [ ] バウンドボールの最大3回バウンドを検証するテストを追加する。
- [ ] 火炎瓶の飛翔中非ダメージを検証するテストを追加する。
- [ ] 炎エリアの継続ダメージ対象に破壊可能オブジェクトを含むことを検証するテストを追加する。
- [ ] `u tests run edit` でEditModeテストを実行する。
- [ ] `u play` でMainシーンを起動し、武器強化UIから新規武器を取得できることを確認する。
- [ ] `u console get -l E` でエラーが出ていないことを確認する。
