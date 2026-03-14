/// <summary>
/// カウガールスタイルの効果を提供します
/// </summary>
public class CowgirlStyleEffect : IPlayerStyleEffect
{
    const float AttackIntervalMultiplier = 0.75f;

    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Cowgirl;

    /// <summary>
    /// 攻撃間隔短縮効果を適用します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        context?.PlayerState?.SetAttackIntervalMultiplier(AttackIntervalMultiplier);
    }

    /// <summary>
    /// 毎フレーム更新処理は不要です
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
    }

    /// <summary>
    /// 終了時の後始末はありません
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void Cleanup(PlayerStyleEffectContext context)
    {
    }
}
