using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アクティブな海賊戦闘員の登録管理と近傍探索を担当します
/// </summary>
public class PirateCrewRegistry : MonoBehaviour
{
    const int MaxEnemiesPerCrew = 3;

    readonly List<PirateCrewTarget> _activeTargets = new List<PirateCrewTarget>();
    readonly Dictionary<EnemyController, PirateCrewTarget> _enemyAssignments = new Dictionary<EnemyController, PirateCrewTarget>();
    readonly Dictionary<PirateCrewTarget, int> _assignmentCounts = new Dictionary<PirateCrewTarget, int>();

    /// <summary>
    /// アクティブな戦闘員数を取得します
    /// </summary>
    public int ActiveCount
    {
        get
        {
            CleanupState();
            return _activeTargets.Count;
        }
    }

    /// <summary>
    /// 戦闘員ターゲットを登録します
    /// </summary>
    /// <param name="target">登録対象</param>
    public void Register(PirateCrewTarget target)
    {
        if (target == null || _activeTargets.Contains(target))
        {
            return;
        }

        _activeTargets.Add(target);
        if (!_assignmentCounts.ContainsKey(target))
        {
            _assignmentCounts[target] = 0;
        }
    }

    /// <summary>
    /// 戦闘員ターゲットを登録解除します
    /// </summary>
    /// <param name="target">登録解除対象</param>
    public void Unregister(PirateCrewTarget target)
    {
        if (target == null)
        {
            return;
        }

        _activeTargets.Remove(target);
        ReleaseAssignmentsForTarget(target);
        _assignmentCounts.Remove(target);
    }

    /// <summary>
    /// 指定敵へ優先追跡させる戦闘員を返します
    /// </summary>
    /// <param name="enemy">割り当て対象の敵</param>
    /// <param name="position">探索基準位置</param>
    /// <returns>割り当てられた戦闘員Transform。存在しない場合はnull</returns>
    public Transform FindPreferredCrewTarget(EnemyController enemy, Vector3 position)
    {
        CleanupState();

        if (enemy == null)
        {
            return null;
        }

        if (_enemyAssignments.TryGetValue(enemy, out PirateCrewTarget assignedTarget) &&
            assignedTarget != null &&
            assignedTarget.IsAvailable &&
            _activeTargets.Contains(assignedTarget))
        {
            return assignedTarget.transform;
        }

        ReleaseEnemyAssignment(enemy);

        PirateCrewTarget nearestTarget = null;
        float nearestDistanceSq = float.MaxValue;

        foreach (PirateCrewTarget target in _activeTargets)
        {
            if (GetAssignmentCount(target) >= MaxEnemiesPerCrew)
            {
                continue;
            }

            float distanceSq = (target.transform.position - position).sqrMagnitude;
            if (distanceSq < nearestDistanceSq)
            {
                nearestDistanceSq = distanceSq;
                nearestTarget = target;
            }
        }

        if (nearestTarget == null)
        {
            return null;
        }

        _enemyAssignments[enemy] = nearestTarget;
        _assignmentCounts[nearestTarget] = GetAssignmentCount(nearestTarget) + 1;
        return nearestTarget.transform;
    }

    /// <summary>
    /// 指定敵の戦闘員割り当てを解除します
    /// </summary>
    /// <param name="enemy">解除対象の敵</param>
    public void ReleaseEnemyAssignment(EnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!_enemyAssignments.TryGetValue(enemy, out PirateCrewTarget assignedTarget))
        {
            return;
        }

        _enemyAssignments.Remove(enemy);
        if (assignedTarget != null && _assignmentCounts.ContainsKey(assignedTarget))
        {
            _assignmentCounts[assignedTarget] = Mathf.Max(0, _assignmentCounts[assignedTarget] - 1);
        }
    }

    /// <summary>
    /// 無効なターゲットと割り当てを整理します
    /// </summary>
    void CleanupState()
    {
        for (int i = _activeTargets.Count - 1; i >= 0; i--)
        {
            PirateCrewTarget target = _activeTargets[i];
            if (target == null || !target.IsAvailable)
            {
                ReleaseAssignmentsForTargetInternal(target);
                _assignmentCounts.Remove(target);
                _activeTargets.RemoveAt(i);
            }
        }

        List<KeyValuePair<EnemyController, PirateCrewTarget>> invalidEnemies = null;
        foreach (KeyValuePair<EnemyController, PirateCrewTarget> pair in _enemyAssignments)
        {
            if (pair.Key == null)
            {
                invalidEnemies ??= new List<KeyValuePair<EnemyController, PirateCrewTarget>>();
                invalidEnemies.Add(pair);
            }
        }

        if (invalidEnemies == null)
        {
            return;
        }

        foreach (KeyValuePair<EnemyController, PirateCrewTarget> pair in invalidEnemies)
        {
            _enemyAssignments.Remove(pair.Key);
            if (pair.Value != null && _assignmentCounts.ContainsKey(pair.Value))
            {
                _assignmentCounts[pair.Value] = Mathf.Max(0, _assignmentCounts[pair.Value] - 1);
            }
        }
    }

    /// <summary>
    /// 指定ターゲットに割り当てられた敵をすべて解除します
    /// </summary>
    /// <param name="target">解除対象ターゲット</param>
    void ReleaseAssignmentsForTarget(PirateCrewTarget target)
    {
        ReleaseAssignmentsForTargetInternal(target);
    }

    /// <summary>
    /// 指定ターゲットに割り当てられた敵をすべて解除します
    /// </summary>
    /// <param name="target">解除対象ターゲット</param>
    void ReleaseAssignmentsForTargetInternal(PirateCrewTarget target)
    {
        List<EnemyController> enemiesToRelease = null;
        foreach (KeyValuePair<EnemyController, PirateCrewTarget> pair in _enemyAssignments)
        {
            if (pair.Value != target)
            {
                continue;
            }

            enemiesToRelease ??= new List<EnemyController>();
            enemiesToRelease.Add(pair.Key);
        }

        if (enemiesToRelease == null)
        {
            return;
        }

        foreach (EnemyController enemy in enemiesToRelease)
        {
            ReleaseEnemyAssignment(enemy);
        }
    }

    /// <summary>
    /// 指定ターゲットの割り当て数を取得します
    /// </summary>
    /// <param name="target">対象ターゲット</param>
    /// <returns>現在の割り当て数</returns>
    int GetAssignmentCount(PirateCrewTarget target)
    {
        if (target == null)
        {
            return 0;
        }

        return _assignmentCounts.TryGetValue(target, out int count) ? count : 0;
    }
}
