using UnityEngine;

/// <summary>
/// プレイヤーの周囲にダメージフィールドを生成するコンポーネント
/// </summary>
public class DamageFieldSpawner : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField]
    [Tooltip("ダメージフィールドのプレハブ")]
    GameObject _damageFieldPrefab;

    [SerializeField]
    [Tooltip("生成間隔（秒）")]
    float _spawnInterval = 3f;

    [SerializeField]
    [Tooltip("生成位置のオフセット")]
    Vector3 _spawnOffset = Vector3.zero;

    float _spawnTimer;

    /// <summary>
    /// 生成間隔を取得または設定します
    /// </summary>
    public float SpawnInterval
    {
        get => _spawnInterval;
        set => _spawnInterval = value;
    }

    void Start()
    {
        _spawnTimer = _spawnInterval;
    }

    void Update()
    {
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnDamageField();
            _spawnTimer = _spawnInterval;
        }
    }

    /// <summary>
    /// ダメージフィールドを生成します
    /// </summary>
    void SpawnDamageField()
    {
        if (_damageFieldPrefab == null)
        {
            Debug.LogWarning("ダメージフィールドのプレハブが設定されていません");
            return;
        }

        Vector3 spawnPosition = transform.position + _spawnOffset;
        Instantiate(_damageFieldPrefab, spawnPosition, Quaternion.identity);
    }
}
