using UnityEngine;

/// <summary>
/// 経験値オーブの生成とプール管理を行うシングルトンクラス
/// 敵が死亡した際にオーブをスポーンします
/// </summary>
public class ExperienceOrbSpawner : MonoBehaviour
{
    [Header("オーブ設定")]
    [SerializeField]
    [Tooltip("経験値オーブのプレハブ")]
    ExperienceOrb _orbPrefab;

    [SerializeField]
    [Tooltip("初期プールサイズ")]
    int _initialPoolSize = 20;

    ObjectPool<ExperienceOrb> _orbPool;
    Transform _poolRoot;
    GameMessageBus _gameMessageBus;

    static ExperienceOrbSpawner _instance;

    /// <summary>
    /// シングルトンインスタンスを取得します
    /// </summary>
    public static ExperienceOrbSpawner Instance => _instance;

    void Awake()
    {
        // シングルトンの設定
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // プールルートの作成
        _poolRoot = new GameObject("ExperienceOrbPool").transform;
        _poolRoot.SetParent(transform);

        // オブジェクトプールの初期化
        if (_orbPrefab != null)
        {
            _orbPool = new ObjectPool<ExperienceOrb>(_orbPrefab, _initialPoolSize, _poolRoot);
        }
        else
        {
            Debug.LogError("ExperienceOrbSpawner: オーブプレハブが設定されていません");
        }
    }

    void OnEnable()
    {
        RegisterMessageBus();
    }

    void OnDisable()
    {
        UnregisterMessageBus();
    }

    /// <summary>
    /// シーン内通知に使用するメッセージバスを設定します。
    /// </summary>
    /// <param name="gameMessageBus">設定する通知バス</param>
    public void SetMessageBus(GameMessageBus gameMessageBus)
    {
        if (_gameMessageBus == gameMessageBus)
        {
            return;
        }

        UnregisterMessageBus();
        _gameMessageBus = gameMessageBus;
        RegisterMessageBus();
    }

    /// <summary>
    /// 敵が死亡した時の処理
    /// </summary>
    /// <param name="enemy">死亡した敵のGameObject</param>
    void OnEnemyDied(GameObject enemy)
    {
        if (enemy != null)
        {
            SpawnOrb(enemy.transform.position);
        }
    }

    /// <summary>
    /// メッセージバスの購読を登録します。
    /// </summary>
    void RegisterMessageBus()
    {
        if (!isActiveAndEnabled || _gameMessageBus == null)
        {
            return;
        }

        _gameMessageBus.EnemyDied -= OnEnemyDied;
        _gameMessageBus.EnemyDied += OnEnemyDied;
    }

    /// <summary>
    /// メッセージバスの購読を解除します。
    /// </summary>
    void UnregisterMessageBus()
    {
        if (_gameMessageBus != null)
        {
            _gameMessageBus.EnemyDied -= OnEnemyDied;
        }
    }

    /// <summary>
    /// 指定位置に経験値オーブをスポーンします
    /// </summary>
    /// <param name="position">スポーン位置</param>
    public void SpawnOrb(Vector3 position)
    {
        if (_orbPool == null)
        {
            Debug.LogWarning("ExperienceOrbSpawner: オーブプールが初期化されていません");
            return;
        }

        ExperienceOrb orb = _orbPool.Get();
        orb.transform.position = position;
    }
}
