using UnityEngine;

/// <summary>
/// ダメージ適用前に無効化判定を行うガードを定義します。
/// </summary>
public interface IDamageGuard
{
    /// <summary>
    /// 指定ダメージを無効化するかどうかを返します。
    /// </summary>
    /// <param name="damage">適用予定のダメージ量</param>
    /// <param name="knockbackDirection">適用予定のノックバック方向</param>
    /// <returns>ダメージ適用を抑止する場合はtrue</returns>
    bool ShouldIgnoreDamage(int damage, Vector3 knockbackDirection);
}
