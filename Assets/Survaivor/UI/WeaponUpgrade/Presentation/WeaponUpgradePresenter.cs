using UnityEngine;

/// <summary>
/// 武器強化 UI の入力と適用処理を仲介します。
/// </summary>
public class WeaponUpgradePresenter
{
    readonly WeaponUpgradeUiController _view;
    readonly PlayerController _playerController;
    readonly GameMessageBus _messageBus;

    bool _isActive;

    /// <summary>
    /// Presenter を初期化します。
    /// </summary>
    /// <param name="view">表示先の UI</param>
    /// <param name="playerController">強化対象プレイヤー</param>
    /// <param name="messageBus">局所通知ハブ</param>
    public WeaponUpgradePresenter(
        WeaponUpgradeUiController view,
        PlayerController playerController,
        GameMessageBus messageBus)
    {
        _view = view;
        _playerController = playerController;
        _messageBus = messageBus;
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

        if (_view != null)
        {
            _view.OnUpgradeCardSelected += OnUpgradeCardSelected;
        }

        if (_messageBus != null)
        {
            _messageBus.PlayerLevelUp += OnPlayerLevelUp;
        }
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

        if (_view != null)
        {
            _view.OnUpgradeCardSelected -= OnUpgradeCardSelected;
        }

        if (_messageBus != null)
        {
            _messageBus.PlayerLevelUp -= OnPlayerLevelUp;
        }
    }

    void OnUpgradeCardSelected(int cardIndex)
    {
        if (_playerController == null)
        {
            Debug.LogWarning("WeaponUpgradePresenter: PlayerControllerが未設定のため武器強化を適用できません。");
            return;
        }

        _playerController.ApplyWeaponUpgrade((WeaponUpgradeUiController.UpgradeCardType)cardIndex);
    }

    void OnPlayerLevelUp(int newLevel)
    {
        if (!LevelUpUiDisplayRule.ShouldOpenWeaponUpgradeUi(newLevel))
        {
            return;
        }

        _view?.OpenUpgradeUi();
    }
}
