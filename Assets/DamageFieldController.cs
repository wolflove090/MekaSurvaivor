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
    Transform _playerTransform;

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

    void Start()
    {
        if (_followPlayer && PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }

        Destroy(gameObject, _duration);
    }

    void Update()
    {
        if (_followPlayer && _playerTransform != null)
        {
            transform.position = _playerTransform.position + _positionOffset;
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
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            Vector3 knockbackDirection = enemy.transform.position - transform.position;
            knockbackDirection.y = 0f;
            enemyController.TakeDamage(_damage, knockbackDirection);
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
}
