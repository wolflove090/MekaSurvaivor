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

    /// <summary>
    /// EnemySpawnerのシングルトンインスタンスを取得します
    /// </summary>
    public static EnemySpawner Instance
    {
        get => _instance;
    }

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

        // スポーンイベントを発火
        GameEvents.RaiseEnemySpawned(enemy);
    }

    /// <summary>
    /// 指定された位置から一番近いエネミーを検索します
    /// </summary>
    /// <param name="position">基準位置</param>
    /// <param name="onlyVisible">カメラに写っているエネミーのみを対象にするか</param>
    /// <returns>一番近いエネミーのGameObject、存在しない場合はnull</returns>
    public GameObject FindNearestEnemy(Vector3 position, bool onlyVisible = false)
    {
        return _enemyRegistry != null ? _enemyRegistry.FindNearestEnemy(position, onlyVisible) : null;
    }

    /// <summary>
    /// エネミーをリストから削除します
    /// </summary>
    /// <param name="enemy">削除するエネミー</param>
    public void RemoveEnemy(GameObject enemy)
    {
        _enemyRegistry?.UnregisterEnemy(enemy);
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
