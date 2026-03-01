using UnityEngine;

/// <summary>
/// 敵スポーン制御のランタイム状態を保持します。
/// </summary>
public class EnemySpawnState
{
    /// <summary>
    /// スポーン間隔を取得します。
    /// </summary>
    public float SpawnInterval { get; private set; }

    /// <summary>
    /// スポーン距離の最小値を取得します。
    /// </summary>
    public float SpawnDistanceMin { get; private set; }

    /// <summary>
    /// スポーン距離の最大値を取得します。
    /// </summary>
    public float SpawnDistanceMax { get; private set; }

    /// <summary>
    /// 次回スポーンまでの残り時間を取得します。
    /// </summary>
    public float SpawnTimer { get; private set; }

    /// <summary>
    /// 敵スポーン状態を初期化します。
    /// </summary>
    /// <param name="spawnInterval">スポーン間隔</param>
    /// <param name="spawnDistanceMin">スポーン距離の最小値</param>
    /// <param name="spawnDistanceMax">スポーン距離の最大値</param>
    public EnemySpawnState(float spawnInterval, float spawnDistanceMin, float spawnDistanceMax)
    {
        SetSpawnInterval(spawnInterval);
        SetSpawnDistanceRange(spawnDistanceMin, spawnDistanceMax);
        ResetSpawnTimer();
    }

    /// <summary>
    /// スポーン間隔を更新します。
    /// </summary>
    /// <param name="spawnInterval">新しいスポーン間隔</param>
    public void SetSpawnInterval(float spawnInterval)
    {
        SpawnInterval = Mathf.Max(0.01f, spawnInterval);
        SpawnTimer = Mathf.Min(SpawnTimer, SpawnInterval);
    }

    /// <summary>
    /// スポーン距離の範囲を更新します。
    /// </summary>
    /// <param name="spawnDistanceMin">スポーン距離の最小値</param>
    /// <param name="spawnDistanceMax">スポーン距離の最大値</param>
    public void SetSpawnDistanceRange(float spawnDistanceMin, float spawnDistanceMax)
    {
        SpawnDistanceMin = Mathf.Max(0f, spawnDistanceMin);
        SpawnDistanceMax = Mathf.Max(SpawnDistanceMin, spawnDistanceMax);
    }

    /// <summary>
    /// 指定時間だけスポーンタイマーを進行します。
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    public void Advance(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        SpawnTimer -= deltaTime;
    }

    /// <summary>
    /// スポーン実行タイミングかどうかを取得します。
    /// </summary>
    public bool ShouldSpawn()
    {
        return SpawnTimer <= 0f;
    }

    /// <summary>
    /// スポーンタイマーを次回分へ戻します。
    /// </summary>
    public void ResetSpawnTimer()
    {
        SpawnTimer = SpawnInterval;
    }
}
