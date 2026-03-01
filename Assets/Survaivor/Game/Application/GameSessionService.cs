using System;

/// <summary>
/// ゲーム進行状態の更新と遷移通知を管理します。
/// </summary>
public class GameSessionService
{
    readonly Action _onGameClear;
    readonly Action _onGameOver;

    /// <summary>
    /// ゲーム進行状態を取得します。
    /// </summary>
    public GameSessionState State { get; }

    /// <summary>
    /// 残り時間を取得します。
    /// </summary>
    public float RemainingTime => State.RemainingTime;

    /// <summary>
    /// ゲームクリア状態かどうかを取得します。
    /// </summary>
    public bool IsGameClear => State.IsGameClear;

    /// <summary>
    /// ゲームオーバー状態かどうかを取得します。
    /// </summary>
    public bool IsGameOver => State.IsGameOver;

    /// <summary>
    /// ゲーム進行サービスを初期化します。
    /// </summary>
    /// <param name="state">対象の進行状態</param>
    /// <param name="onGameClear">ゲームクリア時の通知</param>
    /// <param name="onGameOver">ゲームオーバー時の通知</param>
    public GameSessionService(GameSessionState state, Action onGameClear = null, Action onGameOver = null)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        _onGameClear = onGameClear;
        _onGameOver = onGameOver;
    }

    /// <summary>
    /// 毎フレームの進行更新を行います。
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    public void Tick(float deltaTime)
    {
        bool wasGameClear = State.IsGameClear;
        State.Advance(deltaTime);

        if (!wasGameClear && State.IsGameClear)
        {
            _onGameClear?.Invoke();
        }
    }

    /// <summary>
    /// ゲームオーバー状態へ遷移させます。
    /// </summary>
    public void MarkGameOver()
    {
        bool wasGameOver = State.IsGameOver;
        State.MarkGameOver();

        if (!wasGameOver && State.IsGameOver)
        {
            _onGameOver?.Invoke();
        }
    }
}
