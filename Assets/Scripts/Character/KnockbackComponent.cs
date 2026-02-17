using UnityEngine;

/// <summary>
/// ノックバック処理を担当するコンポーネント
/// </summary>
public class KnockbackComponent : MonoBehaviour, IKnockbackable
{
    [SerializeField]
    [Tooltip("ノックバックの距離")]
    float _knockbackDistance = 1f;

    [SerializeField]
    [Tooltip("ノックバックの持続時間")]
    float _knockbackDuration = 0.2f;

    bool _isKnockedBack;
    float _knockbackTimer;

    /// <summary>
    /// ノックバック中かどうかを取得します
    /// </summary>
    public bool IsKnockedBack => _isKnockedBack;

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
    /// ノックバック状態をリセットします
    /// </summary>
    public void ResetState()
    {
        _isKnockedBack = false;
        _knockbackTimer = 0f;
    }

    void Update()
    {
        if (_isKnockedBack)
        {
            _knockbackTimer -= Time.deltaTime;

            if (_knockbackTimer <= 0f)
            {
                _isKnockedBack = false;
            }
        }
    }
}
