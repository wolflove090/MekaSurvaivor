using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ホーム画面UIの構築とボタン押下イベントの管理を行うコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class HomeScreenUiController : MonoBehaviour
{
    const float TargetAspectRatio = 16f / 9f;

    [SerializeField]
    [Tooltip("ホーム画面UIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("ホーム画面UIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("背景に表示するスプライト")]
    Sprite _backgroundSprite;

    [SerializeField]
    [Tooltip("中央に表示するキャラクタースプライト")]
    Sprite _characterSprite;

    UIDocument _uiDocument;
    VisualElement _viewportElement;
    VisualElement _safeAreaElement;
    VisualElement _backgroundElement;
    VisualElement _characterElement;
    Button _encyclopediaButton;
    Button _changeStyleButton;
    Button _sortieButton;
    Button _storyButton;

    Action _onEncyclopediaClicked;
    Action _onChangeStyleClicked;
    Action _onSortieClicked;
    Action _onStoryClicked;
    EventCallback<GeometryChangedEvent> _rootGeometryChangedHandler;

    /// <summary>
    /// 初期化時にUI構築と参照取得を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        BuildUi();
        CacheElements();
        ApplySprites();
        BuildHandlers();
        BuildLayoutHandlers();
    }

    /// <summary>
    /// 有効化時にボタン押下イベントを登録します。
    /// </summary>
    void OnEnable()
    {
        RegisterCallbacks();
        RegisterLayoutCallbacks();
        UpdateSafeAreaLayout();
    }

    /// <summary>
    /// 無効化時にボタン押下イベントを解除します。
    /// </summary>
    void OnDisable()
    {
        UnregisterCallbacks();
        UnregisterLayoutCallbacks();
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

        Debug.LogWarning("HomeScreenUiController: Layout Assetが未設定です。");
    }

    /// <summary>
    /// UXML上の参照を取得します。
    /// </summary>
    void CacheElements()
    {
        VisualElement root = _uiDocument.rootVisualElement;
        _viewportElement = root.Q<VisualElement>("home-viewport");
        _safeAreaElement = root.Q<VisualElement>("home-safe-area");
        _backgroundElement = root.Q<VisualElement>("home-background");
        _characterElement = root.Q<VisualElement>("home-character");
        _encyclopediaButton = root.Q<Button>("home-encyclopedia-button");
        _changeStyleButton = root.Q<Button>("home-change-style-button");
        _sortieButton = root.Q<Button>("home-sortie-button");
        _storyButton = root.Q<Button>("home-story-button");
    }

    /// <summary>
    /// 背景とキャラクター画像を反映します。
    /// </summary>
    void ApplySprites()
    {
        if (_backgroundElement != null && _backgroundSprite != null)
        {
            _backgroundElement.style.backgroundImage = new StyleBackground(_backgroundSprite);
        }

        if (_characterElement != null && _characterSprite != null)
        {
            _characterElement.style.backgroundImage = new StyleBackground(_characterSprite);
        }
    }

    /// <summary>
    /// 押下イベント登録に使用するハンドラを生成します。
    /// </summary>
    void BuildHandlers()
    {
        _onEncyclopediaClicked = () => LogButtonClick("図鑑");
        _onChangeStyleClicked = () => LogButtonClick("着替え");
        _onSortieClicked = () => LogButtonClick("出撃");
        _onStoryClicked = () => LogButtonClick("物語");
    }

    /// <summary>
    /// レイアウト更新に使用するハンドラを生成します。
    /// </summary>
    void BuildLayoutHandlers()
    {
        _rootGeometryChangedHandler = _ => UpdateSafeAreaLayout();
    }

    /// <summary>
    /// ボタン押下イベントを登録します。
    /// </summary>
    void RegisterCallbacks()
    {
        if (_encyclopediaButton != null)
        {
            _encyclopediaButton.clicked += _onEncyclopediaClicked;
        }

        if (_changeStyleButton != null)
        {
            _changeStyleButton.clicked += _onChangeStyleClicked;
        }

        if (_sortieButton != null)
        {
            _sortieButton.clicked += _onSortieClicked;
        }

        if (_storyButton != null)
        {
            _storyButton.clicked += _onStoryClicked;
        }
    }

    /// <summary>
    /// ボタン押下イベントを解除します。
    /// </summary>
    void UnregisterCallbacks()
    {
        if (_encyclopediaButton != null)
        {
            _encyclopediaButton.clicked -= _onEncyclopediaClicked;
        }

        if (_changeStyleButton != null)
        {
            _changeStyleButton.clicked -= _onChangeStyleClicked;
        }

        if (_sortieButton != null)
        {
            _sortieButton.clicked -= _onSortieClicked;
        }

        if (_storyButton != null)
        {
            _storyButton.clicked -= _onStoryClicked;
        }
    }

    /// <summary>
    /// ルートサイズ変更イベントを登録します。
    /// </summary>
    void RegisterLayoutCallbacks()
    {
        if (_viewportElement != null && _rootGeometryChangedHandler != null)
        {
            _viewportElement.RegisterCallback(_rootGeometryChangedHandler);
        }
    }

    /// <summary>
    /// ルートサイズ変更イベントを解除します。
    /// </summary>
    void UnregisterLayoutCallbacks()
    {
        if (_viewportElement != null && _rootGeometryChangedHandler != null)
        {
            _viewportElement.UnregisterCallback(_rootGeometryChangedHandler);
        }
    }

    /// <summary>
    /// 描画領域の中央に16:9のセーフエリアを配置します。
    /// </summary>
    void UpdateSafeAreaLayout()
    {
        if (_viewportElement == null || _safeAreaElement == null)
        {
            return;
        }

        float viewportWidth = _viewportElement.resolvedStyle.width;
        float viewportHeight = _viewportElement.resolvedStyle.height;

        if (viewportWidth <= 0f || viewportHeight <= 0f)
        {
            return;
        }

        float safeAreaWidth = viewportWidth;
        float safeAreaHeight = safeAreaWidth / TargetAspectRatio;

        if (safeAreaHeight > viewportHeight)
        {
            safeAreaHeight = viewportHeight;
            safeAreaWidth = safeAreaHeight * TargetAspectRatio;
        }

        _safeAreaElement.style.width = safeAreaWidth;
        _safeAreaElement.style.height = safeAreaHeight;
        _safeAreaElement.style.left = (viewportWidth - safeAreaWidth) * 0.5f;
        _safeAreaElement.style.top = (viewportHeight - safeAreaHeight) * 0.5f;

        // 右側UIを16:9エリアの1/4に固定
        VisualElement rightRail = _safeAreaElement.Q<VisualElement>("home-right-rail");
        if (rightRail != null)
        {
            rightRail.style.width = safeAreaWidth * 0.25f;
        }

        // ボタンの高さを16:9エリアの高さに基づいて設定
        float compactButtonHeight = safeAreaHeight * 0.07f; // 高さの7%
        float largeButtonHeight = safeAreaHeight * 0.14f;   // 高さの14%

        if (_encyclopediaButton != null)
        {
            _encyclopediaButton.style.height = compactButtonHeight;
        }

        if (_changeStyleButton != null)
        {
            _changeStyleButton.style.height = compactButtonHeight;
        }

        if (_sortieButton != null)
        {
            _sortieButton.style.height = largeButtonHeight;
        }

        if (_storyButton != null)
        {
            _storyButton.style.height = largeButtonHeight;
        }
    }

    /// <summary>
    /// ボタン押下ログを出力します。
    /// </summary>
    /// <param name="buttonName">押下されたボタン名</param>
    void LogButtonClick(string buttonName)
    {
        Debug.Log($"HomeScreenUiController: {buttonName} ボタンが押されました。");
    }
}
