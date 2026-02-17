using UnityEngine;

/// <summary>
/// プレイヤーの向いている方向に投擲弾を発射するコンポーネント
/// </summary>
public class ThrowingBulletShooter : MonoBehaviour
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

    float _shootTimer;
    PlayerController _playerController;
    ObjectPool<ThrowingBulletController> _throwingBulletPool;

    void Start()
    {
        _shootTimer = _shootInterval;
        _playerController = GetComponent<PlayerController>();
        InitializePool();
    }

    void Update()
    {
        _shootTimer -= Time.deltaTime;
        if (_shootTimer <= 0f)
        {
            Shoot();
            _shootTimer = _shootInterval;
        }
    }

    /// <summary>
    /// プレイヤーの向いている方向に投擲弾を発射します
    /// </summary>
    void Shoot()
    {
        if (_throwingBulletPrefab == null || _playerController == null) return;

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
