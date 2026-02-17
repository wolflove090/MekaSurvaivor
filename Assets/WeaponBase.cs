using UnityEngine;

/// <summary>
/// 武器のクールダウン制御を提供する基底クラス
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    float _cooldownTimer;

    /// <summary>
    /// 発動間隔（秒）を取得します
    /// </summary>
    protected abstract float CooldownDuration { get; }

    protected virtual void Start()
    {
        _cooldownTimer = CooldownDuration;
    }

    protected virtual void Update()
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
}
