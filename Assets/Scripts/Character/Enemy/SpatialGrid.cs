using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// セル分割によってオブジェクトの近傍検索を効率化するグリッド。
/// </summary>
public class SpatialGrid
{
    readonly Dictionary<Vector2Int, HashSet<GameObject>> _grid = new Dictionary<Vector2Int, HashSet<GameObject>>();
    readonly Dictionary<GameObject, Vector2Int> _objectCells = new Dictionary<GameObject, Vector2Int>();
    readonly float _cellSize;

    /// <summary>
    /// グリッドのセルサイズを取得します。
    /// </summary>
    public float CellSize => _cellSize;

    /// <summary>
    /// SpatialGridを初期化します。
    /// </summary>
    /// <param name="cellSize">1セルのサイズ</param>
    public SpatialGrid(float cellSize)
    {
        _cellSize = Mathf.Max(0.1f, cellSize);
    }

    /// <summary>
    /// オブジェクトを現在位置のセルに登録します。
    /// </summary>
    public void Register(GameObject obj)
    {
        if (obj == null || _objectCells.ContainsKey(obj))
        {
            return;
        }

        Vector2Int cell = WorldToCell(obj.transform.position);
        _objectCells[obj] = cell;
        GetOrCreateCellSet(cell).Add(obj);
    }

    /// <summary>
    /// オブジェクトをグリッドから登録解除します。
    /// </summary>
    public void Unregister(GameObject obj)
    {
        if (obj == null || !_objectCells.TryGetValue(obj, out Vector2Int cell))
        {
            return;
        }

        if (_grid.TryGetValue(cell, out HashSet<GameObject> cellSet))
        {
            cellSet.Remove(obj);
            if (cellSet.Count == 0)
            {
                _grid.Remove(cell);
            }
        }

        _objectCells.Remove(obj);
    }

    /// <summary>
    /// オブジェクトの位置変更を反映します。
    /// </summary>
    public void UpdateObjectPosition(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        if (!_objectCells.TryGetValue(obj, out Vector2Int previousCell))
        {
            Register(obj);
            return;
        }

        Vector2Int currentCell = WorldToCell(obj.transform.position);
        if (currentCell == previousCell)
        {
            return;
        }

        if (_grid.TryGetValue(previousCell, out HashSet<GameObject> previousCellSet))
        {
            previousCellSet.Remove(obj);
            if (previousCellSet.Count == 0)
            {
                _grid.Remove(previousCell);
            }
        }

        _objectCells[obj] = currentCell;
        GetOrCreateCellSet(currentCell).Add(obj);
    }

    /// <summary>
    /// 指定位置・半径の範囲に含まれる近傍オブジェクトを取得します。
    /// </summary>
    public List<GameObject> GetNearbyObjects(Vector3 position, float radius)
    {
        List<GameObject> results = new List<GameObject>();
        GetNearbyObjects(position, radius, results);
        return results;
    }

    /// <summary>
    /// 指定位置・半径の範囲に含まれる近傍オブジェクトを結果リストへ格納します。
    /// </summary>
    public void GetNearbyObjects(Vector3 position, float radius, List<GameObject> results)
    {
        results.Clear();

        int searchRange = Mathf.CeilToInt(Mathf.Max(0f, radius) / _cellSize);
        Vector2Int centerCell = WorldToCell(position);

        for (int x = -searchRange; x <= searchRange; x++)
        {
            for (int y = -searchRange; y <= searchRange; y++)
            {
                Vector2Int cell = new Vector2Int(centerCell.x + x, centerCell.y + y);
                if (!_grid.TryGetValue(cell, out HashSet<GameObject> cellSet))
                {
                    continue;
                }

                foreach (GameObject obj in cellSet)
                {
                    if (obj != null)
                    {
                        results.Add(obj);
                    }
                }
            }
        }
    }

    /// <summary>
    /// ワールド座標から対応するセル座標を算出します。
    /// </summary>
    /// <param name="worldPosition">変換対象のワールド座標</param>
    /// <returns>グリッド上のセル座標</returns>
    Vector2Int WorldToCell(Vector3 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / _cellSize),
            Mathf.FloorToInt(worldPosition.z / _cellSize)
        );
    }

    /// <summary>
    /// 指定セルの集合を取得し、未作成の場合は新規作成します。
    /// </summary>
    /// <param name="cell">対象セル座標</param>
    /// <returns>セルに紐づくオブジェクト集合</returns>
    HashSet<GameObject> GetOrCreateCellSet(Vector2Int cell)
    {
        if (!_grid.TryGetValue(cell, out HashSet<GameObject> cellSet))
        {
            cellSet = new HashSet<GameObject>();
            _grid[cell] = cellSet;
        }

        return cellSet;
    }
}
