using UnityEngine;

/// <summary>
/// プレイヤー全体の状態を統合管理するコンポーネント
/// </summary>
public class PlayerController : MonoBehaviour
{
    static PlayerController _instance;

    bool _isGameOver;

    HealthComponent _healthComponent;
    PlayerMovement _playerMovement;
    CharacterStats _characterStats;

    /// <summary>
    /// PlayerControllerのシングルトンインスタンスを取得します
    /// </summary>
    public static PlayerController Instance
    {
        get => _instance;
    }

    /// <summary>
    /// 移動速度を取得または設定します
    /// </summary>
    public float MoveSpeed
    {
        get => _playerMovement != null ? _playerMovement.MoveSpeed : 0f;
        set
        {
            if (_playerMovement != null)
            {
                _playerMovement.MoveSpeed = value;
            }
        }
    }

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _healthComponent != null ? _healthComponent.CurrentHp : 0;

    /// <summary>
    /// プレイヤーの攻撃力を取得します
    /// </summary>
    public int Pow => _characterStats != null ? _characterStats.Pow : 1;

    /// <summary>
    /// ゲームオーバー状態かどうかを取得します
    /// </summary>
    public bool IsGameOver => _isGameOver;

    /// <summary>
    /// プレイヤーの向いている方向を取得します
    /// </summary>
    /// <returns>向いている方向ベクトル（左または右）</returns>
    public Vector3 GetFacingDirection()
    {
        if (_playerMovement != null)
        {
            return _playerMovement.GetFacingDirection();
        }

        return Vector3.right;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"PlayerControllerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;

        if (GetComponent<PlayerInput>() == null)
        {
            gameObject.AddComponent<PlayerInput>();
        }

        _healthComponent = GetComponent<HealthComponent>();
        _playerMovement = GetComponent<PlayerMovement>();
        _characterStats = GetComponent<CharacterStats>();

        if (_characterStats == null)
        {
            _characterStats = gameObject.AddComponent<CharacterStats>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = gameObject.AddComponent<PlayerMovement>();
        }

        _isGameOver = false;

        if (_healthComponent != null)
        {
            _healthComponent.OnDied += GameOver;
            _healthComponent.OnDamaged += OnDamaged;
        }
        
        if (_playerMovement != null)
        {
            _playerMovement.OnMoved += OnMoved;
        }

        ApplyMoveSpeedFromStats();
    }

    void OnEnable()
    {
        ApplyMoveSpeedFromStats();
    }

    /// <summary>
    /// ダメージを受けた時のコールバック
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    void OnDamaged(int damage)
    {
        Debug.Log($"プレイヤーがダメージを受けました。残りHP: {CurrentHp}");
        GameEvents.RaisePlayerDamaged(damage);
    }

    /// <summary>
    /// ゲームオーバー処理を実行します
    /// </summary>
    void GameOver()
    {
        _isGameOver = true;
        if (_playerMovement != null)
        {
            _playerMovement.SetCanMove(false);
        }

        Debug.Log("ゲームオーバー！");
        GameEvents.RaisePlayerDied();
        GameEvents.RaiseGameOver();
        Time.timeScale = 0f;
    }

    void OnDestroy()
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnDied -= GameOver;
            _healthComponent.OnDamaged -= OnDamaged;
        }

        if (_playerMovement != null)
        {
            _playerMovement.OnMoved -= OnMoved;
        }

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 移動時のコールバック
    /// </summary>
    /// <param name="position">プレイヤーの現在位置</param>
    void OnMoved(Vector3 position)
    {
        if (_isGameOver)
        {
            return;
        }

        GameEvents.RaisePlayerMoved(position);
    }

    /// <summary>
    /// エネミーとのトリガー接触時の処理
    /// </summary>
    /// <param name="other">接触したCollider</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 knockbackDirection = transform.position - other.transform.position;
            knockbackDirection.y = 0f;
            CharacterStats enemyStats = other.GetComponent<CharacterStats>();
            int enemyPow = enemyStats != null ? enemyStats.Pow : 1;

            if (_healthComponent != null)
            {
                _healthComponent.TakeDamage(enemyPow, knockbackDirection);
            }
        }
    }

    /// <summary>
    /// ステータスから移動速度を反映します
    /// </summary>
    void ApplyMoveSpeedFromStats()
    {
        if (_playerMovement != null && _characterStats != null)
        {
            _playerMovement.MoveSpeed = _characterStats.Spd;
        }
    }
}
