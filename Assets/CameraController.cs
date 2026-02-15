using UnityEngine;

/// <summary>
/// カメラをプレイヤーに追従させるコンポーネント
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("追従設定")]
    [SerializeField]
    [Tooltip("追従するターゲット（nullの場合はPlayerControllerを自動検索）")]
    Transform _target;

    [SerializeField]
    [Tooltip("カメラのオフセット位置")]
    Vector3 _offset = new Vector3(0f, 2.5f, -16.5f);

    [SerializeField]
    [Tooltip("追従の滑らかさ（0で即座に追従、値が大きいほど滑らか）")]
    float _smoothSpeed = 0.125f;

    void Start()
    {
        // ターゲットが設定されていない場合、PlayerControllerを検索
        if (_target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _target = player.transform;
            }
            else
            {
                Debug.LogWarning("追従するプレイヤーが見つかりません。");
            }
        }

        // 初期位置を設定
        if (_target != null)
        {
            transform.position = _target.position + _offset;
        }
    }

    void LateUpdate()
    {
        // ターゲットが存在しない場合は処理をスキップ
        if (_target == null)
        {
            return;
        }

        // 目標位置を計算
        Vector3 desiredPosition = _target.position + _offset;

        // 滑らかに追従
        if (_smoothSpeed > 0f)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            // 即座に追従
            transform.position = desiredPosition;
        }
    }

    /// <summary>
    /// 追従ターゲットを設定します
    /// </summary>
    /// <param name="target">追従するターゲットのTransform</param>
    public void SetTarget(Transform target)
    {
        _target = target;
    }

    /// <summary>
    /// カメラのオフセットを設定します
    /// </summary>
    /// <param name="offset">オフセット位置</param>
    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    /// <summary>
    /// 追従の滑らかさを設定します
    /// </summary>
    /// <param name="smoothSpeed">滑らかさ（0で即座に追従）</param>
    public void SetSmoothSpeed(float smoothSpeed)
    {
        _smoothSpeed = Mathf.Max(0f, smoothSpeed);
    }
}
