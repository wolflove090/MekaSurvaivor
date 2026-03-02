using System;
using UnityEngine;

/// <summary>
/// 固定方向に反射弾を発射する武器コンポーネント
/// </summary>
public class BoundBallWeapon : WeaponBase
{
    readonly Func<int> _sourcePowProvider;

    [Tooltip("発射間隔（秒）")]
    float _shootInterval = 1.75f;

    [Tooltip("強化1段階ごとの発射間隔短縮率")]
    float _intervalReductionPerLevel = 0.1f;

    [Tooltip("発射間隔の最小値（秒）")]
    float _minShootInterval = 0.35f;

    [Tooltip("最大バウンド回数")]
    int _maxBounceCount = 3;

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
    /// ワールド固定の右下方向へバウンドボールを発射します。
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        Vector3 direction = new Vector3(1f, 0f, -1f).normalized;
        _effectExecutor.FireBoundBall(
            new BoundBallFireRequest(
                GetOriginPosition(),
                direction,
                sourcePow,
                _maxBounceCount));
    }

    /// <summary>
    /// 武器を1段階強化し、発射間隔を短縮します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();

        float reducedInterval = _shootInterval * (1f - _intervalReductionPerLevel);
        _shootInterval = Mathf.Max(_minShootInterval, reducedInterval);
        ClampCooldownTimerToDuration();

        Debug.Log($"BoundBallWeapon: レベル {UpgradeLevel} に強化。発射間隔: {_shootInterval:0.00}s");
    }
}
