using UnityEngine;

/// <summary>
/// 投擲弾の移動と衝突処理を制御するコンポーネント
/// </summary>
public class ThrowingBulletController : ProjectileController
{
    /// <summary>
    /// 弾の移動方向を設定します
    /// Y軸を0に固定します
    /// </summary>
    /// <param name="direction">移動方向ベクトル</param>
    public override void SetDirection(Vector3 direction)
    {
        base.SetDirection(direction);
        _direction.y = 0f;
    }

    /// <summary>
    /// 敵に命中した時の処理
    /// </summary>
    /// <param name="enemy">命中した敵</param>
    protected override void OnHitEnemy(EnemyController enemy)
    {
        Vector3 knockbackDirection = _direction;
        knockbackDirection.y = 0f;
        enemy.TakeDamage(Damage, knockbackDirection);
    }
}
