using NUnit.Framework;
using UnityEngine;

/// <summary>
/// EnemySpawnServiceのスポーン要求生成を検証します。
/// </summary>
public class EnemySpawnServiceTests
{
    /// <summary>
    /// スポーン間隔経過前は要求を返さないことを検証します。
    /// </summary>
    [Test]
    public void TryCreateSpawnRequest_BeforeIntervalElapsed_ReturnsFalse()
    {
        EnemySpawnService service = new EnemySpawnService(new EnemySpawnState(2f, 10f, 15f));

        bool shouldSpawn = service.TryCreateSpawnRequest(0.5f, Vector3.zero, (_, _) => 0f, out Vector3 spawnPosition);

        Assert.That(shouldSpawn, Is.False);
        Assert.That(spawnPosition, Is.EqualTo(Vector3.zero));
        Assert.That(service.State.SpawnTimer, Is.EqualTo(1.5f));
    }

    /// <summary>
    /// スポーン間隔経過後は要求と計算済み位置を返すことを検証します。
    /// </summary>
    [Test]
    public void TryCreateSpawnRequest_AfterIntervalElapsed_ReturnsPosition()
    {
        EnemySpawnService service = new EnemySpawnService(new EnemySpawnState(1f, 10f, 15f));
        Vector3 origin = new Vector3(3f, 2f, -4f);
        int callCount = 0;

        bool shouldSpawn = service.TryCreateSpawnRequest(
            1f,
            origin,
            (min, max) =>
            {
                callCount++;
                return callCount == 1 ? 0f : 12f;
            },
            out Vector3 spawnPosition);

        Assert.That(shouldSpawn, Is.True);
        Assert.That(spawnPosition.x, Is.EqualTo(15f));
        Assert.That(spawnPosition.y, Is.EqualTo(2f));
        Assert.That(spawnPosition.z, Is.EqualTo(-4f));
        Assert.That(service.State.SpawnTimer, Is.EqualTo(1f));
    }
}
