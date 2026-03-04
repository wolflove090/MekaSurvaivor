using UnityEngine;

/// <summary>
/// ドローン展開要求を表します。
/// </summary>
public sealed class DroneSpawnRequest : WeaponFireRequest
{
    /// <summary>
    /// 追従対象を取得します。
    /// </summary>
    public Transform FollowTarget { get; }

    /// <summary>
    /// プレイヤー周囲の周回半径を取得します。
    /// </summary>
    public float OrbitRadius { get; }

    /// <summary>
    /// ドローン内部の攻撃間隔を取得します。
    /// </summary>
    public float ShotInterval { get; }

    /// <summary>
    /// 周回開始時の位相オフセット角度を取得します。
    /// </summary>
    public float PhaseOffsetDegrees { get; }

    /// <summary>
    /// ドローン展開要求を初期化します。
    /// </summary>
    /// <param name="origin">展開位置</param>
    /// <param name="followTarget">追従対象</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="orbitRadius">周回半径</param>
    /// <param name="shotInterval">攻撃間隔</param>
    /// <param name="phaseOffsetDegrees">周回開始位相のオフセット角度</param>
    public DroneSpawnRequest(
        Vector3 origin,
        Transform followTarget,
        int sourcePow,
        float orbitRadius,
        float shotInterval,
        float phaseOffsetDegrees) : base(origin, sourcePow)
    {
        FollowTarget = followTarget;
        OrbitRadius = orbitRadius;
        ShotInterval = shotInterval;
        PhaseOffsetDegrees = phaseOffsetDegrees;
    }
}
