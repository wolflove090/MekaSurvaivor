using System;
using System.Collections.Generic;
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
}
