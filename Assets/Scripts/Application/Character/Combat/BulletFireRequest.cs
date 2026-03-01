using UnityEngine;

/// <summary>
/// 通常弾の発射要求を表します。
/// </summary>
public sealed class BulletFireRequest : WeaponFireRequest
{
    /// <summary>
    /// 発射方向を取得します。
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    /// 通常弾の発射要求を初期化します。
    /// </summary>
    /// <param name="origin">発動元の位置</param>
    /// <param name="direction">発射方向</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    public BulletFireRequest(Vector3 origin, Vector3 direction, int sourcePow) : base(origin, sourcePow)
    {
        Direction = direction;
    }
}
