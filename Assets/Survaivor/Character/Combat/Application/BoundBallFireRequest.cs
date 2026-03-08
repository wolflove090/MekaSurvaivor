using UnityEngine;

/// <summary>
/// バウンドボールの発射要求を表します。
/// </summary>
public sealed class BoundBallFireRequest : WeaponFireRequest
{
    /// <summary>
    /// 初期進行方向を取得します。
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    /// 最大バウンド回数を取得します。
    /// </summary>
    public int MaxBounceCount { get; }

    /// <summary>
    /// バウンドボールの発射要求を初期化します。
    /// </summary>
    /// <param name="origin">発動元の位置</param>
    /// <param name="direction">発射方向</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="maxBounceCount">最大バウンド回数</param>
    public BoundBallFireRequest(Vector3 origin, Vector3 direction, float sourcePow, int maxBounceCount) : base(origin, sourcePow)
    {
        Direction = direction;
        MaxBounceCount = maxBounceCount;
    }
}
