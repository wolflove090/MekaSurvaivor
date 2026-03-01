using UnityEngine;

/// <summary>
/// ダメージを受けることができるオブジェクトのインターフェース
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// ダメージを受けます
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（デフォルトはVector3.zero）</param>
    void TakeDamage(int damage, Vector3 knockbackDirection = default);

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    int CurrentHp { get; }

    /// <summary>
    /// 死亡状態かどうかを取得します
    /// </summary>
    bool IsDead { get; }
}
