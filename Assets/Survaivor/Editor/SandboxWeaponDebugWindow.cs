using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sandboxシーン再生中の武器追加とレベル調整を行うデバッグウィンドウです。
/// </summary>
public class SandboxWeaponDebugWindow : EditorWindow
{
    const string WindowTitle = "Sandbox Weapon Debug";
    const float WindowMinWidth = 320f;
    const int MinWeaponLevel = 1;
    const int MaxWeaponLevel = 5;

    readonly Dictionary<WeaponUpgradeUiController.UpgradeCardType, bool> _displayedToggleStates =
        new Dictionary<WeaponUpgradeUiController.UpgradeCardType, bool>();
    readonly Dictionary<WeaponUpgradeUiController.UpgradeCardType, int> _displayedLevels =
        new Dictionary<WeaponUpgradeUiController.UpgradeCardType, int>();

    PlayerController _playerController;

    /// <summary>
    /// デバッグウィンドウを開きます。
    /// </summary>
    public static void Open()
    {
        SandboxWeaponDebugWindow window = GetWindow<SandboxWeaponDebugWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(WindowMinWidth, 0f);
        window.Show();
        window.Focus();
    }

    /// <summary>
    /// テストで使用するウィンドウタイトルを返します。
    /// </summary>
    /// <returns>ウィンドウタイトル文字列</returns>
    internal static string GetWindowTitle()
    {
        return WindowTitle;
    }

    /// <summary>
    /// ウィンドウ有効化時にプレイヤー参照を同期します。
    /// </summary>
    void OnEnable()
    {
        RefreshPlayerController();
        SyncDisplayStateFromPlayer();
    }

    /// <summary>
    /// エディタ更新ごとにプレイヤー参照を再解決します。
    /// </summary>
    void OnInspectorUpdate()
    {
        RefreshPlayerController();
        Repaint();
    }

    /// <summary>
    /// IMGUIでデバッグ操作UIを描画します。
    /// </summary>
    void OnGUI()
    {
        RefreshPlayerController();

        EditorGUILayout.LabelField("Sandbox Weapon Debug", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("Play中のみ操作できます。", MessageType.Info);
            return;
        }

        if (_playerController == null)
        {
            EditorGUILayout.HelpBox("PlayerController が見つからないため操作できません。", MessageType.Warning);
            return;
        }

        foreach (WeaponUpgradeUiController.UpgradeCardType type in _playerController.GetAvailableWeaponUpgradeTypes())
        {
            DrawWeaponControl(type);
            EditorGUILayout.Space();
        }
    }

    /// <summary>
    /// 武器ごとの追加トグルとレベルスライダーを描画します。
    /// </summary>
    /// <param name="type">描画する武器種別</param>
    void DrawWeaponControl(WeaponUpgradeUiController.UpgradeCardType type)
    {
        ReadWeaponState(type, out bool hasWeapon, out int currentLevel);

        bool displayedToggle = GetDisplayedToggleState(type, hasWeapon);
        int displayedLevel = GetDisplayedLevel(type, hasWeapon ? currentLevel : MinWeaponLevel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        bool nextToggle = EditorGUILayout.ToggleLeft(type.ToString(), displayedToggle);

        using (new EditorGUI.DisabledScope(!hasWeapon))
        {
            int nextLevel = EditorGUILayout.IntSlider("Level", displayedLevel, MinWeaponLevel, MaxWeaponLevel);
            if (nextLevel != displayedLevel)
            {
                HandleLevelChange(type, nextLevel);
            }
        }

        if (nextToggle != displayedToggle)
        {
            HandleToggleChange(type, hasWeapon, nextToggle);
        }

        EditorGUILayout.EndVertical();

        ReadWeaponState(type, out bool syncedHasWeapon, out int syncedLevel);
        SyncDisplayState(type, syncedHasWeapon, syncedLevel);
    }

    /// <summary>
    /// プレイヤーの武器所持状態とレベルを読み取ります。
    /// </summary>
    /// <param name="type">確認する武器種別</param>
    /// <param name="hasWeapon">武器所持状態</param>
    /// <param name="currentLevel">現在レベル</param>
    void ReadWeaponState(
        WeaponUpgradeUiController.UpgradeCardType type,
        out bool hasWeapon,
        out int currentLevel)
    {
        hasWeapon = _playerController != null && _playerController.HasWeapon(type);
        currentLevel = 0;

        if (!hasWeapon || _playerController == null)
        {
            return;
        }

        _playerController.TryGetWeaponLevel(type, out currentLevel);
    }

    /// <summary>
    /// トグル変更操作を反映します。
    /// </summary>
    /// <param name="type">対象武器種別</param>
    /// <param name="hasWeapon">現在の所持状態</param>
    /// <param name="nextToggle">変更後トグル状態</param>
    void HandleToggleChange(
        WeaponUpgradeUiController.UpgradeCardType type,
        bool hasWeapon,
        bool nextToggle)
    {
        if (!hasWeapon && nextToggle)
        {
            _playerController.TrySetWeaponLevel(type, MinWeaponLevel);
            SyncDisplayStateFromPlayer();
            return;
        }

        if (hasWeapon && !nextToggle)
        {
            _playerController.TrySetWeaponLevel(type, 0);
            SyncDisplayStateFromPlayer();
        }
    }

    /// <summary>
    /// レベル変更操作を反映します。
    /// </summary>
    /// <param name="type">対象武器種別</param>
    /// <param name="requestedLevel">要求レベル</param>
    void HandleLevelChange(
        WeaponUpgradeUiController.UpgradeCardType type,
        int requestedLevel)
    {
        _playerController.TrySetWeaponLevel(type, requestedLevel);
        SyncDisplayStateFromPlayer();
    }

    /// <summary>
    /// シーン内のPlayerController参照を更新します。
    /// </summary>
    void RefreshPlayerController()
    {
        if (!EditorApplication.isPlaying)
        {
            if (_playerController != null)
            {
                _playerController = null;
                SyncDisplayStateFromPlayer();
            }

            return;
        }

        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<PlayerController>();
            SyncDisplayStateFromPlayer();
        }
    }

    /// <summary>
    /// 実際の武器状態を表示キャッシュへ同期します。
    /// </summary>
    void SyncDisplayStateFromPlayer()
    {
        foreach (WeaponUpgradeUiController.UpgradeCardType type in System.Enum.GetValues(typeof(WeaponUpgradeUiController.UpgradeCardType)))
        {
            if (_playerController == null)
            {
                _displayedToggleStates[type] = false;
                _displayedLevels[type] = MinWeaponLevel;
                continue;
            }

            ReadWeaponState(type, out bool hasWeapon, out int currentLevel);
            SyncDisplayState(type, hasWeapon, currentLevel);
        }
    }

    /// <summary>
    /// 指定武器の表示キャッシュを同期します。
    /// </summary>
    /// <param name="type">対象武器種別</param>
    /// <param name="hasWeapon">現在の所持状態</param>
    /// <param name="currentLevel">現在レベル</param>
    void SyncDisplayState(WeaponUpgradeUiController.UpgradeCardType type, bool hasWeapon, int currentLevel)
    {
        _displayedToggleStates[type] = hasWeapon;
        _displayedLevels[type] = hasWeapon ? Mathf.Clamp(currentLevel, MinWeaponLevel, MaxWeaponLevel) : MinWeaponLevel;
    }

    /// <summary>
    /// トグル表示キャッシュを取得します。
    /// </summary>
    /// <param name="type">対象武器種別</param>
    /// <param name="fallbackValue">未初期化時の値</param>
    /// <returns>現在の表示キャッシュ値</returns>
    bool GetDisplayedToggleState(WeaponUpgradeUiController.UpgradeCardType type, bool fallbackValue)
    {
        if (!_displayedToggleStates.TryGetValue(type, out bool displayedValue))
        {
            displayedValue = fallbackValue;
            _displayedToggleStates[type] = displayedValue;
        }

        return displayedValue;
    }

    /// <summary>
    /// レベル表示キャッシュを取得します。
    /// </summary>
    /// <param name="type">対象武器種別</param>
    /// <param name="fallbackValue">未初期化時の値</param>
    /// <returns>現在の表示キャッシュ値</returns>
    int GetDisplayedLevel(WeaponUpgradeUiController.UpgradeCardType type, int fallbackValue)
    {
        if (!_displayedLevels.TryGetValue(type, out int displayedValue))
        {
            displayedValue = Mathf.Clamp(fallbackValue, MinWeaponLevel, MaxWeaponLevel);
            _displayedLevels[type] = displayedValue;
        }

        return displayedValue;
    }
}
