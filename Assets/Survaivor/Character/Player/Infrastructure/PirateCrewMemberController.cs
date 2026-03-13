using System;
using UnityEngine;

/// <summary>
/// 海賊戦闘員の移動とライフサイクルを管理します
/// </summary>
public class PirateCrewMemberController : MonoBehaviour
{
    CharacterStats _characterStats;
    HealthComponent _healthComponent;
    PirateCrewTarget _pirateCrewTarget;
    PirateCrewRegistry _pirateCrewRegistry;
    Action<PirateCrewMemberController> _releaseToPool;
    Vector3 _moveDirection = Vector3.right;
    bool _isRegistered;
    bool _isReturning;

    /// <summary>
    /// 識別用ターゲットを取得します
    /// </summary>
    public PirateCrewTarget Target => _pirateCrewTarget;

    void Awake()
    {
        _characterStats = GetComponent<CharacterStats>();
        if (_characterStats == null)
        {
            _characterStats = gameObject.AddComponent<CharacterStats>();
        }

        _healthComponent = GetComponent<HealthComponent>();
        if (_healthComponent == null)
        {
            _healthComponent = gameObject.AddComponent<HealthComponent>();
        }

        _pirateCrewTarget = GetComponent<PirateCrewTarget>();
        if (_pirateCrewTarget == null)
        {
            _pirateCrewTarget = gameObject.AddComponent<PirateCrewTarget>();
        }

        _healthComponent.OnDied += OnDied;
    }

    void OnDestroy()
    {
        if (_healthComponent != null)
        {
            _healthComponent.OnDied -= OnDied;
        }
    }

    void Update()
    {
        TickMovement(Time.deltaTime);
    }

    /// <summary>
    /// 戦闘員を初期化します
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform</param>
    /// <param name="enemyRegistry">敵レジストリ</param>
    /// <param name="pirateCrewRegistry">海賊戦闘員レジストリ</param>
    /// <param name="statsData">適用する戦闘員ステータス</param>
    /// <param name="moveDirection">進行方向</param>
    /// <param name="releaseToPool">返却処理</param>
    public void Initialize(
        Transform playerTransform,
        EnemyRegistry enemyRegistry,
        PirateCrewRegistry pirateCrewRegistry,
        CharacterStatsData statsData,
        Vector3 moveDirection,
        Action<PirateCrewMemberController> releaseToPool)
    {
        _pirateCrewRegistry = pirateCrewRegistry;
        _releaseToPool = releaseToPool;
        _moveDirection = ProjectToGround(moveDirection);
        if (_moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            _moveDirection = Vector3.right;
        }
        _moveDirection.Normalize();

        if (statsData != null)
        {
            _characterStats.ApplyStatsData(statsData);
            _healthComponent.ResetToMaxHp();
        }

        RegisterToCrewRegistry();
        _isReturning = false;
    }

    /// <summary>
    /// 指定経過時間ぶん移動を更新します
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    public void TickMovement(float deltaTime)
    {
        if (!gameObject.activeInHierarchy || deltaTime <= 0f)
        {
            return;
        }

        Vector3 moveDirection = ResolveMovementDirection();
        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        transform.position += moveDirection * _characterStats.CurrentValues.Spd * deltaTime;
    }

    /// <summary>
    /// 戦闘員をプールへ返却します
    /// </summary>
    public void ReturnToPool()
    {
        if (_isReturning)
        {
            return;
        }

        _isReturning = true;
        UnregisterFromCrewRegistry();
        _releaseToPool?.Invoke(this);
    }

    /// <summary>
    /// 現在の移動方向を返します
    /// </summary>
    /// <returns>初期化時に設定された正規化済みの移動方向</returns>
    public Vector3 ResolveMovementDirection()
    {
        return _moveDirection;
    }

    /// <summary>
    /// 戦闘員レジストリへ登録します
    /// </summary>
    void RegisterToCrewRegistry()
    {
        if (_isRegistered || _pirateCrewRegistry == null || _pirateCrewTarget == null)
        {
            return;
        }

        _pirateCrewRegistry.Register(_pirateCrewTarget);
        _isRegistered = true;
    }

    /// <summary>
    /// 戦闘員レジストリから登録解除します
    /// </summary>
    void UnregisterFromCrewRegistry()
    {
        if (!_isRegistered || _pirateCrewRegistry == null || _pirateCrewTarget == null)
        {
            return;
        }

        _pirateCrewRegistry.Unregister(_pirateCrewTarget);
        _isRegistered = false;
    }

    /// <summary>
    /// 死亡時にプールへ返却します
    /// </summary>
    void OnDied()
    {
        ReturnToPool();
    }

    /// <summary>
    /// ベクトルをXZ平面へ射影します
    /// </summary>
    /// <param name="vector">射影対象</param>
    /// <returns>XZ平面ベクトル</returns>
    Vector3 ProjectToGround(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
