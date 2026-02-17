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
}
