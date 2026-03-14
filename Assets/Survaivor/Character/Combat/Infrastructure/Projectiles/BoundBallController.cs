using UnityEngine;

/// <summary>
/// 反射しながら進むバウンドボールを制御するコンポーネント
/// </summary>
public class BoundBallController : ProjectileController
{
    [SerializeField]
    [Tooltip("反射時のめり込みを防ぐ押し戻し距離")]
    float _pushbackDistance = 0.1f;

    int _remainingBounceCount;

    /// <summary>
    /// 残りバウンド回数を設定します。
    /// </summary>
    /// <param name="bounceCount">設定するバウンド回数</param>
    public void SetBounceCount(int bounceCount)
    {
        _remainingBounceCount = Mathf.Max(1, bounceCount);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!IsDamageTarget(other))
        {
            return;
        }

        ApplyDamage(other);
        _remainingBounceCount--;

        if (_remainingBounceCount <= 0)
        {
            ReturnToPoolOrDestroy();
            return;
        }

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 normal = transform.position - hitPoint;
        if (normal.sqrMagnitude <= Mathf.Epsilon)
        {
            normal = transform.position - other.bounds.center;
        }

        if (normal.sqrMagnitude <= Mathf.Epsilon)
        {
            normal = -_direction;
        }

        normal.y = 0f;
        if (normal.sqrMagnitude <= Mathf.Epsilon)
        {
            normal = -_direction;
        }

        normal.Normalize();
        _direction = Vector3.Reflect(_direction, normal).normalized;
        transform.position = hitPoint + normal * _pushbackDistance;
    }
}
