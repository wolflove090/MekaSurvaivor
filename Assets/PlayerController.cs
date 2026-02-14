using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの移動を制御するコンポーネント
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    [Tooltip("移動速度")]
    float _moveSpeed = 5f;

    // 入力アクション
    PlayerInputActions _inputActions;
    
    // 移動入力値
    Vector2 _moveInput;

    /// <summary>
    /// 移動速度を取得または設定します
    /// </summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    void Awake()
    {
        // Input Actions の初期化
        _inputActions = new PlayerInputActions();
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
        // 移動処理
        if (_moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
            transform.position += movement * _moveSpeed * Time.deltaTime;
        }
    }
}
