using System;
using UnityEngine;

/// <summary>
/// 放物線で火炎瓶を投擲する武器コンポーネント
/// </summary>
public class FlameBottleWeapon : WeaponBase
{
    static readonly float[] THROW_INTERVALS =
    {
        2.0f,
        1.5f,
        1.5f,
        1.5f,
        1.5f
    };

    static readonly int[] PROJECTILE_COUNTS =
    {
        1,
        1,
        2,
        3,
        4
    };

    readonly Func<float> _sourcePowProvider;
    readonly Func<Vector3> _facingDirectionProvider;

    [Tooltip("投擲間隔（秒）")]
    float _throwInterval = THROW_INTERVALS[0];

    [Tooltip("1本ごとの扇状角度ステップ")]
    float _fanAngleStep = 40f;

    [Tooltip("水平初速")]
    float _horizontalSpeed = 8f;

    [Tooltip("上方向初速")]
    float _upwardSpeed = 6f;

    [Tooltip("プレイヤー前方へ出す投擲開始位置オフセット")]
    float _forwardSpawnOffset = 0.75f;

    [Tooltip("炎エリア持続時間（秒）")]
    float _flameDuration = 3f;

    [Tooltip("炎エリア半径")]
    float _flameRadius = 2f;

    [Tooltip("1回の投擲で発射する火炎瓶数")]
    int _projectileCount = PROJECTILE_COUNTS[0];

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
        Func<float> sourcePowProvider,
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

        float sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1f;
        Vector3 origin = GetOriginPosition() + horizontalDirection * _forwardSpawnOffset;
        float centerIndex = (_projectileCount - 1) * 0.5f;
        for (int i = 0; i < _projectileCount; i++)
        {
            float angleOffset = (i - centerIndex) * _fanAngleStep;
            Vector3 spreadDirection = Quaternion.Euler(0f, angleOffset, 0f) * horizontalDirection;
            Vector3 initialVelocity = spreadDirection * _horizontalSpeed + Vector3.up * _upwardSpeed;
            _effectExecutor.FireFlameBottle(
                new FlameBottleFireRequest(
                    origin,
                    initialVelocity,
                    sourcePow,
                    origin.y,
                    _flameDuration,
                    _flameRadius));
        }
    }

    /// <summary>
    /// 武器を1段階強化し、投擲間隔と投射本数を更新します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        _throwInterval = GetThrowIntervalForCurrentLevel();
        _projectileCount = GetProjectileCountForCurrentLevel();
        ClampCooldownTimerToDuration();

        Debug.Log($"FlameBottleWeapon: レベル {UpgradeLevel} に強化。投擲間隔: {_throwInterval:0.00}s, 投射数: {_projectileCount}");
    }

    float GetThrowIntervalForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, THROW_INTERVALS.Length - 1);
        return THROW_INTERVALS[levelIndex];
    }

    int GetProjectileCountForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, PROJECTILE_COUNTS.Length - 1);
        return PROJECTILE_COUNTS[levelIndex];
    }
}
