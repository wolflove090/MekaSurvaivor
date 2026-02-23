using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 移動方向に応じたスプライト制御を担当するコンポーネント
/// </summary>
public class SpriteDirectionController : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("_rightSprite")]
    [Tooltip("右向き基準のスプライト")]
    Sprite _rightFacingSprite;

    SpriteRenderer _spriteRenderer;
    bool _isFacingLeft;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            return;
        }

        if (_rightFacingSprite != null)
        {
            _spriteRenderer.sprite = _rightFacingSprite;
        }

        _spriteRenderer.flipX = false;
        _isFacingLeft = false;
    }

    /// <summary>
    /// 水平方向の入力に基づいてスプライトを更新します
    /// </summary>
    /// <param name="horizontalInput">水平方向の入力値（負の値で左、正の値で右）</param>
    public void UpdateDirection(float horizontalInput)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (horizontalInput < 0f)
        {
            _spriteRenderer.flipX = true;
            _isFacingLeft = true;
        }
        else if (horizontalInput > 0f)
        {
            _spriteRenderer.flipX = false;
            _isFacingLeft = false;
        }
    }

    /// <summary>
    /// 右向き基準のスプライトを適用し、向きを右に初期化します
    /// </summary>
    /// <param name="rightSprite">右向き基準のスプライト</param>
    public void ApplyRightFacingSprite(Sprite rightSprite)
    {
        if (rightSprite == null)
        {
            Debug.LogWarning("SpriteDirectionController: 右向きスプライトが未設定のため画像を更新できません。");
            return;
        }

        _rightFacingSprite = rightSprite;

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_spriteRenderer == null)
        {
            Debug.LogWarning("SpriteDirectionController: SpriteRendererが見つからないため画像を更新できません。");
            return;
        }

        _spriteRenderer.sprite = _rightFacingSprite;
        _spriteRenderer.flipX = false;
        _isFacingLeft = false;
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
