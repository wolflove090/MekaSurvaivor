# インゲーム帰還導線 実装計画

## 実装方針
- リザルトUIは `GameScreenUiController` へ混在させず、独立した `ResultUiController` として別 `UIDocument` で実装する。これにより、HUD更新責務と終了導線責務を分離する。
- 終了状態の監視は `GameScreenPresenter` が担当し、撃破数集計は専用の `GameStatsTracker` に分離する。`GameStatsTracker` は `EnemyDied` 通知で撃破数を加算し、`GameScreenPresenter` は `GameCleared` / `GameOver` 通知時にその集計値を `ResultUiController` へ渡す。
- ホーム遷移操作は `ResultUiController` 側で受ける。`SceneManager.LoadScene("Home")` の直前に `Time.timeScale = 1f` を復元し、ホーム復帰後の操作不能を防ぐ。
- 既存の勝敗判定ロジック (`GameManager`, `GameSessionService`, `PlayerController`) と既存HUD更新 (`GameScreenUiController`) は極力変更せず、結果表示の導線だけを追加する。
- 1プレイ単位の状態（統計値、リザルト表示済みフラグ、ホーム遷移中フラグ）はシーン内オブジェクトに閉じ込め、ホームへ戻った時点で破棄される構成にする。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/UI/GameScreen/Presentation/GameScreenPresenter.cs`
  - `GameCleared` / `GameOver` 受信時に `GameStatsTracker` の撃破数を読み取り、`ResultUiController` へリザルト表示を依頼する。
  - 初期化時にリザルト表示状態をリセットする。
- `Assets/Survaivor/Game/Application/GameStatsTracker.cs`
  - 新規作成。`GameMessageBus.EnemyDied` を購読して1プレイ中の撃破数を集計する。
  - 必要に応じて将来の統計項目追加先となるランタイム集計責務を持つ。
- `Assets/Survaivor/Game/Infrastructure/GameBootstrapper.cs`
  - `GameStatsTracker` を生成し、`GameScreenPresenter` へ渡す。
  - シーン内の `ResultUiController` を参照解決する。
  - `GameScreenPresenter` 生成時に `ResultUiController` 参照を渡す。
- `Assets/Survaivor/UI/Result/View/ResultUiController.cs`
  - 新規作成。リザルトUIの構築、表示/非表示制御、「ホームへ」ボタン押下時のホーム遷移を担当する。
- `Assets/Survaivor/UI/Result/View/ResultUI.uxml`
  - 新規作成。リザルトオーバーレイ、見出し「撤退」、撃破数表示、「ホームへ」ボタンを定義する。
- `Assets/Survaivor/UI/Result/View/ResultUI.uss`
  - 新規作成。リザルトモーダル、背景オーバーレイ、ボタンのスタイルを定義する。

### 確認対象（原則コード変更なし）
- `Assets/Survaivor/Game/Application/GameMessageBus.cs`
  - 既存の `EnemyDied` / `GameCleared` / `GameOver` 通知をそのまま利用する。追加イベントは不要。
- `Assets/Survaivor/UI/GameScreen/View/GameScreenUiController.cs`
  - HUD専用のまま維持し、リザルトUI責務を持たせないことを確認する。
- `ProjectSettings/EditorBuildSettings.asset`
  - `Home` / `Main` シーンが既に登録済みのため、差分が不要か確認する。

## データフロー / 処理フロー

### 1. インゲームUI初期化
1. `Main.unity` 起動時に `GameBootstrapper` が `GameMessageBus` を生成し、`GameScreenUiController` と `ResultUiController` の参照を解決する。
2. `GameBootstrapper` が `GameMessageBus` を引数に `GameStatsTracker` を生成し、1プレイ分の統計集計を開始する。
3. `ResultUiController.Awake()` が UXML / USS を読み込み、見出し、撃破数ラベル、「ホームへ」ボタンの参照を取得する。
4. `GameBootstrapper` が `GameScreenPresenter` に `GameScreenUiController`、プレイヤー参照群、`GameMessageBus`、`GameStatsTracker`、`ResultUiController` を渡して接続する。
5. `GameScreenPresenter.Activate()` で HUD 購読を開始し、同時に `ResultUiController` を非表示へ初期化する。

### 2. プレイ中の撃破数集計
1. 敵が死亡すると `EnemyController.OnDied()` が `GameMessageBus.RaiseEnemyDied(gameObject)` を呼ぶ。
2. `GameStatsTracker` が `EnemyDied` を受け取り、内部の撃破数カウンタを1増やす。
3. 通常プレイ中は `GameScreenUiController` でHUDのみ更新し、リザルト表示用の撃破数は `GameStatsTracker` に保持する。

### 3. ゲーム終了時のリザルト表示
1. ゲームオーバー時は `PlayerController.GameOver()` から `GameManager.MarkGameOver()` が呼ばれ、`GameManager.HandleGameOver()` が `RaiseGameOver()` と `Time.timeScale = 0f` を実行する。
2. ゲームクリア時は `GameSessionService.Tick()` 経由で `GameManager.HandleGameClear()` が `RaiseGameCleared()` と `Time.timeScale = 0f` を実行する。
3. `GameScreenPresenter` が `GameOver` または `GameCleared` を受け取り、未表示なら `GameStatsTracker` の累計撃破数を読み取って `ResultUiController` に対して「撤退」と累計撃破数を渡し、リザルトUIを表示する。
4. `ResultUiController` はオーバーレイを表示し、「ホームへ」ボタンのみ押せる状態にする。

### 4. ホーム遷移
1. ユーザーが `ResultUiController` 上の「ホームへ」ボタンを押す。
2. `ResultUiController` が多重遷移防止フラグを確認し、`Home` シーンがロード可能か検証する。
3. 遷移可能なら `Time.timeScale = 1f` を復元してから `SceneManager.LoadScene("Home")` を実行する。
4. `Home.unity` 読み込み後は `HomeScreenUiController` の既存導線で再度 `Main` へ出撃できる。

## 実装ステップ

### Phase 1: Result UI を新規作成
- `Assets/Survaivor/UI/Result/View/` 配下を作成する。
- `ResultUI.uxml` にオーバーレイ、見出し「撤退」、撃破数表示ラベル、「ホームへ」ボタンを定義する。
- `ResultUI.uss` に中央モーダル表示、背景オーバーレイ、ボタンのスタイルを追加する。
- `ResultUiController` を新規作成し、UI構築、参照取得、初期非表示化を実装する。

### Phase 2: ResultUiController にホーム復帰導線を追加
- `ResultUiController` に「ホームへ」ボタンのコールバック登録/解除を追加する。
- `SceneManager` ベースの `Home` シーン遷移処理を追加する。
- `Time.timeScale = 1f` 復元、多重押下防止、`Application.CanStreamedLevelBeLoaded("Home")` チェック、例外ログ出力を追加する。

### Phase 3: GameStatsTracker と Presenter / Bootstrapper を接続
- `GameStatsTracker` を新規作成し、`EnemyDied` 購読と撃破数集計を実装する。
- `GameScreenPresenter` に `GameStatsTracker` 参照、`ResultUiController` 参照、リザルト表示済みフラグを追加する。
- `GameOver` / `GameCleared` 受信時に `GameStatsTracker` の値を使って `ResultUiController` の表示APIを呼ぶ。
- `GameBootstrapper` に `GameStatsTracker` の生成、`ResultUiController` の参照解決、Presenter への注入を追加する。

### Phase 4: 動作確認
- ゲームオーバー時にリザルトUIが1回だけ表示されることを確認する。
- ゲームクリア時に同じリザルトUIが表示されることを確認する。
- 撃破した敵数と表示値が一致することを確認する。
- 「ホームへ」押下でホームに戻り、その後「出撃」で再プレイできることを確認する。
- 既存HUDが従来どおり更新され続けることを確認する。

## リスクと対策
- リスク: `ResultUiController` 用の `UIDocument` がシーンに未配置だと、終了時にリザルトが出ない。
  - 対策: `GameBootstrapper` の参照解決時に未検出なら警告ログを出し、シーン設定漏れを即座に把握できるようにする。
- リスク: `GameOver` と `PlayerDied`、あるいは重複通知でリザルトUIが複数回更新される。
  - 対策: `GameScreenPresenter` に表示済みフラグを持たせ、終了通知は最初の1回だけ `ResultUiController` へ反映する。
- リスク: `Time.timeScale` を戻さずにホームへ遷移すると、ホーム復帰後も UI や入力が停止したままになる。
  - 対策: `SceneManager.LoadScene("Home")` の直前に必ず `Time.timeScale = 1f` を実行する。
- リスク: 撃破数を `EnemyDied` で数えると、将来プレイヤー以外の原因で倒れた敵も同じくカウントされる可能性がある。
  - 対策: 今回は `GameStatsTracker` 上で「プレイ中に倒れた敵数」を定義として `EnemyDied` をそのまま採用し、撃破原因の区別が必要になった時点でイベント拡張を検討する。
- リスク: `Home` シーン名解決に失敗すると、終了後に復帰不能になる。
  - 対策: `Application.CanStreamedLevelBeLoaded("Home")` を事前確認し、失敗時は警告ログを出して遷移しない。

## 検証方針
- 手動確認
  - `Assets/Survaivor/Main.unity` を再生し、敵を数体倒した後にゲームオーバーへ遷移して、撃破数が一致したリザルトUIが表示されること。
  - 制限時間経過でゲームクリアになっても、同様にリザルトUIが表示されること。
  - リザルトUI表示中に「ホームへ」ボタンが押せること。
  - ホーム復帰後に `Assets/Home/Home.unity` 上の UI 操作が停止していないこと。
  - ホームから再度「出撃」して、次回プレイ開始時の撃破数表示がリセットされていること。
- ログ確認
  - `ResultUiController` が未配置の場合に警告ログが出ること。
  - `Home` シーン未登録時に、遷移失敗が Unity Console で追跡できる警告またはエラーとして出ること。
- 必要に応じた回帰確認
  - 既存HUD（時間、レベル、HP、EXP、POW、DEF、SPD）が従来どおり更新されること。

## コードスニペット（主要変更イメージ）
```csharp
public class GameScreenPresenter
{
    readonly GameStatsTracker _gameStatsTracker;
    readonly ResultUiController _resultUiController;
    bool _isResultShown;

    public GameScreenPresenter(
        GameScreenUiController view,
        PlayerController playerController,
        PlayerExperience playerExperience,
        GameManager gameManager,
        GameMessageBus messageBus,
        GameStatsTracker gameStatsTracker,
        ResultUiController resultUiController)
    {
        _view = view;
        _playerController = playerController;
        _playerExperience = playerExperience;
        _gameManager = gameManager;
        _messageBus = messageBus;
        _gameStatsTracker = gameStatsTracker;
        _resultUiController = resultUiController;
    }

    void OnGameFinished()
    {
        if (_isResultShown)
        {
            return;
        }

        _isResultShown = true;
        _resultUiController?.Show("撤退", _gameStatsTracker != null ? _gameStatsTracker.KillCount : 0);
    }
}
```

```csharp
public class GameStatsTracker : IDisposable
{
    readonly GameMessageBus _messageBus;

    public int KillCount { get; private set; }

    public GameStatsTracker(GameMessageBus messageBus)
    {
        _messageBus = messageBus;
        _messageBus.EnemyDied += OnEnemyDied;
        KillCount = 0;
    }

    void OnEnemyDied(GameObject enemy)
    {
        KillCount++;
    }

    public void Dispose()
    {
        _messageBus.EnemyDied -= OnEnemyDied;
    }
}
```

```csharp
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultUiController : MonoBehaviour
{
    const string HomeSceneName = "Home";

    bool _isSceneLoading;
    Button _homeButton;

    public void Show(string title, int killCount)
    {
        // ラベル更新とオーバーレイ表示
    }

    public void Hide()
    {
        // オーバーレイ非表示
    }

    void OnHomeButtonClicked()
    {
        if (_isSceneLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(HomeSceneName))
        {
            Debug.LogWarning($"ResultUiController: シーン '{HomeSceneName}' をロードできません。");
            return;
        }

        _isSceneLoading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(HomeSceneName);
    }
}
```
