using NUnit.Framework;
using UnityEngine;

/// <summary>
/// SandboxDummyEnemyの無敵化と登録挙動を検証します。
/// </summary>
public class SandboxDummyEnemyTests
{
    /// <summary>
    /// ダメージを受けてもHPが減少しないことを検証します。
    /// </summary>
    [Test]
    public void TakeDamage_WhenSandboxDummyIsAttached_DoesNotReduceHp()
    {
        GameObject enemy = new GameObject("SandboxDummy");

        try
        {
            enemy.AddComponent<CharacterStats>();
            HealthComponent healthComponent = enemy.AddComponent<HealthComponent>();
            enemy.AddComponent<SandboxDummyEnemy>();

            int hpBeforeDamage = healthComponent.CurrentHp;

            healthComponent.TakeDamage(10, Vector3.right);

            Assert.That(healthComponent.CurrentHp, Is.EqualTo(hpBeforeDamage));
            Assert.That(healthComponent.IsDead, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
        }
    }

    /// <summary>
    /// 有効化時に敵レジストリへ登録され、無効化時に外れることを検証します。
    /// </summary>
    [Test]
    public void OnEnableAndDisable_RegistersAndUnregistersFromEnemyRegistry()
    {
        GameObject registryObject = new GameObject("EnemyRegistry");
        GameObject enemy = new GameObject("SandboxDummy");

        try
        {
            EnemyRegistry registry = registryObject.AddComponent<EnemyRegistry>();
            enemy.SetActive(false);
            enemy.AddComponent<SandboxDummyEnemy>();
            enemy.transform.position = new Vector3(3f, 0f, 0f);

            enemy.SetActive(true);
            GameObject registeredEnemy = registry.FindNearestEnemy(Vector3.zero);

            Assert.That(registeredEnemy, Is.SameAs(enemy));

            enemy.SetActive(false);
            GameObject unregisteredEnemy = registry.FindNearestEnemy(Vector3.zero);

            Assert.That(unregisteredEnemy, Is.Null);
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(registryObject);
        }
    }
}
