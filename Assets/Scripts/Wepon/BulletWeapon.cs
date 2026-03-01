using System;
using UnityEngine;

/// <summary>
/// 一番近いエネミーの方向に弾を発射する武器コンポーネント
/// </summary>
public class BulletWeapon : WeaponBase
{
    readonly EnemyRegistry _enemyRegistry;
    readonly BreakableObjectSpawner _breakableObjectSpawner;
    readonly Func<int> _sourcePowProvider;

    [Tooltip("発射間隔（秒）")]
    float _shootInterval = 1f;

    [Tooltip("強化1段階ごとの発射間隔短縮率")]
    float _intervalReductionPerLevel = 0.15f;

    [Tooltip("発射間隔の最小値（秒）")]
    float _minShootInterval = 0.2f;

    [Tooltip("弾の発射位置のオフセット")]
    Vector3 _shootOffset = Vector3.zero;

    /// <summary>
    /// 発射間隔を取得または設定します
    /// </summary>
    public float ShootInterval
    {
        get => _shootInterval;
        set => _shootInterval = value;
    }

    protected override float CooldownDuration => _shootInterval;

    /// <summary>
    /// 通常弾武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="enemyRegistry">探索対象の敵レジストリ</param>
    /// <param name="breakableObjectSpawner">探索対象の破壊可能オブジェクトスポナー</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    public BulletWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        EnemyRegistry enemyRegistry,
        BreakableObjectSpawner breakableObjectSpawner,
        Func<int> sourcePowProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _enemyRegistry = enemyRegistry;
        _breakableObjectSpawner = breakableObjectSpawner;
        _sourcePowProvider = sourcePowProvider;
    }

    /// <summary>
    /// 弾の発射を試みます
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        Vector3 shootPosition = GetOriginPosition() + _shootOffset;
        GameObject target = null;

        if (_enemyRegistry != null)
        {
            target = _enemyRegistry.FindNearestEnemy(shootPosition, onlyVisible: true);
        }

        if (target == null && _breakableObjectSpawner != null)
        {
            target = _breakableObjectSpawner.FindNearestBreakableObject(shootPosition, onlyVisible: true);
        }

        if (target == null)
        {
            return;
        }

        Vector3 direction = (target.transform.position - shootPosition).normalized;
        direction.y = 0f;
        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        _effectExecutor.FireBullet(new BulletFireRequest(shootPosition, direction, sourcePow));
    }

    /// <summary>
    /// 武器を1段階強化し、発射間隔を短縮します
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();

        float reducedInterval = _shootInterval * (1f - _intervalReductionPerLevel);
        _shootInterval = Mathf.Max(_minShootInterval, reducedInterval);
        ClampCooldownTimerToDuration();

        Debug.Log($"BulletWeapon: レベル {UpgradeLevel} に強化。発射間隔: {_shootInterval:0.00}s");
    }
}
