using UnityEngine;

/// <summary>
/// ターゲットを追跡するエネミー行動
/// </summary>
public class ChasePlayerBehavior : IEnemyBehavior
{
    /// <summary>
    /// ターゲットに向かって移動します
    /// </summary>
    /// <param name="enemy">行動対象のエネミー</param>
    public void Execute(EnemyController enemy)
    {
        if (enemy == null || enemy.TargetTransform == null)
        {
            return;
        }

        Vector3 direction = enemy.TargetTransform.position - enemy.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        enemy.MoveInDirection(direction.normalized);
    }
}
