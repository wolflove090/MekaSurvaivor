using UnityEngine;

/// <summary>
/// ダメージフィールドの生成要求を表します。
/// </summary>
public sealed class DamageFieldSpawnRequest : WeaponFireRequest
{
    /// <summary>
    /// 追従対象を取得します。
    /// </summary>
    public Transform FollowTarget { get; }

    /// <summary>
    /// 適用するエリア倍率を取得します。
    /// </summary>
    public float AreaScale { get; }

    /// <summary>
    /// ダメージフィールド生成要求を初期化します。
    /// </summary>
    /// <param name="origin">生成位置</param>
    /// <param name="followTarget">追従対象</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="areaScale">適用するエリア倍率</param>
    public DamageFieldSpawnRequest(Vector3 origin, Transform followTarget, int sourcePow, float areaScale) : base(origin, sourcePow)
    {
        FollowTarget = followTarget;
        AreaScale = areaScale;
    }
}
