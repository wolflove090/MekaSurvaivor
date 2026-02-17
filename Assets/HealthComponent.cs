using UnityEngine;

/// <summary>
/// HP管理とダメージ処理を担当するコンポーネント
/// </summary>
public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField]
    [Tooltip("最大HP")]
    int _maxHp = 10;

    int _currentHp;

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _currentHp;

    /// <summary>
    /// 最大HPを取得します
    /// </summary>
    public int MaxHp => _maxHp;

    /// <summary>
    /// 死亡状態かどうかを取得します
    /// </summary>
    public bool IsDead => _currentHp <= 0;

    /// <summary>
    /// ダメージを受けた時に発火するイベント
    /// </summary>
    public event System.Action<int> OnDamaged;

    /// <summary>
    /// 死亡時に発火するイベント
    /// </summary>
    public event System.Action OnDied;

    void Awake()
    {
        _currentHp = _maxHp;
    }

    /// <summary>
    /// HPを最大値までリセットします
    /// </summary>
    public void ResetToMaxHp()
    {
        _currentHp = _maxHp;
    }

    /// <summary>
    /// ダメージを受けます
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（デフォルトはVector3.zero）</param>
    public void TakeDamage(int damage, Vector3 knockbackDirection = default)
    {
        if (IsDead)
        {
            return;
        }

        _currentHp -= damage;
        _currentHp = Mathf.Max(_currentHp, 0);

        OnDamaged?.Invoke(damage);

        // ノックバック方向が指定されている場合、KnockbackComponentに適用
        if (knockbackDirection != Vector3.zero)
        {
            KnockbackComponent knockback = GetComponent<KnockbackComponent>();
            if (knockback != null)
            {
                knockback.ApplyKnockback(knockbackDirection);
            }
        }

        if (IsDead)
        {
            OnDied?.Invoke();
        }
    }

    /// <summary>
    /// HPを回復します
    /// </summary>
    /// <param name="amount">回復量</param>
    public void Heal(int amount)
    {
        if (IsDead)
        {
            return;
        }

        _currentHp += amount;
        _currentHp = Mathf.Min(_currentHp, _maxHp);
    }
}
