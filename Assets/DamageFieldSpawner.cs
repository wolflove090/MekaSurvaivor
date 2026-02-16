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

    // 次の生成までの残り時間
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
        // 初期タイマー設定
        _spawnTimer = _spawnInterval;
    }

    void Update()
    {
        // タイマーを減算
        _spawnTimer -= Time.deltaTime;

        // 生成処理
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
        // プレハブが設定されていない場合は警告
        if (_damageFieldPrefab == null)
        {
            Debug.LogWarning("ダメージフィールドのプレハブが設定されていません");
            return;
        }

        // 生成位置を計算
        Vector3 spawnPosition = transform.position + _spawnOffset;

        // ダメージフィールドを生成
        Instantiate(_damageFieldPrefab, spawnPosition, Quaternion.identity);
    }
}
