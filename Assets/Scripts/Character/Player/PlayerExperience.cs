using UnityEngine;

/// <summary>
/// プレイヤーの経験値とレベルを管理するコンポーネント
/// </summary>
public class PlayerExperience : MonoBehaviour
{
    [Header("初期設定")]
    [SerializeField]
    [Tooltip("開始レベル")]
    int _startLevel = 1;

    [SerializeField]
    [Tooltip("レベル1から2に必要な基礎経験値")]
    int _baseExperienceRequired = 10;

    [SerializeField]
    [Tooltip("レベルごとの経験値増加率")]
    float _experienceScaling = 1.5f;

    PlayerState _playerState;
    PlayerProgressionService _playerProgressionService;

    /// <summary>
    /// プレイヤー進行状態を取得します
    /// </summary>
    public PlayerState State
    {
        get
        {
            EnsureInitialized();
            return _playerState;
        }
    }

    /// <summary>
    /// 現在のレベルを取得します
    /// </summary>
    public int CurrentLevel
    {
        get
        {
            EnsureInitialized();
            return _playerState != null ? _playerState.CurrentLevel : Mathf.Max(1, _startLevel);
        }
    }

    /// <summary>
    /// 現在の経験値を取得します
    /// </summary>
    public int CurrentExperience
    {
        get
        {
            EnsureInitialized();
            return _playerState != null ? _playerState.CurrentExperience : 0;
        }
    }

    /// <summary>
    /// 次のレベルまでに必要な経験値を取得します
    /// </summary>
    public int ExperienceToNextLevel
    {
        get
        {
            EnsureInitialized();
            return _playerState != null ? _playerState.ExperienceToNextLevel : 0;
        }
    }

    /// <summary>
    /// 経験値倍率を取得します
    /// </summary>
    public float ExperienceMultiplier
    {
        get
        {
            EnsureInitialized();
            return _playerState != null ? _playerState.ExperienceMultiplier : 1f;
        }
    }

    void Awake()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// 経験値を追加します
    /// </summary>
    /// <param name="amount">追加する経験値量</param>
    public void AddExperience(int amount)
    {
        EnsureInitialized();
        _playerProgressionService?.AddExperience(amount);
    }

    /// <summary>
    /// 経験値倍率を設定します
    /// </summary>
    /// <param name="multiplier">設定する倍率</param>
    public void SetExperienceMultiplier(float multiplier)
    {
        EnsureInitialized();
        _playerProgressionService?.SetExperienceMultiplier(multiplier);
    }

    /// <summary>
    /// 経験値倍率を初期値へ戻します
    /// </summary>
    public void ResetExperienceMultiplier()
    {
        EnsureInitialized();
        _playerProgressionService?.ResetExperienceMultiplier();
    }

    /// <summary>
    /// プレイヤーのスタイルを変更します
    /// </summary>
    /// <param name="styleType">適用するスタイル種別</param>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ChangeStyle(PlayerStyleType styleType, PlayerStyleEffectContext context)
    {
        EnsureInitialized();
        _playerProgressionService?.ChangeStyle(styleType, context);
    }

    /// <summary>
    /// 現在のスタイル効果を更新します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void TickStyleEffect(PlayerStyleEffectContext context, float deltaTime)
    {
        EnsureInitialized();
        _playerProgressionService?.TickStyleEffect(context, deltaTime);
    }

    /// <summary>
    /// スタイル効果で変更されるパラメータを基準値へ戻します
    /// </summary>
    public void ResetStyleParameters()
    {
        EnsureInitialized();
        _playerProgressionService?.ResetStyleParameters();
    }

    /// <summary>
    /// 経験値獲得イベントを通知します
    /// </summary>
    /// <param name="gainedAmount">加算後の経験値量</param>
    /// <param name="currentExperience">現在経験値</param>
    /// <param name="experienceToNextLevel">次のレベルまでに必要な経験値</param>
    void OnExperienceGained(int gainedAmount, int currentExperience, int experienceToNextLevel)
    {
        GameEvents.RaiseExperienceGained(gainedAmount, currentExperience, experienceToNextLevel);
    }

    /// <summary>
    /// レベルアップイベントを通知します
    /// </summary>
    /// <param name="currentLevel">現在レベル</param>
    void OnLevelUp(int currentLevel)
    {
        Debug.Log($"レベルアップ！ レベル {currentLevel} に到達しました");
        GameEvents.RaisePlayerLevelUp(currentLevel);
    }

    void EnsureInitialized()
    {
        if (_playerState != null && _playerProgressionService != null)
        {
            return;
        }

        _playerState = new PlayerState(_startLevel);
        _playerProgressionService = new PlayerProgressionService(
            _playerState,
            _baseExperienceRequired,
            _experienceScaling,
            OnExperienceGained,
            OnLevelUp);
    }
}
