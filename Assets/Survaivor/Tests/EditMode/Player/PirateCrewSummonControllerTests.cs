using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PirateCrewSummonControllerの再召喚とプール再利用を検証します。
/// </summary>
public class PirateCrewSummonControllerTests
{
    /// <summary>
    /// 再召喚時に前回の戦闘員を返却し、同じインスタンスを再利用することを検証します。
    /// </summary>
    [Test]
    public void ResummonCrew_ReusesPooledCrewMembers()
    {
        GameObject player = new GameObject("Player");
        GameObject enemyRegistryObject = new GameObject("EnemyRegistry");

        try
        {
            EnemyRegistry enemyRegistry = enemyRegistryObject.AddComponent<EnemyRegistry>();
            PirateCrewSummonController summonController = player.AddComponent<PirateCrewSummonController>();

            summonController.Configure(player.transform, enemyRegistry);
            summonController.ResummonCrew(Vector3.zero, Vector3.right);
            PirateCrewMemberController[] firstCrew = summonController.ActiveCrewMembers.ToArray();

            summonController.ResummonCrew(Vector3.zero, Vector3.right);
            PirateCrewMemberController[] secondCrew = summonController.ActiveCrewMembers.ToArray();

            Assert.That(firstCrew.Length, Is.EqualTo(2));
            Assert.That(secondCrew.Length, Is.EqualTo(2));
            Assert.That(firstCrew.All(member => member != null), Is.True);
            Assert.That(secondCrew.All(member => member != null), Is.True);
            Assert.That(firstCrew.Select(member => member.GetInstanceID()).OrderBy(id => id),
                Is.EqualTo(secondCrew.Select(member => member.GetInstanceID()).OrderBy(id => id)));
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemyRegistryObject);
        }
    }

    /// <summary>
    /// プレイヤー前方と右方向に1体ずつ出現し、その方向へ進むことを検証します。
    /// </summary>
    [Test]
    public void ResummonCrew_SpawnsOneForwardAndOneRight()
    {
        GameObject player = new GameObject("Player");
        GameObject enemyRegistryObject = new GameObject("EnemyRegistry");

        try
        {
            EnemyRegistry enemyRegistry = enemyRegistryObject.AddComponent<EnemyRegistry>();
            PirateCrewSummonController summonController = player.AddComponent<PirateCrewSummonController>();

            summonController.Configure(player.transform, enemyRegistry);
            summonController.ResummonCrew(Vector3.zero, Vector3.forward);

            PirateCrewMemberController[] crew = summonController.ActiveCrewMembers.ToArray();

            Assert.That(crew.Length, Is.EqualTo(2));
            Assert.That(crew[0].transform.position, Is.EqualTo(new Vector3(0f, 0f, 1.5f)));
            Assert.That(crew[1].transform.position, Is.EqualTo(new Vector3(1.5f, 0f, 0f)));
            Assert.That(crew[0].ResolveMovementDirection(), Is.EqualTo(Vector3.forward));
            Assert.That(crew[1].ResolveMovementDirection(), Is.EqualTo(Vector3.right));
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemyRegistryObject);
        }
    }
}
