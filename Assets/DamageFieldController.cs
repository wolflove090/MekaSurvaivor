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

    // フィールド内のエネミーとダメージタイマーを管理
    Dictionary<GameObject, float> _enemiesInField = new Dictionary<GameObject, float>();

    // プレイヤーへの参照
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
        // プレイヤーへの参照を取得
        if (_followPlayer && PlayerController.Instance != null)
        {
            _playerTransform = PlayerController.Instance.transform;
        }

        // 持続時間後に自動的に破棄
        Destroy(gameObject, _duration);
    }

    void Update()
    {
        // プレイヤーに追従
        if (_followPlayer && _playerTransform != null)
        {
            transform.position = _playerTransform.position + _positionOffset;
        }

        // フィールド内の各エネミーのダメージタイマーを更新
        List<GameObject> enemiesToRemove = new List<GameObject>();
        List<GameObject> enemiesToUpdate = new List<GameObject>(_enemiesInField.Keys);

        foreach (GameObject enemy in enemiesToUpdate)
        {
            // エネミーが破棄されている場合はリストから削除
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            // タイマーを減算
            float timer = _enemiesInField[enemy] - Time.deltaTime;

            // ダメージを与えるタイミング
            if (timer <= 0f)
            {
                ApplyDamageToEnemy(enemy);
                timer = _damageInterval;
            }

            _enemiesInField[enemy] = timer;
        }

        // 破棄されたエネミーをリストから削除
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
            // ノックバック方向を計算（フィールドの中心からエネミーへの方向）
            Vector3 knockbackDirection = enemy.transform.position - transform.position;
            knockbackDirection.y = 0f; // Y軸方向を無効化
            
            enemyController.TakeDamage(_damage, knockbackDirection);
        }
    }

    /// <summary>
    /// トリガー範囲にエネミーが入った時の処理
    /// </summary>
    /// <param name="other">入ってきたCollider</param>
    void OnTriggerEnter(Collider other)
    {
        // エネミーがフィールドに入った場合
        if (other.CompareTag("Enemy"))
        {
            // まだリストに存在しない場合のみ追加
            if (!_enemiesInField.ContainsKey(other.gameObject))
            {
                // 即座にダメージを与える
                ApplyDamageToEnemy(other.gameObject);
                
                // タイマーを設定してリストに追加
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
        // エネミーがフィールドから出た場合
        if (other.CompareTag("Enemy"))
        {
            // リストから削除
            if (_enemiesInField.ContainsKey(other.gameObject))
            {
                _enemiesInField.Remove(other.gameObject);
            }
        }
    }
}
