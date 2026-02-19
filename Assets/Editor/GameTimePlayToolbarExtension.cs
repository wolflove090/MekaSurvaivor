using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

/// <summary>
/// Unityエディタのメインツールバーに制限時間指定付き再生操作を追加します
/// </summary>
public static class GameTimePlayToolbarExtension
{
    const string TimeDropdownPath = "MekaSurvaivor/PlayTime";
    const string SetAndPlayButtonPath = "MekaSurvaivor/SetAndPlay";
    static readonly float[] _timeOptions = { 10f, 120f };
    static int _selectedTimeIndex;

    /// <summary>
    /// メインツールバーへプレイ時間選択ドロップダウンを登録します
    /// </summary>
    [MainToolbarElement(TimeDropdownPath, defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 110)]
    static MainToolbarElement CreateTimeDropdown()
    {
        return new MainToolbarDropdown(CreateDropdownContent(), OpenTimeDropdownMenu);
    }

    /// <summary>
    /// メインツールバーへ制限時間適用付き再生ボタンを登録します
    /// </summary>
    [MainToolbarElement(SetAndPlayButtonPath, defaultDockPosition = MainToolbarDockPosition.Right, defaultDockIndex = 111)]
    static MainToolbarElement CreateSetAndPlayButton()
    {
        var content = new MainToolbarContent("Set & Play", "選択した制限時間を適用して再生します。");
        return new MainToolbarButton(content, StartPlayWithSelectedTime);
    }

    /// <summary>
    /// 現在選択中の時間ラベルを使用したドロップダウン表示情報を生成します
    /// </summary>
    static MainToolbarContent CreateDropdownContent()
    {
        var selectedSeconds = _timeOptions[_selectedTimeIndex];
        var label = $"{selectedSeconds:0}秒";
        return new MainToolbarContent(label, "ゲームのクリア時間を選択します。");
    }

    /// <summary>
    /// プレイ時間選択メニューを表示します
    /// </summary>
    static void OpenTimeDropdownMenu(Rect buttonRect)
    {
        var menu = new GenericMenu();
        for (var i = 0; i < _timeOptions.Length; i++)
        {
            var optionIndex = i;
            var optionLabel = $"{_timeOptions[optionIndex]:0}秒";
            menu.AddItem(new GUIContent(optionLabel), optionIndex == _selectedTimeIndex, () => SelectTime(optionIndex));
        }

        menu.DropDown(buttonRect);
    }

    /// <summary>
    /// ドロップダウンで選択された時間を保持しツールバー表示を更新します
    /// </summary>
    static void SelectTime(int optionIndex)
    {
        _selectedTimeIndex = optionIndex;
        MainToolbar.Refresh(TimeDropdownPath);
    }

    /// <summary>
    /// 選択した秒数を保存して再生開始します
    /// </summary>
    static void StartPlayWithSelectedTime()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        var timeLimit = _timeOptions[_selectedTimeIndex];
        EditorPrefs.SetFloat(GameManager.EditorTimeLimitOverrideKey, timeLimit);
        EditorApplication.isPlaying = true;
    }
}
