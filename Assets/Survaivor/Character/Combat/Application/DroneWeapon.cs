using System;
using UnityEngine;

/// <summary>
/// プレイヤー追従ドローンの展開を管理する武器コンポーネント
/// </summary>
public class DroneWeapon : WeaponBase
{
    static readonly float[] SHOT_INTERVALS =
    {
        2.0f,
        1.5f,
        1.0f,
        0.8f,
        0.8f
    };

    readonly Func<float> _sourcePowProvider;

    [Tooltip("ドローンの再展開・再設定間隔（秒）")]
    float _deployInterval = 5f;

    [Tooltip("ドローンの攻撃間隔（秒）")]
    float _droneShotInterval = SHOT_INTERVALS[0];

    [Tooltip("プレイヤーからの周回半径")]
    float _orbitRadius = 1.5f;

    protected override float CooldownDuration => _deployInterval;

    /// <summary>
    /// ドローン武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    public DroneWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        Func<float> sourcePowProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
        ReadyCooldownForImmediateTrigger();
    }

    /// <summary>
    /// ドローンの展開または設定更新を要求します。
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        float sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1f;
        int droneCount = UpgradeLevel >= 5 ? 2 : 1;
        for (int index = 0; index < droneCount; index++)
        {
            // Lv5では2機を180度ずらして展開し、常に対角配置で周回させる。
            float phaseOffsetDegrees = droneCount == 2 && index == 1 ? 180f : 0f;
            _effectExecutor.DeployDrone(
                new DroneSpawnRequest(
                    GetOriginPosition(),
                    _originTransform,
                    sourcePow,
                    _orbitRadius,
                    _droneShotInterval,
                    phaseOffsetDegrees));
        }
    }

    /// <summary>
    /// 武器を1段階強化し、ドローンの攻撃間隔を短縮します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        _droneShotInterval = GetShotIntervalForCurrentLevel();

        Debug.Log($"DroneWeapon: レベル {UpgradeLevel} に強化。攻撃間隔: {_droneShotInterval:0.00}s");
    }

    float GetShotIntervalForCurrentLevel()
    {
        int levelIndex = Mathf.Clamp(UpgradeLevel - 1, 0, SHOT_INTERVALS.Length - 1);
        return SHOT_INTERVALS[levelIndex];
    }
}
