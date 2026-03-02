using UnityEngine;

/// <summary>
/// プレイヤー追従と自律射撃を行うドローンを制御するコンポーネント
/// </summary>
public class DroneController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("プレイヤー周囲を周回する角速度")]
    float _orbitAngularSpeed = 180f;

    Transform _followTarget;
    EnemyRegistry _enemyRegistry;
    IWeaponEffectExecutor _effectExecutor;
    float _orbitRadius = 1.5f;
    float _shotInterval = 1f;
    float _shotTimer;
    float _orbitAngle;
    int _sourcePow = 1;

    /// <summary>
    /// ドローンの追従先と戦闘設定を初期化します。
    /// </summary>
    /// <param name="followTarget">追従対象</param>
    /// <param name="enemyRegistry">敵探索に使うレジストリ</param>
    /// <param name="effectExecutor">弾発射に使う実行ポート</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="orbitRadius">周回半径</param>
    /// <param name="shotInterval">射撃間隔</param>
    public void Initialize(
        Transform followTarget,
        EnemyRegistry enemyRegistry,
        IWeaponEffectExecutor effectExecutor,
        int sourcePow,
        float orbitRadius,
        float shotInterval)
    {
        _followTarget = followTarget;
        _enemyRegistry = enemyRegistry;
        _effectExecutor = effectExecutor;
        _sourcePow = Mathf.Max(1, sourcePow);
        _orbitRadius = Mathf.Max(0.1f, orbitRadius);
        _shotInterval = Mathf.Max(0.1f, shotInterval);
        _shotTimer = 0f;
    }

    void Update()
    {
        if (_followTarget == null)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        _orbitAngle += _orbitAngularSpeed * Time.deltaTime;
        float radians = _orbitAngle * Mathf.Deg2Rad;
        Vector3 center = _followTarget.position;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * _orbitRadius;
        transform.position = center + orbitOffset;

        _shotTimer -= Time.deltaTime;
        if (_shotTimer > 0f)
        {
            return;
        }

        GameObject target = _enemyRegistry != null ? _enemyRegistry.FindNearestEnemy(transform.position, onlyVisible: true) : null;
        if (target == null || _effectExecutor == null)
        {
            return;
        }

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        _effectExecutor.FireBullet(new BulletFireRequest(transform.position, direction.normalized, _sourcePow));
        _shotTimer = _shotInterval;
    }

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
