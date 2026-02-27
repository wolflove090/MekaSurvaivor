using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー全体の状態を統合管理するコンポーネント
/// </summary>
public class PlayerController : MonoBehaviour
{
    static PlayerController _instance;

    bool _isGameOver;

    HealthComponent _healthComponent;
    PlayerMovement _playerMovement;
    CharacterStats _characterStats;
    PlayerExperience _playerExperience;
    IPlayerStyleEffect _activeStyleEffect;
    PlayerStyleEffectContext _styleEffectContext;
    PlayerStyleEffectFactory _styleEffectFactory;
    float _moveSpeedMultiplier = 1f;

    WeaponBase _weapon;
    Dictionary<Type, WeaponBase> _weapons = new Dictionary<Type, WeaponBase>();

    /// <summary>
    /// PlayerControllerのシングルトンインスタンスを取得します
    /// </summary>
    public static PlayerController Instance
    {
        get => _instance;
    }

    /// <summary>
    /// 移動速度を取得または設定します
    /// </summary>
    public float MoveSpeed
    {
        get => _playerMovement != null ? _playerMovement.MoveSpeed : 0f;
        set
        {
            if (_playerMovement != null)
            {
                _playerMovement.MoveSpeed = value;
            }
        }
    }

    /// <summary>
    /// 現在のHPを取得します
    /// </summary>
    public int CurrentHp => _healthComponent != null ? _healthComponent.CurrentHp : 0;

    /// <summary>
    /// プレイヤーの攻撃力を取得します
    /// </summary>
    public int Pow => _characterStats != null ? _characterStats.Pow : 1;

    /// <summary>
    /// ゲームオーバー状態かどうかを取得します
    /// </summary>
    public bool IsGameOver => _isGameOver;

    /// <summary>
    /// プレイヤーの向いている方向を取得します
    /// </summary>
    /// <returns>向いている方向ベクトル（左または右）</returns>
    public Vector3 GetFacingDirection()
    {
        if (_playerMovement != null)
        {
            return _playerMovement.GetFacingDirection();
        }

        return Vector3.right;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"PlayerControllerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }
        _instance = this;

        if (GetComponent<PlayerInput>() == null)
        {
            gameObject.AddComponent<PlayerInput>();
        }

        _weapon = new BulletWeapon(transform, null);
        _healthComponent = GetComponent<HealthComponent>();
        _playerMovement = GetComponent<PlayerMovement>();
        _characterStats = GetComponent<CharacterStats>();
        _playerExperience = GetComponent<PlayerExperience>();
        _styleEffectFactory = new PlayerStyleEffectFactory();

        if (_characterStats == null)
        {
            _characterStats = gameObject.AddComponent<CharacterStats>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = gameObject.AddComponent<PlayerMovement>();
        }

        if (_playerExperience == null)
        {
            Debug.LogWarning("PlayerController: PlayerExperienceが見つからないため、スタイル効果の経験値倍率は適用されません。");
        }

        _isGameOver = false;
        BuildStyleEffectContext();

        if (_healthComponent != null)
        {
            _healthComponent.OnDied += GameOver;
            _healthComponent.OnDamaged += OnDamaged;
        }

        if (_characterStats != null)
        {
            _characterStats.OnStatsDataChanged += OnStatsDataChanged;
        }
        
        if (_playerMovement != null)
        {
            _playerMovement.OnMoved += OnMoved;
        }

        ApplyMoveSpeedFromStats();
    }

    void OnEnable()
    {
        ApplyMoveSpeedFromStats();
    }

    void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        _weapon.Update();

        if(_activeStyleEffect != null && _styleEffectContext != null)
        {
            _activeStyleEffect.Tick(_styleEffectContext, Time.deltaTime);
        }
    }

    public void ApplyWeaponUpgrade(WeaponUpgradeUiController.UpgradeCardType type)
    {
        switch (type)
        {
            case WeaponUpgradeUiController.UpgradeCardType.Shooter:
                {
                    if(!_weapons.TryGetValue(typeof(BulletWeapon), out WeaponBase target))
                    {
                        var newWeapon = new BulletWeapon(transform, _weapon);
                        _weapon = newWeapon;
                        _weapons[typeof(BulletWeapon)] = newWeapon;
                    }
                    else
                    {
                        target.LevelUp();
                    }
                }
                break;
            case WeaponUpgradeUiController.UpgradeCardType.Throwing:
                {
                    if(!_weapons.TryGetValue(typeof(ThrowingWeapon), out WeaponBase target))
                    {
                        var newWeapon = new ThrowingWeapon(transform, _weapon);
                        _weapon = newWeapon;
                        _weapons[typeof(ThrowingWeapon)] = newWeapon;
                    }
                    else
                    {
                        target.LevelUp();
                    }
                }
                break;
            case WeaponUpgradeUiController.UpgradeCardType.DamageField:
                {
                    if(!_weapons.TryGetValue(typeof(DamageFieldWeapon), out WeaponBase target))
                    {
                        var newWeapon = new DamageFieldWeapon(transform, _weapon);
                        _weapon = newWeapon;
                        _weapons[typeof(DamageFieldWeapon)] = newWeapon;                        
                    }
                    else
                    {
                        target.LevelUp();
                    }
                }                
                break;
            default:
                Debug.LogWarning($"PlayerController: 未対応のタイプです。 type={type}");
                break;
        }
    }

    /// <summary>
    /// プレイヤーのスタイル効果を切り替えます
    /// </summary>
    /// <param name="styleType">適用するスタイル種別</param>
    public void ChangeStyle(PlayerStyleType styleType)
    {
        if (_styleEffectContext == null)
        {
            BuildStyleEffectContext();
            if (_styleEffectContext == null)
            {
                Debug.LogWarning("PlayerController: スタイル効果コンテキストが未初期化のためスタイル変更できません。");
                return;
            }
        }

        ResetStyleParameters();

        try
        {
            _activeStyleEffect = _styleEffectFactory.Create(styleType);
            _activeStyleEffect.ApplyParameters(_styleEffectContext);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PlayerController: スタイル変更中にエラーが発生しました。 styleType={styleType}, message={ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 移動速度倍率を設定します
    /// </summary>
    /// <param name="multiplier">移動速度倍率</param>
    public void SetMoveSpeedMultiplier(float multiplier)
    {
        _moveSpeedMultiplier = Mathf.Max(0f, multiplier);
        ApplyMoveSpeedFromStats();
    }

    /// <summary>
    /// ダメージを受けた時のコールバック
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    void OnDamaged(int damage)
    {
        Debug.Log($"プレイヤーがダメージを受けました。残りHP: {CurrentHp}");
        GameEvents.RaisePlayerDamaged(damage);
    }

    /// <summary>
    /// ゲームオーバー処理を実行します
    /// </summary>
    void GameOver()
    {
        _isGameOver = true;
        if (_playerMovement != null)
        {
            _playerMovement.SetCanMove(false);
        }

        Debug.Log("ゲームオーバー！");
        GameEvents.RaisePlayerDied();
        GameEvents.RaiseGameOver();
        Time.timeScale = 0f;
    }

    void OnDestroy()
    {
        ResetStyleParameters();
        _activeStyleEffect = null;

        if (_healthComponent != null)
        {
            _healthComponent.OnDied -= GameOver;
            _healthComponent.OnDamaged -= OnDamaged;
        }

        if (_playerMovement != null)
        {
            _playerMovement.OnMoved -= OnMoved;
        }

        if (_characterStats != null)
        {
            _characterStats.OnStatsDataChanged -= OnStatsDataChanged;
        }

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 移動時のコールバック
    /// </summary>
    /// <param name="position">プレイヤーの現在位置</param>
    void OnMoved(Vector3 position)
    {
        if (_isGameOver)
        {
            return;
        }

        GameEvents.RaisePlayerMoved(position);
    }

    /// <summary>
    /// エネミーとのトリガー接触時の処理
    /// </summary>
    /// <param name="other">接触したCollider</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector3 knockbackDirection = transform.position - other.transform.position;
            knockbackDirection.y = 0f;
            CharacterStats enemyStats = other.GetComponent<CharacterStats>();
            int enemyPow = enemyStats != null ? enemyStats.Pow : 1;

            if (_healthComponent != null)
            {
                _healthComponent.TakeDamage(enemyPow, knockbackDirection);
            }
        }
    }

    /// <summary>
    /// ステータスから移動速度を反映します
    /// </summary>
    void ApplyMoveSpeedFromStats()
    {
        if (_playerMovement != null && _characterStats != null)
        {
            _playerMovement.MoveSpeed = _characterStats.Spd * _moveSpeedMultiplier;
        }
    }

    /// <summary>
    /// ステータス変更時に移動速度を再反映します
    /// </summary>
    void OnStatsDataChanged()
    {
        ApplyMoveSpeedFromStats();
    }

    /// <summary>
    /// スタイル効果で変更されるパラメータを基準値へ戻します
    /// </summary>
    void ResetStyleParameters()
    {
        SetMoveSpeedMultiplier(1f);
        _playerExperience?.ResetExperienceMultiplier();
    }

    /// <summary>
    /// スタイル効果に必要な参照コンテキストを構築します
    /// </summary>
    void BuildStyleEffectContext()
    {
        if (_healthComponent == null || _playerExperience == null)
        {
            _styleEffectContext = null;
            return;
        }

        _styleEffectContext = new PlayerStyleEffectContext(_healthComponent, _playerExperience, this);
    }
}
