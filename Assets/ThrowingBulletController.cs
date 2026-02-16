using UnityEngine;

/// <summary>
/// 投擲弾の移動と衝突処理を制御するコンポーネント
/// </summary>
public class ThrowingBulletController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("弾の移動速度")]
    float _speed = 10f;

    [SerializeField]
    [Tooltip("弾の生存時間（秒）")]
    float _lifetime = 3f;

    Vector3 _direction;

    /// <summary>
    /// 弾の移動方向を設定します
    /// </summary>
    /// <param name="direction">移動方向ベクトル</param>
    public void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
        _direction.y = 0f;
    }

    void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // ノックバック方向を計算（弾の進行方向）
                Vector3 knockbackDirection = _direction;
                knockbackDirection.y = 0f; // Y軸方向を無効化
                
                enemy.TakeDamage(1, knockbackDirection);
            }
            
            Destroy(gameObject);
        }
    }
}
