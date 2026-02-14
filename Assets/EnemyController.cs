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

    [Header("スプライト設定")]
    [SerializeField]
    [Tooltip("左向きのスプライト")]
    Sprite _leftSprite;

    [SerializeField]
    [Tooltip("右向きのスプライト")]
    Sprite _rightSprite;

    // SpriteRenderer コンポーネント
    SpriteRenderer _spriteRenderer;

    // 現在のHP
    int _currentHp;

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
        // SpriteRenderer コンポーネントの取得
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // HPの初期化
        _currentHp = _maxHp;
    }

    /// <summary>
    /// ダメージを受けます
    /// HPが0以下になった場合、自身を破棄します
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;

        // HPが0以下になった場合
        if (_currentHp <= 0)
        {
            // EnemySpawnerのリストから削除
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RemoveEnemy(gameObject);
            }

            // 自身を破棄
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // PlayerControllerのインスタンスが存在する場合のみ移動処理を実行
        if (PlayerController.Instance != null)
        {
            MoveTowardsPlayer();
            UpdateSprite();
        }
    }

    /// <summary>
    /// プレイヤーに向かって移動します
    /// </summary>
    void MoveTowardsPlayer()
    {
        // プレイヤーへの方向ベクトルを計算
        Vector3 direction = (PlayerController.Instance.transform.position - transform.position).normalized;

        // Y軸の移動を無効化（2D平面上での移動）
        direction.y = 0f;

        // プレイヤーに向かって移動
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 移動方向に応じてスプライトを更新します
    /// </summary>
    void UpdateSprite()
    {
        if (_spriteRenderer == null || PlayerController.Instance == null) return;

        // プレイヤーとの相対位置から向きを判定
        float directionX = PlayerController.Instance.transform.position.x - transform.position.x;

        if (directionX < 0f)
        {
            // 左向き
            _spriteRenderer.sprite = _leftSprite;
        }
        else if (directionX > 0f)
        {
            // 右向き
            _spriteRenderer.sprite = _rightSprite;
        }
    }
}
