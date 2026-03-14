using UnityEngine;

/// <summary>
/// 各武器で使用するプレハブを提供するファクトリーコンポーネント
/// </summary>
public class BulletFactory : MonoBehaviour
{
    [SerializeField]
    [Tooltip("弾のプレハブ")]
    GameObject _bulletPrefab;

    [SerializeField]
    [Tooltip("投擲弾のプレハブ")]
    GameObject _throwingBulletPrefab;

    [SerializeField]
    [Tooltip("ダメージフィールドのプレハブ")]
    GameObject _damageFieldPrefab;

    [SerializeField]
    [Tooltip("ドローンのプレハブ")]
    GameObject _dronePrefab;

    [SerializeField]
    [Tooltip("バウンドボールのプレハブ")]
    GameObject _boundBallPrefab;

    [SerializeField]
    [Tooltip("火炎瓶のプレハブ")]
    GameObject _flameBottlePrefab;

    [SerializeField]
    [Tooltip("炎エリアのプレハブ")]
    GameObject _flameAreaPrefab;

    EnemyRegistry _enemyRegistry;
    BreakableObjectSpawner _breakableObjectSpawner;

    /// <summary>
    /// 弾のプレハブを取得します
    /// </summary>
    public GameObject BulletPrefab => _bulletPrefab;

    /// <summary>
    /// 投擲弾のプレハブを取得します
    /// </summary>
    public GameObject ThrowingBulletPrefab => _throwingBulletPrefab;

    /// <summary>
    /// ダメージフィールドのプレハブを取得します
    /// </summary>
    public GameObject DamageFieldPrefab => _damageFieldPrefab;

    /// <summary>
    /// ドローンのプレハブを取得します。
    /// </summary>
    public GameObject DronePrefab => _dronePrefab;

    /// <summary>
    /// バウンドボールのプレハブを取得します。
    /// </summary>
    public GameObject BoundBallPrefab => _boundBallPrefab;

    /// <summary>
    /// 火炎瓶のプレハブを取得します。
    /// </summary>
    public GameObject FlameBottlePrefab => _flameBottlePrefab;

    /// <summary>
    /// 炎エリアのプレハブを取得します。
    /// </summary>
    public GameObject FlameAreaPrefab => _flameAreaPrefab;

    /// <summary>
    /// 武器の探索先となる敵レジストリを取得します。
    /// </summary>
    public EnemyRegistry EnemyRegistry
    {
        get
        {
            if (_enemyRegistry == null)
            {
                _enemyRegistry = FindFirstObjectByType<EnemyRegistry>();
            }

            return _enemyRegistry;
        }
    }

    /// <summary>
    /// 武器の探索先となる破壊可能オブジェクトスポナーを取得します。
    /// </summary>
    public BreakableObjectSpawner BreakableObjectSpawner
    {
        get
        {
            if (_breakableObjectSpawner == null)
            {
                _breakableObjectSpawner = FindFirstObjectByType<BreakableObjectSpawner>();
            }

            return _breakableObjectSpawner;
        }
    }

    /// <summary>
    /// 武器の探索先となる敵レジストリを設定します。
    /// </summary>
    /// <param name="enemyRegistry">設定する敵レジストリ</param>
    public void SetEnemyRegistry(EnemyRegistry enemyRegistry)
    {
        _enemyRegistry = enemyRegistry;
    }

    /// <summary>
    /// 武器の探索先となる破壊可能オブジェクトスポナーを設定します。
    /// </summary>
    /// <param name="breakableObjectSpawner">設定するスポナー</param>
    public void SetBreakableObjectSpawner(BreakableObjectSpawner breakableObjectSpawner)
    {
        _breakableObjectSpawner = breakableObjectSpawner;
    }
}
