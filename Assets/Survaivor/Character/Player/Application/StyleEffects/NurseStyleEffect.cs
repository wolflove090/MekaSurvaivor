/// <summary>
/// ナーススタイルの効果を提供します
/// </summary>
public class NurseStyleEffect : IPlayerStyleEffect
{
    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Nurse;

    /// <summary>
    /// 現時点では追加効果を適用しません
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ApplyParameters(PlayerStyleEffectContext context)
    {
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
