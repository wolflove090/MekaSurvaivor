using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PlayerProgressionServiceの進行更新とスタイル切り替えを検証します。
/// </summary>
public class PlayerProgressionServiceTests
{
    /// <summary>
    /// 経験値倍率込みで加算し、レベルアップ時の繰り越しが正しく反映されることを検証します。
    /// </summary>
    [Test]
    public void AddExperience_WithMultiplierAndCarryOver_UpdatesStateAndRaisesCallbacks()
    {
        PlayerState state = new PlayerState(1);
        int gainedAmount = 0;
        int reportedCurrentExperience = 0;
        int reportedExperienceToNextLevel = 0;
        int reportedLevel = 0;
        PlayerProgressionService service = new PlayerProgressionService(
            state,
            10,
            2f,
            (amount, currentExperience, experienceToNextLevel) =>
            {
                gainedAmount = amount;
                reportedCurrentExperience = currentExperience;
                reportedExperienceToNextLevel = experienceToNextLevel;
            },
            level => reportedLevel = level);

        service.SetExperienceMultiplier(2f);

        service.AddExperience(15);

        Assert.That(gainedAmount, Is.EqualTo(30));
        Assert.That(reportedCurrentExperience, Is.EqualTo(30));
        Assert.That(reportedExperienceToNextLevel, Is.EqualTo(10));
        Assert.That(reportedLevel, Is.EqualTo(3));
        Assert.That(state.CurrentLevel, Is.EqualTo(3));
        Assert.That(state.CurrentExperience, Is.EqualTo(0));
        Assert.That(state.ExperienceToNextLevel, Is.EqualTo(40));
    }

    /// <summary>
    /// スタイル変更時に前の倍率が残らず、新しいスタイルの効果だけが反映されることを検証します。
    /// </summary>
    [Test]
    public void ChangeStyle_SwitchingStyle_ResetsPreviousMultipliers()
    {
        PlayerState state = new PlayerState(1);
        PlayerProgressionService service = new PlayerProgressionService(state, 10, 1.5f);
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(null, state);

        service.ChangeStyle(PlayerStyleType.Idol, context);

        Assert.That(state.CurrentStyleType, Is.EqualTo(PlayerStyleType.Idol));
        Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.2f));
        Assert.That(state.ExperienceMultiplier, Is.EqualTo(1f));

        service.ChangeStyle(PlayerStyleType.Celeb, context);

        Assert.That(state.CurrentStyleType, Is.EqualTo(PlayerStyleType.Celeb));
        Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1f));
        Assert.That(state.ExperienceMultiplier, Is.EqualTo(2f));

        service.ResetStyleParameters();

        Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1f));
        Assert.That(state.ExperienceMultiplier, Is.EqualTo(1f));
    }

    /// <summary>
    /// セレブから別スタイルへ切り替えた後は経験値倍率が残らないことを検証します。
    /// </summary>
    [Test]
    public void AddExperience_AfterChangingFromCelebToIdol_DoesNotKeepPreviousExperienceMultiplier()
    {
        PlayerState state = new PlayerState(1);
        PlayerProgressionService service = new PlayerProgressionService(state, 10, 1.5f);
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(null, state);

        service.ChangeStyle(PlayerStyleType.Celeb, context);
        service.ChangeStyle(PlayerStyleType.Idol, context);
        service.AddExperience(5);

        Assert.That(state.CurrentStyleType, Is.EqualTo(PlayerStyleType.Idol));
        Assert.That(state.CurrentExperience, Is.EqualTo(5));
        Assert.That(state.ExperienceMultiplier, Is.EqualTo(1f));
    }

    /// <summary>
    /// 巫女スタイルから切り替えた後は回復タイマーが残らず、旧効果が継続しないことを検証します。
    /// </summary>
    [Test]
    public void TickStyleEffect_AfterChangingFromMiko_DoesNotApplyPreviousHealEffect()
    {
        GameObject playerObject = new GameObject("Player");

        try
        {
            playerObject.AddComponent<CharacterStats>();
            HealthComponent healthComponent = playerObject.AddComponent<HealthComponent>();
            PlayerState state = new PlayerState(1);
            PlayerProgressionService service = new PlayerProgressionService(state, 10, 1.5f);
            PlayerStyleEffectContext context = new PlayerStyleEffectContext(healthComponent, state);

            healthComponent.TakeDamage(3);
            int hpBeforeTick = healthComponent.CurrentHp;

            service.ChangeStyle(PlayerStyleType.Miko, context);
            service.TickStyleEffect(context, 29f);
            service.ChangeStyle(PlayerStyleType.Idol, context);
            service.TickStyleEffect(context, 1f);

            Assert.That(state.CurrentStyleType, Is.EqualTo(PlayerStyleType.Idol));
            Assert.That(healthComponent.CurrentHp, Is.EqualTo(hpBeforeTick));
            Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.2f));
            Assert.That(state.ExperienceMultiplier, Is.EqualTo(1f));
        }
        finally
        {
            Object.DestroyImmediate(playerObject);
        }
    }
}
