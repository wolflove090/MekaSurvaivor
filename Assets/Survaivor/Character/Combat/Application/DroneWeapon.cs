using System;
using UnityEngine;

/// <summary>
/// プレイヤー追従ドローンの展開を管理する武器コンポーネント
/// </summary>
public class DroneWeapon : WeaponBase
{
    readonly Func<int> _sourcePowProvider;

    [Tooltip("ドローンの再展開・再設定間隔（秒）")]
    float _deployInterval = 5f;

    [Tooltip("ドローンの攻撃間隔（秒）")]
    float _droneShotInterval = 1.25f;

    [Tooltip("強化1段階ごとの攻撃間隔短縮率")]
    float _shotIntervalReductionPerLevel = 0.15f;

    [Tooltip("ドローン攻撃間隔の最小値（秒）")]
    float _minShotInterval = 0.25f;

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
        Func<int> sourcePowProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
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

        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        _effectExecutor.DeployDrone(
            new DroneSpawnRequest(
                GetOriginPosition(),
                _originTransform,
                sourcePow,
                _orbitRadius,
                _droneShotInterval));
    }

    /// <summary>
    /// 武器を1段階強化し、ドローンの攻撃間隔を短縮します。
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();

        float reducedInterval = _droneShotInterval * (1f - _shotIntervalReductionPerLevel);
        _droneShotInterval = Mathf.Max(_minShotInterval, reducedInterval);

        Debug.Log($"DroneWeapon: レベル {UpgradeLevel} に強化。攻撃間隔: {_droneShotInterval:0.00}s");
    }
}
