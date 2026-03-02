using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// リザルトUIの表示とホーム復帰導線を管理します。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class ResultUiController : MonoBehaviour
{
    const string HomeSceneName = "Home";

    [SerializeField]
    [Tooltip("リザルトUIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("リザルトUIのUSSアセット")]
    StyleSheet _styleSheet;

    [SerializeField]
    [Tooltip("リザルトUI表示中に適用するUIDocumentのSort Order")]
    float _sortOrderWhileOpen = 200f;

    UIDocument _uiDocument;
    Label _titleLabel;
    Label _killCountLabel;
    Button _homeButton;
    bool _isSceneLoading;
    float _defaultSortOrder;

    /// <summary>
    /// 初期化時にUI構築と参照取得を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _defaultSortOrder = _uiDocument != null ? _uiDocument.sortingOrder : 0f;
        BuildUi();
        CacheElements();
        Hide();
    }

    /// <summary>
    /// 有効化時にボタン押下イベントを登録します。
    /// </summary>
    void OnEnable()
    {
        if (_homeButton != null)
        {
            _homeButton.clicked += OnHomeButtonClicked;
        }
    }

    /// <summary>
    /// 無効化時にボタン押下イベントを解除します。
    /// </summary>
    void OnDisable()
    {
        if (_homeButton != null)
        {
            _homeButton.clicked -= OnHomeButtonClicked;
        }
    }

    /// <summary>
    /// リザルトUIを表示します。
    /// </summary>
    /// <param name="title">表示する見出し</param>
    /// <param name="killCount">表示する撃破数</param>
    public void Show(string title, int killCount)
    {
        SetLabelText(_titleLabel, string.IsNullOrWhiteSpace(title) ? "撤退" : title);
        SetLabelText(_killCountLabel, $"撃破数 {Mathf.Max(0, killCount)}");
        _isSceneLoading = false;
        ApplyFrontmostSortOrder();
        SetVisible(true);
    }

    /// <summary>
    /// リザルトUIを非表示にします。
    /// </summary>
    public void Hide()
    {
        _isSceneLoading = false;
        RestoreDefaultSortOrder();
        SetVisible(false);
    }

    /// <summary>
    /// UXMLとUSSを読み込み、UIルートを構築します。
    /// </summary>
    void BuildUi()
    {
        VisualElement root = _uiDocument?.rootVisualElement;
        if (root == null)
        {
            return;
        }

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

        Debug.LogWarning("ResultUiController: Layout Assetが未設定です。");
    }

    /// <summary>
    /// UXML上の表示要素参照を取得します。
    /// </summary>
    void CacheElements()
    {
        VisualElement root = _uiDocument?.rootVisualElement;
        if (root == null)
        {
            return;
        }

        _titleLabel = root.Q<Label>("result-title");
        _killCountLabel = root.Q<Label>("result-kill-count");
        _homeButton = root.Q<Button>("result-home-button");
    }

    /// <summary>
    /// UIの表示状態を切り替えます。
    /// </summary>
    /// <param name="isVisible">表示する場合はtrue</param>
    void SetVisible(bool isVisible)
    {
        VisualElement root = _uiDocument?.rootVisualElement;
        if (root == null)
        {
            return;
        }

        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        root.pickingMode = isVisible ? PickingMode.Position : PickingMode.Ignore;
    }

    /// <summary>
    /// リザルト表示時に描画順を最前面へ引き上げます。
    /// </summary>
    void ApplyFrontmostSortOrder()
    {
        if (_uiDocument == null)
        {
            return;
        }

        _uiDocument.sortingOrder = Mathf.RoundToInt(Mathf.Max(_defaultSortOrder, _sortOrderWhileOpen));
        _uiDocument.rootVisualElement?.BringToFront();
    }

    /// <summary>
    /// 非表示時に描画順を初期値へ戻します。
    /// </summary>
    void RestoreDefaultSortOrder()
    {
        if (_uiDocument != null)
        {
            _uiDocument.sortingOrder = Mathf.RoundToInt(_defaultSortOrder);
        }
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

    /// <summary>
    /// ホームへ戻るボタン押下時のシーン遷移を実行します。
    /// </summary>
    void OnHomeButtonClicked()
    {
        if (_isSceneLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(HomeSceneName))
        {
            Debug.LogWarning($"ResultUiController: シーン '{HomeSceneName}' をロードできません。");
            return;
        }

        _isSceneLoading = true;
        Time.timeScale = 1f;

        try
        {
            SceneManager.LoadScene(HomeSceneName);
        }
        catch (Exception exception)
        {
            _isSceneLoading = false;
            Debug.LogException(exception);
        }
    }
}
