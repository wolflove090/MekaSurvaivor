using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの移動を制御するコンポーネント
/// </summary>
public class PlayerController : MonoBehaviour
{
    // シングルトンインスタンス
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

    // 入力アクション
    PlayerInputActions _inputActions;
    
    // 移動入力値
    Vector2 _moveInput;

    // SpriteRenderer コンポーネント
    SpriteRenderer _spriteRenderer;

    // ノックバック関連
    bool _isKnockedBack;
    float _knockbackTimer;

    // 現在のHP
    int _currentHp;

    // ゲームオーバーフラグ
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
        
        // 移動入力がない場合はスプライトから判定
        if (_spriteRenderer != null && _spriteRenderer.sprite == _leftSprite)
        {
            return Vector3.left;
        }
        
        return Vector3.right;
    }

    void Awake()
    {
        // シングルトンの設定
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"PlayerControllerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;

        // Input Actions の初期化
        _inputActions = new PlayerInputActions();

        // SpriteRenderer コンポーネントの取得
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // HPの初期化
        _currentHp = _maxHp;
        _isGameOver = false;
    }

    void OnEnable()
    {
        // Move アクションの有効化とコールバック登録
        _inputActions.Player.Move.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
    }

    void OnDisable()
    {
        // Move アクションの無効化とコールバック解除
        _inputActions.Player.Move.Disable();
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
    }

    void OnDestroy()
    {
        // インスタンスのクリア
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
        // ゲームオーバー時は処理を停止
        if (_isGameOver)
        {
            return;
        }

        // ノックバック中の処理
        if (_isKnockedBack)
        {
            UpdateKnockback();
            return;
        }

        // 移動処理
        if (_moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
            transform.position += movement * _moveSpeed * Time.deltaTime;

            // スプライトの切り替え
            UpdateSprite();
        }
    }

    /// <summary>
    /// ノックバック処理を更新します
    /// </summary>
    void UpdateKnockback()
    {
        // ノックバックタイマーを減少
        _knockbackTimer -= Time.deltaTime;

        // ノックバック終了判定
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
        // ゲームオーバー時はダメージを受けない
        if (_isGameOver)
        {
            return;
        }

        _currentHp -= damage;
        Debug.Log($"プレイヤーがダメージを受けました。残りHP: {_currentHp}");

        // HPが0以下になった場合
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

        // 画面を一時停止
        Time.timeScale = 0f;
    }

    /// <summary>
    /// エネミーとのトリガー接触時の処理
    /// </summary>
    /// <param name="other">接触したCollider</param>
    void OnTriggerEnter(Collider other)
    {
        // エネミーとの接触判定
        if (other.CompareTag("Enemy"))
        {
            // 接触方向の計算（エネミーからプレイヤーへの方向）
            Vector3 knockbackDirection = transform.position - other.transform.position;
            knockbackDirection.y = 0f; // Y軸方向を無効化

            // ノックバックを適用
            ApplyKnockback(knockbackDirection);

            // ダメージを受ける
            TakeDamage(1);
        }
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteRenderer == null) return;

        // 横方向の入力がある場合のみスプライトを切り替え
        if (_moveInput.x < 0f)
        {
            // 左向き
            _spriteRenderer.sprite = _leftSprite;
        }
        else if (_moveInput.x > 0f)
        {
            // 右向き
            _spriteRenderer.sprite = _rightSprite;
        }
    }
}
