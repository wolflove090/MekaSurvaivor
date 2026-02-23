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
    /// 経験値管理コンポーネントを取得します
    /// </summary>
    public PlayerExperience PlayerExperience { get; }

    /// <summary>
    /// プレイヤー制御コンポーネントを取得します
    /// </summary>
    public PlayerController PlayerController { get; }

    /// <summary>
    /// コンテキストを初期化します
    /// </summary>
    /// <param name="healthComponent">HP管理コンポーネント</param>
    /// <param name="playerExperience">経験値管理コンポーネント</param>
    /// <param name="playerController">プレイヤー制御コンポーネント</param>
    public PlayerStyleEffectContext(
        HealthComponent healthComponent,
        PlayerExperience playerExperience,
        PlayerController playerController)
    {
        HealthComponent = healthComponent;
        PlayerExperience = playerExperience;
        PlayerController = playerController;
    }
}
