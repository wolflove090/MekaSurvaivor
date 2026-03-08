using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// スタイル変更UIの表示とカード押下イベントを管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class StyleChangeUiController : MonoBehaviour
{
    const int DISPLAY_CARD_COUNT = 3;

    public enum StyleCardType
    {
        Miko = 0,
        Maid = 1,
        Nurse = 2,
        Pirate = 3,
        Cowgirl = 4,
        Celeb = 5
    }

    static readonly string[] CARD_TITLES = { "巫女", "メイド", "ナース", "海賊", "カウガール", "セレブ" };
    static readonly string[] CARD_DESCRIPTIONS =
    {
        "HP120 / DEF2 / POW10",
        "HP120 / DEF2 / POW12.5",
        "HP140 / DEF0 / POW10",
        "HP100 / DEF0 / POW20",
        "HP100 / DEF4 / POW15",
        "HP120 / DEF4 / POW10"
    };
    static readonly StyleCardType[] ALL_STYLE_TYPES =
    {
        StyleCardType.Miko,
        StyleCardType.Maid,
        StyleCardType.Nurse,
        StyleCardType.Pirate,
        StyleCardType.Cowgirl,
        StyleCardType.Celeb
    };

    [SerializeField]
    [Tooltip("スタイル変更UIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("スタイル変更UIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("巫女スタイルのステータス")]
    CharacterStatsData _mikoStyleStats;

    [SerializeField]
    [Tooltip("メイドスタイルのステータス")]
    CharacterStatsData _maidStyleStats;

    [SerializeField]
    [Tooltip("ナーススタイルのステータス")]
    CharacterStatsData _nurseStyleStats;

    [SerializeField]
    [Tooltip("海賊スタイルのステータス")]
    CharacterStatsData _pirateStyleStats;

    [SerializeField]
    [Tooltip("カウガールスタイルのステータス")]
    CharacterStatsData _cowgirlStyleStats;

    [SerializeField]
    [Tooltip("セレブスタイルのステータス")]
    CharacterStatsData _celebStyleStats;

    [SerializeField]
    [Tooltip("巫女スタイルの右向き画像")]
    Sprite _mikoRightSprite;

    [SerializeField]
    [Tooltip("メイドスタイルの右向き画像")]
    Sprite _maidRightSprite;

    [SerializeField]
    [Tooltip("ナーススタイルの右向き画像")]
    Sprite _nurseRightSprite;

    [SerializeField]
    [Tooltip("海賊スタイルの右向き画像")]
    Sprite _pirateRightSprite;

    [SerializeField]
    [Tooltip("カウガールスタイルの右向き画像")]
    Sprite _cowgirlRightSprite;

    [SerializeField]
    [Tooltip("セレブスタイルの右向き画像")]
    Sprite _celebRightSprite;

    [SerializeField]
    [Tooltip("スタイル変更UI表示中に適用するUIDocumentのSort Order")]
    float _sortOrderWhileOpen = 100f;

    UIDocument _uiDocument;
    Button[] _styleCards;
    Label[] _titleLabels;
    Label[] _descriptionLabels;
    Action[] _cardClickHandlers;
    StyleCardType[] _displayedStyleTypes;
    GameMessageBus _gameMessageBus;
    PlayerController _playerController;
    HealthComponent _playerHealthComponent;
    SpriteDirectionController _spriteDirectionController;
    bool _isStyleUiOpen;
    float _timeScaleBeforePause;
    float _defaultSortOrder;

    /// <summary>
    /// スタイルカードが押下された時に発火するイベント。
    /// </summary>
    public event Action<int> OnStyleCardSelected;

    /// <summary>
    /// 初期化時にUI構築とプレイヤー参照解決を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _defaultSortOrder = _uiDocument.sortingOrder;
        BuildUi();
        CacheElements();
        ApplyCardTexts();
        BuildCardClickHandlers();
        SetStyleUiVisible(false);
    }

    /// <summary>
    /// 有効化時にカード押下イベントを登録します。
    /// </summary>
    void OnEnable()
    {
        RegisterCallbacks();
        RegisterMessageBus();
    }

    /// <summary>
    /// 無効化時にカード押下イベントを解除します。
    /// </summary>
    void OnDisable()
    {
        if (_isStyleUiOpen)
        {
            CloseStyleUi();
        }

        UnregisterCallbacks();
        UnregisterMessageBus();
    }

    /// <summary>
    /// ブートストラップからプレイヤー参照を設定します。
    /// </summary>
    /// <param name="playerController">参照するプレイヤー</param>
    public void SetPlayer(PlayerController playerController)
    {
        _playerController = playerController;
        _playerHealthComponent = playerController != null ? playerController.GetComponent<HealthComponent>() : null;
        _spriteDirectionController = playerController != null ? playerController.GetComponent<SpriteDirectionController>() : null;
    }

    /// <summary>
    /// シーン内通知に使用するメッセージバスを設定します。
    /// </summary>
    /// <param name="gameMessageBus">設定する通知バス</param>
    public void SetMessageBus(GameMessageBus gameMessageBus)
    {
        if (_gameMessageBus == gameMessageBus)
        {
            return;
        }

        UnregisterMessageBus();
        _gameMessageBus = gameMessageBus;
        RegisterMessageBus();
    }

    /// <summary>
    /// UXMLとUSSを読み込み、UIのルートを構築します。
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

        Debug.LogWarning("StyleChangeUiController: Layout Assetが未設定です。");
    }

    /// <summary>
    /// UXML上のカードとラベル参照を取得します。
    /// </summary>
    void CacheElements()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        _styleCards = new[]
        {
            root.Q<Button>("upgrade-card-1"),
            root.Q<Button>("upgrade-card-2"),
            root.Q<Button>("upgrade-card-3")
        };

        _titleLabels = new[]
        {
            root.Q<Label>("upgrade-card-1-title"),
            root.Q<Label>("upgrade-card-2-title"),
            root.Q<Label>("upgrade-card-3-title")
        };

        _descriptionLabels = new[]
        {
            root.Q<Label>("upgrade-card-1-description"),
            root.Q<Label>("upgrade-card-2-description"),
            root.Q<Label>("upgrade-card-3-description")
        };

        _displayedStyleTypes = new StyleCardType[_styleCards.Length];
    }

    /// <summary>
    /// カードに表示する文言を設定します。
    /// </summary>
    void ApplyCardTexts()
    {
        for (int i = 0; i < _styleCards.Length; i++)
        {
            bool hasCandidate = _displayedStyleTypes != null && i < _displayedStyleTypes.Length;
            StyleCardType cardType = hasCandidate ? _displayedStyleTypes[i] : default;

            if (_titleLabels != null && i < _titleLabels.Length && _titleLabels[i] != null)
            {
                _titleLabels[i].text = hasCandidate ? GetStyleName(cardType) : "-";
            }

            if (_descriptionLabels != null && i < _descriptionLabels.Length && _descriptionLabels[i] != null)
            {
                _descriptionLabels[i].text = hasCandidate ? GetStyleDescription(cardType) : string.Empty;
            }

            if (_styleCards != null && i < _styleCards.Length && _styleCards[i] != null)
            {
                _styleCards[i].SetEnabled(hasCandidate);
            }
        }
    }

    /// <summary>
    /// 押下イベント登録に使用するハンドラを事前に生成します。
    /// </summary>
    void BuildCardClickHandlers()
    {
        _cardClickHandlers = new Action[_styleCards != null ? _styleCards.Length : CARD_TITLES.Length];

        for (int i = 0; i < _cardClickHandlers.Length; i++)
        {
            int index = i;
            _cardClickHandlers[i] = () => OnCardClicked(index);
        }
    }

    /// <summary>
    /// カードボタンに押下コールバックを登録します。
    /// </summary>
    void RegisterCallbacks()
    {
        if (_styleCards == null || _cardClickHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _styleCards.Length; i++)
        {
            Button card = _styleCards[i];
            if (card == null)
            {
                continue;
            }

            card.clicked += _cardClickHandlers[i];
        }
    }

    /// <summary>
    /// カードボタンから押下コールバックを解除します。
    /// </summary>
    void UnregisterCallbacks()
    {
        if (_styleCards == null || _cardClickHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _styleCards.Length; i++)
        {
            Button card = _styleCards[i];
            if (card == null)
            {
                continue;
            }

            card.clicked -= _cardClickHandlers[i];
        }
    }

    /// <summary>
    /// カード押下時に対応するスタイルを適用します。
    /// </summary>
    /// <param name="cardIndex">押下されたカードのインデックス（0始まり）</param>
    void OnCardClicked(int cardIndex)
    {
        if (!_isStyleUiOpen)
        {
            return;
        }

        if (!TryApplyStyle(cardIndex))
        {
            return;
        }

        TryChangePlayerStyle(cardIndex);
        OnStyleCardSelected?.Invoke(cardIndex);
        CloseStyleUi();
    }

    /// <summary>
    /// カード種別に応じたスタイルを適用します。
    /// </summary>
    /// <param name="cardIndex">押下されたカードのインデックス（0始まり）</param>
    /// <returns>適用に成功した場合はtrue</returns>
    bool TryApplyStyle(int cardIndex)
    {
        if (_playerHealthComponent == null)
        {
            Debug.LogWarning("StyleChangeUiController: HealthComponentが見つからないためスタイルを適用できません。");
            return false;
        }

        if (!TryGetDisplayedStyleType(cardIndex, out StyleCardType styleType))
        {
            Debug.LogWarning($"StyleChangeUiController: 未対応のカードインデックスです。 index={cardIndex}");
            return false;
        }

        CharacterStatsData statsData = GetStyleStatsData(styleType);
        Sprite rightSprite = GetRightSprite(styleType);

        if (statsData == null)
        {
            string styleName = GetStyleName(styleType);
            Debug.LogWarning($"StyleChangeUiController: {styleName}のCharacterStatsDataが未設定です。");
            return false;
        }

        _playerHealthComponent.ApplyStatsDataAndKeepConsumedHp(statsData);

        if (rightSprite == null)
        {
            string styleName = GetStyleName(styleType);
            Debug.LogWarning($"StyleChangeUiController: {styleName}の右向き画像が未設定です。現在の画像を維持します。");
            return true;
        }

        if (_spriteDirectionController == null)
        {
            Debug.LogWarning("StyleChangeUiController: SpriteDirectionControllerが見つからないため画像を更新できません。");
            return true;
        }

        _spriteDirectionController.ApplyRightFacingSprite(rightSprite);
        return true;
    }

    /// <summary>
    /// カード選択に対応するスタイル効果をプレイヤーへ適用します。
    /// </summary>
    /// <param name="cardIndex">押下されたカードのインデックス（0始まり）</param>
    void TryChangePlayerStyle(int cardIndex)
    {
        if (_playerController == null)
        {
            Debug.LogWarning("StyleChangeUiController: PlayerController参照が未解決のためスタイル効果を適用できません。");
            return;
        }

        if (!TryConvertToPlayerStyleType(cardIndex, out PlayerStyleType styleType))
        {
            Debug.LogWarning($"StyleChangeUiController: PlayerStyleTypeへ変換できないカードインデックスです。 index={cardIndex}");
            return;
        }

        _playerController.ChangeStyle(styleType);
    }

    /// <summary>
    /// カードインデックスをプレイヤースタイル種別へ変換します。
    /// </summary>
    /// <param name="cardIndex">カードのインデックス</param>
    /// <param name="styleType">変換後のスタイル種別</param>
    /// <returns>変換に成功した場合はtrue</returns>
    bool TryConvertToPlayerStyleType(int cardIndex, out PlayerStyleType styleType)
    {
        if (!TryGetDisplayedStyleType(cardIndex, out StyleCardType displayedType))
        {
            styleType = default;
            return false;
        }

        switch (displayedType)
        {
            case StyleCardType.Miko:
                styleType = PlayerStyleType.Miko;
                return true;
            case StyleCardType.Maid:
                styleType = PlayerStyleType.Maid;
                return true;
            case StyleCardType.Nurse:
                styleType = PlayerStyleType.Nurse;
                return true;
            case StyleCardType.Pirate:
                styleType = PlayerStyleType.Pirate;
                return true;
            case StyleCardType.Cowgirl:
                styleType = PlayerStyleType.Cowgirl;
                return true;
            case StyleCardType.Celeb:
                styleType = PlayerStyleType.Celeb;
                return true;
            default:
                styleType = default;
                return false;
        }
    }

    /// <summary>
    /// レベルアップ時に4,7,10...でスタイル変更UIを開きます。
    /// </summary>
    /// <param name="newLevel">レベルアップ後のレベル</param>
    void OnPlayerLevelUp(int newLevel)
    {
        if (!LevelUpUiDisplayRule.ShouldOpenStyleUi(newLevel))
        {
            return;
        }

        OpenStyleUi();
    }

    /// <summary>
    /// メッセージバスの購読を登録します。
    /// </summary>
    void RegisterMessageBus()
    {
        if (!isActiveAndEnabled || _gameMessageBus == null)
        {
            return;
        }

        _gameMessageBus.PlayerLevelUp -= OnPlayerLevelUp;
        _gameMessageBus.PlayerLevelUp += OnPlayerLevelUp;
    }

    /// <summary>
    /// メッセージバスの購読を解除します。
    /// </summary>
    void UnregisterMessageBus()
    {
        if (_gameMessageBus != null)
        {
            _gameMessageBus.PlayerLevelUp -= OnPlayerLevelUp;
        }
    }

    /// <summary>
    /// スタイル変更UIを表示し、ゲーム進行を一時停止します。
    /// </summary>
    void OpenStyleUi()
    {
        if (_isStyleUiOpen)
        {
            return;
        }

        UpdateDisplayedStyleCandidates();
        _isStyleUiOpen = true;
        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        ApplyFrontmostSortOrder();
        SetStyleUiVisible(true);
    }

    /// <summary>
    /// スタイル変更UIを非表示にし、ゲーム進行を再開します。
    /// </summary>
    void CloseStyleUi()
    {
        if (!_isStyleUiOpen)
        {
            return;
        }

        _isStyleUiOpen = false;
        SetStyleUiVisible(false);
        RestoreDefaultSortOrder();
        Time.timeScale = _timeScaleBeforePause > 0f ? _timeScaleBeforePause : 1f;
    }

    /// <summary>
    /// スタイル変更UIの表示状態を切り替えます。
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue</param>
    void SetStyleUiVisible(bool isVisible)
    {
        VisualElement root = _uiDocument?.rootVisualElement;
        if (root == null)
        {
            return;
        }

        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// スタイル変更UI表示中の描画順を最前面へ引き上げます。
    /// </summary>
    void ApplyFrontmostSortOrder()
    {
        if (_uiDocument == null)
        {
            return;
        }

        _uiDocument.sortingOrder = Mathf.Max(_defaultSortOrder, _sortOrderWhileOpen);
        _uiDocument.rootVisualElement?.BringToFront();
    }

    /// <summary>
    /// スタイル変更UI非表示時に描画順を初期値へ戻します。
    /// </summary>
    void RestoreDefaultSortOrder()
    {
        if (_uiDocument == null)
        {
            return;
        }

        _uiDocument.sortingOrder = _defaultSortOrder;
    }

    /// <summary>
    /// カード種別に対応するスタイル名を取得します。
    /// </summary>
    /// <param name="cardType">カード種別</param>
    /// <returns>スタイル名</returns>
    string GetStyleName(StyleCardType cardType)
    {
        int index = (int)cardType;
        return index >= 0 && index < CARD_TITLES.Length ? CARD_TITLES[index] : "不明";
    }

    /// <summary>
    /// カード種別に対応する説明文を取得します。
    /// </summary>
    /// <param name="cardType">カード種別</param>
    /// <returns>説明文</returns>
    string GetStyleDescription(StyleCardType cardType)
    {
        int index = (int)cardType;
        return index >= 0 && index < CARD_DESCRIPTIONS.Length ? CARD_DESCRIPTIONS[index] : string.Empty;
    }

    /// <summary>
    /// カード種別に対応するステータスデータを取得します。
    /// </summary>
    /// <param name="cardType">カード種別</param>
    /// <returns>ステータスデータ</returns>
    CharacterStatsData GetStyleStatsData(StyleCardType cardType)
    {
        switch (cardType)
        {
            case StyleCardType.Miko:
                return _mikoStyleStats;
            case StyleCardType.Maid:
                return _maidStyleStats;
            case StyleCardType.Nurse:
                return _nurseStyleStats;
            case StyleCardType.Pirate:
                return _pirateStyleStats;
            case StyleCardType.Cowgirl:
                return _cowgirlStyleStats;
            case StyleCardType.Celeb:
                return _celebStyleStats;
            default:
                return null;
        }
    }

    /// <summary>
    /// カード種別に対応する右向き画像を取得します。
    /// </summary>
    /// <param name="cardType">カード種別</param>
    /// <returns>右向き画像</returns>
    Sprite GetRightSprite(StyleCardType cardType)
    {
        switch (cardType)
        {
            case StyleCardType.Miko:
                return _mikoRightSprite;
            case StyleCardType.Maid:
                return _maidRightSprite;
            case StyleCardType.Nurse:
                return _nurseRightSprite;
            case StyleCardType.Pirate:
                return _pirateRightSprite;
            case StyleCardType.Cowgirl:
                return _cowgirlRightSprite;
            case StyleCardType.Celeb:
                return _celebRightSprite;
            default:
                return null;
        }
    }

    /// <summary>
    /// 現在表示するスタイル候補をランダム抽選し、カード文言を更新します。
    /// </summary>
    void UpdateDisplayedStyleCandidates()
    {
        if (_displayedStyleTypes == null || _displayedStyleTypes.Length == 0)
        {
            return;
        }

        IReadOnlyList<StyleCardType> candidates = BuildRandomizedCandidates(ALL_STYLE_TYPES);
        int count = Mathf.Min(_displayedStyleTypes.Length, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            _displayedStyleTypes[i] = candidates[i];
        }

        ApplyCardTexts();
    }

    /// <summary>
    /// 候補一覧から重複なしで最大3件をランダム抽選します。
    /// </summary>
    /// <param name="availableTypes">抽選元候補</param>
    /// <returns>表示候補一覧</returns>
    public static IReadOnlyList<StyleCardType> BuildRandomizedCandidates(IReadOnlyList<StyleCardType> availableTypes)
    {
        if (availableTypes == null || availableTypes.Count == 0)
        {
            return Array.Empty<StyleCardType>();
        }

        System.Collections.Generic.List<StyleCardType> pool =
            new System.Collections.Generic.List<StyleCardType>(availableTypes.Count);

        for (int i = 0; i < availableTypes.Count; i++)
        {
            StyleCardType type = availableTypes[i];
            if (!pool.Contains(type))
            {
                pool.Add(type);
            }
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[swapIndex]) = (pool[swapIndex], pool[i]);
        }

        int resultCount = Mathf.Min(DISPLAY_CARD_COUNT, pool.Count);
        StyleCardType[] result = new StyleCardType[resultCount];
        for (int i = 0; i < resultCount; i++)
        {
            result[i] = pool[i];
        }

        return result;
    }

    /// <summary>
    /// 表示中カードインデックスに対応するスタイル種別を取得します。
    /// </summary>
    /// <param name="cardIndex">表示中カードインデックス</param>
    /// <param name="styleType">対応するスタイル種別</param>
    /// <returns>取得できた場合はtrue</returns>
    bool TryGetDisplayedStyleType(int cardIndex, out StyleCardType styleType)
    {
        if (_displayedStyleTypes != null && cardIndex >= 0 && cardIndex < _displayedStyleTypes.Length)
        {
            styleType = _displayedStyleTypes[cardIndex];
            return true;
        }

        styleType = default;
        return false;
    }

}
