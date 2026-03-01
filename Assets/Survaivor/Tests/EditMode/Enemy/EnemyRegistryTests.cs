using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EnemyRegistryの登録管理と近傍探索を検証します。
/// </summary>
public class EnemyRegistryTests
{
    /// <summary>
    /// 近い敵を正しく返すことを検証します。
    /// </summary>
    [Test]
    public void FindNearestEnemy_ReturnsClosestRegisteredEnemy()
    {
        GameObject registryObject = new GameObject("EnemyRegistry");
        EnemyRegistry registry = registryObject.AddComponent<EnemyRegistry>();
        GameObject nearEnemy = new GameObject("NearEnemy");
        GameObject farEnemy = new GameObject("FarEnemy");

        try
        {
            nearEnemy.transform.position = new Vector3(2f, 0f, 0f);
            farEnemy.transform.position = new Vector3(8f, 0f, 0f);
            registry.RegisterEnemy(farEnemy);
            registry.RegisterEnemy(nearEnemy);

            GameObject result = registry.FindNearestEnemy(Vector3.zero);

            Assert.That(result, Is.SameAs(nearEnemy));
        }
        finally
        {
            Object.DestroyImmediate(nearEnemy);
            Object.DestroyImmediate(farEnemy);
            Object.DestroyImmediate(registryObject);
        }
    }

    /// <summary>
    /// 登録解除した敵は探索対象から外れることを検証します。
    /// </summary>
    [Test]
    public void UnregisterEnemy_RemovesEnemyFromSearchResults()
    {
        GameObject registryObject = new GameObject("EnemyRegistry");
        EnemyRegistry registry = registryObject.AddComponent<EnemyRegistry>();
        GameObject enemy = new GameObject("Enemy");

        try
        {
            enemy.transform.position = new Vector3(1f, 0f, 0f);
            registry.RegisterEnemy(enemy);
            registry.UnregisterEnemy(enemy);

            GameObject result = registry.FindNearestEnemy(Vector3.zero);

            Assert.That(result, Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(registryObject);
        }
    }
}
