using System;
using UnityEngine;

/// <summary>
/// シーン内で使用する局所通知ハブ。
/// </summary>
public class GameMessageBus
{
    /// <summary>
    /// プレイヤーがダメージを受けた時に発火します。
    /// </summary>
    public event Action<int> PlayerDamaged;

    /// <summary>
    /// 経験値を獲得した時に発火します。
    /// </summary>
    public event Action<int, int, int> ExperienceGained;

    /// <summary>
    /// プレイヤーがレベルアップした時に発火します。
    /// </summary>
    public event Action<int> PlayerLevelUp;

    /// <summary>
    /// プレイヤーが死亡した時に発火します。
    /// </summary>
    public event Action PlayerDied;

    /// <summary>
    /// ゲームオーバーになった時に発火します。
    /// </summary>
    public event Action GameOver;

    /// <summary>
    /// 敵が死亡した時に発火します。
    /// </summary>
    public event Action<GameObject> EnemyDied;

    /// <summary>
    /// プレイヤーダメージ通知を発火します。
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    public void RaisePlayerDamaged(int damage)
    {
        PlayerDamaged?.Invoke(damage);
    }

    /// <summary>
    /// 経験値獲得通知を発火します。
    /// </summary>
    /// <param name="gainedAmount">今回獲得した経験値量</param>
    /// <param name="currentExperience">現在経験値</param>
    /// <param name="experienceToNextLevel">次のレベルまでに必要な経験値</param>
    public void RaiseExperienceGained(int gainedAmount, int currentExperience, int experienceToNextLevel)
    {
        ExperienceGained?.Invoke(gainedAmount, currentExperience, experienceToNextLevel);
    }

    /// <summary>
    /// レベルアップ通知を発火します。
    /// </summary>
    /// <param name="newLevel">新しいレベル</param>
    public void RaisePlayerLevelUp(int newLevel)
    {
        PlayerLevelUp?.Invoke(newLevel);
    }

    /// <summary>
    /// プレイヤー死亡通知を発火します。
    /// </summary>
    public void RaisePlayerDied()
    {
        PlayerDied?.Invoke();
    }

    /// <summary>
    /// ゲームオーバー通知を発火します。
    /// </summary>
    public void RaiseGameOver()
    {
        GameOver?.Invoke();
    }

    /// <summary>
    /// 敵死亡通知を発火します。
    /// </summary>
    /// <param name="enemy">死亡した敵</param>
    public void RaiseEnemyDied(GameObject enemy)
    {
        EnemyDied?.Invoke(enemy);
    }
}
