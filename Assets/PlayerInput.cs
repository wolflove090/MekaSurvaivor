using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤー入力を管理するコンポーネント
/// </summary>
public class PlayerInput : MonoBehaviour
{
    PlayerInputActions _inputActions;
    Vector2 _moveInput;

    /// <summary>
    /// 現在の移動入力を取得します
    /// </summary>
    public Vector2 MoveInput => _moveInput;

    void Awake()
    {
        _inputActions = new PlayerInputActions();
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
        _inputActions?.Dispose();
    }

    /// <summary>
    /// 移動入力のコールバック
    /// </summary>
    /// <param name="context">入力コンテキスト</param>
    void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
}
