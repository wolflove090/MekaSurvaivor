using UnityEngine;

/// <summary>
/// 弾の移動と衝突処理を制御するコンポーネント
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("弾設定")]
    [SerializeField]
    [Tooltip("弾の移動速度")]
    float _speed = 10f;

    [SerializeField]
    [Tooltip("弾の生存時間（秒）")]
    float _lifetime = 5f;

    // 移動方向
    Vector3 _direction;

    /// <summary>
    /// 弾の移動速度を取得または設定します
    /// </summary>
    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    /// <summary>
    /// 弾の移動方向を設定します
    /// </summary>
    /// <param name="direction">正規化された方向ベクトル</param>
    public void SetDirection(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    void Start()
    {
        // 生存時間後に自動的に破棄
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        // 設定された方向に移動
        transform.position += _direction * _speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // エネミーに衝突した場合
        if (other.CompareTag("Enemy"))
        {
            // EnemySpawnerのリストから削除
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RemoveEnemy(other.gameObject);
            }

            // エネミーを破棄
            Destroy(other.gameObject);
            
            // 弾自身も破棄
            Destroy(gameObject);
        }
    }
}
