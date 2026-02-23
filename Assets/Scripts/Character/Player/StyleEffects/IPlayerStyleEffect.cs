/// <summary>
/// プレイヤースタイル効果の共通契約を定義します
/// </summary>
public interface IPlayerStyleEffect
{
    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    PlayerStyleType StyleType { get; }

    /// <summary>
    /// 効果パラメータを適用します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    void ApplyParameters(PlayerStyleEffectContext context);

    /// <summary>
    /// 毎フレームの効果更新処理を実行します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    void Tick(PlayerStyleEffectContext context, float deltaTime);
}
