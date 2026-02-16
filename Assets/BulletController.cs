using UnityEngine;

/// <summary>
/// 弾の移動と衝突処理を制御するコンポーネント
/// </summary>
public class BulletController : ProjectileController
{
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
