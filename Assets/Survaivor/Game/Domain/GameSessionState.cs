/// <summary>
/// ゲーム進行のランタイム状態を保持します。
/// </summary>
public class GameSessionState
{
    /// <summary>
    /// 制限時間を取得します。
    /// </summary>
    public float TimeLimit { get; }

    /// <summary>
    /// 経過時間を取得します。
    /// </summary>
    public float ElapsedTime { get; private set; }

    /// <summary>
    /// ゲームクリア状態かどうかを取得します。
    /// </summary>
    public bool IsGameClear { get; private set; }

    /// <summary>
    /// ゲームオーバー状態かどうかを取得します。
    /// </summary>
    public bool IsGameOver { get; private set; }

    /// <summary>
    /// 残り時間を取得します。
    /// </summary>
    public float RemainingTime => UnityEngine.Mathf.Max(0f, TimeLimit - ElapsedTime);

    /// <summary>
    /// ゲーム進行状態を初期化します。
    /// </summary>
    /// <param name="timeLimit">制限時間</param>
    public GameSessionState(float timeLimit)
    {
        TimeLimit = UnityEngine.Mathf.Max(0f, timeLimit);
        ElapsedTime = 0f;
        IsGameClear = false;
        IsGameOver = false;
    }

    /// <summary>
    /// 指定時間だけゲーム進行を更新します。
    /// </summary>
    /// <param name="deltaTime">経過時間</param>
    public void Advance(float deltaTime)
    {
        if (IsGameClear || IsGameOver || deltaTime <= 0f)
        {
            return;
        }

        ElapsedTime += deltaTime;
        if (ElapsedTime >= TimeLimit)
        {
            ElapsedTime = TimeLimit;
            IsGameClear = true;
        }
    }

    /// <summary>
    /// ゲームオーバー状態へ遷移させます。
    /// </summary>
    public void MarkGameOver()
    {
        if (IsGameClear || IsGameOver)
        {
            return;
        }

        IsGameOver = true;
    }
}
