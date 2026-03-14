using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// WeaponServiceのクールダウン制御とガード条件を検証します。
/// </summary>
public class WeaponServiceTests
{
    /// <summary>
    /// クールダウンが残っている間は発動せず、残り時間だけ減少することを検証します。
    /// </summary>
    [Test]
    public void Tick_BeforeCooldownExpires_DoesNotTriggerAndReducesRemaining()
    {
        WeaponService service = new WeaponService();
        WeaponState state = new WeaponState(2f);

        bool triggered = service.Tick(state, 0.5f, 2f);

        Assert.That(triggered, Is.False);
        Assert.That(state.CooldownRemaining, Is.EqualTo(1.5f));
    }

    /// <summary>
    /// クールダウンを使い切ると発動し、次回分のクールダウンに戻ることを検証します。
    /// </summary>
    [Test]
    public void Tick_WhenCooldownExpires_TriggersAndResetsCooldown()
    {
        WeaponService service = new WeaponService();
        WeaponState state = new WeaponState(1f);

        bool triggered = service.Tick(state, 1f, 0.75f);

        Assert.That(triggered, Is.True);
        Assert.That(state.CooldownRemaining, Is.EqualTo(0.75f));
    }

    /// <summary>
    /// 現在のクールダウンを超えている残り時間を現在の発動間隔まで補正することを検証します。
    /// </summary>
    [Test]
    public void ClampCooldownToDuration_WhenRemainingExceedsDuration_ClampsToDuration()
    {
        WeaponService service = new WeaponService();
        WeaponState state = new WeaponState(5f);

        service.ClampCooldownToDuration(state, 2f);

        Assert.That(state.CooldownRemaining, Is.EqualTo(2f));
    }

    /// <summary>
    /// 武器生成用に初期化していない場合は強化適用で例外になることを検証します。
    /// </summary>
    [Test]
    public void ApplyUpgrade_WithoutWeaponOriginInitialization_ThrowsInvalidOperationException()
    {
        WeaponService service = new WeaponService();

        TestDelegate action = () => service.ApplyUpgrade(
            WeaponUpgradeUiController.UpgradeCardType.Shooter,
            null,
            new Dictionary<Type, WeaponBase>());

        Assert.That(action, Throws.TypeOf<InvalidOperationException>());
    }

    /// <summary>
    /// 未取得の武器を選択すると新しい武器が生成され、管理テーブルへ登録されることを検証します。
    /// </summary>
    [Test]
    public void ApplyUpgrade_WhenWeaponIsMissing_CreatesAndRegistersWeapon()
    {
        GameObject player = new GameObject("Player");

        try
        {
            WeaponService service = new WeaponService(
                player.transform,
                null,
                () => 3,
                () => Vector3.right);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();

            WeaponBase activeWeapon = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                null,
                weapons);

            Assert.That(activeWeapon, Is.TypeOf<BulletWeapon>());
            Assert.That(weapons.ContainsKey(typeof(BulletWeapon)), Is.True);
            Assert.That(weapons[typeof(BulletWeapon)], Is.SameAs(activeWeapon));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 既に取得済みの武器を選択すると同一インスタンスを強化し、先頭武器は維持されることを検証します。
    /// </summary>
    [Test]
    public void ApplyUpgrade_WhenWeaponAlreadyExists_LevelsUpExistingWeapon()
    {
        GameObject player = new GameObject("Player");

        try
        {
            WeaponService service = new WeaponService(
                player.transform,
                null,
                () => 2,
                () => Vector3.left);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();
            WeaponBase currentWeapon = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                null,
                weapons);

            WeaponBase result = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                currentWeapon,
                weapons);

            Assert.That(result, Is.SameAs(currentWeapon));
            Assert.That(weapons[typeof(BulletWeapon)].UpgradeLevel, Is.EqualTo(2));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 強化候補一覧に新規武器3種を含む全武器種が返ることを検証します。
    /// </summary>
    [Test]
    public void GetAvailableUpgradeTypes_ReturnsAllSupportedWeaponTypes()
    {
        WeaponService service = new WeaponService();

        IReadOnlyList<WeaponUpgradeUiController.UpgradeCardType> types = service.GetAvailableUpgradeTypes();

        Assert.That(types, Is.EqualTo(new[]
        {
            WeaponUpgradeUiController.UpgradeCardType.Shooter,
            WeaponUpgradeUiController.UpgradeCardType.Throwing,
            WeaponUpgradeUiController.UpgradeCardType.DamageField,
            WeaponUpgradeUiController.UpgradeCardType.Drone,
            WeaponUpgradeUiController.UpgradeCardType.BoundBall,
            WeaponUpgradeUiController.UpgradeCardType.FlameBottle
        }));
    }

    /// <summary>
    /// 新規武器を選択すると対応する武器が生成されることを検証します。
    /// </summary>
    [TestCase(WeaponUpgradeUiController.UpgradeCardType.Drone, typeof(DroneWeapon))]
    [TestCase(WeaponUpgradeUiController.UpgradeCardType.BoundBall, typeof(BoundBallWeapon))]
    [TestCase(WeaponUpgradeUiController.UpgradeCardType.FlameBottle, typeof(FlameBottleWeapon))]
    public void ApplyUpgrade_WhenSelectingNewWeaponType_CreatesExpectedWeapon(
        WeaponUpgradeUiController.UpgradeCardType upgradeType,
        Type expectedWeaponType)
    {
        GameObject player = new GameObject("Player");

        try
        {
            WeaponService service = new WeaponService(
                player.transform,
                null,
                () => 4,
                () => Vector3.right);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();

            WeaponBase activeWeapon = service.ApplyUpgrade(upgradeType, null, weapons);

            Assert.That(activeWeapon, Is.TypeOf(expectedWeaponType));
            Assert.That(weapons.ContainsKey(expectedWeaponType), Is.True);
            Assert.That(weapons[expectedWeaponType], Is.SameAs(activeWeapon));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// ドローン武器を新規取得した直後の最初のTickで展開されることを検証します。
    /// </summary>
    [Test]
    public void ApplyUpgrade_WhenAddingDrone_FirstTickDeploysDroneImmediately()
    {
        GameObject player = new GameObject("Player");

        try
        {
            RecordingWeaponEffectExecutor effectExecutor = new RecordingWeaponEffectExecutor();
            WeaponService service = new WeaponService(
                player.transform,
                effectExecutor,
                () => 4,
                () => Vector3.right);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();

            WeaponBase activeWeapon = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Drone,
                null,
                weapons);

            activeWeapon.Tick(0f);

            Assert.That(effectExecutor.DeployDroneCallCount, Is.EqualTo(1));
            Assert.That(effectExecutor.LastDroneRequest, Is.Not.Null);
            Assert.That(effectExecutor.LastDroneRequest.FollowTarget, Is.EqualTo(player.transform));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 目標レベル一覧から武器チェーンを再構築できることを検証します。
    /// </summary>
    [Test]
    public void RebuildWeapons_WhenTargetLevelsIncludeRemovalAndDowngrade_RebuildsExpectedLoadout()
    {
        GameObject player = new GameObject("Player");

        try
        {
            WeaponService service = new WeaponService(
                player.transform,
                null,
                () => 4,
                () => Vector3.right);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();
            Dictionary<WeaponUpgradeUiController.UpgradeCardType, int> targetLevels =
                new Dictionary<WeaponUpgradeUiController.UpgradeCardType, int>
                {
                    { WeaponUpgradeUiController.UpgradeCardType.Shooter, 2 },
                    { WeaponUpgradeUiController.UpgradeCardType.Throwing, 0 },
                    { WeaponUpgradeUiController.UpgradeCardType.Drone, 3 }
                };

            WeaponBase activeWeapon = service.RebuildWeapons(targetLevels, weapons);

            Assert.That(activeWeapon, Is.TypeOf<DroneWeapon>());
            Assert.That(weapons.ContainsKey(typeof(BulletWeapon)), Is.True);
            Assert.That(weapons[typeof(BulletWeapon)].UpgradeLevel, Is.EqualTo(2));
            Assert.That(weapons.ContainsKey(typeof(ThrowingWeapon)), Is.False);
            Assert.That(weapons.ContainsKey(typeof(DroneWeapon)), Is.True);
            Assert.That(weapons[typeof(DroneWeapon)].UpgradeLevel, Is.EqualTo(3));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 武器生成サービスが攻撃間隔倍率providerを各武器へ配線することを検証します。
    /// </summary>
    [Test]
    public void ApplyUpgrade_WithAttackIntervalMultiplierProvider_WiresScaledCooldownToCreatedWeapons()
    {
        GameObject player = new GameObject("Player");

        try
        {
            RecordingWeaponEffectExecutor effectExecutor = new RecordingWeaponEffectExecutor();
            WeaponService service = new WeaponService(
                player.transform,
                effectExecutor,
                () => 1f,
                () => Vector3.right,
                () => 0.5f);
            Dictionary<Type, WeaponBase> weapons = new Dictionary<Type, WeaponBase>();

            WeaponBase bulletWeapon = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                null,
                weapons);
            bulletWeapon.Tick(0f);

            WeaponState bulletState = GetPrivateField<WeaponState>(bulletWeapon, "_weaponState", typeof(WeaponBase));
            Assert.That(bulletState.CooldownRemaining, Is.EqualTo(0.75f).Within(0.0001f));

            WeaponBase droneWeapon = service.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Drone,
                bulletWeapon,
                weapons);
            droneWeapon.Tick(0f);

            WeaponState droneState = GetPrivateField<WeaponState>(droneWeapon, "_weaponState", typeof(WeaponBase));
            Assert.That(droneState.CooldownRemaining, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(effectExecutor.LastDroneRequest, Is.Not.Null);
            Assert.That(effectExecutor.LastDroneRequest.ShotInterval, Is.EqualTo(1f).Within(0.0001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// テスト用に武器発動要求を記録する実装です。
    /// </summary>
    sealed class RecordingWeaponEffectExecutor : IWeaponEffectExecutor
    {
        /// <summary>
        /// ドローン展開が呼ばれた回数を取得します。
        /// </summary>
        public int DeployDroneCallCount { get; private set; }

        /// <summary>
        /// 最後に受け取ったドローン展開要求を取得します。
        /// </summary>
        public DroneSpawnRequest LastDroneRequest { get; private set; }

        /// <summary>
        /// 通常弾の発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireBullet(BulletFireRequest request)
        {
        }

        /// <summary>
        /// 投擲弾の発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireThrowing(ThrowingFireRequest request)
        {
        }

        /// <summary>
        /// ダメージフィールド生成要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void SpawnDamageField(DamageFieldSpawnRequest request)
        {
        }

        /// <summary>
        /// ドローン展開要求を記録します。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void DeployDrone(DroneSpawnRequest request)
        {
            DeployDroneCallCount++;
            LastDroneRequest = request;
        }

        /// <summary>
        /// バウンドボールの発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireBoundBall(BoundBallFireRequest request)
        {
        }

        /// <summary>
        /// 火炎瓶の発射要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void FireFlameBottle(FlameBottleFireRequest request)
        {
        }

        /// <summary>
        /// 炎エリア生成要求を受け取ります。
        /// </summary>
        /// <param name="request">受け取った要求</param>
        public void SpawnFlameArea(FlameAreaSpawnRequest request)
        {
        }
    }

    /// <summary>
    /// privateフィールド値を取得します。
    /// </summary>
    /// <typeparam name="T">取得する型</typeparam>
    /// <param name="instance">取得対象インスタンス</param>
    /// <param name="fieldName">フィールド名</param>
    /// <param name="declaringType">フィールド宣言元型</param>
    /// <returns>取得したフィールド値</returns>
    static T GetPrivateField<T>(object instance, string fieldName, Type declaringType = null)
    {
        Type targetType = declaringType ?? instance.GetType();
        FieldInfo field = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field not found: {fieldName}");
        return (T)field.GetValue(instance);
    }
}
