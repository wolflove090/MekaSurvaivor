using UnityEngine;

/// <summary>
/// 回復アイテムの生成とプール管理を行うシングルトンクラス
/// </summary>
public class HealPickupSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("回復アイテムのプレハブ")]
    HealPickup _healPickupPrefab;

    [SerializeField]
    [Tooltip("初期プールサイズ")]
    int _initialPoolSize = 10;

    ObjectPool<HealPickup> _healPickupPool;
    Transform _poolRoot;

    static HealPickupSpawner _instance;

    /// <summary>
    /// HealPickupSpawnerのシングルトンインスタンスを取得します
    /// </summary>
    public static HealPickupSpawner Instance => _instance;

    /// <summary>
    /// シングルトン設定とプール初期化を行います
    /// </summary>
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        _poolRoot = new GameObject("HealPickupPool").transform;
        _poolRoot.SetParent(transform);

        if (_healPickupPrefab != null)
        {
            _healPickupPool = new ObjectPool<HealPickup>(_healPickupPrefab, _initialPoolSize, _poolRoot);
        }
        else
        {
            Debug.LogError("HealPickupSpawner: 回復アイテムプレハブが設定されていません");
        }
    }

    /// <summary>
    /// シーン破棄時にシングルトン参照を解除します
    /// </summary>
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 指定位置に回復アイテムをスポーンします
    /// </summary>
    /// <param name="position">スポーン位置</param>
    public void SpawnHealPickup(Vector3 position)
    {
        if (_healPickupPool == null)
        {
            Debug.LogWarning("HealPickupSpawner: 回復アイテムプールが初期化されていません");
            return;
        }

        HealPickup healPickup = _healPickupPool.Get();
        healPickup.transform.position = position;
    }
}
