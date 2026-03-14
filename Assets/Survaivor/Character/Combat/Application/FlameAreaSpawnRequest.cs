using UnityEngine;

/// <summary>
/// 炎エリア生成要求を表します。
/// </summary>
public sealed class FlameAreaSpawnRequest : WeaponFireRequest
{
    /// <summary>
    /// 炎エリアの持続時間を取得します。
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// 炎エリアの半径を取得します。
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// 継続ダメージ間隔を取得します。
    /// </summary>
    public float TickInterval { get; }

    /// <summary>
    /// 炎エリア生成要求を初期化します。
    /// </summary>
    /// <param name="origin">生成位置</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="duration">持続時間</param>
    /// <param name="radius">半径</param>
    /// <param name="tickInterval">継続ダメージ間隔</param>
    public FlameAreaSpawnRequest(
        Vector3 origin,
        float sourcePow,
        float duration,
        float radius,
        float tickInterval = 0.5f) : base(origin, sourcePow)
    {
        Duration = duration;
        Radius = radius;
        TickInterval = tickInterval;
    }
}
