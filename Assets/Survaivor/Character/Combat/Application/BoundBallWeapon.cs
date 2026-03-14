using System;
using UnityEngine;

/// <summary>
/// 固定方向に反射弾を発射する武器コンポーネント
/// </summary>
public class BoundBallWeapon : WeaponBase
{
    const int VERTICAL_SPLIT_LEVEL = 5;

    static readonly float[] SHOOT_INTERVALS =
    {
        2.0f,
        1.5f,
        1.5f,
        0.8f,
        0.8f
    };

    static readonly int[] MAX_BOUNCE_COUNTS =
    {
        2,
        2,
        3,
        3,
        3
    };

    readonly Func<int> _sourcePowProvider;

    [Tooltip("発射間隔（秒）")]
    float _shootInterval = SHOOT_INTERVALS[0];

    [Tooltip("最大バウンド回数")]
    int _maxBounceCount = MAX_BOUNCE_COUNTS[0];

    protected override float CooldownDuration => _shootInterval;

    /// <summary>
    /// バウンドボール武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    public BoundBallWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        Func<int> sourcePowProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
    }

    /// <summary>
    /// ワールド固定方向へバウンドボールを発射します。
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        Vector3 origin = GetOriginPosition();
        if (UpgradeLevel >= VERTICAL_SPLIT_LEVEL)
        {
            _effectExecutor.FireBoundBall(
                new BoundBallFireRequest(
                    origin,
                    Vector3.forward,
                    sourcePow,
                    _maxBounceCount));
            _effectExecutor.FireBoundBall(
                new BoundBallFireRequest(
                    origin,
                    Vector3.back,
                    sourcePow,
                    _maxBounceCount));
            return;
        }

        _effectExecutor.FireBoundBall(
            new BoundBallFireRequest(
                origin,
                Vector3.back,
                sourcePow,
                _maxBounceCount));
    }

    /// <summary>
    /// 武器を1段階強化し、発射間隔を短縮します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        _shootInterval = GetShootIntervalForCurrentLevel();
        _maxBounceCount = GetMaxBounceCountForCurrentLevel();
        ClampCooldownTimerToDuration();

        Debug.Log($"BoundBallWeapon: レベル {UpgradeLevel} に強化。発射間隔: {_shootInterval:0.00}s, バウンド回数: {_maxBounceCount}");
    }

    float GetShootIntervalForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, SHOOT_INTERVALS.Length - 1);
        return SHOOT_INTERVALS[levelIndex];
    }

    int GetMaxBounceCountForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, MAX_BOUNCE_COUNTS.Length - 1);
        return MAX_BOUNCE_COUNTS[levelIndex];
    }
}
