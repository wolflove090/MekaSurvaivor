/// <summary>
/// メイドスタイルの効果を提供します
/// </summary>
public class MaidStyleEffect : IPlayerStyleEffect
{
    const float MOVE_SPEED_MULTIPLIER = 1.5f;

    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Maid;

    /// <summary>
    /// 効果パラメータ適用時に移動速度倍率を設定します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        context?.PlayerState?.SetMoveSpeedMultiplier(MOVE_SPEED_MULTIPLIER);
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
