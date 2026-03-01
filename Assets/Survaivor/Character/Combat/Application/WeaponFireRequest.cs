using UnityEngine;

/// <summary>
/// 武器発動要求の共通データを表します。
/// </summary>
public abstract class WeaponFireRequest
{
    /// <summary>
    /// 発動元の位置を取得します。
    /// </summary>
    public Vector3 Origin { get; }

    /// <summary>
    /// 攻撃元の攻撃力を取得します。
    /// </summary>
    public int SourcePow { get; }

    /// <summary>
    /// 武器発動要求を初期化します。
    /// </summary>
    /// <param name="origin">発動元の位置</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    protected WeaponFireRequest(Vector3 origin, int sourcePow)
    {
        Origin = origin;
        SourcePow = sourcePow;
    }
}
