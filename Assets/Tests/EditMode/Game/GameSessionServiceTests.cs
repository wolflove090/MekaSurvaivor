using NUnit.Framework;

/// <summary>
/// GameSessionServiceの進行更新を検証します。
/// </summary>
public class GameSessionServiceTests
{
    /// <summary>
    /// 制限時間経過でゲームクリアになることを検証します。
    /// </summary>
    [Test]
    public void Tick_ReachesTimeLimit_MarksGameClearOnce()
    {
        bool gameClearRaised = false;
        int gameClearCount = 0;
        GameSessionService service = new GameSessionService(
            new GameSessionState(10f),
            () =>
            {
                gameClearRaised = true;
                gameClearCount++;
            });

        service.Tick(4f);
        service.Tick(6f);
        service.Tick(1f);

        Assert.That(service.IsGameClear, Is.True);
        Assert.That(service.RemainingTime, Is.EqualTo(0f));
        Assert.That(gameClearRaised, Is.True);
        Assert.That(gameClearCount, Is.EqualTo(1));
    }

    /// <summary>
    /// ゲームオーバー後は進行が止まることを検証します。
    /// </summary>
    [Test]
    public void Tick_AfterGameOver_DoesNotAdvance()
    {
        GameSessionService service = new GameSessionService(new GameSessionState(10f));

        service.Tick(3f);
        service.MarkGameOver();
        service.Tick(5f);

        Assert.That(service.IsGameOver, Is.True);
        Assert.That(service.State.ElapsedTime, Is.EqualTo(3f));
        Assert.That(service.RemainingTime, Is.EqualTo(7f));
        Assert.That(service.IsGameClear, Is.False);
    }

    /// <summary>
    /// ゲームオーバー遷移時の通知が一度だけ発火することを検証します。
    /// </summary>
    [Test]
    public void MarkGameOver_RaisedOnce_InvokesCallbackOnce()
    {
        int gameOverCount = 0;
        GameSessionService service = new GameSessionService(
            new GameSessionState(10f),
            onGameOver: () => gameOverCount++);

        service.MarkGameOver();
        service.MarkGameOver();

        Assert.That(service.IsGameOver, Is.True);
        Assert.That(gameOverCount, Is.EqualTo(1));
    }
}
