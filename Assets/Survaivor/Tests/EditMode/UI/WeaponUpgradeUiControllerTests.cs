using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// WeaponUpgradeUiController の候補抽選ロジックを検証します。
/// </summary>
public class WeaponUpgradeUiControllerTests
{
    /// <summary>
    /// 候補抽選が最大3件の一意な武器種別を返すことを検証します。
    /// </summary>
    [Test]
    public void BuildRandomizedCandidates_ReturnsUpToThreeUniqueEntries()
    {
        WeaponUpgradeUiController.UpgradeCardType[] source =
        {
            WeaponUpgradeUiController.UpgradeCardType.Shooter,
            WeaponUpgradeUiController.UpgradeCardType.Throwing,
            WeaponUpgradeUiController.UpgradeCardType.DamageField,
            WeaponUpgradeUiController.UpgradeCardType.Drone,
            WeaponUpgradeUiController.UpgradeCardType.BoundBall,
            WeaponUpgradeUiController.UpgradeCardType.FlameBottle
        };

        IReadOnlyList<WeaponUpgradeUiController.UpgradeCardType> result =
            WeaponUpgradeUiController.BuildRandomizedCandidates(source);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Distinct().Count(), Is.EqualTo(result.Count));
        Assert.That(result.All(source.Contains), Is.True);
    }
}
