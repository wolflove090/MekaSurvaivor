# プロジェクト再構築リファクタ ToDo

## Phase 1: 事前整備と配置整理
- [x] `Assets/Scripts/Domain` を新規作成する
- [x] `Assets/Scripts/Application` を新規作成する
- [ ] `Assets/Scripts/Infrastructure/Unity` を新規作成する
- [ ] `Assets/Scripts/Presentation/UI` を新規作成する
- [ ] `Assets/Scripts/Presentation/World` を新規作成する
- [ ] 既存スクリプトの責務マッピング表を作成し、移行先レイヤーを明記する

## Phase 2: ブートストラップと依存解決
- [ ] `GameBootstrapper` を新規作成する
- [ ] メインシーンで必要な参照を `GameBootstrapper` に集約する
- [ ] `GameManager.Instance` への新規依存を止める
- [ ] `PlayerController.Instance` への新規依存を止める
- [ ] `EnemySpawner.Instance` への新規依存を止める

## Phase 3: ゲーム進行の純C#化
- [x] `GameSessionState` を作成する
- [x] `GameSessionService` を作成する
- [x] `GameManager` から時間進行ロジックを `GameSessionService` へ委譲する
- [ ] ゲームクリア・ゲームオーバー状態の管理元を `GameSessionState` に一本化する
- [ ] ゲーム進行イベントの通知方法を整理する

## Phase 4: プレイヤー進行の純C#化
- [x] `PlayerState` を作成する
- [x] `PlayerProgressionService` を作成する
- [x] `PlayerExperience` の経験値加算とレベルアップ計算をサービスへ委譲する
- [x] スタイル変更処理を `PlayerController` からサービスへ移す
- [x] プレイヤーの速度倍率・経験値倍率の保持先を `PlayerState` に集約する
- [ ] `CharacterStats` の実計算責務を見直し、参照用責務へ縮小する

## Phase 5: 武器システム再編
- [x] `WeaponState` を作成する
- [x] `WeaponService` を作成する
- [x] `WeaponBase` のクールダウン進行をサービス管理へ移す
- [x] `PlayerController.ApplyWeaponUpgrade` の分岐をサービスへ移す
- [x] 武器種別の管理方法を `enum + switch` から登録テーブル化する
- [ ] 既存武器の発動挙動が維持されることを確認する

## Phase 6: 敵生成システム再編
- [ ] `EnemySpawnState` を作成する
- [ ] `EnemySpawnService` を作成する
- [ ] `EnemySpawner` のスポーンタイマー管理をサービスへ移す
- [ ] スポーン位置決定ロジックをサービスへ移す
- [ ] Unity側の `EnemySpawner` は生成とターゲット接続に責務を限定する
- [ ] 近傍探索ロジックの互換性を確認する

## Phase 7: UIのPresenter化
- [ ] `GameScreenPresenter` を新規作成する
- [ ] `WeaponUpgradePresenter` を新規作成する
- [ ] `GameScreenUiController` から状態収集と差分判定を Presenter へ移す
- [ ] `GameScreenUiController` の `FindFirstObjectByType` 依存を削減する
- [ ] `WeaponUpgradeUiController` の `PlayerController` 直参照を削除し、選択通知に限定する
- [ ] レベルアップ時のUI表示フローを Presenter 経由へ差し替える

## Phase 8: 通知基盤の整理
- [ ] `GameMessageBus` または同等の局所通知手段を実装する
- [ ] 新規コードで `GameEvents` を使用しない
- [ ] プレイヤー進行通知を `GameEvents` から新通知基盤へ移す
- [ ] UI更新通知を `GameEvents` から新通知基盤へ移す
- [ ] 旧 `GameEvents` の残存利用箇所を洗い出す

## Phase 9: テスト整備
- [ ] `Assets/Tests/EditMode` 配下のテスト配置方針を決める
- [ ] `GameSessionService` の EditMode テストを追加する
- [ ] `PlayerProgressionService` の EditMode テストを追加する
- [ ] `WeaponService` の EditMode テストを追加する
- [ ] `EnemySpawnService` の EditMode テストを追加する
- [ ] スタイル変更と倍率解除の回帰テストを追加する

## Phase 10: 旧実装の縮退と最終確認
- [ ] 旧シングルトン依存を削減または撤去する
- [ ] 不要になった `GameEvents` のイベントを削減する
- [ ] 役割を失った旧メソッドを削除する
- [ ] メインシーンで回帰確認を実施する
- [ ] 必要に応じてドキュメントを更新し、保守ルールを明記する
