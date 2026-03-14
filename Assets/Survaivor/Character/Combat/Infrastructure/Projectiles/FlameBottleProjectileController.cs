using UnityEngine;

/// <summary>
/// 放物線で飛行し、着地時に炎エリアを生成する火炎瓶を制御するコンポーネント
/// </summary>
public class FlameBottleProjectileController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("飛翔の最大生存時間（秒）")]
    float _maxLifetime = 5f;

    Vector3 _velocity;
    IWeaponEffectExecutor _effectExecutor;
    float _groundY;
    float _flameDuration;
    float _flameRadius;
    float _lifetimeTimer;
    int _sourcePow = 1;

    /// <summary>
    /// 火炎瓶の飛翔パラメータを初期化します。
    /// </summary>
    /// <param name="initialVelocity">初速</param>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    /// <param name="groundY">着地判定に使う地面のY座標</param>
    /// <param name="flameDuration">着地後の炎エリア持続時間</param>
    /// <param name="flameRadius">着地後の炎エリア半径</param>
    /// <param name="effectExecutor">炎エリア生成に使う実行ポート</param>
    public void Initialize(
        Vector3 initialVelocity,
        int sourcePow,
        float groundY,
        float flameDuration,
        float flameRadius,
        IWeaponEffectExecutor effectExecutor)
    {
        _velocity = initialVelocity;
        _sourcePow = Mathf.Max(1, sourcePow);
        _groundY = groundY;
        _flameDuration = Mathf.Max(0.1f, flameDuration);
        _flameRadius = Mathf.Max(0.1f, flameRadius);
        _effectExecutor = effectExecutor;
        _lifetimeTimer = _maxLifetime;
    }

    void Update()
    {
        _velocity += Physics.gravity * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        if (transform.position.y > _groundY)
        {
            return;
        }

        Vector3 spawnPosition = transform.position;
        spawnPosition.y = _groundY;
        _effectExecutor?.SpawnFlameArea(
            new FlameAreaSpawnRequest(
                spawnPosition,
                _sourcePow,
                _flameDuration,
                _flameRadius));
        ReturnToPoolOrDestroy();
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
