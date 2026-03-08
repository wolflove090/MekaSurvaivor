using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// StyleChangeUiController の候補抽選ロジックを検証します。
/// </summary>
public class StyleChangeUiControllerTests
{
    /// <summary>
    /// 候補抽選が最大3件の一意なスタイル種別を返すことを検証します。
    /// </summary>
    [Test]
    public void BuildRandomizedCandidates_ReturnsThreeUniqueEntries()
    {
        IReadOnlyList<StyleChangeUiController.StyleCardType> source = new[]
        {
            StyleChangeUiController.StyleCardType.Miko,
            StyleChangeUiController.StyleCardType.Maid,
            StyleChangeUiController.StyleCardType.Nurse,
            StyleChangeUiController.StyleCardType.Pirate,
            StyleChangeUiController.StyleCardType.Cowgirl,
            StyleChangeUiController.StyleCardType.Celeb
        };

        IReadOnlyList<StyleChangeUiController.StyleCardType> result =
            StyleChangeUiController.BuildRandomizedCandidates(source);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.Distinct().Count(), Is.EqualTo(result.Count));
        Assert.That(result.All(source.Contains), Is.True);
    }
}
