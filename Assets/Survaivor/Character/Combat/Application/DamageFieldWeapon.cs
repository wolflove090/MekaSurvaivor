using System;
using UnityEngine;

/// <summary>
/// プレイヤーの周囲にダメージフィールドを生成する武器コンポーネント
/// </summary>
public class DamageFieldWeapon : WeaponBase
{
    readonly Func<int> _sourcePowProvider;

    [Tooltip("生成間隔（秒）")]
    float _spawnInterval = 3f;

    [Tooltip("強化1段階ごとのダメージエリア拡大率")]
    float _areaScaleGrowthPerLevel = 1f;

    [Tooltip("生成位置のオフセット")]
    Vector3 _spawnOffset = Vector3.zero;
    const float DEFAULT_AREA_SCALE = 3f;
    float _currentAreaScale = DEFAULT_AREA_SCALE;

    /// <summary>
    /// 生成間隔を取得または設定します
    /// </summary>
    public float SpawnInterval
    {
        get => _spawnInterval;
        set => _spawnInterval = value;
    }

    protected override float CooldownDuration => _spawnInterval;

    /// <summary>
    /// ダメージフィールド武器を初期化します。
    /// </summary>
    /// <param name="originTransform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    public DamageFieldWeapon(
        Transform originTransform,
        WeaponBase rideWeapon,
        IWeaponEffectExecutor effectExecutor,
        Func<int> sourcePowProvider) : base(originTransform, rideWeapon, effectExecutor)
    {
        _sourcePowProvider = sourcePowProvider;
    }

    /// <summary>
    /// ダメージフィールドを生成します
    /// </summary>
    protected override void Fire()
    {
        if (_effectExecutor == null)
        {
            return;
        }

        Vector3 spawnPosition = GetOriginPosition() + _spawnOffset;
        int sourcePow = _sourcePowProvider != null ? _sourcePowProvider() : 1;
        _effectExecutor.SpawnDamageField(
            new DamageFieldSpawnRequest(spawnPosition, _originTransform, sourcePow, _currentAreaScale));
    }

    /// <summary>
    /// 武器を1段階強化し、ダメージエリアを拡大します
    /// </summary>
    public override void LevelUp()
    {
        _weaponState.IncrementUpgradeLevel();
        _currentAreaScale = DEFAULT_AREA_SCALE + _areaScaleGrowthPerLevel * (UpgradeLevel - 1);

        Debug.Log($"DamageFieldWeapon: レベル {UpgradeLevel} に強化。エリア倍率: {_currentAreaScale:0.00}x");
    }
}
