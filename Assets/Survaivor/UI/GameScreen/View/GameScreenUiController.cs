using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// プレイヤーステータス表示UIの構築とPresenter連携を管理するコンポーネント。
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class GameScreenUiController : MonoBehaviour
{
    internal const string UnknownGameTimeText = "--:--";

    [SerializeField]
    [Tooltip("プレイヤーステータスUIのUXMLアセット")]
    VisualTreeAsset _layoutAsset;

    [SerializeField]
    [Tooltip("プレイヤーステータスUIのUSSアセット")]
    StyleSheet _styleSheet;

    UIDocument _uiDocument;
    GameScreenPresenter _presenter;

    Label _gameTimeValueLabel;
    Label _levelValueLabel;
    Label _hpValueLabel;
    Label _expValueLabel;
    Label _powValueLabel;
    Label _defValueLabel;
    Label _spdValueLabel;

    /// <summary>
    /// 初期化時にUI構築を実行します。
    /// </summary>
    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        BuildUi();
        CacheElements();
    }

    /// <summary>
    /// 有効化時にPresenterの購読を開始します。
    /// </summary>
    void OnEnable()
    {
        _presenter?.Activate();
    }

    /// <summary>
    /// 無効化時にPresenterの購読を停止します。
    /// </summary>
    void OnDisable()
    {
        _presenter?.Deactivate();
    }

    /// <summary>
    /// 毎フレーム、Presenterに更新処理を委譲します。
    /// </summary>
    void Update()
    {
        _presenter?.Tick();
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
    /// ブートストラップからPresenterを設定します。
    /// </summary>
    /// <param name="presenter">UI更新を担当するPresenter</param>
    public void SetPresenter(GameScreenPresenter presenter)
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

    /// <summary>
    /// 残り時間表示を更新します。
    /// </summary>
    /// <param name="value">表示する残り時間文字列</param>
    public void SetGameTimeText(string value)
    {
        SetLabelText(_gameTimeValueLabel, value);
    }

    /// <summary>
    /// ステータス表示を更新します。
    /// </summary>
    /// <param name="levelText">レベル表示</param>
    /// <param name="hpText">HP表示</param>
    /// <param name="expText">経験値表示</param>
    /// <param name="powText">攻撃力表示</param>
    /// <param name="defText">防御力表示</param>
    /// <param name="spdText">速度表示</param>
    public void SetStatusText(
        string levelText,
        string hpText,
        string expText,
        string powText,
        string defText,
        string spdText)
    {
        SetLabelText(_levelValueLabel, levelText);
        SetLabelText(_hpValueLabel, hpText);
        SetLabelText(_expValueLabel, expText);
        SetLabelText(_powValueLabel, powText);
        SetLabelText(_defValueLabel, defText);
        SetLabelText(_spdValueLabel, spdText);
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
