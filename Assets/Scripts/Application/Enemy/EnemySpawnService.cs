using System;
using UnityEngine;

/// <summary>
/// 敵スポーンのタイマー進行とスポーン要求生成を管理します。
/// </summary>
public class EnemySpawnService
{
    /// <summary>
    /// 敵スポーン状態を取得します。
    /// </summary>
    public EnemySpawnState State { get; }

    /// <summary>
    /// 敵スポーンサービスを初期化します。
    /// </summary>
    /// <param name="state">対象のスポーン状態</param>
    public EnemySpawnService(EnemySpawnState state)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    /// <summary>
    /// スポーン間隔を更新します。
    /// </summary>
    /// <param name="spawnInterval">新しいスポーン間隔</param>
    public void UpdateSpawnInterval(float spawnInterval)
    {
        State.SetSpawnInterval(spawnInterval);
    }

    /// <summary>
    /// スポーンタイマーを更新し、スポーン要求の有無を返します。
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    /// <returns>スポーン要求が発生した場合はtrue</returns>
    public bool Tick(float deltaTime)
    {
        State.Advance(deltaTime);

        if (!State.ShouldSpawn())
        {
            return false;
        }

        State.ResetSpawnTimer();
        return true;
    }

    /// <summary>
    /// スポーンタイマー更新後に、必要ならスポーン位置を生成します。
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    /// <param name="targetPosition">スポーン基準の位置</param>
    /// <param name="randomRange">乱数取得関数</param>
    /// <param name="spawnPosition">生成されたスポーン位置</param>
    /// <returns>スポーン要求が発生した場合はtrue</returns>
    public bool TryCreateSpawnRequest(
        float deltaTime,
        Vector3 targetPosition,
        Func<float, float, float> randomRange,
        out Vector3 spawnPosition)
    {
        spawnPosition = targetPosition;

        if (!Tick(deltaTime))
        {
            return false;
        }

        spawnPosition = CalculateSpawnPosition(targetPosition, randomRange);
        return true;
    }

    /// <summary>
    /// ターゲット位置の周囲にスポーン位置を計算します。
    /// </summary>
    /// <param name="targetPosition">スポーン基準の位置</param>
    /// <param name="randomRange">乱数取得関数</param>
    /// <returns>計算したスポーン位置</returns>
    public Vector3 CalculateSpawnPosition(Vector3 targetPosition, Func<float, float, float> randomRange)
    {
        if (randomRange == null)
        {
            throw new ArgumentNullException(nameof(randomRange));
        }

        float angle = randomRange(0f, 360f) * Mathf.Deg2Rad;
        float distance = randomRange(State.SpawnDistanceMin, State.SpawnDistanceMax);
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;

        return new Vector3(
            targetPosition.x + x,
            targetPosition.y,
            targetPosition.z + z);
    }
}
