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
    /// カウガールへ切り替えた直後に既存武器の残りクールダウンが短縮後間隔へ補正されることを検証します。
    /// </summary>
    [Test]
    public void ChangeStyle_WhenSwitchingToCowgirl_ClampsExistingWeaponCooldownImmediately()
    {
        GameObject player = new GameObject("Player");
        player.SetActive(false);

        try
        {
            player.AddComponent<PlayerExperience>();
            PlayerController controller = player.AddComponent<PlayerController>();

            player.SetActive(true);

            WeaponBase weapon = GetPrivateField<WeaponBase>(controller, "_weapon");
            weapon.Tick(0f);

            WeaponState weaponState = GetPrivateField<WeaponState>(weapon, "_weaponState", typeof(WeaponBase));
            Assert.That(weaponState.CooldownRemaining, Is.EqualTo(1.5f).Within(0.0001f));

            controller.ChangeStyle(PlayerStyleType.Cowgirl);

            Assert.That(weaponState.CooldownRemaining, Is.EqualTo(1.125f).Within(0.0001f));
            Assert.That(controller.GetComponent<PlayerExperience>().State.AttackIntervalMultiplier, Is.EqualTo(0.75f));
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

    /// <summary>
    /// privateフィールド値を取得します。
    /// </summary>
    /// <typeparam name="T">取得する型</typeparam>
    /// <param name="instance">取得対象インスタンス</param>
    /// <param name="fieldName">フィールド名</param>
    /// <param name="declaringType">フィールド宣言元型</param>
    /// <returns>取得したフィールド値</returns>
    static T GetPrivateField<T>(object instance, string fieldName, System.Type declaringType = null)
    {
        System.Type targetType = declaringType ?? instance.GetType();
        FieldInfo field = targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"Field not found: {fieldName}");
        return (T)field.GetValue(instance);
    }
}
