using UnityEngine;

/// <summary>
/// プレイヤーの移動処理を担当するコンポーネント
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    [Tooltip("移動速度")]
    float _moveSpeed = 5f;

    PlayerInput _playerInput;
    KnockbackComponent _knockbackComponent;
    SpriteDirectionController _spriteDirectionController;
    bool _canMove = true;

    /// <summary>
    /// プレイヤー移動時に発火するイベント
    /// </summary>
    public event System.Action<Vector3> OnMoved;

    /// <summary>
    /// 移動速度を取得または設定します
    /// </summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    /// <summary>
    /// 移動可能かどうかを設定します
    /// </summary>
    /// <param name="canMove">移動可能ならtrue</param>
    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
    }

    /// <summary>
    /// プレイヤーの向いている方向を取得します
    /// </summary>
    /// <returns>向いている方向ベクトル（左または右）</returns>
    public Vector3 GetFacingDirection()
    {
        if (_playerInput != null && _playerInput.MoveInput.x != 0f)
        {
            return _playerInput.MoveInput.x < 0f ? Vector3.left : Vector3.right;
        }

        if (_spriteDirectionController != null)
        {
            return _spriteDirectionController.GetFacingDirection();
        }

        return Vector3.right;
    }

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _knockbackComponent = GetComponent<KnockbackComponent>();
        _spriteDirectionController = GetComponent<SpriteDirectionController>();
    }

    void OnEnable()
    {
        _canMove = true;
    }

    void Update()
    {
        if (!_canMove)
        {
            return;
        }

        if (_knockbackComponent != null && _knockbackComponent.IsKnockedBack)
        {
            return;
        }

        if (_playerInput == null || _playerInput.MoveInput == Vector2.zero)
        {
            return;
        }

        Vector2 moveInput = _playerInput.MoveInput;
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += movement * _moveSpeed * Time.deltaTime;

        if (_spriteDirectionController != null)
        {
            _spriteDirectionController.UpdateDirection(moveInput.x);
        }

        OnMoved?.Invoke(transform.position);
    }
}
