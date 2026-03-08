using UnityEngine;

/// <summary>
/// 火炎瓶の発射要求を表します。
/// </summary>
public sealed class FlameBottleFireRequest : WeaponFireRequest
{
    /// <summary>
    /// 初速ベクトルを取得します。
    /// </summary>
    public Vector3 InitialVelocity { get; }

    /// <summary>
    /// 着地判定に使用する地面のY座標を取得します。
    /// </summary>
    public float GroundY { get; }

    /// <summary>
    /// 生成する炎エリアの持続時間を取得します。
    /// </summary>
    public float FlameDuration { get; }

    /// <summary>
    /// 生成する炎エリアの半径を取得します。
    /// </summary>
    public float FlameRadius { get; }

    /// <summary>
    /// 火炎瓶の発射要求を初期化します。
    /// </summary>
    /// <param name="origin">発動元の位置</param>
    /// <param name="initialVelocity">初速ベクトル</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="groundY">着地判定に使う地面のY座標</param>
    /// <param name="flameDuration">炎エリア持続時間</param>
    /// <param name="flameRadius">炎エリア半径</param>
    public FlameBottleFireRequest(
        Vector3 origin,
        Vector3 initialVelocity,
        float sourcePow,
        float groundY,
        float flameDuration,
        float flameRadius) : base(origin, sourcePow)
    {
        InitialVelocity = initialVelocity;
        GroundY = groundY;
        FlameDuration = flameDuration;
        FlameRadius = flameRadius;
    }
}
