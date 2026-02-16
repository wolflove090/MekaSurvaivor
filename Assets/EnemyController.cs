using UnityEngine;

/// <summary>
/// エネミーの移動を制御するコンポーネント
/// プレイヤーに向かって徐々に近づく動作を行います
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
    /// ダメージを受けます
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（正規化済み）</param>
    public void TakeDamage(int damage, Vector3 knockbackDirection = default)
    {
        if (_healthComponent != null)
        {
            _healthComponent.TakeDamage(damage);
        }

        if (knockbackDirection != Vector3.zero && _knockbackComponent != null)
        {
            _knockbackComponent.ApplyKnockback(knockbackDirection);
        }
    }

    /// <summary>
    /// 死亡時の処理
    /// </summary>
    void OnDied()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RemoveEnemy(gameObject);
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (PlayerController.Instance != null)
        {
            if (_knockbackComponent == null || !_knockbackComponent.IsKnockedBack)
            {
                MoveTowardsPlayer();
            }
            
            UpdateSprite();
        }
    }

    /// <summary>
    /// プレイヤーに向かって移動します
    /// </summary>
    void MoveTowardsPlayer()
    {
        Vector3 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
        direction.y = 0f;
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteDirectionController == null || PlayerController.Instance == null) return;

        float directionX = PlayerController.Instance.transform.position.x - transform.position.x;
        _spriteDirectionController.UpdateDirection(directionX);
    }
}
