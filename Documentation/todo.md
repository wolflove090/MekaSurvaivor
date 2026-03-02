# デバッグ用トレーニングルーム ToDo

## Phase 1: ランタイム側の武器初期化と参照 API を整備
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` に初期武器自動付与の ON/OFF を切り替えるシリアライズ設定を追加する。
- [x] `PlayerController.InitializeWeaponSystems()` で初期武器自動付与 OFF 時は `Shooter` を追加しないようにする。
- [x] `PlayerController.Update()` の `_weapon` 実行を null 安全にする。
- [x] `PlayerController` に指定武器の所持有無を参照する公開 API を追加する。
- [x] `PlayerController` に指定武器の現在レベルを参照する公開 API を追加する。
- [x] `Assets/Survaivor/Character/Combat/Application/WeaponService.cs` に `UpgradeCardType` から武器 `Type` を引く読み取り用 API を追加する。
- [x] `WeaponService` に指定武器の所持判定を行う補助 API を追加する。
- [x] `WeaponService` に指定武器の現在レベルを取得する補助 API を追加する。
- [ ] `Main` シーン相当の既存挙動で初期武器が維持される前提を崩していないことを確認する。

## Phase 2: サンドボックス用 EditorWindow を実装
- [x] `Assets/Survaivor/Editor/SandboxWeaponDebugWindow.cs` を新規作成する。
- [x] EditorWindow で既存全武器を列挙して描画する。
- [x] 各武器ごとに追加用トグルを表示する。
- [x] 各武器ごとにレベル 1 から 5 のスライダーを表示する。
- [x] Play 中の `PlayerController` を解決できない場合は、操作を無効化して案内表示だけを出す。
- [x] 未所持武器のトグルを OFF から ON にした時、武器をレベル 1 で追加する。
- [x] 武器追加後にトグル ON / スライダー 1 の状態へ再同期する。
- [x] スライダーを変更した時、目標レベルへ武器状態を同期する。
- [x] 操作後に `PlayerController` から状態を再読込し、EditorWindow 表示を実状態へ同期する。
- [x] トグルを ON から OFF にした時は武器を取り上げる。
- [x] トグル OFF 操作後は UI を即時に実状態へ同期する。
- [x] スライダーを現在レベル未満へ下げた時はレベル低下を適用する。
- [x] スライダー減少操作後は UI を即時に実状態へ同期する。
- [x] IMGUI 再描画で同じ操作が重複適用されないよう、前回反映値の管理を入れる。

## Phase 3: `Sandbox.unity` 再生時の自動起動フックを実装
- [x] `Assets/Survaivor/Editor/SandboxPlayModeHook.cs` を新規作成する。
- [x] `EditorApplication.playModeStateChanged` を購読し、`EnteredPlayMode` のみ処理する。
- [x] アクティブシーンのパスが `Assets/Survaivor/Editor/Sandbox.unity` と一致する場合だけ処理する。
- [x] `Sandbox.unity` 再生時に `SandboxWeaponDebugWindow` を自動で開く。
- [x] 他シーン再生時には Window を開かない。
- [ ] 既存の `Assets/Survaivor/Editor/GameTimePlayToolbarExtension.cs` とイベント競合しないことを確認する。

## Phase 4: サンドバッグ用エネミーを実装
- [x] `Assets/Survaivor/Character/Enemy/Infrastructure/SandboxDummyEnemy.cs` を新規作成する。
- [x] サンドバッグ用コンポーネントで HP 減少を無効化する。
- [x] サンドバッグ用コンポーネントで死亡処理への遷移を抑止する。
- [x] サンドバッグ用コンポーネントで移動を停止し、配置位置から離脱しないようにする。
- [x] サンドバッグ用コンポーネントでプレイヤーへの接触ダメージを発生させない構成を入れる。
- [x] `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyController.cs` または `Assets/Survaivor/Character/Infrastructure/HealthComponent.cs` に、通常敵を壊さない最小限の拡張ポイントを追加する。
- [ ] サンドバッグが `EnemyRegistry` 上では攻撃対象として残り続けることを確認する。

## Phase 5: `Sandbox.unity` のシーン構成を調整
- [x] `Assets/Survaivor/Editor/Sandbox.unity` のプレイヤーで初期武器自動付与を OFF に設定する。
- [x] `Sandbox.unity` から `EnemySpawner` を外す、または無効化する。
- [x] `Sandbox.unity` から経験値関連オブジェクトを外す、または無効化する。
- [ ] `Sandbox.unity` から武器強化 UI を外す、または無効化する。
- [x] `Sandbox.unity` の `GameBootstrapper` 参照を最小構成に合わせて再設定する。
- [x] `Sandbox.unity` 上のサンドバッグ用エネミーへ `SandboxDummyEnemy` をアタッチする。
- [ ] `Sandbox.unity` が単体再生可能で、未設定参照で起動不能にならないことを確認する。

## Phase 6: テストを追加
- [x] `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs` を新規作成する。
- [x] 初期武器自動付与 OFF で武器なし開始できることを検証する。
- [x] 武器追加後に所持判定とレベル 1 が参照できることを検証する。
- [x] 目標レベルまでの差分適用でレベルが上がることを検証する。
- [x] レベル低下時に指定レベルまで状態が再構築されることを検証する。
- [x] 武器削除時に未所持状態へ遷移することを検証する。
- [x] `Assets/Survaivor/Tests/EditMode/Editor/SandboxWeaponDebugWindowTests.cs` を新規作成する。
- [x] `Sandbox.unity` 再生時のみ自動起動対象になることを検証する。
- [ ] `PlayerController` 未検出時に操作不能表示へ入ることを検証する。
- [x] `SandboxDummyEnemy` の無敵と非接触ダメージを検証するテストを追加する。

## Phase 7: 動作確認
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] `Sandbox.unity` を再生し、EditorWindow が自動表示されることを確認する。
- [ ] 初期状態でプレイヤーが移動可能かつ攻撃しないことを確認する。
- [ ] 任意の武器トグルを ON にして武器追加できることを確認する。
- [ ] スライダーを 1 から 5 に上げて、武器レベルが指定どおり上がることを確認する。
- [ ] スライダーを下げた時にレベルが指定どおり下がることを確認する。
- [ ] トグルを OFF にした時に武器が削除されることを確認する。
- [ ] サンドバッグへ攻撃しても破壊されず、プレイヤーへ接触ダメージを与えないことを確認する。
- [ ] `u console get -l E` で `Sandbox.unity` 再生時にエラーが出ていないことを確認する。
