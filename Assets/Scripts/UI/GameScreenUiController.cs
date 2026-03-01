using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// プレイヤーステータス表示UIの構築と更新を管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class GameScreenUiController : MonoBehaviour
{
    const string UnknownGameTimeText = "--:--";

    [SerializeField]
    [Tooltip("プレイヤーステータスUIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("プレイヤーステータスUIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("参照するプレイヤーコントローラー。未設定時はシーンから補完")]
    PlayerController _playerController;

    [SerializeField]
    [Tooltip("参照するプレイヤー経験値コンポーネント。未設定時はシーンから補完")]
    PlayerExperience _playerExperience;

    UIDocument _uiDocument;
    HealthComponent _healthComponent;
    CharacterStats _characterStats;
    GameManager _gameManager;

    Label _gameTimeValueLabel;
    Label _levelValueLabel;
    Label _hpValueLabel;
    Label _expValueLabel;
    Label _powValueLabel;
    Label _defValueLabel;
    Label _spdValueLabel;

    int _cachedLevel = int.MinValue;
    int _cachedCurrentHp = int.MinValue;
    int _cachedMaxHp = int.MinValue;
    int _cachedCurrentExperience = int.MinValue;
    int _cachedExperienceToNext = int.MinValue;
    int _cachedPow = int.MinValue;
    int _cachedDef = int.MinValue;
    float _cachedSpd = float.MinValue;
    string _cachedGameTimeText;

    /// <summary>
    /// 初期化時にUI構築と参照解決を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        BuildUi();
        CacheElements();
        ResolvePlayerReferences();
        RefreshStatusUi(true);
    }

    /// <summary>
    /// 有効化時にイベント登録とUI更新を行います。
    /// </summary>
    void OnEnable()
    {
        RegisterEvents();
        RefreshStatusUi(true);
    }

    /// <summary>
    /// 無効化時にイベント登録を解除します。
    /// </summary>
    void OnDisable()
    {
        UnregisterEvents();
    }

    /// <summary>
    /// 毎フレーム、ステータスの差分を監視してUIを更新します。
    /// </summary>
    void Update()
    {
        RefreshStatusUi(false);
    }

    /// <summary>
    /// UXMLとUSSを読み込み、UIルートを構築します。
    /// </summary>
    void BuildUi()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        root.Clear();

        if (_styleSheet != null && !root.styleSheets.Contains(_styleSheet))
        {
            root.styleSheets.Add(_styleSheet);
        }

        if (_layoutAsset != null)
        {
            _layoutAsset.CloneTree(root);
            return;
        }

        Debug.LogWarning("GameScreenUiController: Layout Assetが未設定です。");
    }

    /// <summary>
    /// UXML上の表示ラベル参照を取得します。
    /// </summary>
    void CacheElements()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        _gameTimeValueLabel = root.Q<Label>("game-time-value");
        _levelValueLabel = root.Q<Label>("status-level-value");
        _hpValueLabel = root.Q<Label>("status-hp-value");
        _expValueLabel = root.Q<Label>("status-exp-value");
        _powValueLabel = root.Q<Label>("status-pow-value");
        _defValueLabel = root.Q<Label>("status-def-value");
        _spdValueLabel = root.Q<Label>("status-spd-value");
    }

    /// <summary>
    /// プレイヤー関連コンポーネント参照を解決します。
    /// </summary>
    void ResolvePlayerReferences()
    {
        if (_playerController == null)
        {
            _playerController = PlayerController.Instance != null
                ? PlayerController.Instance
                : FindFirstObjectByType<PlayerController>();
        }

        if (_playerController == null)
        {
            return;
        }

        if (_playerExperience == null)
        {
            _playerExperience = _playerController.GetComponent<PlayerExperience>();

            if (_playerExperience == null)
            {
                _playerExperience = FindFirstObjectByType<PlayerExperience>();
            }
        }

        _healthComponent = _playerController.GetComponent<HealthComponent>();
        _characterStats = _playerController.GetComponent<CharacterStats>();
    }

    /// <summary>
    /// ゲーム進行管理コンポーネント参照を解決します。
    /// </summary>
    void ResolveGameManagerReference()
    {
        if (_gameManager == null)
        {
            _gameManager = GameManager.Instance != null
                ? GameManager.Instance
                : FindFirstObjectByType<GameManager>();
        }
    }

    /// <summary>
    /// ステータス更新に使用するイベントを登録します。
    /// </summary>
    void RegisterEvents()
    {
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        GameEvents.OnExperienceGained += OnExperienceGained;
        GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
        GameEvents.OnPlayerDied += OnPlayerDied;
    }

    /// <summary>
    /// ステータス更新に使用するイベントを解除します。
    /// </summary>
    void UnregisterEvents()
    {
        GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        GameEvents.OnExperienceGained -= OnExperienceGained;
        GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }

    /// <summary>
    /// ダメージイベント受信時にUIを更新します。
    /// </summary>
    /// <param name="damage">受けたダメージ量</param>
    void OnPlayerDamaged(int damage)
    {
        RefreshStatusUi(true);
    }

    /// <summary>
    /// 経験値獲得イベント受信時にUIを更新します。
    /// </summary>
    /// <param name="gainedAmount">今回獲得した経験値</param>
    /// <param name="currentExperience">現在経験値</param>
    /// <param name="experienceToNextLevel">次レベルまでに必要な経験値</param>
    void OnExperienceGained(int gainedAmount, int currentExperience, int experienceToNextLevel)
    {
        RefreshStatusUi(true);
    }

    /// <summary>
    /// レベルアップイベント受信時にUIを更新します。
    /// </summary>
    /// <param name="newLevel">新しいレベル</param>
    void OnPlayerLevelUp(int newLevel)
    {
        RefreshStatusUi(true);
    }

    /// <summary>
    /// プレイヤー死亡イベント受信時にUIを更新します。
    /// </summary>
    void OnPlayerDied()
    {
        RefreshStatusUi(true);
    }

    /// <summary>
    /// 現在のステータス値を取得し、差分があればUIに反映します。
    /// </summary>
    /// <param name="force">強制更新する場合はtrue</param>
    void RefreshStatusUi(bool force)
    {
        ResolvePlayerReferences();
        ResolveGameManagerReference();
        RefreshGameTimeUi(force);

        if (_playerController == null)
        {
            return;
        }

        int currentLevel = _playerExperience != null ? _playerExperience.CurrentLevel : 1;
        int currentHp = _healthComponent != null ? _healthComponent.CurrentHp : _playerController.CurrentHp;
        int maxHp = _healthComponent != null ? _healthComponent.MaxHp : currentHp;
        int currentExperience = _playerExperience != null ? _playerExperience.CurrentExperience : 0;
        int experienceToNext = _playerExperience != null ? _playerExperience.ExperienceToNextLevel : 0;
        int pow = _characterStats != null ? _characterStats.Pow : _playerController.Pow;
        int def = _characterStats != null ? _characterStats.Def : 0;
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

        SetLabelText(_levelValueLabel, currentLevel.ToString());
        SetLabelText(_hpValueLabel, $"{currentHp}/{maxHp}");
        SetLabelText(_expValueLabel, $"{currentExperience}/{experienceToNext}");
        SetLabelText(_powValueLabel, pow.ToString());
        SetLabelText(_defValueLabel, def.ToString());
        SetLabelText(_spdValueLabel, spd.ToString("0.0"));
    }

    /// <summary>
    /// ブートストラップから参照を設定します。
    /// </summary>
    /// <param name="playerController">参照するプレイヤーコントローラー</param>
    /// <param name="playerExperience">参照するプレイヤー経験値</param>
    /// <param name="gameManager">参照するゲーム進行管理</param>
    public void SetReferences(PlayerController playerController, PlayerExperience playerExperience, GameManager gameManager)
    {
        _playerController = playerController;
        _playerExperience = playerExperience;
        _gameManager = gameManager;

        if (_playerController != null)
        {
            _healthComponent = _playerController.GetComponent<HealthComponent>();
            _characterStats = _playerController.GetComponent<CharacterStats>();
        }
        else
        {
            _healthComponent = null;
            _characterStats = null;
        }
    }

    /// <summary>
    /// 残り時間表示を差分更新で反映します。
    /// </summary>
    /// <param name="force">強制更新する場合はtrue</param>
    void RefreshGameTimeUi(bool force)
    {
        string nextGameTimeText = _gameManager != null
            ? FormatGameTime(_gameManager.RemainingTime)
            : UnknownGameTimeText;

        if (!force && _cachedGameTimeText == nextGameTimeText)
        {
            return;
        }

        _cachedGameTimeText = nextGameTimeText;
        SetLabelText(_gameTimeValueLabel, nextGameTimeText);
    }

    /// <summary>
    /// 残り時間（秒）をMM:SS形式へ変換します。
    /// </summary>
    /// <param name="remainingTime">残り時間（秒）</param>
    /// <returns>MM:SS形式の文字列</returns>
    string FormatGameTime(float remainingTime)
    {
        int totalSeconds = Mathf.FloorToInt(Mathf.Max(0f, remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// ラベルが存在する場合のみテキストを反映します。
    /// </summary>
    /// <param name="targetLabel">反映先ラベル</param>
    /// <param name="value">表示文字列</param>
    void SetLabelText(Label targetLabel, string value)
    {
        if (targetLabel != null)
        {
            targetLabel.text = value;
        }
    }
}
