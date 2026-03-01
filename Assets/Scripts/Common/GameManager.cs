using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するコンポーネント
/// 制限時間の管理とゲームクリア処理を行います
/// </summary>
public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    /// <summary>
    /// エディタ再生時に制限時間を一時上書きするためのEditorPrefsキーです
    /// </summary>
    public const string EditorTimeLimitOverrideKey = "MekaSurvaivor.GameManager.TimeLimitOverride";

    [Header("ゲーム設定")]
    [SerializeField]
    [Tooltip("制限時間（秒）")]
    float _timeLimit = 30f;

    GameSessionService _gameSessionService;
    GameMessageBus _gameMessageBus;

    /// <summary>
    /// GameManagerのシングルトンインスタンスを取得します
    /// </summary>
    public static GameManager Instance
    {
        get => _instance;
    }

    /// <summary>
    /// 残り時間を取得します
    /// </summary>
    public float RemainingTime => _gameSessionService != null ? _gameSessionService.RemainingTime : _timeLimit;

    /// <summary>
    /// ゲームクリア状態かどうかを取得します
    /// </summary>
    public bool IsGameClear => _gameSessionService != null && _gameSessionService.IsGameClear;

    /// <summary>
    /// ゲームオーバー状態かどうかを取得します。
    /// </summary>
    public bool IsGameOver => _gameSessionService != null && _gameSessionService.IsGameOver;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"GameManagerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;

        ApplyEditorTimeLimitOverride();
        _gameSessionService = new GameSessionService(new GameSessionState(_timeLimit), HandleGameClear, HandleGameOver);
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// シーン内通知に使用するメッセージバスを設定します。
    /// </summary>
    /// <param name="gameMessageBus">設定する通知バス</param>
    public void SetMessageBus(GameMessageBus gameMessageBus)
    {
        _gameMessageBus = gameMessageBus;
    }

    /// <summary>
    /// ゲームオーバー状態へ遷移させます。
    /// </summary>
    public void MarkGameOver()
    {
        _gameSessionService?.MarkGameOver();
    }

    void Update()
    {
        _gameSessionService?.Tick(Time.deltaTime);
    }

    /// <summary>
    /// ゲームクリア処理を実行します
    /// </summary>
    void HandleGameClear()
    {
        Debug.Log("ゲームクリア");
        _gameMessageBus?.RaiseGameCleared();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ゲームオーバー通知を処理します
    /// </summary>
    void HandleGameOver()
    {
        Debug.Log("GameManager: ゲームオーバーを検知しました");
        _gameMessageBus?.RaiseGameOver();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// エディタ上の一時設定が存在する場合に制限時間へ反映します
    /// </summary>
    void ApplyEditorTimeLimitOverride()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorPrefs.HasKey(EditorTimeLimitOverrideKey))
        {
            return;
        }

        _timeLimit = UnityEditor.EditorPrefs.GetFloat(EditorTimeLimitOverrideKey, _timeLimit);
        UnityEditor.EditorPrefs.DeleteKey(EditorTimeLimitOverrideKey);
#endif
    }
}
