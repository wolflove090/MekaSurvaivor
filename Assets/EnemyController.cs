using UnityEngine;

/// <summary>
/// エネミーの移動を制御するコンポーネント
/// ターゲットに向かって徐々に近づく動作を行います
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    [Tooltip("移動速度")]
    float _moveSpeed = 3f;

    HealthComponent _healthComponent;
    KnockbackComponent _knockbackComponent;
    SpriteDirectionController _spriteDirectionController;
    Transform _targetTransform;

    /// <summary>
    /// 移動速度を取得または設定します
    /// </summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _healthComponent != null ? _healthComponent.CurrentHp : 0;

    /// <summary>
    /// 追跡するターゲットを設定します
    /// </summary>
    /// <param name="target">追跡対象のTransform</param>
    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }

    void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        _knockbackComponent = GetComponent<KnockbackComponent>();
        _spriteDirectionController = GetComponent<SpriteDirectionController>();

        if (_healthComponent != null)
        {
            _healthComponent.OnDied += OnDied;
        }
    }

    /// <summary>
    /// 再利用時に状態を初期化します
    /// </summary>
    void OnEnable()
    {
        if (_healthComponent != null)
        {
            _healthComponent.ResetToMaxHp();
        }

        if (_knockbackComponent != null)
        {
            _knockbackComponent.ResetState();
        }
    }

    /// <summary>
    /// ダメージを受けます
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（正規化済み）</param>
    public void TakeDamage(int damage, Vector3 knockbackDirection = default)
    {
        if (_healthComponent != null)
        {
            _healthComponent.TakeDamage(damage, knockbackDirection);
        }
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    void OnDied()
    {
        GameEvents.RaiseEnemyDied(gameObject);

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RemoveEnemy(gameObject);
        }

        PooledObject pooledObject = GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (_targetTransform != null)
        {
            if (_knockbackComponent == null || !_knockbackComponent.IsKnockedBack)
            {
                MoveTowardsTarget();
            }
            
            UpdateSprite();
        }
    }

    /// <summary>
    /// ターゲットに向かって移動します
    /// </summary>
    void MoveTowardsTarget()
    {
        if (_targetTransform == null) return;

        Vector3 direction = (_targetTransform.position - transform.position).normalized;
        direction.y = 0f;
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteDirectionController == null || _targetTransform == null) return;

        float directionX = _targetTransform.position.x - transform.position.x;
        _spriteDirectionController.UpdateDirection(directionX);
    }
}
