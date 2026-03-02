# デバッグ用トレーニングルーム ToDo

## Phase 1: ランタイム側の武器初期化と参照 API を整備
- [ ] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` に初期武器自動付与の ON/OFF を切り替えるシリアライズ設定を追加する。
- [ ] `PlayerController.InitializeWeaponSystems()` で初期武器自動付与 OFF 時は `Shooter` を追加しないようにする。
- [ ] `PlayerController.Update()` の `_weapon` 実行を null 安全にする。
- [ ] `PlayerController` に指定武器の所持有無を参照する公開 API を追加する。
- [ ] `PlayerController` に指定武器の現在レベルを参照する公開 API を追加する。
- [ ] `Assets/Survaivor/Character/Combat/Application/WeaponService.cs` に `UpgradeCardType` から武器 `Type` を引く読み取り用 API を追加する。
- [ ] `WeaponService` に指定武器の所持判定を行う補助 API を追加する。
- [ ] `WeaponService` に指定武器の現在レベルを取得する補助 API を追加する。
- [ ] `Main` シーン相当の既存挙動で初期武器が維持される前提を崩していないことを確認する。

## Phase 2: サンドボックス用 EditorWindow を実装
- [ ] `Assets/Survaivor/Editor/SandboxWeaponDebugWindow.cs` を新規作成する。
- [ ] EditorWindow で既存全武器を列挙して描画する。
- [ ] 各武器ごとに追加用トグルを表示する。
- [ ] 各武器ごとにレベル 1 から 5 のスライダーを表示する。
- [ ] Play 中の `PlayerController` を解決できない場合は、操作を無効化して案内表示だけを出す。
- [ ] 未所持武器のトグルを OFF から ON にした時、`PlayerController.ApplyWeaponUpgrade()` を1回呼んで武器追加する。
- [ ] 武器追加後にトグル ON / スライダー 1 の状態へ再同期する。
- [ ] スライダーを現在レベルより高い値へ動かした時、差分回数だけ `ApplyWeaponUpgrade()` を呼ぶ。
- [ ] 操作後に `PlayerController` から状態を再読込し、EditorWindow 表示を実状態へ同期する。
- [ ] トグルを ON から OFF にした時は武器削除を行わず、未対応ログを出す。
- [ ] トグル OFF 操作後は UI を即時に実状態へ差し戻す。
- [ ] スライダーを現在レベル未満へ下げた時はレベル低下を行わず、未対応ログを出す。
- [ ] スライダー減少操作後は UI を即時に実状態へ差し戻す。
- [ ] IMGUI 再描画で同じ操作が重複適用されないよう、前回反映値の管理を入れる。

## Phase 3: `Sandbox.unity` 再生時の自動起動フックを実装
- [ ] `Assets/Survaivor/Editor/SandboxPlayModeHook.cs` を新規作成する。
- [ ] `EditorApplication.playModeStateChanged` を購読し、`EnteredPlayMode` のみ処理する。
- [ ] アクティブシーンのパスが `Assets/Survaivor/Editor/Sandbox.unity` と一致する場合だけ処理する。
- [ ] `Sandbox.unity` 再生時に `SandboxWeaponDebugWindow` を自動で開く。
- [ ] 他シーン再生時には Window を開かない。
- [ ] 既存の `Assets/Survaivor/Editor/GameTimePlayToolbarExtension.cs` とイベント競合しないことを確認する。

## Phase 4: サンドバッグ用エネミーを実装
- [ ] `Assets/Survaivor/Character/Enemy/Infrastructure/SandboxDummyEnemy.cs` を新規作成する。
- [ ] サンドバッグ用コンポーネントで HP 減少を無効化する。
- [ ] サンドバッグ用コンポーネントで死亡処理への遷移を抑止する。
- [ ] サンドバッグ用コンポーネントで移動を停止し、配置位置から離脱しないようにする。
- [ ] サンドバッグ用コンポーネントでプレイヤーへの接触ダメージを発生させない構成を入れる。
- [ ] `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyController.cs` または `Assets/Survaivor/Character/Infrastructure/HealthComponent.cs` に、通常敵を壊さない最小限の拡張ポイントを追加する。
- [ ] サンドバッグが `EnemyRegistry` 上では攻撃対象として残り続けることを確認する。

## Phase 5: `Sandbox.unity` のシーン構成を調整
- [ ] `Assets/Survaivor/Editor/Sandbox.unity` のプレイヤーで初期武器自動付与を OFF に設定する。
- [ ] `Sandbox.unity` から `EnemySpawner` を外す、または無効化する。
- [ ] `Sandbox.unity` から経験値関連オブジェクトを外す、または無効化する。
- [ ] `Sandbox.unity` から武器強化 UI を外す、または無効化する。
- [ ] `Sandbox.unity` の `GameBootstrapper` 参照を最小構成に合わせて再設定する。
- [ ] `Sandbox.unity` 上のサンドバッグ用エネミーへ `SandboxDummyEnemy` をアタッチする。
- [ ] `Sandbox.unity` が単体再生可能で、未設定参照で起動不能にならないことを確認する。

## Phase 6: テストを追加
- [ ] `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs` を新規作成する。
- [ ] 初期武器自動付与 OFF で武器なし開始できることを検証する。
- [ ] 武器追加後に所持判定とレベル 1 が参照できることを検証する。
- [ ] 目標レベルまでの差分適用でレベルが上がることを検証する。
- [ ] レベル低下未対応時に状態を変えない分岐を検証する。
- [ ] 武器削除未対応時に状態を変えない分岐を検証する。
- [ ] `Assets/Survaivor/Tests/EditMode/Editor/SandboxWeaponDebugWindowTests.cs` を新規作成する。
- [ ] `Sandbox.unity` 再生時のみ自動起動対象になることを検証する。
- [ ] `PlayerController` 未検出時に操作不能表示へ入ることを検証する。
- [ ] `SandboxDummyEnemy` の無敵と非接触ダメージを検証するテストを追加する。

## Phase 7: 動作確認
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] `Sandbox.unity` を再生し、EditorWindow が自動表示されることを確認する。
- [ ] 初期状態でプレイヤーが移動可能かつ攻撃しないことを確認する。
- [ ] 任意の武器トグルを ON にして武器追加できることを確認する。
- [ ] スライダーを 1 から 5 に上げて、武器レベルが指定どおり上がることを確認する。
- [ ] スライダーを下げた時にレベルが下がらず、未対応ログが出ることを確認する。
- [ ] トグルを OFF にした時に武器が削除されず、未対応ログが出ることを確認する。
- [ ] サンドバッグへ攻撃しても破壊されず、プレイヤーへ接触ダメージを与えないことを確認する。
- [ ] `u console get -l E` で `Sandbox.unity` 再生時にエラーが出ていないことを確認する。
