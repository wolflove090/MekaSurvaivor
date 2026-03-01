using UnityEngine;

/// <summary>
/// 武器のランタイム状態を保持します。
/// </summary>
public class WeaponState
{
    /// <summary>
    /// 現在の強化レベルを取得します。
    /// </summary>
    public int UpgradeLevel { get; private set; }

    /// <summary>
    /// 次回発動までの残り時間を取得します。
    /// </summary>
    public float CooldownRemaining { get; private set; }

    /// <summary>
    /// 武器状態を初期化します。
    /// </summary>
    /// <param name="initialCooldown">初期クールダウン時間</param>
    public WeaponState(float initialCooldown)
    {
        UpgradeLevel = 1;
        CooldownRemaining = Mathf.Max(0f, initialCooldown);
    }

    /// <summary>
    /// 強化レベルを1段階上げます。
    /// </summary>
    public void IncrementUpgradeLevel()
    {
        UpgradeLevel++;
    }

    /// <summary>
    /// 残りクールダウン時間を設定します。
    /// </summary>
    /// <param name="cooldownRemaining">設定する残り時間</param>
    public void SetCooldownRemaining(float cooldownRemaining)
    {
        CooldownRemaining = Mathf.Max(0f, cooldownRemaining);
    }
}
