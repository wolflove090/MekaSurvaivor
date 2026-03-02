using UnityEngine;

/// <summary>
/// Sandbox用のサンドバッグ敵として、無敵・固定位置・接触ダメージ無効を提供します。
/// </summary>
public class SandboxDummyEnemy : MonoBehaviour, IDamageGuard
{
    EnemyController _enemyController;
    EnemyRegistry _enemyRegistry;
    Vector3 _spawnPosition;

    /// <summary>
    /// 指定ダメージを無効化するかどうかを返します。
    /// </summary>
    /// <param name="damage">適用予定のダメージ量</param>
    /// <param name="knockbackDirection">適用予定のノックバック方向</param>
    /// <returns>常にtrue</returns>
    public bool ShouldIgnoreDamage(int damage, Vector3 knockbackDirection)
    {
        return true;
    }

    void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        _spawnPosition = transform.position;
    }

    /// <summary>
    /// 有効化時に敵レジストリへ登録し、固定位置状態を初期化します。
    /// </summary>
    void OnEnable()
    {
        _spawnPosition = transform.position;

        if (_enemyController != null)
        {
            _enemyController.MoveSpeed = 0f;
            _enemyController.SetTarget(null);
        }

        _enemyRegistry = FindFirstObjectByType<EnemyRegistry>();
        _enemyRegistry?.RegisterEnemy(gameObject);
    }

    /// <summary>
    /// 無効化時に敵レジストリから登録解除します。
    /// </summary>
    void OnDisable()
    {
        _enemyRegistry?.UnregisterEnemy(gameObject);
    }

    /// <summary>
    /// 毎フレーム固定位置へ戻し、移動しない状態を維持します。
    /// </summary>
    void LateUpdate()
    {
        if (transform.position != _spawnPosition)
        {
            transform.position = _spawnPosition;
        }
    }
}
