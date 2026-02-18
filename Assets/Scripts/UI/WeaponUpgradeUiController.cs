using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 武器強化UIの表示とカード押下イベントを管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class WeaponUpgradeUiController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("武器強化UIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("武器強化UIのUSSアセット")]
    StyleSheet _styleSheet;

    UIDocument _uiDocument;
    Button[] _upgradeCards;
    Action[] _cardClickHandlers;

    /// <summary>
    /// 強化カードが押下された時に発火するイベント。
    /// </summary>
    public event Action<int> OnUpgradeCardSelected;

    /// <summary>
    /// 初期化時にUI構築と参照取得を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        BuildUi();
        CacheElements();
        BuildCardClickHandlers();
    }

    /// <summary>
    /// 有効化時にカード押下イベントを登録します。
    /// </summary>
    void OnEnable()
    {
        RegisterCallbacks();
    }

    /// <summary>
    /// 無効化時にカード押下イベントを解除します。
    /// </summary>
    void OnDisable()
    {
        UnregisterCallbacks();
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
        Debug.Log($"WeaponUpgradeUiController: 強化カード {cardIndex + 1} が選択されました。");
        OnUpgradeCardSelected?.Invoke(cardIndex);
    }
}
