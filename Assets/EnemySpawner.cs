using UnityEngine;

/// <summary>
/// エネミーのスポーン処理を管理するコンポーネント
/// プレイヤーの一定距離の場所にランダムでエネミーを出現させます
/// </summary>
public class EnemySpawner : MonoBehaviour
{
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

    // 次のスポーンまでの残り時間
    float _spawnTimer;

    /// <summary>
    /// スポーン間隔を取得または設定します
    /// </summary>
    public float SpawnInterval
    {
        get => _spawnInterval;
        set => _spawnInterval = value;
    }

    void Start()
    {
        // 初期タイマー設定
        _spawnTimer = _spawnInterval;
    }

    void Update()
    {
        // PlayerControllerが存在しない場合は処理しない
        if (PlayerController.Instance == null) return;

        // タイマーを減算
        _spawnTimer -= Time.deltaTime;

        // スポーン処理
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
        // エネミープレハブが設定されていない場合は警告
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("エネミープレハブが設定されていません");
            return;
        }

        // スポーン位置を計算
        Vector3 spawnPosition = CalculateSpawnPosition();

        // エネミーを生成
        Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// プレイヤーの周囲のランダムな位置を計算します
    /// </summary>
    /// <returns>スポーン位置</returns>
    Vector3 CalculateSpawnPosition()
    {
        // プレイヤーの位置を取得
        Vector3 playerPosition = PlayerController.Instance.transform.position;

        // ランダムな角度を生成（0～360度）
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // ランダムな距離を生成
        float distance = Random.Range(_spawnDistanceMin, _spawnDistanceMax);

        // 極座標から直交座標に変換
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        // スポーン位置を計算（Y座標はプレイヤーと同じ）
        Vector3 spawnPosition = new Vector3(
            playerPosition.x + x,
            playerPosition.y,
            playerPosition.z + z
        );

        return spawnPosition;
    }
}
