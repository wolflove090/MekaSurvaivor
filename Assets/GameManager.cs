using UnityEngine;

/// <summary>
/// ゲーム全体の進行を管理するコンポーネント
/// 制限時間の管理とゲームクリア処理を行います
/// </summary>
public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    static GameManager _instance;

    [Header("ゲーム設定")]
    [SerializeField]
    [Tooltip("制限時間（秒）")]
    float _timeLimit = 30f;

    // 経過時間
    float _elapsedTime;

    // ゲームクリアフラグ
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
        // シングルトンの設定
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"GameManagerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;

        // 初期化
        _elapsedTime = 0f;
        _isGameClear = false;
    }

    void OnDestroy()
    {
        // インスタンスのクリア
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Update()
    {
        // ゲームクリア済みまたはゲームオーバーの場合は処理しない
        if (_isGameClear || (PlayerController.Instance != null && PlayerController.Instance.IsGameOver))
        {
            return;
        }

        // 経過時間を加算
        _elapsedTime += Time.deltaTime;

        // 制限時間に到達した場合
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

        // 画面を一時停止
        Time.timeScale = 0f;
    }
}
