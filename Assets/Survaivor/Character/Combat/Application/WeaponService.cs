using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器クールダウンの進行と発動判定を管理します。
/// </summary>
public class WeaponService
{
    static readonly WeaponUpgradeUiController.UpgradeCardType[] AVAILABLE_UPGRADE_TYPES =
    {
        WeaponUpgradeUiController.UpgradeCardType.Shooter,
        WeaponUpgradeUiController.UpgradeCardType.Throwing,
        WeaponUpgradeUiController.UpgradeCardType.DamageField,
        WeaponUpgradeUiController.UpgradeCardType.Drone,
        WeaponUpgradeUiController.UpgradeCardType.BoundBall,
        WeaponUpgradeUiController.UpgradeCardType.FlameBottle
    };

    readonly Transform _weaponOrigin;
    readonly Dictionary<WeaponUpgradeUiController.UpgradeCardType, Func<WeaponBase, WeaponBase>> _weaponBuilders;
    readonly Dictionary<WeaponUpgradeUiController.UpgradeCardType, Type> _weaponTypes;

    /// <summary>
    /// クールダウン制御専用のサービスを初期化します。
    /// </summary>
    public WeaponService()
    {
    }

    /// <summary>
    /// 武器生成と強化管理用のサービスを初期化します。
    /// </summary>
    /// <param name="weaponOrigin">武器の発動基準となるTransform</param>
    /// <param name="effectExecutor">武器発動要求の実行ポート</param>
    /// <param name="sourcePowProvider">攻撃力取得デリゲート</param>
    /// <param name="facingDirectionProvider">プレイヤー向き取得デリゲート</param>
    /// <param name="enemyRegistry">探索対象の敵レジストリ</param>
    /// <param name="breakableObjectSpawner">探索対象の破壊可能オブジェクトスポナー</param>
    public WeaponService(
        Transform weaponOrigin,
        IWeaponEffectExecutor effectExecutor,
        Func<int> sourcePowProvider,
        Func<Vector3> facingDirectionProvider,
        EnemyRegistry enemyRegistry = null,
        BreakableObjectSpawner breakableObjectSpawner = null)
    {
        _weaponOrigin = weaponOrigin ?? throw new ArgumentNullException(nameof(weaponOrigin));
        _weaponBuilders = new Dictionary<WeaponUpgradeUiController.UpgradeCardType, Func<WeaponBase, WeaponBase>>
        {
            {
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                rideWeapon => new BulletWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    enemyRegistry,
                    breakableObjectSpawner,
                    sourcePowProvider)
            },
            {
                WeaponUpgradeUiController.UpgradeCardType.Throwing,
                rideWeapon => new ThrowingWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    sourcePowProvider,
                    facingDirectionProvider)
            },
            {
                WeaponUpgradeUiController.UpgradeCardType.DamageField,
                rideWeapon => new DamageFieldWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    sourcePowProvider)
            },
            {
                WeaponUpgradeUiController.UpgradeCardType.Drone,
                rideWeapon => new DroneWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    sourcePowProvider)
            },
            {
                WeaponUpgradeUiController.UpgradeCardType.BoundBall,
                rideWeapon => new BoundBallWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    sourcePowProvider)
            },
            {
                WeaponUpgradeUiController.UpgradeCardType.FlameBottle,
                rideWeapon => new FlameBottleWeapon(
                    _weaponOrigin,
                    rideWeapon,
                    effectExecutor,
                    sourcePowProvider,
                    facingDirectionProvider)
            }
        };
        _weaponTypes = new Dictionary<WeaponUpgradeUiController.UpgradeCardType, Type>
        {
            { WeaponUpgradeUiController.UpgradeCardType.Shooter, typeof(BulletWeapon) },
            { WeaponUpgradeUiController.UpgradeCardType.Throwing, typeof(ThrowingWeapon) },
            { WeaponUpgradeUiController.UpgradeCardType.DamageField, typeof(DamageFieldWeapon) },
            { WeaponUpgradeUiController.UpgradeCardType.Drone, typeof(DroneWeapon) },
            { WeaponUpgradeUiController.UpgradeCardType.BoundBall, typeof(BoundBallWeapon) },
            { WeaponUpgradeUiController.UpgradeCardType.FlameBottle, typeof(FlameBottleWeapon) }
        };
    }

    /// <summary>
    /// クールダウンを進行させ、発動可能かどうかを返します。
    /// </summary>
    /// <param name="state">対象の武器状態</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    /// <param name="cooldownDuration">現在のクールダウン時間</param>
    /// <returns>発動可能になった場合はtrue</returns>
    public bool Tick(WeaponState state, float deltaTime, float cooldownDuration)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        float normalizedCooldownDuration = Mathf.Max(0f, cooldownDuration);
        float nextCooldownRemaining = state.CooldownRemaining - Mathf.Max(0f, deltaTime);
        state.SetCooldownRemaining(nextCooldownRemaining);

        if (state.CooldownRemaining > 0f)
        {
            return false;
        }

        state.SetCooldownRemaining(normalizedCooldownDuration);
        return true;
    }

    /// <summary>
    /// 武器強化を適用し、現在アクティブな武器チェーンの先頭を返します。
    /// </summary>
    /// <param name="type">適用する武器種別</param>
    /// <param name="currentWeapon">現在アクティブな武器チェーンの先頭</param>
    /// <param name="weapons">取得済み武器の管理テーブル</param>
    /// <returns>更新後のアクティブ武器チェーンの先頭</returns>
    public WeaponBase ApplyUpgrade(
        WeaponUpgradeUiController.UpgradeCardType type,
        WeaponBase currentWeapon,
        Dictionary<Type, WeaponBase> weapons)
    {
        if (_weaponBuilders == null || _weaponTypes == null)
        {
            throw new InvalidOperationException("WeaponService: 武器強化管理用の初期化が行われていません。");
        }

        if (weapons == null)
        {
            throw new ArgumentNullException(nameof(weapons));
        }

        if (!_weaponBuilders.TryGetValue(type, out Func<WeaponBase, WeaponBase> builder) ||
            !_weaponTypes.TryGetValue(type, out Type weaponType))
        {
            Debug.LogWarning($"WeaponService: 未対応の武器種別です。 type={type}");
            return currentWeapon;
        }

        if (!weapons.TryGetValue(weaponType, out WeaponBase targetWeapon))
        {
            WeaponBase newWeapon = builder(currentWeapon);
            weapons[weaponType] = newWeapon;
            return newWeapon;
        }

        targetWeapon.LevelUp();
        return currentWeapon;
    }

    /// <summary>
    /// 指定した武器種別に対応する武器型を取得します。
    /// </summary>
    /// <param name="type">確認する武器種別</param>
    /// <param name="weaponType">取得できた武器型</param>
    /// <returns>対応する武器型がある場合はtrue</returns>
    public bool TryGetWeaponType(WeaponUpgradeUiController.UpgradeCardType type, out Type weaponType)
    {
        weaponType = null;

        if (_weaponTypes == null)
        {
            return false;
        }

        return _weaponTypes.TryGetValue(type, out weaponType);
    }

    /// <summary>
    /// 指定した武器を現在所持しているかどうかを返します。
    /// </summary>
    /// <param name="type">確認する武器種別</param>
    /// <param name="weapons">取得済み武器の管理テーブル</param>
    /// <returns>所持している場合はtrue</returns>
    public bool HasWeapon(
        WeaponUpgradeUiController.UpgradeCardType type,
        Dictionary<Type, WeaponBase> weapons)
    {
        return TryGetWeapon(type, weapons, out _);
    }

    /// <summary>
    /// 指定した武器の現在レベルを取得します。
    /// </summary>
    /// <param name="type">確認する武器種別</param>
    /// <param name="weapons">取得済み武器の管理テーブル</param>
    /// <param name="level">取得できた現在レベル</param>
    /// <returns>武器を所持していてレベル取得できた場合はtrue</returns>
    public bool TryGetWeaponLevel(
        WeaponUpgradeUiController.UpgradeCardType type,
        Dictionary<Type, WeaponBase> weapons,
        out int level)
    {
        level = 0;

        if (!TryGetWeapon(type, weapons, out WeaponBase weapon))
        {
            return false;
        }

        level = weapon.UpgradeLevel;
        return true;
    }

    /// <summary>
    /// 指定した武器レベル一覧から武器チェーン全体を再構築します。
    /// </summary>
    /// <param name="targetLevels">適用する目標レベル一覧</param>
    /// <param name="weapons">更新対象の取得済み武器テーブル</param>
    /// <returns>再構築後のアクティブ武器チェーン先頭</returns>
    public WeaponBase RebuildWeapons(
        IReadOnlyDictionary<WeaponUpgradeUiController.UpgradeCardType, int> targetLevels,
        Dictionary<Type, WeaponBase> weapons)
    {
        if (_weaponBuilders == null || _weaponTypes == null)
        {
            throw new InvalidOperationException("WeaponService: 武器強化管理用の初期化が行われていません。");
        }

        if (targetLevels == null)
        {
            throw new ArgumentNullException(nameof(targetLevels));
        }

        if (weapons == null)
        {
            throw new ArgumentNullException(nameof(weapons));
        }

        weapons.Clear();

        WeaponBase activeWeapon = null;
        foreach (WeaponUpgradeUiController.UpgradeCardType type in AVAILABLE_UPGRADE_TYPES)
        {
            if (!targetLevels.TryGetValue(type, out int targetLevel) || targetLevel <= 0)
            {
                continue;
            }

            if (!_weaponBuilders.TryGetValue(type, out Func<WeaponBase, WeaponBase> builder) ||
                !_weaponTypes.TryGetValue(type, out Type weaponType))
            {
                Debug.LogWarning($"WeaponService: 未対応の武器種別です。 type={type}");
                continue;
            }

            WeaponBase weapon = builder(activeWeapon);
            for (int level = 1; level < targetLevel; level++)
            {
                weapon.LevelUp();
            }

            weapons[weaponType] = weapon;
            activeWeapon = weapon;
        }

        return activeWeapon;
    }

    /// <summary>
    /// クールダウン残り時間を現在の発動間隔以内に補正します。
    /// </summary>
    /// <param name="state">対象の武器状態</param>
    /// <param name="cooldownDuration">現在のクールダウン時間</param>
    public void ClampCooldownToDuration(WeaponState state, float cooldownDuration)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        float normalizedCooldownDuration = Mathf.Max(0f, cooldownDuration);
        state.SetCooldownRemaining(Mathf.Min(state.CooldownRemaining, normalizedCooldownDuration));
    }

    /// <summary>
    /// 強化UIで提示可能な全武器候補一覧を取得します。
    /// </summary>
    /// <returns>提示可能な武器候補一覧</returns>
    public IReadOnlyList<WeaponUpgradeUiController.UpgradeCardType> GetAvailableUpgradeTypes()
    {
        return AVAILABLE_UPGRADE_TYPES;
    }

    bool TryGetWeapon(
        WeaponUpgradeUiController.UpgradeCardType type,
        Dictionary<Type, WeaponBase> weapons,
        out WeaponBase weapon)
    {
        weapon = null;

        if (weapons == null)
        {
            return false;
        }

        if (!TryGetWeaponType(type, out Type weaponType))
        {
            return false;
        }

        return weapons.TryGetValue(weaponType, out weapon);
    }
}
