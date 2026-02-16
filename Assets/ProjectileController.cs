using UnityEngine;

/// <summary>
/// 弾の共通処理を提供する基底クラス
/// </summary>
public abstract class ProjectileController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("弾の移動速度")]
    float _speed = 10f;

    [SerializeField]
    [Tooltip("弾の生存時間（秒）")]
    float _lifetime = 5f;

    [SerializeField]
    [Tooltip("与えるダメージ量")]
    int _damage = 1;

    protected Vector3 _direction;

    /// <summary>
    /// 弾の移動速度を取得または設定します
    /// </summary>
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    /// <summary>
    /// 与えるダメージ量を取得します
    /// </summary>
    public int Damage => _damage;

    /// <summary>
    /// 弾の移動方向を設定します
    /// </summary>
    /// <param name="direction">移動方向ベクトル</param>
    public virtual void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    protected virtual void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    protected virtual void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                OnHitEnemy(enemy);
            }
            
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 敵に命中した時の処理
    /// </summary>
    /// <param name="enemy">命中した敵</param>
    protected abstract void OnHitEnemy(EnemyController enemy);
}
