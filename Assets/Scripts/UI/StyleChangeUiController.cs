using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// スタイル変更UIの表示とカード押下イベントを管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class StyleChangeUiController : MonoBehaviour
{
    enum StyleCardType
    {
        Miko = 0,
        Maid = 1,
        Celeb = 2
    }

    static readonly string[] CARD_TITLES = { "巫女", "メイド", "セレブ" };
    static readonly string[] CARD_DESCRIPTIONS =
    {
        "神秘の加護でバランスよく戦う",
        "俊敏な所作で素早く立ち回る",
        "豪華絢爛に攻撃力を押し上げる"
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
    [Tooltip("セレブスタイルのステータス")]
    CharacterStatsData _celebStyleStats;

    [SerializeField]
    [Tooltip("スタイル変更UI表示中に適用するUIDocumentのSort Order")]
    float _sortOrderWhileOpen = 100f;

    UIDocument _uiDocument;
    Button[] _styleCards;
    Label[] _titleLabels;
    Label[] _descriptionLabels;
    Action[] _cardClickHandlers;
    HealthComponent _playerHealthComponent;
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
        ResolvePlayerReferences();
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
        GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
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
        GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
    }

    /// <summary>
    /// プレイヤーの必要コンポーネント参照を解決します。
    /// </summary>
    void ResolvePlayerReferences()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player == null)
        {
            Debug.LogWarning("StyleChangeUiController: PlayerControllerが見つかりません。");
            return;
        }

        _playerHealthComponent = player.GetComponent<HealthComponent>();
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
    }

    /// <summary>
    /// カードに表示する文言を設定します。
    /// </summary>
    void ApplyCardTexts()
    {
        for (int i = 0; i < CARD_TITLES.Length; i++)
        {
            if (_titleLabels != null && i < _titleLabels.Length && _titleLabels[i] != null)
            {
                _titleLabels[i].text = CARD_TITLES[i];
            }

            if (_descriptionLabels != null && i < _descriptionLabels.Length && _descriptionLabels[i] != null)
            {
                _descriptionLabels[i].text = CARD_DESCRIPTIONS[i];
            }
        }
    }

    /// <summary>
    /// 押下イベント登録に使用するハンドラを事前に生成します。
    /// </summary>
    void BuildCardClickHandlers()
    {
        _cardClickHandlers = new Action[3];

        for (int i = 0; i < _cardClickHandlers.Length; i++)
        {
            int index = i;
            _cardClickHandlers[i] = () => OnCardClicked(index);
        }
    }

    /// <summary>
    /// 3枚のカードボタンに押下コールバックを登録します。
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
    /// 3枚のカードボタンから押下コールバックを解除します。
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

        if (cardIndex < 0 || cardIndex >= CARD_TITLES.Length)
        {
            Debug.LogWarning($"StyleChangeUiController: 未対応のカードインデックスです。 index={cardIndex}");
            return false;
        }

        CharacterStatsData statsData = GetStyleStatsData((StyleCardType)cardIndex);
        if (statsData == null)
        {
            string styleName = GetStyleName((StyleCardType)cardIndex);
            Debug.LogWarning($"StyleChangeUiController: {styleName}のCharacterStatsDataが未設定です。");
            return false;
        }

        _playerHealthComponent.ApplyStatsDataAndKeepConsumedHp(statsData);
        return true;
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
    /// スタイル変更UIを表示し、ゲーム進行を一時停止します。
    /// </summary>
    void OpenStyleUi()
    {
        if (_isStyleUiOpen)
        {
            return;
        }

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
        switch (cardType)
        {
            case StyleCardType.Miko:
                return "巫女";
            case StyleCardType.Maid:
                return "メイド";
            case StyleCardType.Celeb:
                return "セレブ";
            default:
                return "不明";
        }
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
            case StyleCardType.Celeb:
                return _celebStyleStats;
            default:
                return null;
        }
    }

}
