using UnityEngine;

/// <summary>
/// ノックバックを受けることができるオブジェクトのインターフェース
/// </summary>
public interface IKnockbackable
{
    /// <summary>
    /// ノックバックを適用します
    /// </summary>
    /// <param name="direction">ノックバック方向</param>
    void ApplyKnockback(Vector3 direction);

    /// <summary>
    /// ノックバック中かどうかを取得します
    /// </summary>
    bool IsKnockedBack { get; }
}
