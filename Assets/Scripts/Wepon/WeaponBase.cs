using UnityEngine;

/// <summary>
/// 武器のクールダウン制御を提供する基底クラス
/// </summary>
public abstract class WeaponBase
{
    protected Transform _transform;

    float _cooldownTimer;

    /// <summary>
    /// 現在の強化レベルを取得します
    /// </summary>
    public int UpgradeLevel { get; protected set; } = 1;

    /// <summary>
    /// 発動間隔（秒）を取得します
    /// </summary>
    protected abstract float CooldownDuration { get; }


    public WeaponBase(Transform transform)
    {
        _transform = transform;
        _cooldownTimer = CooldownDuration;
    }

    public virtual void Update()
    {
        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0f)
        {
            return;
        }

        Fire();
        _cooldownTimer = CooldownDuration;
    }

    /// <summary>
    /// 武器効果を発動します
    /// </summary>
    protected abstract void Fire();

    /// <summary>
    /// 武器を1段階強化します
    /// </summary>
    public abstract void LevelUp();

    /// <summary>
    /// クールダウン残り時間を現在の発動間隔以内に補正します
    /// </summary>
    protected void ClampCooldownTimerToDuration()
    {
        _cooldownTimer = Mathf.Min(_cooldownTimer, CooldownDuration);
    }
}
