using UnityEngine;

/// <summary>
/// プレイヤーの周囲にダメージフィールドを生成する武器コンポーネント
/// </summary>
public class DamageFieldWeapon : WeaponBase
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

    [SerializeField]
    [Tooltip("ダメージフィールドプールの初期サイズ")]
    int _initialPoolSize = 8;

    ObjectPool<DamageFieldController> _damageFieldPool;

    /// <summary>
    /// 生成間隔を取得または設定します
    /// </summary>
    public float SpawnInterval
    {
        get => _spawnInterval;
        set => _spawnInterval = value;
    }

    protected override float CooldownDuration => _spawnInterval;

    protected override void Start()
    {
        base.Start();
        InitializePool();
    }

    /// <summary>
    /// ダメージフィールドを生成します
    /// </summary>
    protected override void Fire()
    {
        if (_damageFieldPrefab == null)
        {
            Debug.LogWarning("ダメージフィールドのプレハブが設定されていません");
            return;
        }

        Vector3 spawnPosition = transform.position + _spawnOffset;
        DamageFieldController damageFieldController = null;

        if (_damageFieldPool != null)
        {
            damageFieldController = _damageFieldPool.Get();
            damageFieldController.transform.position = spawnPosition;
            damageFieldController.transform.rotation = Quaternion.identity;
        }
        else
        {
            GameObject damageField = Instantiate(_damageFieldPrefab, spawnPosition, Quaternion.identity);
            damageFieldController = damageField.GetComponent<DamageFieldController>();
        }

        if (damageFieldController != null)
        {
            damageFieldController.SetFollowTarget(transform);
        }
    }

    /// <summary>
    /// ダメージフィールドプールを初期化します
    /// </summary>
    void InitializePool()
    {
        if (_damageFieldPrefab == null)
        {
            return;
        }

        DamageFieldController prefabController = _damageFieldPrefab.GetComponent<DamageFieldController>();
        if (prefabController == null)
        {
            Debug.LogWarning("ダメージフィールドプレハブにDamageFieldControllerがアタッチされていません");
            return;
        }

        _damageFieldPool = new ObjectPool<DamageFieldController>(prefabController, _initialPoolSize, transform);
    }
}
