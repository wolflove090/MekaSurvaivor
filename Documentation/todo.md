# インゲーム帰還導線 ToDo

## Phase 1: Result UI を新規作成
- [x] `Assets/Survaivor/UI/Result/View/ResultUI.uxml` を新規作成する。
- [x] `ResultUI.uxml` に「撤退」ラベル、撃破数表示ラベル、「ホームへ」ボタンを配置する。
- [x] `Assets/Survaivor/UI/Result/View/ResultUI.uss` を新規作成し、オーバーレイとモーダルのスタイルを定義する。
- [x] `Assets/Survaivor/UI/Result/View/ResultUiController.cs` を新規作成し、UI構築と初期非表示化を実装する。
- [x] `Main.unity` に `ResultUiController` 用の `UIDocument` を配置し、UXML / USS を割り当てる。

## Phase 2: ResultUiController にホーム復帰導線を追加
- [x] `ResultUiController` でリザルトUI要素の参照取得を追加する。
- [x] リザルトUIの表示/非表示を切り替えるメソッドを追加する。
- [x] 「ホームへ」ボタンのクリックイベント登録・解除を追加する。
- [x] `Home` シーンへ戻る専用メソッドを追加する。
- [x] ホーム遷移時に `Time.timeScale = 1f` を復元する。
- [x] 多重押下防止とシーン未登録時の警告ログを追加する。

## Phase 3: GameStatsTracker を追加して統計集計を分離
- [x] `Assets/Survaivor/Game/Application/GameStatsTracker.cs` を新規作成する。
- [x] `GameStatsTracker` で `GameMessageBus.EnemyDied` の購読を追加する。
- [x] `GameStatsTracker` に 1プレイ単位の撃破数カウンタを追加する。
- [x] 将来の統計追加を見据えた責務名・公開APIに整理する。

## Phase 4: GameScreenPresenter と Bootstrapper を更新
- [x] `Assets/Survaivor/UI/GameScreen/Presentation/GameScreenPresenter.cs` に `GameStatsTracker` 参照を追加する。
- [x] `GameCleared` / `GameOver` 通知受信時に `GameStatsTracker` の値を使って `ResultUiController` を表示する処理を追加する。
- [x] リザルトUIの二重表示を防ぐフラグを追加する。
- [x] `Activate()` 時にリザルト表示状態を初期化し、再プレイ時の持ち越しを防ぐ。
- [x] `Assets/Survaivor/Game/Infrastructure/GameBootstrapper.cs` に `ResultUiController` の参照を追加する。
- [x] `Assets/Survaivor/Game/Infrastructure/GameBootstrapper.cs` で `GameStatsTracker` を生成する。
- [x] `FindFirstObjectByType<ResultUiController>()` で未設定時の補完を追加する。
- [x] `GameScreenPresenter` 生成時に `GameStatsTracker` と `ResultUiController` を注入する。
- [x] `ResultUiController` が未配置のときに検知できるログ方針を実装時に反映する。

## Phase 5: シーン設定と動作確認
- [x] `ProjectSettings/EditorBuildSettings.asset` に `Assets/Home/Home.unity` と `Assets/Survaivor/Main.unity` が登録済みであることを確認する。
- [ ] ゲームオーバー時にリザルトUIが1回だけ表示されることを確認する。
- [ ] ゲームクリア時に同じリザルトUIが表示されることを確認する。
- [ ] リザルトUIの撃破数が実際の撃破数と一致することを確認する。
- [ ] リザルトUIの「ホームへ」ボタンで `Assets/Home/Home.unity` へ戻れることを確認する。
- [ ] ホーム復帰後に UI と入力が停止していないことを確認する。
- [ ] ホームから再度「出撃」して、前回のリザルト状態と撃破数が持ち越されないことを確認する。
- [ ] 既存HUDが従来どおり更新されることを確認する。
