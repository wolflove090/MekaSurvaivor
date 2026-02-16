using UnityEngine;

/// <summary>
/// 一番近いエネミーの方向に弾を発射するコンポーネント
/// </summary>
public class BulletShooter : MonoBehaviour
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

    float _shootTimer;

    /// <summary>
    /// 発射間隔を取得または設定します
    /// </summary>
    public float ShootInterval
    {
        get => _shootInterval;
        set => _shootInterval = value;
    }

    void Start()
    {
        _shootTimer = _shootInterval;
    }

    void Update()
    {
        _shootTimer -= Time.deltaTime;

        if (_shootTimer <= 0f)
        {
            TryShoot();
            _shootTimer = _shootInterval;
        }
    }

    /// <summary>
    /// 弾の発射を試みます
    /// </summary>
    void TryShoot()
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

        GameObject bullet = Instantiate(_bulletPrefab, shootPosition, Quaternion.identity);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.SetDirection(direction);
        }
    }
}
