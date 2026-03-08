using System;
using UnityEngine;

/// <summary>
/// プレイヤーの向いている方向に投擲弾を発射する武器コンポーネント
/// </summary>
public class ThrowingWeapon : WeaponBase
{
    const int SIDE_THROW_LEVEL = 5;

    static readonly float[] SHOOT_INTERVALS =
    {
        2.0f,
        1.5f,
        1.0f,
        0.8f,
        0.4f
    };

    readonly Func<float> _sourcePowProvider;
    readonly Func<Vector3> _facingDirectionProvider;

    [Tooltip("発射間隔（秒）")]
    float _shootInterval = SHOOT_INTERVALS[0];

    [Tooltip("弾の発射位置のオフセット")]
    Vector3 _shootOffset = Vector3.zero;

    protected override float CooldownDuration => _shootInterval;

    /// <summary>
    /// 投擲武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    /// <param name="facingDirectionProvider">プレイヤー向き取得デリゲート</param>
    public ThrowingWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        Func<float> sourcePowProvider,
        Func<Vector3> facingDirectionProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
        _facingDirectionProvider = facingDirectionProvider;
    }

    /// <summary>
    /// プレイヤーの向いている方向に投擲弾を発射します
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        Vector3 spawnPosition = GetOriginPosition() + _shootOffset;
        Vector3 direction = _facingDirectionProvider != null ? _facingDirectionProvider() : Vector3.right;
        direction.y = 0f;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = Vector3.right;
        }

        direction.Normalize();
        float sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1f;
        if (UpgradeLevel >= SIDE_THROW_LEVEL)
        {
            _effectExecutor.FireThrowing(new ThrowingFireRequest(spawnPosition, direction, sourcePow));
            _effectExecutor.FireThrowing(new ThrowingFireRequest(spawnPosition, -direction, sourcePow));
            return;
        }

        _effectExecutor.FireThrowing(new ThrowingFireRequest(spawnPosition, direction, sourcePow));
    }

    /// <summary>
    /// 武器を1段階強化し、発射間隔を短縮します
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        _shootInterval = GetShootIntervalForCurrentLevel();
        ClampCooldownTimerToDuration();

        Debug.Log($"ThrowingWeapon: レベル {UpgradeLevel} に強化。発射間隔: {_shootInterval:0.00}s");
    }

    float GetShootIntervalForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, SHOOT_INTERVALS.Length - 1);
        return SHOOT_INTERVALS[levelIndex];
    }
}
