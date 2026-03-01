using UnityEngine;

/// <summary>
/// HP管理とダメージ処理を担当するコンポーネント
/// </summary>
public class HealthComponent : MonoBehaviour, IDamageable
{
    CharacterStats _characterStats;
    int _currentHp;

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _currentHp;

    /// <summary>
    /// 最大HPを取得します
    /// </summary>
    public int MaxHp => _characterStats != null ? _characterStats.CurrentValues.MaxHp : CharacterStatValues.Default.MaxHp;

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
        _characterStats = GetComponent<CharacterStats>();
        _currentHp = MaxHp;
    }

    /// <summary>
    /// HPを最大値までリセットします
    /// </summary>
    public void ResetToMaxHp()
    {
        _currentHp = MaxHp;
    }

    /// <summary>
    /// ステータスデータを差し替え、消費HPを維持したまま現在HPを再計算します
    /// </summary>
    /// <param name="newStatsData">適用する新しいステータスデータ</param>
    public void ApplyStatsDataAndKeepConsumedHp(CharacterStatsData newStatsData)
    {
        if (_characterStats == null)
        {
            Debug.LogWarning("HealthComponent: CharacterStatsが見つからないためステータスを適用できません。");
            return;
        }

        if (newStatsData == null)
        {
            Debug.LogWarning("HealthComponent: 適用するCharacterStatsDataがnullです。");
            return;
        }

        int previousCurrentHp = _currentHp;
        int previousMaxHp = MaxHp;
        int consumedHp = Mathf.Max(0, previousMaxHp - previousCurrentHp);

        _characterStats.ApplyStatsData(newStatsData);

        int recalculatedHp = Mathf.Max(1, MaxHp - consumedHp);
        _currentHp = Mathf.Min(recalculatedHp, MaxHp);
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

        int baseDamage = Mathf.Max(0, damage);
        CharacterStatValues currentValues = _characterStats != null ? _characterStats.CurrentValues : CharacterStatValues.Default;
        int defense = currentValues.Def;
        int finalDamage = Mathf.Max(1, baseDamage - defense);

        _currentHp -= finalDamage;
        _currentHp = Mathf.Max(_currentHp, 0);

        OnDamaged?.Invoke(finalDamage);

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
        _currentHp = Mathf.Min(_currentHp, MaxHp);
    }
}
