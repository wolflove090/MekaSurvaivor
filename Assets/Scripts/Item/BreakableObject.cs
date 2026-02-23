using UnityEngine;

/// <summary>
/// 攻撃で破壊可能なオブジェクトを制御するコンポーネント
/// </summary>
public class BreakableObject : MonoBehaviour, IDamageable
{
    [SerializeField]
    [Tooltip("最大HP（仕様上は1固定）")]
    int _maxHp = 1;

    int _currentHp;
    bool _isDestroyHandled;
    BreakableObjectSpawner _ownerSpawner;

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _currentHp;

    /// <summary>
    /// 死亡状態かどうかを取得します
    /// </summary>
    public bool IsDead => _currentHp <= 0;

    /// <summary>
    /// 所有スポーナーを設定し、初期状態を再設定します
    /// </summary>
    /// <param name="ownerSpawner">生成元のスポーナー</param>
    public void Initialize(BreakableObjectSpawner ownerSpawner)
    {
        _ownerSpawner = ownerSpawner;
        ResetState();
    }

    /// <summary>
    /// 有効化時に状態を初期化します
    /// </summary>
    void OnEnable()
    {
        ResetState();
    }

    /// <summary>
    /// ダメージを受けます
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（未使用）</param>
    public void TakeDamage(int damage, Vector3 knockbackDirection = default)
    {
        if (IsDead)
        {
            return;
        }

        int finalDamage = Mathf.Max(1, damage);
        _currentHp = Mathf.Max(0, _currentHp - finalDamage);
        if (_currentHp <= 0)
        {
            HandleDestroyed();
        }
    }

    /// <summary>
    /// 破壊時の処理を実行します
    /// </summary>
    void HandleDestroyed()
    {
        if (_isDestroyHandled)
        {
            return;
        }

        _isDestroyHandled = true;

        if (HealPickupSpawner.Instance != null)
        {
            HealPickupSpawner.Instance.SpawnHealPickup(transform.position);
        }

        if (_ownerSpawner != null)
        {
            _ownerSpawner.UnregisterBreakableObject(gameObject);
        }

        ReturnToPoolOrDestroy();
    }

    /// <summary>
    /// 内部状態を初期値に戻します
    /// </summary>
    void ResetState()
    {
        _currentHp = Mathf.Max(1, _maxHp);
        _isDestroyHandled = false;
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
