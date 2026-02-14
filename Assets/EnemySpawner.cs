using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エネミーのスポーン処理を管理するコンポーネント
/// プレイヤーの一定距離の場所にランダムでエネミーを出現させます
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // シングルトンインスタンス
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

    // 次のスポーンまでの残り時間
    float _spawnTimer;

    // 生成されたエネミーのリスト
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
        // シングルトンの設定
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
        // インスタンスのクリア
        if (_instance == this)
        {
            _instance = null;
        }
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
        GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);

        // リストに追加
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
        // リストから破棄されたエネミーを削除
        _enemies.RemoveAll(enemy => enemy == null);

        // エネミーが存在しない場合
        if (_enemies.Count == 0)
        {
            return null;
        }

        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        // カメラの取得（可視判定が必要な場合）
        Camera mainCamera = onlyVisible ? Camera.main : null;

        // 全てのエネミーとの距離を計算
        // TODO 全件検索のため数が多くなると重たくなる
        foreach (GameObject enemy in _enemies)
        {
            // 可視判定が有効な場合、カメラに写っているかチェック
            if (onlyVisible && mainCamera != null)
            {
                if (!IsVisibleFromCamera(enemy, mainCamera))
                {
                    continue;
                }
            }

            float distance = Vector3.Distance(position, enemy.transform.position);

            // より近いエネミーが見つかった場合
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
        // エネミーの位置をビューポート座標に変換
        Vector3 viewportPoint = camera.WorldToViewportPoint(enemy.transform.position);

        // ビューポート座標が0～1の範囲内かつ、カメラの前方にあるかチェック
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
