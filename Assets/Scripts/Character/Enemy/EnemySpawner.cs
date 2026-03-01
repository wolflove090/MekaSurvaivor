using UnityEngine;

/// <summary>
/// エネミーのスポーン処理を管理するコンポーネント
/// プレイヤーの一定距離の場所にランダムでエネミーを出現させます
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    static EnemySpawner _instance;

    [Header("スポーン設定")]
    [SerializeField]
    [Tooltip("スポーンするエネミーのプレハブ")]
    GameObject _enemyPrefab;

    [SerializeField]
    [Tooltip("プレイヤーからのスポーン距離（最小）")]
    float _spawnDistanceMin = 10f;

    [SerializeField]
    [Tooltip("プレイヤーからのスポーン距離（最大）")]
    float _spawnDistanceMax = 15f;

    [SerializeField]
    [Tooltip("スポーン間隔（秒）")]
    float _spawnInterval = 2f;

    [SerializeField]
    [Tooltip("エネミープールの初期サイズ")]
    int _initialPoolSize = 30;

    Transform _playerTransform;
    ObjectPool<EnemyController> _enemyPool;
    EnemySpawnState _enemySpawnState;
    EnemySpawnService _enemySpawnService;
    EnemyRegistry _enemyRegistry;
    GameMessageBus _gameMessageBus;

    /// <summary>
    /// スポーン間隔を取得または設定します
    /// </summary>
    public float SpawnInterval
    {
        get => _enemySpawnState != null ? _enemySpawnState.SpawnInterval : _spawnInterval;
        set
        {
            _spawnInterval = value;
            _enemySpawnService?.UpdateSpawnInterval(value);
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"EnemySpawnerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;
        _enemySpawnState = new EnemySpawnState(_spawnInterval, _spawnDistanceMin, _spawnDistanceMax);
        _enemySpawnService = new EnemySpawnService(_enemySpawnState);
        _enemyRegistry = FindFirstObjectByType<EnemyRegistry>();
        if (_enemyRegistry == null)
        {
            _enemyRegistry = gameObject.AddComponent<EnemyRegistry>();
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Start()
    {
        InitializePool();
        if (_playerTransform == null)
        {
            Debug.LogWarning("EnemySpawner: スポーンターゲットが未設定です。GameBootstrapperまたはSetSpawnTargetで設定してください。");
        }
    }

    /// <summary>
    /// スポーンの基準となるターゲットを設定します
    /// </summary>
    /// <param name="target">基準となるTransform（通常はプレイヤー）</param>
    public void SetSpawnTarget(Transform target)
    {
        _playerTransform = target;
    }

    /// <summary>
    /// シーン内通知に使用するメッセージバスを設定します。
    /// </summary>
    /// <param name="gameMessageBus">設定する通知バス</param>
    public void SetMessageBus(GameMessageBus gameMessageBus)
    {
        _gameMessageBus = gameMessageBus;
    }

    void Update()
    {
        if (_playerTransform == null || _enemySpawnService == null)
        {
            return;
        }

        if (!_enemySpawnService.TryCreateSpawnRequest(Time.deltaTime, _playerTransform.position, Random.Range, out Vector3 spawnPosition))
        {
            return;
        }

        TrySpawnEnemy(spawnPosition);
    }

    /// <summary>
    /// エネミーのスポーンを試みます
    /// </summary>
    /// <param name="spawnPosition">スポーン位置</param>
    void TrySpawnEnemy(Vector3 spawnPosition)
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("エネミープレハブが設定されていません");
            return;
        }
        GameObject enemy = null;
        EnemyController enemyController = null;

        if (_enemyPool != null)
        {
            enemyController = _enemyPool.Get();
            enemy = enemyController.gameObject;
            enemy.transform.position = spawnPosition;
            enemy.transform.rotation = Quaternion.identity;
        }
        else
        {
            enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            enemyController = enemy.GetComponent<EnemyController>();
        }

        _enemyRegistry?.RegisterEnemy(enemy);

        // エネミーにターゲットを設定
        if (enemyController != null && _playerTransform != null)
        {
            enemyController.SetTarget(_playerTransform);
        }

        enemyController?.SetRegistry(_enemyRegistry);
        enemyController?.SetMessageBus(_gameMessageBus);
    }

    /// <summary>
    /// エネミープールを初期化します
    /// </summary>
    void InitializePool()
    {
        if (_enemyPrefab == null)
        {
            return;
        }

        EnemyController prefabController = _enemyPrefab.GetComponent<EnemyController>();
        if (prefabController == null)
        {
            Debug.LogWarning("エネミープレハブにEnemyControllerがアタッチされていません");
            return;
        }

        _enemyPool = new ObjectPool<EnemyController>(prefabController, _initialPoolSize, transform);
    }
}
