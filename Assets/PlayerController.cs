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

    PlayerInputActions _inputActions;
    Vector2 _moveInput;
    bool _isGameOver;

    HealthComponent _healthComponent;
    KnockbackComponent _knockbackComponent;
    SpriteDirectionController _spriteDirectionController;

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
    public int CurrentHp => _healthComponent != null ? _healthComponent.CurrentHp : 0;

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
        if (_moveInput.x != 0f)
        {
            return _moveInput.x < 0f ? Vector3.left : Vector3.right;
        }
        
        if (_spriteDirectionController != null)
        {
            return _spriteDirectionController.GetFacingDirection();
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
        _healthComponent = GetComponent<HealthComponent>();
        _knockbackComponent = GetComponent<KnockbackComponent>();
        _spriteDirectionController = GetComponent<SpriteDirectionController>();
        _isGameOver = false;

        if (_healthComponent != null)
        {
            _healthComponent.OnDied += GameOver;
            _healthComponent.OnDamaged += OnDamaged;
        }
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

        if (_knockbackComponent != null && _knockbackComponent.IsKnockedBack)
        {
            return;
        }

        if (_moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
            transform.position += movement * _moveSpeed * Time.deltaTime;

            if (_spriteDirectionController != null)
            {
                _spriteDirectionController.UpdateDirection(_moveInput.x);
            }
        }
    }

    /// <summary>
    /// ダメージを受けた時のコールバック
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    void OnDamaged(int damage)
    {
        Debug.Log($"プレイヤーがダメージを受けました。残りHP: {CurrentHp}");
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

            if (_knockbackComponent != null)
            {
                _knockbackComponent.ApplyKnockback(knockbackDirection);
            }

            if (_healthComponent != null)
            {
                _healthComponent.TakeDamage(1);
            }
        }
    }
}
