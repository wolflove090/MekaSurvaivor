/// <summary>
/// 巫女スタイルの効果を提供します
/// </summary>
public class MikoStyleEffect : IPlayerStyleEffect
{
    const float HEAL_INTERVAL_SECONDS = 30f;
    const int HEAL_AMOUNT = 1;

    float _timer;

    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Miko;

    /// <summary>
    /// 効果パラメータ適用時に内部タイマーを初期化します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        _timer = 0f;
    }

    /// <summary>
    /// 経過時間に応じて30秒ごとにHPを1回復します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
        if (context == null || context.HealthComponent == null)
        {
            return;
        }

        _timer += deltaTime;

        while (_timer >= HEAL_INTERVAL_SECONDS)
        {
            context.HealthComponent.Heal(HEAL_AMOUNT);
            _timer -= HEAL_INTERVAL_SECONDS;
        }
    }
}
