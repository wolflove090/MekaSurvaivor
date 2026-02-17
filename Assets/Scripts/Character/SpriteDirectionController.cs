using UnityEngine;

/// <summary>
/// 移動方向に応じたスプライト制御を担当するコンポーネント
/// </summary>
public class SpriteDirectionController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("左向きのスプライト")]
    Sprite _leftSprite;

    [SerializeField]
    [Tooltip("右向きのスプライト")]
    Sprite _rightSprite;

    SpriteRenderer _spriteRenderer;
    bool _isFacingLeft;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 水平方向の入力に基づいてスプライトを更新します
    /// </summary>
    /// <param name="horizontalInput">水平方向の入力値（負の値で左、正の値で右）</param>
    public void UpdateDirection(float horizontalInput)
    {
        if (_spriteRenderer == null) return;

        if (horizontalInput < 0f)
        {
            _spriteRenderer.sprite = _leftSprite;
            _isFacingLeft = true;
        }
        else if (horizontalInput > 0f)
        {
            _spriteRenderer.sprite = _rightSprite;
            _isFacingLeft = false;
        }
    }

    /// <summary>
    /// 現在向いている方向のベクトルを取得します
    /// </summary>
    /// <returns>向いている方向ベクトル（左または右）</returns>
    public Vector3 GetFacingDirection()
    {
        return _isFacingLeft ? Vector3.left : Vector3.right;
    }
}
