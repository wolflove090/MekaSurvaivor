using System;
using UnityEngine;

/// <summary>
/// プレイヤーの経験値加算とレベルアップ計算を管理します。
/// </summary>
public class PlayerProgressionService
{
    readonly int _baseExperienceRequired;
    readonly float _experienceScaling;
    readonly Action<int, int, int> _onExperienceGained;
    readonly Action<int> _onLevelUp;
    readonly PlayerStyleEffectFactory _styleEffectFactory;

    IPlayerStyleEffect _activeStyleEffect;

    /// <summary>
    /// プレイヤー進行状態を取得します。
    /// </summary>
    public PlayerState State { get; }

    /// <summary>
    /// プレイヤー進行サービスを初期化します。
    /// </summary>
    /// <param name="state">対象の進行状態</param>
    /// <param name="baseExperienceRequired">レベル2に必要な基礎経験値</param>
    /// <param name="experienceScaling">レベルごとの経験値増加率</param>
    /// <param name="onExperienceGained">経験値獲得通知</param>
    /// <param name="onLevelUp">レベルアップ通知</param>
    public PlayerProgressionService(
        PlayerState state,
        int baseExperienceRequired,
        float experienceScaling,
        Action<int, int, int> onExperienceGained = null,
        Action<int> onLevelUp = null)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        _baseExperienceRequired = Mathf.Max(1, baseExperienceRequired);
        _experienceScaling = Mathf.Max(1f, experienceScaling);
        _onExperienceGained = onExperienceGained;
        _onLevelUp = onLevelUp;
        _styleEffectFactory = new PlayerStyleEffectFactory();

        State.SetProgress(0, CalculateExperienceForLevel(State.CurrentLevel + 1));
    }

    /// <summary>
    /// 経験値を追加します。
    /// </summary>
    /// <param name="amount">追加する経験値量</param>
    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int adjustedAmount = Mathf.RoundToInt(amount * State.ExperienceMultiplier);
        if (adjustedAmount <= 0)
        {
            return;
        }

        State.SetProgress(State.CurrentExperience + adjustedAmount, State.ExperienceToNextLevel);
        _onExperienceGained?.Invoke(adjustedAmount, State.CurrentExperience, State.ExperienceToNextLevel);

        while (State.CurrentExperience >= State.ExperienceToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 経験値倍率を設定します。
    /// </summary>
    /// <param name="multiplier">設定する倍率</param>
    public void SetExperienceMultiplier(float multiplier)
    {
        State.SetExperienceMultiplier(multiplier);
    }

    /// <summary>
    /// 経験値倍率を初期値へ戻します。
    /// </summary>
    public void ResetExperienceMultiplier()
    {
        State.ResetExperienceMultiplier();
    }

    /// <summary>
    /// スタイルを変更し、対応する効果を適用します。
    /// </summary>
    /// <param name="styleType">適用するスタイル種別</param>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ChangeStyle(PlayerStyleType styleType, PlayerStyleEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        ResetStyleParameters();
        State.SetCurrentStyle(styleType);
        _activeStyleEffect = _styleEffectFactory.Create(styleType);
        _activeStyleEffect.ApplyParameters(context);
    }

    /// <summary>
    /// 現在適用中のスタイル効果を更新します。
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void TickStyleEffect(PlayerStyleEffectContext context, float deltaTime)
    {
        if (_activeStyleEffect == null || context == null)
        {
            return;
        }

        _activeStyleEffect.Tick(context, deltaTime);
    }

    /// <summary>
    /// スタイル効果で変更されるパラメータを基準値へ戻します。
    /// </summary>
    public void ResetStyleParameters()
    {
        State.ResetMoveSpeedMultiplier();
        State.ResetExperienceMultiplier();
    }

    /// <summary>
    /// 指定レベルに必要な経験値を計算します。
    /// </summary>
    /// <param name="level">目標レベル</param>
    /// <returns>必要経験値</returns>
    int CalculateExperienceForLevel(int level)
    {
        if (level <= 1)
        {
            return 0;
        }

        float experience = _baseExperienceRequired * Mathf.Pow(_experienceScaling, level - 2);
        return Mathf.RoundToInt(experience);
    }

    /// <summary>
    /// レベルアップ処理を実行します。
    /// </summary>
    void LevelUp()
    {
        int remainingExperience = State.CurrentExperience - State.ExperienceToNextLevel;
        int nextLevel = State.CurrentLevel + 1;
        int nextExperienceToNextLevel = CalculateExperienceForLevel(nextLevel + 1);

        State.ApplyLevelUp(remainingExperience, nextExperienceToNextLevel);
        _onLevelUp?.Invoke(State.CurrentLevel);
    }
}
