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

    float _spawnTimer;
    List<GameObject> _enemies = new List<GameObject>();

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
    }

    void Update()
    {
        if (PlayerController.Instance == null) return;

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
        GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        _enemies.Add(enemy);
    }

    /// <summary>
    /// 指定された位置から一番近いエネミーを検索します
    /// </summary>
    /// <param name="position">基準位置</param>
    /// <param name="onlyVisible">カメラに写っているエネミーのみを対象にするか</param>
    /// <returns>一番近いエネミーのGameObject、存在しない場合はnull</returns>
    public GameObject FindNearestEnemy(Vector3 position, bool onlyVisible = false)
    {
        _enemies.RemoveAll(enemy => enemy == null);

        if (_enemies.Count == 0)
        {
            return null;
        }

        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        Camera mainCamera = onlyVisible ? Camera.main : null;

        // 全てのエネミーとの距離を計算（TODO: 空間分割による最適化が必要）
        foreach (GameObject enemy in _enemies)
        {
            if (onlyVisible && mainCamera != null)
            {
                if (!IsVisibleFromCamera(enemy, mainCamera))
                {
                    continue;
                }
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
    }

    /// <summary>
    /// プレイヤーの周囲のランダムな位置を計算します
    /// </summary>
    /// <returns>スポーン位置</returns>
    Vector3 CalculateSpawnPosition()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(_spawnDistanceMin, _spawnDistanceMax);
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(
            playerPosition.x + x,
            playerPosition.y,
            playerPosition.z + z
        );
    }
}
