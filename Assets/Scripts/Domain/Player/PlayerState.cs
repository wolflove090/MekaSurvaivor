using UnityEngine;

/// <summary>
/// プレイヤー進行に関するランタイム状態を保持します。
/// </summary>
public class PlayerState
{
    /// <summary>
    /// 現在のスタイル種別を取得します。
    /// </summary>
    public PlayerStyleType CurrentStyleType { get; private set; }

    /// <summary>
    /// 現在レベルを取得します。
    /// </summary>
    public int CurrentLevel { get; private set; }

    /// <summary>
    /// 現在経験値を取得します。
    /// </summary>
    public int CurrentExperience { get; private set; }

    /// <summary>
    /// 次レベルまでの必要経験値を取得します。
    /// </summary>
    public int ExperienceToNextLevel { get; private set; }

    /// <summary>
    /// 経験値倍率を取得します。
    /// </summary>
    public float ExperienceMultiplier { get; private set; }

    /// <summary>
    /// 移動速度倍率を取得します。
    /// </summary>
    public float MoveSpeedMultiplier { get; private set; }

    /// <summary>
    /// プレイヤー進行状態を初期化します。
    /// </summary>
    /// <param name="startLevel">開始レベル</param>
    public PlayerState(int startLevel)
    {
        CurrentStyleType = PlayerStyleType.Miko;
        CurrentLevel = Mathf.Max(1, startLevel);
        CurrentExperience = 0;
        ExperienceToNextLevel = 0;
        ExperienceMultiplier = 1f;
        MoveSpeedMultiplier = 1f;
    }

    /// <summary>
    /// 経験値進行を更新します。
    /// </summary>
    /// <param name="currentExperience">現在経験値</param>
    /// <param name="experienceToNextLevel">次レベルまでの必要経験値</param>
    public void SetProgress(int currentExperience, int experienceToNextLevel)
    {
        CurrentExperience = Mathf.Max(0, currentExperience);
        ExperienceToNextLevel = Mathf.Max(0, experienceToNextLevel);
    }

    /// <summary>
    /// レベルアップ後の状態へ更新します。
    /// </summary>
    /// <param name="remainingExperience">繰り越し経験値</param>
    /// <param name="experienceToNextLevel">次レベルまでの必要経験値</param>
    public void ApplyLevelUp(int remainingExperience, int experienceToNextLevel)
    {
        CurrentLevel++;
        CurrentExperience = Mathf.Max(0, remainingExperience);
        ExperienceToNextLevel = Mathf.Max(0, experienceToNextLevel);
    }

    /// <summary>
    /// 経験値倍率を設定します。
    /// </summary>
    /// <param name="multiplier">設定する倍率</param>
    public void SetExperienceMultiplier(float multiplier)
    {
        ExperienceMultiplier = Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// 経験値倍率を初期値へ戻します。
    /// </summary>
    public void ResetExperienceMultiplier()
    {
        ExperienceMultiplier = 1f;
    }

    /// <summary>
    /// 移動速度倍率を設定します。
    /// </summary>
    /// <param name="multiplier">設定する倍率</param>
    public void SetMoveSpeedMultiplier(float multiplier)
    {
        MoveSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// 移動速度倍率を初期値へ戻します。
    /// </summary>
    public void ResetMoveSpeedMultiplier()
    {
        MoveSpeedMultiplier = 1f;
    }

    /// <summary>
    /// 現在のスタイル種別を更新します。
    /// </summary>
    /// <param name="styleType">適用するスタイル種別</param>
    public void SetCurrentStyle(PlayerStyleType styleType)
    {
        CurrentStyleType = styleType;
    }
}
