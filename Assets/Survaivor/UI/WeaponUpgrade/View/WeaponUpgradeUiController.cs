using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 武器強化UIの表示とカード押下イベントを管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class WeaponUpgradeUiController : MonoBehaviour
{
    /// <summary>
    /// 武器強化カード種別。
    /// </summary>
    public enum UpgradeCardType
    {
        Shooter = 0,
        Throwing = 1,
        DamageField = 2,
        Drone = 3,
        BoundBall = 4,
        FlameBottle = 5
    }

    const int DISPLAY_CARD_COUNT = 3;

    [SerializeField]
    [Tooltip("武器強化UIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("武器強化UIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("武器強化UI表示中に適用するUIDocumentのSort Order")]
    float _sortOrderWhileOpen = 100f;

    UIDocument _uiDocument;
    WeaponUpgradePresenter _presenter;
    Button[] _upgradeCards;
    Label[] _upgradeCardTitles;
    Label[] _upgradeCardDescriptions;
    Action[] _cardClickHandlers;
    UpgradeCardType[] _displayedTypes;
    bool _isUpgradeUiOpen;
    float _timeScaleBeforePause;
    float _defaultSortOrder;

    /// <summary>
    /// 強化カードが押下された時に発火するイベント。
    /// </summary>
    public event Action<UpgradeCardType> OnUpgradeCardSelected;

    /// <summary>
    /// 初期化時にUI構築と参照取得を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _defaultSortOrder = _uiDocument.sortingOrder;
        _displayedTypes = new UpgradeCardType[DISPLAY_CARD_COUNT];
        BuildUi();
        CacheElements();
        BuildCardClickHandlers();
        SetUpgradeUiVisible(false);
    }

    /// <summary>
    /// 有効化時にカード押下イベントを登録します。
    /// </summary>
    void OnEnable()
    {
        RegisterCallbacks();
        _presenter?.Activate();
    }

    /// <summary>
    /// 無効化時にカード押下イベントを解除します。
    /// </summary>
    void OnDisable()
    {
        if (_isUpgradeUiOpen)
        {
            CloseUpgradeUi();
        }

        UnregisterCallbacks();
        _presenter?.Deactivate();
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

        Debug.LogWarning("WeaponUpgradeUiController: Layout Assetが未設定です。");
    }

    /// <summary>
    /// UXML上のカードボタン参照を取得します。
    /// </summary>
    void CacheElements()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        _upgradeCards = new[]
        {
            root.Q<Button>("upgrade-card-1"),
            root.Q<Button>("upgrade-card-2"),
            root.Q<Button>("upgrade-card-3")
        };
        _upgradeCardTitles = new[]
        {
            root.Q<Label>("upgrade-card-1-title"),
            root.Q<Label>("upgrade-card-2-title"),
            root.Q<Label>("upgrade-card-3-title")
        };
        _upgradeCardDescriptions = new[]
        {
            root.Q<Label>("upgrade-card-1-description"),
            root.Q<Label>("upgrade-card-2-description"),
            root.Q<Label>("upgrade-card-3-description")
        };
    }

    /// <summary>
    /// 押下イベント登録に使用するハンドラを事前に生成します。
    /// </summary>
    void BuildCardClickHandlers()
    {
        _cardClickHandlers = new Action[DISPLAY_CARD_COUNT];

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
        if (_upgradeCards == null || _cardClickHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _upgradeCards.Length; i++)
        {
            Button card = _upgradeCards[i];
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
        if (_upgradeCards == null || _cardClickHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _upgradeCards.Length; i++)
        {
            Button card = _upgradeCards[i];
            if (card == null)
            {
                continue;
            }

            card.clicked -= _cardClickHandlers[i];
        }
    }

    /// <summary>
    /// カード押下時の処理を実行します。
    /// </summary>
    /// <param name="cardIndex">押下されたカードのインデックス（0始まり）</param>
    void OnCardClicked(int cardIndex)
    {
        if (!_isUpgradeUiOpen)
        {
            return;
        }

        Debug.Log($"WeaponUpgradeUiController: 強化カード {cardIndex + 1} が選択されました。 type={_displayedTypes[cardIndex]}");
        OnUpgradeCardSelected?.Invoke(_displayedTypes[cardIndex]);
        CloseUpgradeUi();
    }

    /// <summary>
    /// 候補一覧から表示用の武器カードを抽選してUIへ反映します。
    /// </summary>
    /// <param name="availableTypes">抽選元となる武器候補一覧</param>
    public void PrepareUpgradeCandidates(IReadOnlyList<UpgradeCardType> availableTypes)
    {
        SetUpgradeCandidates(BuildRandomizedCandidates(availableTypes));
    }

    /// <summary>
    /// 表示対象の武器カードを設定し、各ボタンの表示内容を更新します。
    /// </summary>
    /// <param name="candidates">表示する武器カード候補</param>
    public void SetUpgradeCandidates(IReadOnlyList<UpgradeCardType> candidates)
    {
        if (_upgradeCards == null)
        {
            return;
        }

        for (int i = 0; i < _upgradeCards.Length; i++)
        {
            Button card = _upgradeCards[i];
            if (card == null)
            {
                continue;
            }

            bool hasCandidate = candidates != null && i < candidates.Count;
            UpgradeCardType cardType = hasCandidate ? candidates[i] : UpgradeCardType.Shooter;
            _displayedTypes[i] = cardType;
            card.text = string.Empty;

            if (_upgradeCardTitles != null && i < _upgradeCardTitles.Length && _upgradeCardTitles[i] != null)
            {
                _upgradeCardTitles[i].text = hasCandidate ? GetCardLabel(cardType) : "-";
            }

            if (_upgradeCardDescriptions != null && i < _upgradeCardDescriptions.Length && _upgradeCardDescriptions[i] != null)
            {
                _upgradeCardDescriptions[i].text = hasCandidate ? GetCardDescription(cardType) : string.Empty;
            }

            card.SetEnabled(hasCandidate);
        }
    }

    /// <summary>
    /// 武器候補一覧から重複なしで最大3件をランダム抽選します。
    /// </summary>
    /// <param name="availableTypes">抽選元となる候補一覧</param>
    /// <returns>表示に使用する候補一覧</returns>
    public static IReadOnlyList<UpgradeCardType> BuildRandomizedCandidates(IReadOnlyList<UpgradeCardType> availableTypes)
    {
        if (availableTypes == null || availableTypes.Count == 0)
        {
            return Array.Empty<UpgradeCardType>();
        }

        List<UpgradeCardType> pool = new List<UpgradeCardType>(availableTypes.Count);
        for (int i = 0; i < availableTypes.Count; i++)
        {
            UpgradeCardType type = availableTypes[i];
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
        UpgradeCardType[] result = new UpgradeCardType[resultCount];
        for (int i = 0; i < resultCount; i++)
        {
            result[i] = pool[i];
        }

        return result;
    }

    /// <summary>
    /// カード表示用の名称を取得します。
    /// </summary>
    /// <param name="type">表示する武器種別</param>
    /// <returns>UIへ表示するラベル</returns>
    public static string GetCardLabel(UpgradeCardType type)
    {
        switch (type)
        {
            case UpgradeCardType.Shooter:
                return "Shooter";
            case UpgradeCardType.Throwing:
                return "Throwing";
            case UpgradeCardType.DamageField:
                return "Damage Field";
            case UpgradeCardType.Drone:
                return "Drone";
            case UpgradeCardType.BoundBall:
                return "Bound Ball";
            case UpgradeCardType.FlameBottle:
                return "Flame Bottle";
            default:
                return type.ToString();
        }
    }

    /// <summary>
    /// カード表示用の説明文を取得します。
    /// </summary>
    /// <param name="type">表示する武器種別</param>
    /// <returns>UIへ表示する説明文</returns>
    public static string GetCardDescription(UpgradeCardType type)
    {
        switch (type)
        {
            case UpgradeCardType.Shooter:
                return "もっとも近い敵を狙う通常弾の発射間隔を短縮します。";
            case UpgradeCardType.Throwing:
                return "向いている方向へ投げる武器の発射間隔を短縮します。";
            case UpgradeCardType.DamageField:
                return "周囲に展開するダメージエリアの範囲を拡大します。";
            case UpgradeCardType.Drone:
                return "追従ドローンを展開し、強化で攻撃間隔を短縮します。";
            case UpgradeCardType.BoundBall:
                return "右下へ飛ぶ反射弾を追加し、強化で発射間隔を短縮します。";
            case UpgradeCardType.FlameBottle:
                return "前方へ火炎瓶を投げ、強化で炎エリアの持続時間を延長します。";
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 武器強化UIを表示し、ゲーム進行を一時停止します。
    /// </summary>
    public void OpenUpgradeUi()
    {
        if (_isUpgradeUiOpen)
        {
            return;
        }

        Debug.Log("WeaponUpgradeUiController: 武器強化UIを表示します。");
        _isUpgradeUiOpen = true;
        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        ApplyFrontmostSortOrder();
        SetUpgradeUiVisible(true);
    }

    /// <summary>
    /// 武器強化UIを非表示にし、ゲーム進行を再開します。
    /// </summary>
    public void CloseUpgradeUi()
    {
        if (!_isUpgradeUiOpen)
        {
            return;
        }

        _isUpgradeUiOpen = false;
        SetUpgradeUiVisible(false);
        RestoreDefaultSortOrder();
        Time.timeScale = _timeScaleBeforePause > 0f ? _timeScaleBeforePause : 1f;
    }

    /// <summary>
    /// 武器強化UIの表示状態を切り替えます。
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue</param>
    void SetUpgradeUiVisible(bool isVisible)
    {
        VisualElement root = _uiDocument?.rootVisualElement;
        if (root == null)
        {
            return;
        }

        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// 武器強化UI表示中の描画順を最前面へ引き上げます。
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
    /// 武器強化UI非表示時に描画順を初期値へ戻します。
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
    /// ブートストラップからPresenterを設定します。
    /// </summary>
    /// <param name="presenter">UI制御を担当するPresenter</param>
    public void SetPresenter(WeaponUpgradePresenter presenter)
    {
        if (_presenter == presenter)
        {
            return;
        }

        if (isActiveAndEnabled)
        {
            _presenter?.Deactivate();
        }

        _presenter = presenter;

        if (isActiveAndEnabled)
        {
            _presenter?.Activate();
        }
    }
}
