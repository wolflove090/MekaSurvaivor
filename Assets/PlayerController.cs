using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの移動を制御するコンポーネント
/// </summary>
public class PlayerController : MonoBehaviour
{
    static PlayerController _instance;

    [Header("移動設定")]
    [SerializeField]
    [Tooltip("移動速度")]
    float _moveSpeed = 5f;

    [Header("ノックバック設定")]
    [SerializeField]
    [Tooltip("ノックバックの距離")]
    float _knockbackDistance = 1f;

    [SerializeField]
    [Tooltip("ノックバックの持続時間")]
    float _knockbackDuration = 0.2f;

    [Header("HP設定")]
    [SerializeField]
    [Tooltip("初期HP")]
    int _maxHp = 10;

    [Header("スプライト設定")]
    [SerializeField]
    [Tooltip("左向きのスプライト")]
    Sprite _leftSprite;

    [SerializeField]
    [Tooltip("右向きのスプライト")]
    Sprite _rightSprite;

    PlayerInputActions _inputActions;
    Vector2 _moveInput;
    SpriteRenderer _spriteRenderer;
    bool _isKnockedBack;
    float _knockbackTimer;
    int _currentHp;
    bool _isGameOver;

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
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _currentHp;

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
        if (_moveInput.x < 0f)
        {
            return Vector3.left;
        }
        else if (_moveInput.x > 0f)
        {
            return Vector3.right;
        }
        
        if (_spriteRenderer != null && _spriteRenderer.sprite == _leftSprite)
        {
            return Vector3.left;
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

        _inputActions = new PlayerInputActions();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHp = _maxHp;
        _isGameOver = false;
    }

    void OnEnable()
    {
        _inputActions.Player.Move.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        _inputActions.Player.Move.Disable();
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 移動入力のコールバック
    /// </summary>
    /// <param name="context">入力コンテキスト</param>
    void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        if (_isKnockedBack)
        {
            UpdateKnockback();
            return;
        }

        if (_moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
            transform.position += movement * _moveSpeed * Time.deltaTime;
            UpdateSprite();
        }
    }

    /// <summary>
    /// ノックバック処理を更新します
    /// </summary>
    void UpdateKnockback()
    {
        _knockbackTimer -= Time.deltaTime;

        if (_knockbackTimer <= 0f)
        {
            _isKnockedBack = false;
        }
    }

    /// <summary>
    /// ノックバックを適用します
    /// </summary>
    /// <param name="direction">ノックバック方向（正規化済み）</param>
    public void ApplyKnockback(Vector3 direction)
    {
        _isKnockedBack = true;
        _knockbackTimer = _knockbackDuration;

        // 瞬間的に指定距離だけ移動
        transform.position += direction.normalized * _knockbackDistance;
    }

    /// <summary>
    /// ダメージを受けます
    /// HPが0以下になった場合、ゲームオーバー処理を実行します
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        if (_isGameOver)
        {
            return;
        }

        _currentHp -= damage;
        Debug.Log($"プレイヤーがダメージを受けました。残りHP: {_currentHp}");

        if (_currentHp <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// ゲームオーバー処理を実行します
    /// </summary>
    void GameOver()
    {
        _isGameOver = true;
        Debug.Log("ゲームオーバー！");
        Time.timeScale = 0f;
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
            ApplyKnockback(knockbackDirection);
            TakeDamage(1);
        }
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteRenderer == null) return;

        if (_moveInput.x < 0f)
        {
            _spriteRenderer.sprite = _leftSprite;
        }
        else if (_moveInput.x > 0f)
        {
            _spriteRenderer.sprite = _rightSprite;
        }
    }
}
