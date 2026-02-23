/// <summary>
/// セレブスタイルの効果を提供します
/// </summary>
public class CelebStyleEffect : IPlayerStyleEffect
{
    const float EXPERIENCE_MULTIPLIER = 2f;

    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Celeb;

    /// <summary>
    /// 効果適用開始時に経験値倍率を設定します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void OnEnter(PlayerStyleEffectContext context)
    {
        context?.PlayerExperience?.SetExperienceMultiplier(EXPERIENCE_MULTIPLIER);
    }

    /// <summary>
    /// 効果解除時に経験値倍率を初期値へ戻します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void OnExit(PlayerStyleEffectContext context)
    {
        context?.PlayerExperience?.ResetExperienceMultiplier();
    }

    /// <summary>
    /// 毎フレーム更新処理は不要です
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
    }
}
