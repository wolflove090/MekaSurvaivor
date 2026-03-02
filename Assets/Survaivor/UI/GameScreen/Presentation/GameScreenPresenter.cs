using UnityEngine;

/// <summary>
/// HUD 表示に必要な状態収集と差分更新を管理します。
/// </summary>
public class GameScreenPresenter
{
    readonly GameScreenUiController _view;
    readonly PlayerController _playerController;
    readonly PlayerExperience _playerExperience;
    readonly GameManager _gameManager;
    readonly GameMessageBus _messageBus;
    readonly GameStatsTracker _gameStatsTracker;
    readonly ResultUiController _resultUiController;

    readonly HealthComponent _healthComponent;
    readonly CharacterStats _characterStats;

    int _cachedLevel = int.MinValue;
    int _cachedCurrentHp = int.MinValue;
    int _cachedMaxHp = int.MinValue;
    int _cachedCurrentExperience = int.MinValue;
    int _cachedExperienceToNext = int.MinValue;
    int _cachedPow = int.MinValue;
    int _cachedDef = int.MinValue;
    float _cachedSpd = float.MinValue;
    string _cachedGameTimeText;
    bool _isActive;
    bool _forceRefresh = true;
    bool _isResultShown;

    /// <summary>
    /// Presenter を初期化します。
    /// </summary>
    /// <param name="view">表示先の UI</param>
    /// <param name="playerController">参照するプレイヤー</param>
    /// <param name="playerExperience">参照する経験値管理</param>
    /// <param name="gameManager">参照するゲーム進行管理</param>
    /// <param name="messageBus">局所通知ハブ</param>
    /// <param name="gameStatsTracker">参照するプレイ統計</param>
    /// <param name="resultUiController">参照するリザルトUI</param>
    public GameScreenPresenter(
        GameScreenUiController view,
        PlayerController playerController,
        PlayerExperience playerExperience,
        GameManager gameManager,
        GameMessageBus messageBus,
        GameStatsTracker gameStatsTracker,
        ResultUiController resultUiController)
    {
        _view = view;
        _playerController = playerController;
        _playerExperience = playerExperience;
        _gameManager = gameManager;
        _messageBus = messageBus;
        _gameStatsTracker = gameStatsTracker;
        _resultUiController = resultUiController;
        _healthComponent = _playerController != null ? _playerController.GetComponent<HealthComponent>() : null;
        _characterStats = _playerController != null ? _playerController.GetComponent<CharacterStats>() : null;
    }

    /// <summary>
    /// Presenter の通知購読を開始します。
    /// </summary>
    public void Activate()
    {
        if (_isActive)
        {
            return;
        }

        _isActive = true;
        _isResultShown = false;
        _resultUiController?.Hide();

        if (_messageBus != null)
        {
            _messageBus.PlayerDamaged += OnStatusChanged;
            _messageBus.ExperienceGained += OnExperienceGained;
            _messageBus.PlayerLevelUp += OnLevelUp;
            _messageBus.PlayerDied += OnPlayerDied;
            _messageBus.GameCleared += OnGameFlowChanged;
            _messageBus.GameOver += OnGameFlowChanged;
        }

        Refresh(true);
    }

    /// <summary>
    /// Presenter の通知購読を停止します。
    /// </summary>
    public void Deactivate()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;

        if (_messageBus != null)
        {
            _messageBus.PlayerDamaged -= OnStatusChanged;
            _messageBus.ExperienceGained -= OnExperienceGained;
            _messageBus.PlayerLevelUp -= OnLevelUp;
            _messageBus.PlayerDied -= OnPlayerDied;
            _messageBus.GameCleared -= OnGameFlowChanged;
            _messageBus.GameOver -= OnGameFlowChanged;
        }
    }

    /// <summary>
    /// 毎フレームの HUD 更新を実行します。
    /// </summary>
    public void Tick()
    {
        Refresh(_forceRefresh);
    }

    void OnStatusChanged(int damage)
    {
        _forceRefresh = true;
    }

    void OnExperienceGained(int gainedAmount, int currentExperience, int experienceToNextLevel)
    {
        _forceRefresh = true;
    }

    void OnLevelUp(int newLevel)
    {
        _forceRefresh = true;
    }

    void OnPlayerDied()
    {
        _forceRefresh = true;
    }

    void OnGameFlowChanged()
    {
        ShowResultIfNeeded();
        _forceRefresh = true;
    }

    /// <summary>
    /// 終了通知時に未表示ならリザルトUIを表示します。
    /// </summary>
    void ShowResultIfNeeded()
    {
        if (_isResultShown)
        {
            return;
        }

        _isResultShown = true;
        _resultUiController?.Show("撤退", _gameStatsTracker != null ? _gameStatsTracker.KillCount : 0);
    }

    void Refresh(bool force)
    {
        _forceRefresh = false;
        RefreshGameTime(force);
        RefreshStatus(force);
    }

    void RefreshGameTime(bool force)
    {
        string nextGameTimeText = _gameManager != null
            ? FormatGameTime(_gameManager.RemainingTime)
            : GameScreenUiController.UnknownGameTimeText;

        if (!force && _cachedGameTimeText == nextGameTimeText)
        {
            return;
        }

        _cachedGameTimeText = nextGameTimeText;
        _view?.SetGameTimeText(nextGameTimeText);
    }

    void RefreshStatus(bool force)
    {
        if (_playerController == null || _view == null)
        {
            return;
        }

        int currentLevel = _playerExperience != null ? _playerExperience.CurrentLevel : 1;
        int currentHp = _healthComponent != null ? _healthComponent.CurrentHp : _playerController.CurrentHp;
        int maxHp = _healthComponent != null ? _healthComponent.MaxHp : currentHp;
        int currentExperience = _playerExperience != null ? _playerExperience.CurrentExperience : 0;
        int experienceToNext = _playerExperience != null ? _playerExperience.ExperienceToNextLevel : 0;
        CharacterStatValues characterValues = _characterStats != null ? _characterStats.CurrentValues : CharacterStatValues.Default;
        int pow = _characterStats != null ? characterValues.Pow : _playerController.Pow;
        int def = _characterStats != null ? characterValues.Def : CharacterStatValues.Default.Def;
        float spd = _playerController.MoveSpeed;

        bool hasChanged =
            force ||
            _cachedLevel != currentLevel ||
            _cachedCurrentHp != currentHp ||
            _cachedMaxHp != maxHp ||
            _cachedCurrentExperience != currentExperience ||
            _cachedExperienceToNext != experienceToNext ||
            _cachedPow != pow ||
            _cachedDef != def ||
            !Mathf.Approximately(_cachedSpd, spd);

        if (!hasChanged)
        {
            return;
        }

        _cachedLevel = currentLevel;
        _cachedCurrentHp = currentHp;
        _cachedMaxHp = maxHp;
        _cachedCurrentExperience = currentExperience;
        _cachedExperienceToNext = experienceToNext;
        _cachedPow = pow;
        _cachedDef = def;
        _cachedSpd = spd;

        _view.SetStatusText(
            currentLevel.ToString(),
            $"{currentHp}/{maxHp}",
            $"{currentExperience}/{experienceToNext}",
            pow.ToString(),
            def.ToString(),
            spd.ToString("0.0"));
    }

    string FormatGameTime(float remainingTime)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
