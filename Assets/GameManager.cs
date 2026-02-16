using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するコンポーネント
/// 制限時間の管理とゲームクリア処理を行います
/// </summary>
public class GameManager : MonoBehaviour
{
    static GameManager _instance;

    [Header("ゲーム設定")]
    [SerializeField]
    [Tooltip("制限時間（秒）")]
    float _timeLimit = 30f;

    float _elapsedTime;
    bool _isGameClear;

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

        _elapsedTime = 0f;
        _isGameClear = false;
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Update()
    {
        if (_isGameClear || (PlayerController.Instance != null && PlayerController.Instance.IsGameOver))
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
}
