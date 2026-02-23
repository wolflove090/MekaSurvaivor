using UnityEngine;

/// <summary>
/// 各武器で使用するプレハブを提供するファクトリーコンポーネント
/// </summary>
public class BulletFactory : MonoBehaviour
{
    [SerializeField]
    [Tooltip("弾のプレハブ")]
    GameObject _bulletPrefab;

    [SerializeField]
    [Tooltip("投擲弾のプレハブ")]
    GameObject _throwingBulletPrefab;

    [SerializeField]
    [Tooltip("ダメージフィールドのプレハブ")]
    GameObject _damageFieldPrefab;

    /// <summary>
    /// 弾のプレハブを取得します
    /// </summary>
    public GameObject BulletPrefab => _bulletPrefab;

    /// <summary>
    /// 投擲弾のプレハブを取得します
    /// </summary>
    public GameObject ThrowingBulletPrefab => _throwingBulletPrefab;

    /// <summary>
    /// ダメージフィールドのプレハブを取得します
    /// </summary>
    public GameObject DamageFieldPrefab => _damageFieldPrefab;
}
