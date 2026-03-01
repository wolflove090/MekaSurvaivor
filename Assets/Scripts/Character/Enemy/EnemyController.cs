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
    CharacterStats _characterStats;
    Transform _targetTransform;
    IEnemyBehavior _enemyBehavior;

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
    /// 接触時の攻撃力を取得します
    /// </summary>
    public int Pow => _characterStats != null ? _characterStats.Pow : 1;

    /// <summary>
    /// 現在の追跡ターゲットを取得します
    /// </summary>
    public Transform TargetTransform => _targetTransform;

    /// <summary>
    /// 追跡するターゲットを設定します
    /// </summary>
    /// <param name="target">追跡対象のTransform</param>
    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }

    /// <summary>
    /// エネミーの行動ロジックを設定します
    /// </summary>
    /// <param name="behavior">設定する行動ロジック</param>
    public void SetBehavior(IEnemyBehavior behavior)
    {
        if (behavior != null)
        {
            _enemyBehavior = behavior;
        }
    }

    /// <summary>
    /// 指定方向へ移動します
    /// </summary>
    /// <param name="direction">移動方向（正規化済み）</param>
    public void MoveInDirection(Vector3 direction)
    {
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    void Awake()
    {
        _healthComponent = GetComponent<HealthComponent>();
        _knockbackComponent = GetComponent<KnockbackComponent>();
        _spriteDirectionController = GetComponent<SpriteDirectionController>();
        _characterStats = GetComponent<CharacterStats>();
        if (_characterStats == null)
        {
            _characterStats = gameObject.AddComponent<CharacterStats>();
        }
        _enemyBehavior = new ChasePlayerBehavior();

        if (_healthComponent != null)
        {
            _healthComponent.OnDied += OnDied;
        }

        ApplyMoveSpeedFromStats();
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

        ApplyMoveSpeedFromStats();
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

        EnemyRegistry.Instance?.UnregisterEnemy(gameObject);

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
                _enemyBehavior?.Execute(this);
            }

            UpdateSprite();
        }
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

    /// <summary>
    /// ステータスから移動速度を反映します
    /// </summary>
    void ApplyMoveSpeedFromStats()
    {
        if (_characterStats != null)
        {
            _moveSpeed = _characterStats.Spd;
        }
    }
}
