using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PlayerControllerの武器デバッグ向け参照APIを検証します。
/// </summary>
public class PlayerControllerWeaponDebugTests
{
    /// <summary>
    /// 初期武器自動付与を無効化した場合、武器なしで開始できることを検証します。
    /// </summary>
    [Test]
    public void Awake_WhenDefaultWeaponGrantIsDisabled_StartsWithoutShooter()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            PlayerController controller = player.AddComponent<PlayerController>();
            SetGrantDefaultWeaponOnStart(controller, false);

            player.SetActive(true);

            Assert.That(controller.HasWeapon(WeaponUpgradeUiController.UpgradeCardType.Shooter), Is.False);
            Assert.That(
                controller.TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.Shooter, out int level),
                Is.False);
            Assert.That(level, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 武器追加後に所持判定とレベル参照ができることを検証します。
    /// </summary>
    [Test]
    public void ApplyWeaponUpgrade_WhenAddingNewWeapon_ReportsOwnedLevelOne()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            PlayerController controller = player.AddComponent<PlayerController>();
            SetGrantDefaultWeaponOnStart(controller, false);

            player.SetActive(true);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.Throwing);

            Assert.That(controller.HasWeapon(WeaponUpgradeUiController.UpgradeCardType.Throwing), Is.True);
            Assert.That(
                controller.TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.Throwing, out int level),
                Is.True);
            Assert.That(level, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 同じ武器を複数回適用すると差分ぶんだけレベルアップすることを検証します。
    /// </summary>
    [Test]
    public void ApplyWeaponUpgrade_WhenApplyingSameWeaponMultipleTimes_LevelsUpWeapon()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            PlayerController controller = player.AddComponent<PlayerController>();
            SetGrantDefaultWeaponOnStart(controller, false);

            player.SetActive(true);

            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.Drone);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.Drone);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.Drone);

            Assert.That(
                controller.TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.Drone, out int level),
                Is.True);
            Assert.That(level, Is.EqualTo(3));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 目標レベルを下げると指定レベルまで再構築されることを検証します。
    /// </summary>
    [Test]
    public void TrySetWeaponLevel_WhenLoweringLevel_RebuildsToRequestedLevel()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            PlayerController controller = player.AddComponent<PlayerController>();
            SetGrantDefaultWeaponOnStart(controller, false);

            player.SetActive(true);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.BoundBall);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.BoundBall);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.BoundBall);

            bool updated = controller.TrySetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.BoundBall, 2);

            Assert.That(updated, Is.True);
            Assert.That(
                controller.TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.BoundBall, out int level),
                Is.True);
            Assert.That(level, Is.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// 目標レベルを0にすると武器が取り上げられることを検証します。
    /// </summary>
    [Test]
    public void TrySetWeaponLevel_WhenSettingZero_RemovesWeapon()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            PlayerController controller = player.AddComponent<PlayerController>();
            SetGrantDefaultWeaponOnStart(controller, false);

            player.SetActive(true);
            controller.ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType.FlameBottle);

            bool updated = controller.TrySetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.FlameBottle, 0);

            Assert.That(updated, Is.True);
            Assert.That(controller.HasWeapon(WeaponUpgradeUiController.UpgradeCardType.FlameBottle), Is.False);
            Assert.That(
                controller.TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType.FlameBottle, out int level),
                Is.False);
            Assert.That(level, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    /// <summary>
    /// テスト用に初期武器自動付与設定を書き換えます。
    /// </summary>
    /// <param name="controller">設定対象のPlayerController</param>
    /// <param name="value">設定するフラグ値</param>
    static void SetGrantDefaultWeaponOnStart(PlayerController controller, bool value)
    {
        FieldInfo field = typeof(PlayerController).GetField(
            "_grantDefaultWeaponOnStart",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        field.SetValue(controller, value);
    }
}
