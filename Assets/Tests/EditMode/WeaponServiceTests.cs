using System;
using System.Collections.Generic;
using NUnit.Framework;

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
}
