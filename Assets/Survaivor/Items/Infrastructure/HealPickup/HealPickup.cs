using UnityEngine;

/// <summary>
/// 回復アイテムの追従と取得処理を制御するコンポーネント
/// </summary>
public class HealPickup : MonoBehaviour
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

    [Header("回復設定")]
    [SerializeField]
    [Tooltip("取得時の回復量")]
    int _healAmount = 3;

    Transform _playerTransform;
    HealthComponent _playerHealthComponent;
    bool _isMovingToPlayer;

    /// <summary>
    /// 有効化時に追従対象を再取得し、状態を初期化します
    /// </summary>
    void OnEnable()
    {
        _isMovingToPlayer = false;
        _playerTransform = null;
        _playerHealthComponent = null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerHealthComponent = player.GetComponent<HealthComponent>();
        }
    }

    /// <summary>
    /// プレイヤーとの距離に応じて追従と取得処理を更新します
    /// </summary>
    void Update()
    {
        if (_playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        if (distanceToPlayer <= _collectDistance)
        {
            CollectPickup();
            return;
        }

        if (!_isMovingToPlayer && distanceToPlayer <= _attractionRange)
        {
            _isMovingToPlayer = true;
        }

        if (_isMovingToPlayer)
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            transform.position += direction * _moveSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 回復アイテムを取得し、回復を適用します
    /// </summary>
    void CollectPickup()
    {
        if (_playerHealthComponent != null)
        {
            _playerHealthComponent.Heal(_healAmount);
        }

        ReturnToPoolOrDestroy();
    }

    /// <summary>
    /// プールへ返却、未対応時は破棄します
    /// </summary>
    void ReturnToPoolOrDestroy()
    {
        PooledObject pooledObject = GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
            return;
        }

        Destroy(gameObject);
    }
}
