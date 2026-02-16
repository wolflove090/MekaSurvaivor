using UnityEngine;

/// <summary>
/// エネミーの移動を制御するコンポーネント
/// プレイヤーに向かって徐々に近づく動作を行います
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField]
    [Tooltip("移動速度")]
    float _moveSpeed = 3f;

    [Header("HP設定")]
    [SerializeField]
    [Tooltip("初期HP")]
    int _maxHp = 3;

    [Header("ノックバック設定")]
    [SerializeField]
    [Tooltip("ノックバックの距離")]
    float _knockbackDistance = 0.5f;

    [SerializeField]
    [Tooltip("ノックバックの持続時間")]
    float _knockbackDuration = 0.1f;

    [Header("スプライト設定")]
    [SerializeField]
    [Tooltip("左向きのスプライト")]
    Sprite _leftSprite;

    [SerializeField]
    [Tooltip("右向きのスプライト")]
    Sprite _rightSprite;

    SpriteRenderer _spriteRenderer;
    int _currentHp;
    bool _isKnockedBack;
    float _knockbackTimer;

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

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHp = _maxHp;
    }

    /// <summary>
    /// ダメージを受けます
    /// HPが0以下になった場合、自身を破棄します
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackDirection">ノックバック方向（正規化済み）</param>
    public void TakeDamage(int damage, Vector3 knockbackDirection = default)
    {
        _currentHp -= damage;

        if (knockbackDirection != Vector3.zero)
        {
            ApplyKnockback(knockbackDirection);
        }

        if (_currentHp <= 0)
        {
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RemoveEnemy(gameObject);
            }

            Destroy(gameObject);
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

    void Update()
    {
        if (PlayerController.Instance != null)
        {
            if (_isKnockedBack)
            {
                UpdateKnockback();
            }
            else
            {
                MoveTowardsPlayer();
            }
            
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
    /// プレイヤーに向かって移動します
    /// </summary>
    void MoveTowardsPlayer()
    {
        Vector3 direction = (PlayerController.Instance.transform.position - transform.position).normalized;
        direction.y = 0f;
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteRenderer == null || PlayerController.Instance == null) return;

        float directionX = PlayerController.Instance.transform.position.x - transform.position.x;

        if (directionX < 0f)
        {
            _spriteRenderer.sprite = _leftSprite;
        }
        else if (directionX > 0f)
        {
            _spriteRenderer.sprite = _rightSprite;
        }
    }
}
