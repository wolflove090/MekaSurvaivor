# ホーム出撃導線 実装計画

## 実装方針
- ホーム画面の「出撃」導線は、既存の `HomeScreenUiController` に最小限のシーン遷移責務を追加して接続する。
- 遷移先は `SceneManager.LoadScene` のシーン名 `"Main"` で固定し、シーン名の定義は `HomeScreenUiController` 内の単一点に集約して分散ハードコードを防ぐ。
- ボタン押下時の既存ログは残しつつ、同じイベントハンドラ内で遷移処理を実行する。
- 遷移先 `Assets/Survaivor/Main.unity` のゲーム本体ロジック（`GameManager`、`PlayerController`）は既存構成を利用し、ホーム側から余計な初期化は追加しない。
- `SceneManager.LoadScene("Main")` が実行可能になるよう、`ProjectSettings/EditorBuildSettings.asset` に少なくとも `Assets/Home/Home.unity` と `Assets/Survaivor/Main.unity` を登録する。
- 今回はホームへ戻る導線は実装しないため、`Time.timeScale` 復元は未対応のままにし、将来の戻り導線実装時に対応する TODO をドキュメントへ明記する。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Home/HomeScreenUiController.cs`
  - `UnityEngine.SceneManagement` を利用して「出撃」押下時に `Main` シーンをロードする。
  - 遷移中の多重押下を抑止するためのフラグを追加する。
  - シーン名未設定や遷移失敗を追跡できるログを追加する。
- `ProjectSettings/EditorBuildSettings.asset`
  - `SceneManager.LoadScene("Main")` で解決できるよう、ビルド設定に `Home` / `Main` シーンを登録する。

### ドキュメント更新対象
- `Documentation/todo.md`
  - 実装タスクと、将来のホーム復帰時に `Time.timeScale` を復元する TODO を整理する。

## データフロー / 処理フロー

### 1. ホーム画面初期化
1. `Assets/Home/Home.unity` 上で `HomeScreenUiController.Awake()` が UI Toolkit のルートとボタン参照を初期化する。
2. `BuildHandlers()` で `_onSortieClicked` が登録される。
3. `OnEnable()` で `_sortieButton.clicked` に `_onSortieClicked` が接続される。

### 2. 出撃ボタン押下
1. ユーザーがホーム画面の「出撃」ボタンを押す。
2. `_onSortieClicked` が既存のログ出力を行う。
3. `HomeScreenUiController` が多重遷移防止フラグを確認する。
4. 未遷移状態であればフラグを立て、`SceneManager.LoadScene("Main")` を実行する。
5. シーン名が空、またはロード不可の状態なら警告ログを出してフラグを戻す。

### 3. サバイバーゲーム開始
1. Unity が `Assets/Survaivor/Main.unity` をロードする。
2. シーン内の既存 `GameManager` が `Awake()` でゲーム進行管理を初期化する。
3. シーン内の既存 `PlayerController` が `Awake()` で入力、武器、各種コンポーネントを初期化する。
4. 追加の接続なしで既存ゲームプレイが開始される。

## 実装ステップ

### Phase 1: ホーム側の遷移処理追加
- `HomeScreenUiController` にシーン遷移用の定数を追加する。
- `_onSortieClicked` の処理を、ログ出力に加えて専用メソッド経由で `Main` シーンをロードする形へ変更する。
- 多重押下で `LoadScene` が連続実行されないようフラグ管理を追加する。

### Phase 2: シーン登録
- `ProjectSettings/EditorBuildSettings.asset` に `Assets/Home/Home.unity` と `Assets/Survaivor/Main.unity` を有効状態で登録する。
- シーン名 `"Main"` でロード可能な状態を担保する。

### Phase 3: 動作確認と TODO 整理
- ホームシーンから「出撃」押下で `Main` へ遷移することを確認する。
- 遷移後に `GameManager` / `PlayerController` が通常どおり動作することを確認する。
- 将来のホーム復帰時に `Time.timeScale` を 1 に戻す TODO を `Documentation/todo.md` に残す。

## リスクと対策
- リスク: `SceneManager.LoadScene("Main")` がビルド設定未登録で失敗する。
  - 対策: `ProjectSettings/EditorBuildSettings.asset` に `Home` と `Main` を明示登録し、ロード先を固定する。
- リスク: ボタン連打で複数回ロード要求が走り、不安定な遷移になる。
  - 対策: `HomeScreenUiController` に遷移中フラグを持たせ、2回目以降の押下を無視する。
- リスク: ホーム側で余計な初期化を追加してサバイバーゲームの既存起動フローを壊す。
  - 対策: ホーム側はシーンロードのみを担当し、ゲーム開始処理は `Main.unity` 内の既存コンポーネントに委ねる。
- リスク: 将来ゲーム終了後にホームへ戻る際、`Time.timeScale` が 0 のまま残る。
  - 対策: 今回は実装しないが、戻り導線実装時に `Time.timeScale = 1f` を復元する TODO を明示して追跡する。

## 検証方針
- 手動確認
  - `Assets/Home/Home.unity` を再生し、「出撃」ボタン押下で `Assets/Survaivor/Main.unity` へ遷移すること。
  - 遷移後にプレイヤーが操作でき、ゲーム進行が開始されること。
  - 「図鑑」「着替え」「物語」ボタンは従来どおりログ出力のみであること。
  - 「出撃」ボタンを連打しても例外や多重遷移が発生しないこと。
- エラー確認
  - シーン参照が不正な場合、Unity Console で追跡可能な警告またはエラーログが出ること。

## コードスニペット（主要変更イメージ）
```csharp
using UnityEngine.SceneManagement;

public class HomeScreenUiController : MonoBehaviour
{
    const string SurvivorSceneName = "Main";

    bool _isSceneLoading;

    void BuildHandlers()
    {
        _onSortieClicked = () =>
        {
            LogButtonClick("出撃");
            LoadSurvivorScene();
        };
    }

    void LoadSurvivorScene()
    {
        if (_isSceneLoading)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SurvivorSceneName))
        {
            Debug.LogWarning("HomeScreenUiController: 遷移先シーン名が未設定です。");
            return;
        }

        _isSceneLoading = true;
        SceneManager.LoadScene(SurvivorSceneName);
    }
}
```
