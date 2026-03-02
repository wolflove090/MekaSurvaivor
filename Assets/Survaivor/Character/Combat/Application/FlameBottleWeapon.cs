using System;
using UnityEngine;

/// <summary>
/// 放物線で火炎瓶を投擲する武器コンポーネント
/// </summary>
public class FlameBottleWeapon : WeaponBase
{
    readonly Func<int> _sourcePowProvider;
    readonly Func<Vector3> _facingDirectionProvider;

    [Tooltip("投擲間隔（秒）")]
    float _throwInterval = 2.5f;

    [Tooltip("強化1段階ごとの投擲間隔短縮率")]
    float _intervalReductionPerLevel = 0.1f;

    [Tooltip("投擲間隔の最小値（秒）")]
    float _minThrowInterval = 0.5f;

    [Tooltip("水平初速")]
    float _horizontalSpeed = 4f;

    [Tooltip("上方向初速")]
    float _upwardSpeed = 3f;

    [Tooltip("炎エリア持続時間（秒）")]
    float _flameDuration = 3f;

    [Tooltip("炎エリア半径")]
    float _flameRadius = 2f;

    [Tooltip("強化1段階ごとの炎エリア持続時間増加量")]
    float _durationIncreasePerLevel = 0.5f;

    protected override float CooldownDuration => _throwInterval;

    /// <summary>
    /// 火炎瓶武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    /// <param name="facingDirectionProvider">プレイヤー向き取得デリゲート</param>
    public FlameBottleWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        Func<int> sourcePowProvider,
        Func<Vector3> facingDirectionProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
        _facingDirectionProvider = facingDirectionProvider;
    }

    /// <summary>
    /// プレイヤーの向きに応じて火炎瓶を投擲します。
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        Vector3 horizontalDirection = _facingDirectionProvider != null ? _facingDirectionProvider() : Vector3.right;
        horizontalDirection.y = 0f;
        if (horizontalDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            horizontalDirection = Vector3.right;
        }

        horizontalDirection.Normalize();

        Vector3 initialVelocity = horizontalDirection * _horizontalSpeed + Vector3.up * _upwardSpeed;
        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        Vector3 origin = GetOriginPosition();
        _effectExecutor.FireFlameBottle(
            new FlameBottleFireRequest(
                origin,
                initialVelocity,
                sourcePow,
                origin.y,
                _flameDuration,
                _flameRadius));
    }

    /// <summary>
    /// 武器を1段階強化し、投擲間隔を短縮して炎エリア持続時間を延長します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();

        float reducedInterval = _throwInterval * (1f - _intervalReductionPerLevel);
        _throwInterval = Mathf.Max(_minThrowInterval, reducedInterval);
        _flameDuration += _durationIncreasePerLevel;
        ClampCooldownTimerToDuration();

        Debug.Log($"FlameBottleWeapon: レベル {UpgradeLevel} に強化。投擲間隔: {_throwInterval:0.00}s, 持続時間: {_flameDuration:0.00}s");
    }
}
