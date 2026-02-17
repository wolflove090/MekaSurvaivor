using UnityEngine;

/// <summary>
/// エネミーの行動ロジックを定義するインターフェース
/// </summary>
public interface IEnemyBehavior
{
    /// <summary>
    /// エネミーの行動を実行します
    /// </summary>
    /// <param name="enemy">行動対象のエネミー</param>
    void Execute(EnemyController enemy);
}
