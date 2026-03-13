using NUnit.Framework;
using UnityEngine;

/// <summary>
/// PirateStyleEffectの召喚タイマー挙動を検証します。
/// </summary>
public class PirateStyleEffectTests
{
    sealed class FakePirateCrewSummonController : IPirateCrewSummonController
    {
        public int ClearAllCount { get; private set; }
        public int ResummonCount { get; private set; }

        public void ResummonCrew(Vector3 playerPosition, Vector3 playerFacingDirection)
        {
            ResummonCount++;
        }

        public void ClearAll()
        {
            ClearAllCount++;
        }
    }

    /// <summary>
    /// 初回1秒経過時に再召喚が発生することを検証します。
    /// </summary>
    [Test]
    public void Tick_AfterOneSecond_ResummonsCrew()
    {
        PirateStyleEffect effect = new PirateStyleEffect();
        PlayerState state = new PlayerState(1);
        FakePirateCrewSummonController fakeController = new FakePirateCrewSummonController();
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(
            null,
            state,
            null,
            () => Vector3.right,
            null,
            fakeController);

        effect.ApplyParameters(context);
        effect.Tick(context, 0.9f);
        effect.Tick(context, 0.1f);

        Assert.That(fakeController.ClearAllCount, Is.EqualTo(1));
        Assert.That(fakeController.ResummonCount, Is.EqualTo(1));
    }

    /// <summary>
    /// 初回召喚後は5秒間隔で再召喚されることを検証します。
    /// </summary>
    [Test]
    public void Tick_AfterInitialSummon_ResummonsEveryFiveSeconds()
    {
        PirateStyleEffect effect = new PirateStyleEffect();
        PlayerState state = new PlayerState(1);
        FakePirateCrewSummonController fakeController = new FakePirateCrewSummonController();
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(
            null,
            state,
            null,
            () => Vector3.right,
            null,
            fakeController);

        effect.ApplyParameters(context);
        effect.Tick(context, 1f);
        effect.Tick(context, 4.9f);
        effect.Tick(context, 0.1f);

        Assert.That(fakeController.ResummonCount, Is.EqualTo(2));
    }

    /// <summary>
    /// 一時停止中は召喚タイマーが進まないことを検証します。
    /// </summary>
    [Test]
    public void Tick_WhenTimeScaleIsZero_DoesNotProgressTimer()
    {
        float originalTimeScale = Time.timeScale;
        PirateStyleEffect effect = new PirateStyleEffect();
        PlayerState state = new PlayerState(1);
        FakePirateCrewSummonController fakeController = new FakePirateCrewSummonController();
        PlayerStyleEffectContext context = new PlayerStyleEffectContext(
            null,
            state,
            null,
            () => Vector3.right,
            null,
            fakeController);

        try
        {
            effect.ApplyParameters(context);
            Time.timeScale = 0f;

            effect.Tick(context, 20f);

            Assert.That(fakeController.ResummonCount, Is.EqualTo(0));
        }
        finally
        {
            Time.timeScale = originalTimeScale;
        }
    }
}
