using System;
using UnityEngine;

/// <summary>
/// 1プレイ中の統計値を集計します。
/// </summary>
public class GameStatsTracker : IDisposable
{
    readonly GameMessageBus _messageBus;

    /// <summary>
    /// 現在プレイ中の撃破数です。
    /// </summary>
    public int KillCount { get; private set; }

    /// <summary>
    /// メッセージバス購読を開始します。
    /// </summary>
    /// <param name="messageBus">購読対象の通知ハブ</param>
    public GameStatsTracker(GameMessageBus messageBus)
    {
        _messageBus = messageBus;
        Reset();

        if (_messageBus != null)
        {
            _messageBus.EnemyDied += OnEnemyDied;
        }
    }

    /// <summary>
    /// 統計値を初期値へ戻します。
    /// </summary>
    public void Reset()
    {
        KillCount = 0;
    }

    /// <summary>
    /// 購読を解除します。
    /// </summary>
    public void Dispose()
    {
        if (_messageBus != null)
        {
            _messageBus.EnemyDied -= OnEnemyDied;
        }
    }

    /// <summary>
    /// 敵撃破時に撃破数を加算します。
    /// </summary>
    /// <param name="enemy">死亡した敵</param>
    void OnEnemyDied(GameObject enemy)
    {
        KillCount++;
    }
}
