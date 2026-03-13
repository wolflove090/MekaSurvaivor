using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PirateCrewMemberControllerの移動方向を検証します。
/// </summary>
public class PirateCrewMemberControllerTests
{
    /// <summary>
    /// 初期化時に指定した方向へそのまま進むことを検証します。
    /// </summary>
    [Test]
    public void ResolveMovementDirection_ReturnsInitializedDirection()
    {
        GameObject crewObject = new GameObject("Crew");
        GameObject player = new GameObject("Player");

        try
        {
            PirateCrewRegistry pirateCrewRegistry = player.AddComponent<PirateCrewRegistry>();
            PirateCrewMemberController crew = crewObject.AddComponent<PirateCrewMemberController>();
            CharacterStatsData statsData = ScriptableObject.CreateInstance<CharacterStatsData>();
            statsData.SetValues(30, 1f, 0, 4f);

            crew.Initialize(player.transform, null, pirateCrewRegistry, statsData, Vector3.left, _ => { });

            Vector3 direction = crew.ResolveMovementDirection();

            Assert.That(direction.x, Is.EqualTo(-1f).Within(0.0001f));
            Assert.That(direction.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(direction.z, Is.EqualTo(0f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(crewObject);
        }
    }

    /// <summary>
    /// 指定した進行方向へそのまま前進することを検証します。
    /// </summary>
    [Test]
    public void TickMovement_MovesStraightAlongInitializedDirection()
    {
        GameObject player = new GameObject("Player");
        GameObject crewObject = new GameObject("Crew");

        try
        {
            PirateCrewRegistry pirateCrewRegistry = player.AddComponent<PirateCrewRegistry>();
            PirateCrewMemberController crew = crewObject.AddComponent<PirateCrewMemberController>();
            CharacterStatsData statsData = ScriptableObject.CreateInstance<CharacterStatsData>();
            statsData.SetValues(30, 1f, 0, 4f);
            crew.Initialize(player.transform, null, pirateCrewRegistry, statsData, Vector3.right, _ => { });

            crew.TickMovement(0.5f);

            Assert.That(crew.transform.position.x, Is.GreaterThan(0f));
            Assert.That(crew.transform.position.z, Is.EqualTo(0f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(crewObject);
        }
    }
}
