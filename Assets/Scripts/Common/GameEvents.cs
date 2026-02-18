using UnityEngine;

/// <summary>
/// ゲーム全体で使用する静的イベントシステム
/// </summary>
public static class GameEvents
{
    // プレイヤー関連イベント

    /// <summary>
    /// プレイヤーが移動した時に発火するイベント
    /// </summary>
    public static event System.Action<Vector3> OnPlayerMoved;

    /// <summary>
    /// プレイヤーがダメージを受けた時に発火するイベント
    /// </summary>
    public static event System.Action<int> OnPlayerDamaged;

    /// <summary>
    /// プレイヤーが死亡した時に発火するイベント
    /// </summary>
    public static event System.Action OnPlayerDied;

    /// <summary>
    /// プレイヤーが経験値を獲得した時に発火するイベント
    /// </summary>
    public static event System.Action<int, int, int> OnExperienceGained;

    /// <summary>
    /// プレイヤーがレベルアップした時に発火するイベント
    /// </summary>
    public static event System.Action<int> OnPlayerLevelUp;

    // 敵関連イベント

    /// <summary>
    /// 敵がスポーンした時に発火するイベント
    /// </summary>
    public static event System.Action<GameObject> OnEnemySpawned;

    /// <summary>
    /// 敵が死亡した時に発火するイベント
    /// </summary>
    public static event System.Action<GameObject> OnEnemyDied;

    // ゲーム状態イベント

    /// <summary>
    /// ゲームオーバーになった時に発火するイベント
    /// </summary>
    public static event System.Action OnGameOver;

    // イベント発火メソッド

    /// <summary>
    /// プレイヤー移動イベントを発火します
    /// </summary>
    /// <param name="position">プレイヤーの位置</param>
    public static void RaisePlayerMoved(Vector3 position)
    {
        OnPlayerMoved?.Invoke(position);
    }

    /// <summary>
    /// プレイヤーダメージイベントを発火します
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    public static void RaisePlayerDamaged(int damage)
    {
        OnPlayerDamaged?.Invoke(damage);
    }

    /// <summary>
    /// プレイヤー死亡イベントを発火します
    /// </summary>
    public static void RaisePlayerDied()
    {
        OnPlayerDied?.Invoke();
    }

    /// <summary>
    /// 経験値獲得イベントを発火します
    /// </summary>
    /// <param name="gainedAmount">獲得した経験値量</param>
    /// <param name="currentExperience">現在の経験値</param>
    /// <param name="experienceToNextLevel">次のレベルまでに必要な経験値</param>
    public static void RaiseExperienceGained(int gainedAmount, int currentExperience, int experienceToNextLevel)
    {
        OnExperienceGained?.Invoke(gainedAmount, currentExperience, experienceToNextLevel);
    }

    /// <summary>
    /// プレイヤーレベルアップイベントを発火します
    /// </summary>
    /// <param name="newLevel">新しいレベル</param>
    public static void RaisePlayerLevelUp(int newLevel)
    {
        OnPlayerLevelUp?.Invoke(newLevel);
    }

    /// <summary>
    /// 敵スポーンイベントを発火します
    /// </summary>
    /// <param name="enemy">スポーンした敵のGameObject</param>
    public static void RaiseEnemySpawned(GameObject enemy)
    {
        OnEnemySpawned?.Invoke(enemy);
    }

    /// <summary>
    /// 敵死亡イベントを発火します
    /// </summary>
    /// <param name="enemy">死亡した敵のGameObject</param>
    public static void RaiseEnemyDied(GameObject enemy)
    {
        OnEnemyDied?.Invoke(enemy);
    }

    /// <summary>
    /// ゲームオーバーイベントを発火します
    /// </summary>
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// 全てのイベントリスナーをクリアします（シーン遷移時などに使用）
    /// </summary>
    public static void ClearAllEvents()
    {
        OnPlayerMoved = null;
        OnPlayerDamaged = null;
        OnPlayerDied = null;
        OnExperienceGained = null;
        OnPlayerLevelUp = null;
        OnEnemySpawned = null;
        OnEnemyDied = null;
        OnGameOver = null;
    }
}
