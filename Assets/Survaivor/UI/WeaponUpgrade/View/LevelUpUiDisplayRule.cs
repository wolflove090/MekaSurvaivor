/// <summary>
/// レベルアップ時に表示するUI種別の判定ルールを提供します。
/// </summary>
public static class LevelUpUiDisplayRule
{
    const int STYLE_UI_FIRST_LEVEL = 4;
    const int STYLE_UI_INTERVAL = 3;

    /// <summary>
    /// 指定レベルでスタイル変更UIを表示するか判定します。
    /// </summary>
    /// <param name="newLevel">レベルアップ後のレベル</param>
    /// <returns>スタイル変更UIを表示する場合はtrue</returns>
    public static bool ShouldOpenStyleUi(int newLevel)
    {
        if (newLevel < STYLE_UI_FIRST_LEVEL)
        {
            return false;
        }

        return (newLevel - 1) % STYLE_UI_INTERVAL == 0;
    }

    /// <summary>
    /// 指定レベルで武器強化UIを表示するか判定します。
    /// </summary>
    /// <param name="newLevel">レベルアップ後のレベル</param>
    /// <returns>武器強化UIを表示する場合はtrue</returns>
    public static bool ShouldOpenWeaponUpgradeUi(int newLevel)
    {
        return !ShouldOpenStyleUi(newLevel);
    }
}
