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

    [SerializeField]
    [Tooltip("破壊可能オブジェクトのタグ名")]
    string _breakableObjectTag = "BreakableObject";

    protected Vector3 _direction;
    float _lifetimeTimer;

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

    /// <summary>
    /// 有効化時に生存時間タイマーを初期化します
    /// </summary>
    protected virtual void OnEnable()
    {
        _lifetimeTimer = _lifetime;
    }

    protected virtual void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
        {
            ReturnToPoolOrDestroy();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (IsDamageTarget(other))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                Vector3 knockbackDirection = _direction;
                knockbackDirection.y = 0f;
                damageable.TakeDamage(CalculateDamage(), knockbackDirection);
            }

            ReturnToPoolOrDestroy();
        }
    }

    /// <summary>
    /// 衝突対象がダメージ適用対象かを判定します
    /// </summary>
    /// <param name="other">衝突したCollider</param>
    /// <returns>対象タグに一致する場合はtrue</returns>
    bool IsDamageTarget(Collider other)
    {
        return other.CompareTag("Enemy") || other.CompareTag(_breakableObjectTag);
    }

    /// <summary>
    /// プレイヤーの攻撃力を反映したダメージ値を計算します
    /// </summary>
    /// <returns>適用する最終ダメージ値</returns>
    int CalculateDamage()
    {
        int playerPow = PlayerController.Instance != null ? PlayerController.Instance.Pow : 1;
        return Mathf.Max(1, _damage + (playerPow - 1));
    }

    /// <summary>
    /// 弾をプールに返却、未対応時は破棄します
    /// </summary>
    protected void ReturnToPoolOrDestroy()
    {
        PooledObject pooledObject = GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}
