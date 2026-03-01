using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の登録管理と近傍探索を担当するコンポーネントです。
/// </summary>
public class EnemyRegistry : MonoBehaviour
{
    static EnemyRegistry _instance;

    [SerializeField]
    [Tooltip("空間分割グリッドのセルサイズ")]
    float _gridCellSize = 5f;

    [SerializeField]
    [Tooltip("近傍探索の最大ステップ数")]
    int _maxGridSearchSteps = 8;

    readonly List<GameObject> _enemies = new List<GameObject>();
    readonly List<GameObject> _nearbyCandidatesBuffer = new List<GameObject>();
    SpatialGrid _spatialGrid;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"EnemyRegistryが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
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

    void LateUpdate()
    {
        CleanupNullEnemies();
        RefreshEnemyGridPositions();
    }

    /// <summary>
    /// 敵を登録します。
    /// </summary>
    /// <param name="enemy">登録する敵</param>
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null || _enemies.Contains(enemy))
        {
            return;
        }

        _enemies.Add(enemy);
        _spatialGrid.Register(enemy);
    }

    /// <summary>
    /// 敵を登録解除します。
    /// </summary>
    /// <param name="enemy">登録解除する敵</param>
    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        _enemies.Remove(enemy);
        _spatialGrid.Unregister(enemy);
    }

    /// <summary>
    /// 指定位置から最も近い敵を検索します。
    /// </summary>
    /// <param name="position">基準位置</param>
    /// <param name="onlyVisible">画面内の敵だけを対象にするか</param>
    /// <returns>見つかった敵。存在しない場合はnull</returns>
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

        return FindNearestFromCandidates(_enemies, position, onlyVisible, mainCamera);
    }

    /// <summary>
    /// エネミーがカメラに写っているかを判定します。
    /// </summary>
    /// <param name="enemy">判定対象</param>
    /// <param name="camera">判定に使うカメラ</param>
    /// <returns>画面内にいる場合はtrue</returns>
    bool IsVisibleFromCamera(GameObject enemy, Camera camera)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(enemy.transform.position);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
               viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
               viewportPoint.z > 0f;
    }

    /// <summary>
    /// 破棄済みの参照を管理対象から除外します。
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
    /// 登録済み敵の位置を空間グリッドへ反映します。
    /// </summary>
    void RefreshEnemyGridPositions()
    {
        foreach (GameObject enemy in _enemies)
        {
            _spatialGrid.UpdateObjectPosition(enemy);
        }
    }

    /// <summary>
    /// 候補群から最短距離の敵を返します。
    /// </summary>
    /// <param name="candidates">候補一覧</param>
    /// <param name="position">基準位置</param>
    /// <param name="onlyVisible">可視敵のみ対象にするか</param>
    /// <param name="mainCamera">可視判定に使うカメラ</param>
    /// <returns>最短距離の敵。見つからない場合はnull</returns>
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
}
