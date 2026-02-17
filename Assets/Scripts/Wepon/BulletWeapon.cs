using UnityEngine;

/// <summary>
/// 一番近いエネミーの方向に弾を発射する武器コンポーネント
/// </summary>
public class BulletWeapon : WeaponBase
{
    [Header("発射設定")]
    [SerializeField]
    [Tooltip("弾のプレハブ")]
    GameObject _bulletPrefab;

    [SerializeField]
    [Tooltip("発射間隔（秒）")]
    float _shootInterval = 1f;

    [SerializeField]
    [Tooltip("弾の発射位置のオフセット")]
    Vector3 _shootOffset = Vector3.zero;

    [SerializeField]
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

    protected override void Start()
    {
        base.Start();
        InitializePool();
    }

    /// <summary>
    /// 弾の発射を試みます
    /// </summary>
    protected override void Fire()
    {
        if (_bulletPrefab == null)
        {
            Debug.LogWarning("弾プレハブが設定されていません");
            return;
        }

        if (EnemySpawner.Instance == null)
        {
            return;
        }

        Vector3 shootPosition = transform.position + _shootOffset;
        GameObject nearestEnemy = EnemySpawner.Instance.FindNearestEnemy(shootPosition, onlyVisible: true);

        if (nearestEnemy == null)
        {
            return;
        }

        Vector3 direction = (nearestEnemy.transform.position - shootPosition).normalized;
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
            GameObject bullet = Instantiate(_bulletPrefab, shootPosition, Quaternion.identity);
            bulletController = bullet.GetComponent<BulletController>();
        }

        if (bulletController != null)
        {
            bulletController.SetDirection(direction);
        }
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

        _bulletPool = new ObjectPool<BulletController>(bulletPrefabController, _initialPoolSize, transform);
    }
}
