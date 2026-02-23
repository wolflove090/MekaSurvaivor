using UnityEngine;

/// <summary>
/// 一番近いエネミーの方向に弾を発射する武器コンポーネント
/// </summary>
public class BulletWeapon : WeaponBase
{
    GameObject _bulletPrefab;

    [Tooltip("発射間隔（秒）")]
    float _shootInterval = 1f;

    [Tooltip("強化1段階ごとの発射間隔短縮率")]
    float _intervalReductionPerLevel = 0.15f;

    [Tooltip("発射間隔の最小値（秒）")]
    float _minShootInterval = 0.2f;

    [Tooltip("弾の発射位置のオフセット")]
    Vector3 _shootOffset = Vector3.zero;

    [Tooltip("弾プールの初期サイズ")]
    int _initialPoolSize = 20;

    ObjectPool<BulletController> _bulletPool;

    /// <summary>
    /// 発射間隔を取得または設定します
    /// </summary>
    public float ShootInterval
    {
        get => _shootInterval;
        set => _shootInterval = value;
    }

    protected override float CooldownDuration => _shootInterval;

    public BulletWeapon(Transform transform , WeaponBase rideWeapon) : base(transform, rideWeapon)
    {
        BulletFactory bulletFactory = _transform.GetComponent<BulletFactory>();
        if (bulletFactory == null)
        {
            Debug.LogWarning("BulletFactoryが見つかりません");
            return;
        }

        _bulletPrefab = bulletFactory.BulletPrefab;
        if (_bulletPrefab == null)
        {
            Debug.LogWarning("BulletFactoryのBulletPrefabが設定されていません");
            return;
        }

        InitializePool();
    }

    /// <summary>
    /// 弾の発射を試みます
    /// </summary>
    protected override void Fire()
    {
        if (_bulletPrefab == null)
        {
            Debug.LogWarning("_bulletPrefabがありません");
            return;
        }

        Vector3 shootPosition = _transform.position + _shootOffset;
        GameObject target = null;

        if (EnemySpawner.Instance != null)
        {
            target = EnemySpawner.Instance.FindNearestEnemy(shootPosition, onlyVisible: true);
        }

        if (target == null && BreakableObjectSpawner.Instance != null)
        {
            target = BreakableObjectSpawner.Instance.FindNearestBreakableObject(shootPosition, onlyVisible: true);
        }

        if (target == null)
        {
            return;
        }

        Vector3 direction = (target.transform.position - shootPosition).normalized;
        direction.y = 0f;

        BulletController bulletController = null;
        if (_bulletPool != null)
        {
            bulletController = _bulletPool.Get();
            bulletController.transform.position = shootPosition;
            bulletController.transform.rotation = Quaternion.identity;
        }
        else
        {
            GameObject bullet = GameObject.Instantiate(_bulletPrefab, shootPosition, Quaternion.identity);
            bulletController = bullet.GetComponent<BulletController>();
        }

        if (bulletController != null)
        {
            bulletController.SetDirection(direction);
        }
    }

    /// <summary>
    /// 武器を1段階強化し、発射間隔を短縮します
    /// </summary>
    public override void LevelUp()
    {
        UpgradeLevel++;

        float reducedInterval = _shootInterval * (1f - _intervalReductionPerLevel);
        _shootInterval = Mathf.Max(_minShootInterval, reducedInterval);
        ClampCooldownTimerToDuration();

        Debug.Log($"BulletWeapon: レベル {UpgradeLevel} に強化。発射間隔: {_shootInterval:0.00}s");
    }

    /// <summary>
    /// 弾プールを初期化します
    /// </summary>
    void InitializePool()
    {
        if (_bulletPrefab == null)
        {
            return;
        }

        BulletController bulletPrefabController = _bulletPrefab.GetComponent<BulletController>();
        if (bulletPrefabController == null)
        {
            Debug.LogWarning("弾プレハブにBulletControllerがアタッチされていません");
            return;
        }

        _bulletPool = new ObjectPool<BulletController>(bulletPrefabController, _initialPoolSize, _transform);
    }
}
