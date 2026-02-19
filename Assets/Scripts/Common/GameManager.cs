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

    float _elapsedTime;
    bool _isGameClear;
    bool _isGameOver;

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
    public float RemainingTime => Mathf.Max(0f, _timeLimit - _elapsedTime);

    /// <summary>
    /// ゲームクリア状態かどうかを取得します
    /// </summary>
    public bool IsGameClear => _isGameClear;

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

        _elapsedTime = 0f;
        _isGameClear = false;

        // イベントを購読
        GameEvents.OnGameOver += OnGameOver;
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        // イベントの購読を解除
        GameEvents.OnGameOver -= OnGameOver;
    }

    /// <summary>
    /// ゲームオーバー時のコールバック
    /// </summary>
    void OnGameOver()
    {
        _isGameOver = true;
        Debug.Log("GameManager: ゲームオーバーを検知しました");
    }

    void Update()
    {
        if (_isGameClear || _isGameOver)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _timeLimit)
        {
            GameClear();
        }
    }

    /// <summary>
    /// ゲームクリア処理を実行します
    /// </summary>
    void GameClear()
    {
        _isGameClear = true;
        Debug.Log("ゲームクリア");
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
