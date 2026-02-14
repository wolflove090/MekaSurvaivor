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

    // 次の発射までの残り時間
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
        // 初期タイマー設定
        _shootTimer = _shootInterval;
    }

    void Update()
    {
        // タイマーを減算
        _shootTimer -= Time.deltaTime;

        // 発射処理
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
        // 弾プレハブが設定されていない場合は警告
        if (_bulletPrefab == null)
        {
            Debug.LogWarning("弾プレハブが設定されていません");
            return;
        }

        // EnemySpawnerが存在しない場合は発射しない
        if (EnemySpawner.Instance == null)
        {
            return;
        }

        // 発射位置を計算
        Vector3 shootPosition = transform.position + _shootOffset;

        // 一番近いエネミーを検索（カメラに写っているもののみ）
        GameObject nearestEnemy = EnemySpawner.Instance.FindNearestEnemy(shootPosition, onlyVisible: true);

        // エネミーが存在しない場合は発射しない
        if (nearestEnemy == null)
        {
            return;
        }

        // エネミーへの方向を計算
        Vector3 direction = (nearestEnemy.transform.position - shootPosition).normalized;
        direction.y = 0f; // Y軸の移動を無効化

        // 弾を生成
        GameObject bullet = Instantiate(_bulletPrefab, shootPosition, Quaternion.identity);

        // 弾の方向を設定
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.SetDirection(direction);
        }
    }
}
