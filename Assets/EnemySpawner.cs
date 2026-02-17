using System.Collections.Generic;
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

    [Header("検索最適化設定")]
    [SerializeField]
    [Tooltip("空間分割グリッドのセルサイズ")]
    float _gridCellSize = 5f;

    [SerializeField]
    [Tooltip("近傍探索の最大ステップ数")]
    int _maxGridSearchSteps = 8;

    [SerializeField]
    [Tooltip("エネミープールの初期サイズ")]
    int _initialPoolSize = 30;

    float _spawnTimer;
    List<GameObject> _enemies = new List<GameObject>();
    SpatialGrid _spatialGrid;
    List<GameObject> _nearbyCandidatesBuffer = new List<GameObject>();
    Transform _playerTransform;
    ObjectPool<EnemyController> _enemyPool;

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
        get => _spawnInterval;
        set => _spawnInterval = value;
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
        _spatialGrid = new SpatialGrid(_gridCellSize);
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
        _spawnTimer = _spawnInterval;
        InitializePool();

        // プレイヤーの参照を取得（後方互換性のため）
        if (PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
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
        CleanupNullEnemies();
        RefreshEnemyGridPositions();

        if (_playerTransform == null) return;

        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            TrySpawnEnemy();
            _spawnTimer = _spawnInterval;
        }
    }

    /// <summary>
    /// エネミーのスポーンを試みます
    /// </summary>
    void TrySpawnEnemy()
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("エネミープレハブが設定されていません");
            return;
        }

        Vector3 spawnPosition = CalculateSpawnPosition();
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

        _enemies.Add(enemy);
        _spatialGrid.Register(enemy);

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
        CleanupNullEnemies();

        if (_enemies.Count == 0)
        {
            return null;
        }

        Camera mainCamera = onlyVisible ? Camera.main : null;

        int maxSteps = Mathf.Max(1, _maxGridSearchSteps);
        for (int step = 1; step <= maxSteps; step++)
        {
            float radius = step * _spatialGrid.CellSize;
            _spatialGrid.GetNearbyObjects(position, radius, _nearbyCandidatesBuffer);
            GameObject nearestInRange = FindNearestFromCandidates(_nearbyCandidatesBuffer, position, onlyVisible, mainCamera);
            if (nearestInRange != null)
            {
                return nearestInRange;
            }

            if (_nearbyCandidatesBuffer.Count >= _enemies.Count)
            {
                return null;
            }
        }

        // 念のためのフォールバック（探索上限を超える距離に敵がいる場合）
        return FindNearestFromCandidates(_enemies, position, onlyVisible, mainCamera);
    }

    /// <summary>
    /// エネミーがカメラに写っているか判定します
    /// </summary>
    /// <param name="enemy">判定対象のエネミー</param>
    /// <param name="camera">判定に使用するカメラ</param>
    /// <returns>カメラに写っている場合はtrue</returns>
    bool IsVisibleFromCamera(GameObject enemy, Camera camera)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(enemy.transform.position);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
               viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
               viewportPoint.z > 0f;
    }

    /// <summary>
    /// エネミーをリストから削除します
    /// </summary>
    /// <param name="enemy">削除するエネミー</param>
    public void RemoveEnemy(GameObject enemy)
    {
        _enemies.Remove(enemy);
        _spatialGrid.Unregister(enemy);
    }

    /// <summary>
    /// 破棄済みのエネミー参照を管理リストから除外します。
    /// </summary>
    void CleanupNullEnemies()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = _enemies[i];
            if (enemy == null)
            {
                _enemies.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 追跡中エネミーの現在位置を空間グリッドへ反映します。
    /// </summary>
    void RefreshEnemyGridPositions()
    {
        foreach (GameObject enemy in _enemies)
        {
            _spatialGrid.UpdateObjectPosition(enemy);
        }
    }

    /// <summary>
    /// 候補群から最短距離のエネミーを検索します。
    /// </summary>
    /// <param name="candidates">探索対象の候補リスト</param>
    /// <param name="position">距離計算の基準位置</param>
    /// <param name="onlyVisible">可視エネミーのみを対象にするか</param>
    /// <param name="mainCamera">可視判定に使用するカメラ</param>
    /// <returns>最短距離のエネミー。候補が無効な場合はnull</returns>
    GameObject FindNearestFromCandidates(List<GameObject> candidates, Vector3 position, bool onlyVisible, Camera mainCamera)
    {
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject enemy in candidates)
        {
            if (enemy == null)
            {
                continue;
            }

            if (onlyVisible && mainCamera != null && !IsVisibleFromCamera(enemy, mainCamera))
            {
                continue;
            }

            float distance = Vector3.Distance(position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    /// <summary>
    /// ターゲットの周囲のランダムな位置を計算します
    /// </summary>
    /// <returns>スポーン位置</returns>
    Vector3 CalculateSpawnPosition()
    {
        if (_playerTransform == null)
        {
            return Vector3.zero;
        }

        Vector3 targetPosition = _playerTransform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(_spawnDistanceMin, _spawnDistanceMax);
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(
            targetPosition.x + x,
            targetPosition.y,
            targetPosition.z + z
        );
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
