using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダメージフィールドの動作を制御するコンポーネント
/// 一定時間存在し、範囲内のエネミーに継続的にダメージを与えます
/// </summary>
public class DamageFieldController : MonoBehaviour
{
    [Header("ダメージ設定")]
    [SerializeField]
    [Tooltip("与えるダメージ量")]
    int _damage = 1;

    [SerializeField]
    [Tooltip("ダメージ間隔（秒）")]
    float _damageInterval = 0.5f;

    [Header("フィールド設定")]
    [SerializeField]
    [Tooltip("フィールドの持続時間（秒）")]
    float _duration = 5f;

    [SerializeField]
    [Tooltip("プレイヤーに追従するかどうか")]
    bool _followPlayer = true;

    [SerializeField]
    [Tooltip("プレイヤーからのオフセット（Z軸で後ろに配置）")]
    Vector3 _positionOffset = new Vector3(0f, 0f, 0.5f);

    [SerializeField]
    [Tooltip("破壊可能オブジェクトにも継続ダメージを与えるかどうか")]
    bool _damageBreakableObjects = true;

    Dictionary<GameObject, float> _targetsInField = new Dictionary<GameObject, float>();
    Transform _targetTransform;
    float _lifetimeTimer;
    float _sourcePow = 1f;

    /// <summary>
    /// 追従するターゲットを設定します
    /// </summary>
    /// <param name="target">追従対象のTransform</param>
    public void SetFollowTarget(Transform target)
    {
        _targetTransform = target;
    }

    /// <summary>
    /// ダメージ計算に使用する攻撃力を設定します。
    /// </summary>
    /// <param name="sourcePow">攻撃元の攻撃力</param>
    public void SetSourcePow(float sourcePow)
    {
        _sourcePow = Mathf.Max(1f, sourcePow);
    }

    /// <summary>
    /// 追従するかどうかを設定します。
    /// </summary>
    /// <param name="shouldFollow">追従する場合はtrue</param>
    public void SetFollowEnabled(bool shouldFollow)
    {
        _followPlayer = shouldFollow;
    }

    /// <summary>
    /// 持続時間を設定します。
    /// </summary>
    /// <param name="duration">設定する持続時間</param>
    public void SetDuration(float duration)
    {
        _duration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// ダメージ間隔を設定します。
    /// </summary>
    /// <param name="damageInterval">設定するダメージ間隔</param>
    public void SetDamageInterval(float damageInterval)
    {
        _damageInterval = Mathf.Max(0.05f, damageInterval);
    }

    /// <summary>
    /// 与えるダメージ量を取得または設定します
    /// </summary>
    public int Damage
    {
        get => _damage;
        set => _damage = value;
    }

    /// <summary>
    /// ダメージ間隔を取得または設定します
    /// </summary>
    public float DamageInterval
    {
        get => _damageInterval;
        set => _damageInterval = value;
    }

    /// <summary>
    /// フィールドの持続時間を取得または設定します
    /// </summary>
    public float Duration
    {
        get => _duration;
        set => _duration = value;
    }

    /// <summary>
    /// ダメージエリアの拡大率を設定します
    /// </summary>
    /// <param name="areaScale">拡大率（1.0が等倍）</param>
    public void SetAreaScale(float areaScale)
    {
        transform.localScale = Vector3.one * Mathf.Max(0.1f, areaScale);
    }

    /// <summary>
    /// 有効化時に寿命タイマーと内部状態を初期化します
    /// </summary>
    void OnEnable()
    {
        _lifetimeTimer = _duration;
        _targetsInField.Clear();
    }

    /// <summary>
    /// 無効化時に内部状態をクリアします
    /// </summary>
    void OnDisable()
    {
        _targetsInField.Clear();
    }

    void Update()
    {
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        if (_followPlayer && _targetTransform != null)
        {
            transform.position = _targetTransform.position + _positionOffset;
        }

        List<GameObject> enemiesToRemove = new List<GameObject>();
        List<GameObject> enemiesToUpdate = new List<GameObject>(_targetsInField.Keys);

        foreach (GameObject enemy in enemiesToUpdate)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            float timer = _targetsInField[enemy] - Time.deltaTime;

            if (timer <= 0f)
            {
                ApplyDamageToTarget(enemy);
                timer = _damageInterval;
            }

            _targetsInField[enemy] = timer;
        }

        foreach (var enemy in enemiesToRemove)
        {
            _targetsInField.Remove(enemy);
        }
    }

    /// <summary>
    /// エネミーにダメージを与えます
    /// </summary>
    /// <param name="enemy">ダメージを与えるエネミー</param>
    void ApplyDamageToTarget(GameObject enemy)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            Vector3 knockbackDirection = enemy.transform.position - transform.position;
            knockbackDirection.y = 0f;
            damageable.TakeDamage(CalculateDamage(), knockbackDirection);
        }
    }

    /// <summary>
    /// トリガー範囲にエネミーが入った時の処理
    /// </summary>
    /// <param name="other">入ってきたCollider</param>
    void OnTriggerEnter(Collider other)
    {
        if (IsDamageTarget(other))
        {
            if (!_targetsInField.ContainsKey(other.gameObject))
            {
                ApplyDamageToTarget(other.gameObject);
                _targetsInField[other.gameObject] = _damageInterval;
            }
        }
    }

    /// <summary>
    /// トリガー範囲からエネミーが出た時の処理
    /// </summary>
    /// <param name="other">出て行ったCollider</param>
    void OnTriggerExit(Collider other)
    {
        if (IsDamageTarget(other))
        {
            if (_targetsInField.ContainsKey(other.gameObject))
            {
                _targetsInField.Remove(other.gameObject);
            }
        }
    }

    /// <summary>
    /// ダメージフィールドをプールへ返却、未対応時は破棄します
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

    /// <summary>
    /// 攻撃元の攻撃力を反映したダメージ値を計算します
    /// </summary>
    /// <returns>適用する最終ダメージ値</returns>
    int CalculateDamage()
    {
        float damage = _damage + (_sourcePow - 1f);
        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    /// <summary>
    /// 対象Colliderが継続ダメージ対象かを判定します。
    /// </summary>
    /// <param name="other">判定対象</param>
    /// <returns>対象の場合はtrue</returns>
    bool IsDamageTarget(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag("Enemy"))
        {
            return true;
        }

        if (_damageBreakableObjects && other.CompareTag("BreakableObject"))
        {
            return true;
        }

        return false;
    }
}
