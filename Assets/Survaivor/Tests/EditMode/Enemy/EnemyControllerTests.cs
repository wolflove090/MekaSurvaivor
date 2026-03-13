using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EnemyControllerの優先ターゲット解決を検証します。
/// </summary>
public class EnemyControllerTests
{
    /// <summary>
    /// 戦闘員がいる間はプレイヤーより戦闘員を優先することを検証します。
    /// </summary>
    [Test]
    public void RefreshTarget_WithActiveCrew_PrioritizesNearestCrew()
    {
        GameObject enemyObject = new GameObject("Enemy");
        GameObject playerObject = new GameObject("Player");
        GameObject pirateCrewRegistryObject = new GameObject("PirateCrewRegistry");
        GameObject nearCrewObject = new GameObject("NearCrew");
        GameObject farCrewObject = new GameObject("FarCrew");

        try
        {
            EnemyController enemyController = enemyObject.AddComponent<EnemyController>();
            PirateCrewRegistry pirateCrewRegistry = pirateCrewRegistryObject.AddComponent<PirateCrewRegistry>();
            PirateCrewTarget nearTarget = nearCrewObject.AddComponent<PirateCrewTarget>();
            PirateCrewTarget farTarget = farCrewObject.AddComponent<PirateCrewTarget>();
            nearCrewObject.transform.position = new Vector3(1f, 0f, 0f);
            farCrewObject.transform.position = new Vector3(5f, 0f, 0f);
            pirateCrewRegistry.Register(nearTarget);
            pirateCrewRegistry.Register(farTarget);

            enemyController.SetTarget(playerObject.transform);
            enemyController.SetPirateCrewRegistry(pirateCrewRegistry);
            enemyController.RefreshTarget();

            Assert.That(enemyController.TargetTransform, Is.SameAs(nearCrewObject.transform));

            pirateCrewRegistry.Unregister(nearTarget);
            pirateCrewRegistry.Unregister(farTarget);
            enemyController.RefreshTarget();

            Assert.That(enemyController.TargetTransform, Is.SameAs(playerObject.transform));
        }
        finally
        {
            Object.DestroyImmediate(enemyObject);
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(pirateCrewRegistryObject);
            Object.DestroyImmediate(nearCrewObject);
            Object.DestroyImmediate(farCrewObject);
        }
    }

    /// <summary>
    /// 1体の戦闘員には最大3体までしか優先攻撃しないことを検証します。
    /// </summary>
    [Test]
    public void RefreshTarget_WithSingleCrew_AssignsAtMostThreeEnemies()
    {
        GameObject playerObject = new GameObject("Player");
        GameObject pirateCrewRegistryObject = new GameObject("PirateCrewRegistry");
        GameObject crewObject = new GameObject("Crew");
        GameObject enemyObject1 = new GameObject("Enemy1");
        GameObject enemyObject2 = new GameObject("Enemy2");
        GameObject enemyObject3 = new GameObject("Enemy3");
        GameObject enemyObject4 = new GameObject("Enemy4");

        try
        {
            PirateCrewRegistry pirateCrewRegistry = pirateCrewRegistryObject.AddComponent<PirateCrewRegistry>();
            PirateCrewTarget crewTarget = crewObject.AddComponent<PirateCrewTarget>();
            pirateCrewRegistry.Register(crewTarget);

            EnemyController enemyController1 = enemyObject1.AddComponent<EnemyController>();
            EnemyController enemyController2 = enemyObject2.AddComponent<EnemyController>();
            EnemyController enemyController3 = enemyObject3.AddComponent<EnemyController>();
            EnemyController enemyController4 = enemyObject4.AddComponent<EnemyController>();

            EnemyController[] enemies =
            {
                enemyController1,
                enemyController2,
                enemyController3,
                enemyController4
            };

            foreach (EnemyController enemy in enemies)
            {
                enemy.SetTarget(playerObject.transform);
                enemy.SetPirateCrewRegistry(pirateCrewRegistry);
                enemy.RefreshTarget();
            }

            Assert.That(enemyController1.TargetTransform, Is.SameAs(crewObject.transform));
            Assert.That(enemyController2.TargetTransform, Is.SameAs(crewObject.transform));
            Assert.That(enemyController3.TargetTransform, Is.SameAs(crewObject.transform));
            Assert.That(enemyController4.TargetTransform, Is.SameAs(playerObject.transform));
        }
        finally
        {
            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(pirateCrewRegistryObject);
            Object.DestroyImmediate(crewObject);
            Object.DestroyImmediate(enemyObject1);
            Object.DestroyImmediate(enemyObject2);
            Object.DestroyImmediate(enemyObject3);
            Object.DestroyImmediate(enemyObject4);
        }
    }
}
