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

    Dictionary<GameObject, float> _enemiesInField = new Dictionary<GameObject, float>();
    Transform _targetTransform;
    float _lifetimeTimer;

    /// <summary>
    /// 追従するターゲットを設定します
    /// </summary>
    /// <param name="target">追従対象のTransform</param>
    public void SetFollowTarget(Transform target)
    {
        _targetTransform = target;
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
    /// 有効化時に寿命タイマーと内部状態を初期化します
    /// </summary>
    void OnEnable()
    {
        _lifetimeTimer = _duration;
        _enemiesInField.Clear();

        if (_followPlayer && PlayerController.Instance != null)
        {
            _targetTransform = PlayerController.Instance.transform;
        }
    }

    /// <summary>
    /// 無効化時に内部状態をクリアします
    /// </summary>
    void OnDisable()
    {
        _enemiesInField.Clear();
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
        List<GameObject> enemiesToUpdate = new List<GameObject>(_enemiesInField.Keys);

        foreach (GameObject enemy in enemiesToUpdate)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            float timer = _enemiesInField[enemy] - Time.deltaTime;

            if (timer <= 0f)
            {
                ApplyDamageToEnemy(enemy);
                timer = _damageInterval;
            }

            _enemiesInField[enemy] = timer;
        }

        foreach (var enemy in enemiesToRemove)
        {
            _enemiesInField.Remove(enemy);
        }
    }

    /// <summary>
    /// エネミーにダメージを与えます
    /// </summary>
    /// <param name="enemy">ダメージを与えるエネミー</param>
    void ApplyDamageToEnemy(GameObject enemy)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null && !damageable.IsDead)
        {
            Vector3 knockbackDirection = enemy.transform.position - transform.position;
            knockbackDirection.y = 0f;
            damageable.TakeDamage(_damage, knockbackDirection);
        }
    }

    /// <summary>
    /// トリガー範囲にエネミーが入った時の処理
    /// </summary>
    /// <param name="other">入ってきたCollider</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!_enemiesInField.ContainsKey(other.gameObject))
            {
                ApplyDamageToEnemy(other.gameObject);
                _enemiesInField[other.gameObject] = _damageInterval;
            }
        }
    }

    /// <summary>
    /// トリガー範囲からエネミーが出た時の処理
    /// </summary>
    /// <param name="other">出て行ったCollider</param>
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (_enemiesInField.ContainsKey(other.gameObject))
            {
                _enemiesInField.Remove(other.gameObject);
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
}
