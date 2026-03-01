# ホーム出撃導線 ToDo

## Phase 1: ホーム側の遷移導線追加
- `HomeScreenUiController` にサバイバーゲーム遷移先シーン名 `Main` の定数を追加する。
- `UnityEngine.SceneManagement` を導入し、「出撃」ボタン押下時に `SceneManager.LoadScene("Main")` を呼ぶ専用メソッドを追加する。
- 「出撃」ボタン押下時に、既存ログ出力後に遷移処理が走るよう `_onSortieClicked` を更新する。
- ボタン連打による多重遷移を防ぐフラグを追加する。
- シーン名不正時に警告ログを出す。

## Phase 2: シーン登録
- `ProjectSettings/EditorBuildSettings.asset` に `Assets/Home/Home.unity` を追加する。
- `ProjectSettings/EditorBuildSettings.asset` に `Assets/Survaivor/Main.unity` を追加する。
- `Main` のシーン名でロードできる状態になっていることを確認する。

## Phase 3: 動作確認
- `Assets/Home/Home.unity` を再生し、「出撃」ボタンで `Assets/Survaivor/Main.unity` に遷移することを確認する。
- 遷移後に `GameManager` と `PlayerController` が通常どおり動作することを確認する。
- 「図鑑」「着替え」「物語」ボタンの既存挙動に影響がないことを確認する。
- 「出撃」ボタン連打時に例外や多重ロードが発生しないことを確認する。

## Future TODO
- インゲームからホームへ戻る導線を実装する際、ホーム遷移前またはホーム復帰時に `Time.timeScale = 1f` を復元する。
