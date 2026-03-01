using UnityEngine;

/// <summary>
/// シーン起動時に主要コンポーネント参照を接続します。
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
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
        _gameScreenUiController ??= FindFirstObjectByType<GameScreenUiController>();
        _weaponUpgradeUiController ??= FindFirstObjectByType<WeaponUpgradeUiController>();
        _styleChangeUiController ??= FindFirstObjectByType<StyleChangeUiController>();
    }

    /// <summary>
    /// 解決済み参照を各コンポーネントへ設定します。
    /// </summary>
    void WireDependencies()
    {
        if (_playerController != null)
        {
            PlayerExperience playerExperience = _playerController.GetComponent<PlayerExperience>();

            _enemySpawner?.SetSpawnTarget(_playerController.transform);
            _weaponUpgradeUiController?.SetPlayer(_playerController);
            _styleChangeUiController?.SetPlayer(_playerController);
            _gameScreenUiController?.SetReferences(_playerController, playerExperience, _gameManager);
        }

        if (_enemySpawner != null && _enemyRegistry == null)
        {
            _enemyRegistry = FindFirstObjectByType<EnemyRegistry>();
        }
    }
}
