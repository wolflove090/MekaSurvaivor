using UnityEngine;

/// <summary>
/// 経験値オーブの動作を制御するコンポーネント
/// プレイヤーに向かって移動し、接触時に経験値を付与します
/// </summary>
public class ExperienceOrb : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    [Tooltip("プレイヤーへの移動速度")]
    float _moveSpeed = 5f;

    [SerializeField]
    [Tooltip("プレイヤーを検知する範囲")]
    float _attractionRange = 3f;

    [SerializeField]
    [Tooltip("プレイヤーとの取得判定距離")]
    float _collectDistance = 0.5f;

    [Header("経験値設定")]
    [SerializeField]
    [Tooltip("このオーブが持つ経験値")]
    int _experienceValue = 1;

    Transform _playerTransform;
    bool _isMovingToPlayer = false;

    /// <summary>
    /// 経験値の値を取得します
    /// </summary>
    public int ExperienceValue => _experienceValue;

    void OnEnable()
    {
        _isMovingToPlayer = false;

        // プレイヤーを検索
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        // プレイヤーが取得範囲内に入ったら経験値を付与
        if (distanceToPlayer <= _collectDistance)
        {
            CollectOrb();
            return;
        }

        // プレイヤーが範囲内に入ったら追跡開始
        if (!_isMovingToPlayer && distanceToPlayer <= _attractionRange)
        {
            _isMovingToPlayer = true;
        }

        // プレイヤーに向かって移動
        if (_isMovingToPlayer)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// オーブを取得する処理
    /// </summary>
    void CollectOrb()
    {
        // プレイヤーに経験値を付与
        PlayerExperience playerExperience = _playerTransform.GetComponent<PlayerExperience>();
        if (playerExperience != null)
        {
            playerExperience.AddExperience(_experienceValue);
        }

        ReturnToPool();
    }

    /// <summary>
    /// オーブをプールに返却します
    /// </summary>
    void ReturnToPool()
    {
        PooledObject pooledObject = GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
