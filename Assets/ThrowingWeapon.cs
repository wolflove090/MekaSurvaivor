using UnityEngine;

/// <summary>
/// プレイヤーの向いている方向に投擲弾を発射する武器コンポーネント
/// </summary>
public class ThrowingWeapon : WeaponBase
{
    [SerializeField]
    [Tooltip("投擲弾のプレハブ")]
    GameObject _throwingBulletPrefab;

    [SerializeField]
    [Tooltip("発射間隔（秒）")]
    float _shootInterval = 2f;

    [SerializeField]
    [Tooltip("弾の発射位置のオフセット")]
    Vector3 _shootOffset = Vector3.zero;

    [SerializeField]
    [Tooltip("投擲弾プールの初期サイズ")]
    int _initialPoolSize = 12;

    PlayerController _playerController;
    ObjectPool<ThrowingBulletController> _throwingBulletPool;

    protected override float CooldownDuration => _shootInterval;

    protected override void Start()
    {
        base.Start();
        _playerController = GetComponent<PlayerController>();
        InitializePool();
    }

    /// <summary>
    /// プレイヤーの向いている方向に投擲弾を発射します
    /// </summary>
    protected override void Fire()
    {
        if (_throwingBulletPrefab == null || _playerController == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + _shootOffset;
        ThrowingBulletController bulletController = null;

        if (_throwingBulletPool != null)
        {
            bulletController = _throwingBulletPool.Get();
            bulletController.transform.position = spawnPosition;
            bulletController.transform.rotation = Quaternion.identity;
        }
        else
        {
            GameObject bullet = Instantiate(_throwingBulletPrefab, spawnPosition, Quaternion.identity);
            bulletController = bullet.GetComponent<ThrowingBulletController>();
        }

        bulletController?.SetDirection(_playerController.GetFacingDirection());
    }

    /// <summary>
    /// 投擲弾プールを初期化します
    /// </summary>
    void InitializePool()
    {
        if (_throwingBulletPrefab == null)
        {
            return;
        }

        ThrowingBulletController prefabController = _throwingBulletPrefab.GetComponent<ThrowingBulletController>();
        if (prefabController == null)
        {
            Debug.LogWarning("投擲弾プレハブにThrowingBulletControllerがアタッチされていません");
            return;
        }

        _throwingBulletPool = new ObjectPool<ThrowingBulletController>(prefabController, _initialPoolSize, transform);
    }
}
