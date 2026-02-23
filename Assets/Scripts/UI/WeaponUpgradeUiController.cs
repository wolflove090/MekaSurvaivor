using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 武器強化UIの表示とカード押下イベントを管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class WeaponUpgradeUiController : MonoBehaviour
{
    // TODO 別ファイルとして定義する
    public enum UpgradeCardType
    {
        Shooter = 0,
        Throwing = 1,
        DamageField = 2
    }

    [SerializeField]
    [Tooltip("武器強化UIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("武器強化UIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("強化対象")]
    PlayerController _player;

    [SerializeField]
    [Tooltip("武器強化UI表示中に適用するUIDocumentのSort Order")]
    float _sortOrderWhileOpen = 100f;

    UIDocument _uiDocument;
    Button[] _upgradeCards;
    Action[] _cardClickHandlers;
    bool _isUpgradeUiOpen;
    float _timeScaleBeforePause;
    float _defaultSortOrder;

    /// <summary>
    /// 強化カードが押下された時に発火するイベント。
    /// </summary>
    public event Action<int> OnUpgradeCardSelected;

    /// <summary>
    /// 初期化時にUI構築と参照取得を実行します。
    /// </summary>
    void Awake()
    {
        // ResolveWeaponReferences();
        _uiDocument = GetComponent<UIDocument>();
        _defaultSortOrder = _uiDocument.sortingOrder;
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
        GameEvents.OnPlayerLevelUp += OnPlayerLevelUp;
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
        GameEvents.OnPlayerLevelUp -= OnPlayerLevelUp;
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

        Debug.Log($"WeaponUpgradeUiController: 強化カード {cardIndex + 1} が選択されました。");
        ApplyWeaponUpgrade(cardIndex);
        OnUpgradeCardSelected?.Invoke(cardIndex);
        CloseUpgradeUi();
    }

    /// <summary>
    /// カード種別に応じた武器強化を適用、または武器の獲得を行います
    /// </summary>
    /// <param name="cardIndex">押下されたカードのインデックス（0始まり）</param>
    void ApplyWeaponUpgrade(int cardIndex)
    {
        _player.AddWeapon((UpgradeCardType)cardIndex);
    }

    /// <summary>
    /// レベルアップ時に武器強化UIを開いてゲームを停止します。
    /// </summary>
    /// <param name="newLevel">レベルアップ後のレベル</param>
    void OnPlayerLevelUp(int newLevel)
    {
        if (!LevelUpUiDisplayRule.ShouldOpenWeaponUpgradeUi(newLevel))
        {
            return;
        }

        Debug.Log($"WeaponUpgradeUiController: レベル {newLevel} 到達。武器強化UIを表示します。");
        OpenUpgradeUi();
    }

    /// <summary>
    /// 武器強化UIを表示し、ゲーム進行を一時停止します。
    /// </summary>
    void OpenUpgradeUi()
    {
        if (_isUpgradeUiOpen)
        {
            return;
        }

        _isUpgradeUiOpen = true;
        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        ApplyFrontmostSortOrder();
        SetUpgradeUiVisible(true);
    }

    /// <summary>
    /// 武器強化UIを非表示にし、ゲーム進行を再開します。
    /// </summary>
    void CloseUpgradeUi()
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

}
