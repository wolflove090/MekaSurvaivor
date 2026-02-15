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

    float _shootTimer;
    PlayerController _playerController;

    void Start()
    {
        _shootTimer = _shootInterval;
        _playerController = GetComponent<PlayerController>();
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
    /// 投擲弾を発射します
    /// </summary>
    void Shoot()
    {
        if (_throwingBulletPrefab == null || _playerController == null) return;

        GameObject bullet = Instantiate(_throwingBulletPrefab, transform.position + _shootOffset, Quaternion.identity);
        bullet.GetComponent<ThrowingBulletController>()?.SetDirection(_playerController.GetFacingDirection());
    }
}
