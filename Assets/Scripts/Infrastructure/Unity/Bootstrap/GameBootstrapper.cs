using UnityEngine;

/// <summary>
/// シーン起動時に主要コンポーネント参照を接続します。
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    GameMessageBus _gameMessageBus;
    GameScreenPresenter _gameScreenPresenter;
    WeaponUpgradePresenter _weaponUpgradePresenter;

    [SerializeField]
    [Tooltip("参照を解決するプレイヤーコントローラー")]
    PlayerController _playerController;

    [SerializeField]
    [Tooltip("参照を解決するゲーム進行管理")]
    GameManager _gameManager;

    [SerializeField]
    [Tooltip("参照を解決する敵スポナー")]
    EnemySpawner _enemySpawner;

    [SerializeField]
    [Tooltip("参照を解決する敵レジストリ")]
    EnemyRegistry _enemyRegistry;

    [SerializeField]
    [Tooltip("参照を解決する経験値オーブスポナー")]
    ExperienceOrbSpawner _experienceOrbSpawner;

    [SerializeField]
    [Tooltip("参照を解決するHUDコントローラー")]
    GameScreenUiController _gameScreenUiController;

    [SerializeField]
    [Tooltip("参照を解決する武器強化UIコントローラー")]
    WeaponUpgradeUiController _weaponUpgradeUiController;

    [SerializeField]
    [Tooltip("参照を解決するスタイル変更UIコントローラー")]
    StyleChangeUiController _styleChangeUiController;

    /// <summary>
    /// 参照解決と依存接続を実行します。
    /// </summary>
    void Awake()
    {
        ResolveReferences();
        WireDependencies();
    }

    /// <summary>
    /// 未設定の参照をシーンから補完します。
    /// </summary>
    void ResolveReferences()
    {
        _playerController ??= FindFirstObjectByType<PlayerController>();
        _gameManager ??= FindFirstObjectByType<GameManager>();
        _enemySpawner ??= FindFirstObjectByType<EnemySpawner>();
        _enemyRegistry ??= FindFirstObjectByType<EnemyRegistry>();
        _experienceOrbSpawner ??= FindFirstObjectByType<ExperienceOrbSpawner>();
        _gameScreenUiController ??= FindFirstObjectByType<GameScreenUiController>();
        _weaponUpgradeUiController ??= FindFirstObjectByType<WeaponUpgradeUiController>();
        _styleChangeUiController ??= FindFirstObjectByType<StyleChangeUiController>();
    }

    /// <summary>
    /// 解決済み参照を各コンポーネントへ設定します。
    /// </summary>
    void WireDependencies()
    {
        _gameMessageBus ??= new GameMessageBus();
        _gameManager?.SetMessageBus(_gameMessageBus);
        _enemySpawner?.SetMessageBus(_gameMessageBus);
        _experienceOrbSpawner?.SetMessageBus(_gameMessageBus);
        _styleChangeUiController?.SetMessageBus(_gameMessageBus);

        if (_playerController != null)
        {
            PlayerExperience playerExperience = _playerController.GetComponent<PlayerExperience>();

            _playerController.SetMessageBus(_gameMessageBus);
            playerExperience?.SetMessageBus(_gameMessageBus);

            _enemySpawner?.SetSpawnTarget(_playerController.transform);
            _styleChangeUiController?.SetPlayer(_playerController);

            if (_gameScreenUiController != null)
            {
                _gameScreenPresenter = new GameScreenPresenter(
                    _gameScreenUiController,
                    _playerController,
                    playerExperience,
                    _gameManager,
                    _gameMessageBus);
                _gameScreenUiController.SetPresenter(_gameScreenPresenter);
            }

            if (_weaponUpgradeUiController != null)
            {
                _weaponUpgradePresenter = new WeaponUpgradePresenter(
                    _weaponUpgradeUiController,
                    _playerController,
                    _gameMessageBus);
                _weaponUpgradeUiController.SetPresenter(_weaponUpgradePresenter);
            }
        }

        if (_enemySpawner != null && _enemyRegistry == null)
        {
            _enemyRegistry = FindFirstObjectByType<EnemyRegistry>();
        }
    }
}
