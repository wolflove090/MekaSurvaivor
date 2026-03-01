/// <summary>
/// スタイル効果で利用するプレイヤー参照を集約します
/// </summary>
public class PlayerStyleEffectContext
{
    /// <summary>
    /// HP管理コンポーネントを取得します
    /// </summary>
    public HealthComponent HealthComponent { get; }

    /// <summary>
    /// プレイヤー進行状態を取得します
    /// </summary>
    public PlayerState PlayerState { get; }

    /// <summary>
    /// コンテキストを初期化します
    /// </summary>
    /// <param name="healthComponent">HP管理コンポーネント</param>
    /// <param name="playerState">プレイヤー進行状態</param>
    public PlayerStyleEffectContext(
        HealthComponent healthComponent,
        PlayerState playerState)
    {
        HealthComponent = healthComponent;
        PlayerState = playerState;
    }
}
