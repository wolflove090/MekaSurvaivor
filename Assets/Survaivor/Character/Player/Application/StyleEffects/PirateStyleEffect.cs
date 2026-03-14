using UnityEngine;

/// <summary>
/// 海賊スタイルの効果を提供します
/// </summary>
public class PirateStyleEffect : IPlayerStyleEffect
{
    const float InitialSummonDelaySeconds = 1f;
    const float ReSummonIntervalSeconds = 5f;

    float _timer;
    float _currentIntervalSeconds;

    /// <summary>
    /// 効果が対応するスタイル種別を取得します
    /// </summary>
    public PlayerStyleType StyleType => PlayerStyleType.Pirate;

    /// <summary>
    /// 内部タイマーを初期化し、残存戦闘員をクリアします
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        _timer = 0f;
        _currentIntervalSeconds = InitialSummonDelaySeconds;
        context?.PirateCrewSummonController?.ClearAll();
    }

    /// <summary>
    /// 初回1秒、その後は5秒ごとに海賊戦闘員の再召喚を実行します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
        if (context?.PirateCrewSummonController == null || Time.timeScale <= 0f)
        {
            return;
        }

        _timer += deltaTime;
        while (_timer >= _currentIntervalSeconds)
        {
            context.PirateCrewSummonController.ResummonCrew(
                context.PlayerTransform != null ? context.PlayerTransform.position : Vector3.zero,
                context.PlayerFacingDirection);
            _timer -= _currentIntervalSeconds;
            _currentIntervalSeconds = ReSummonIntervalSeconds;
        }
    }

    /// <summary>
    /// 効果終了時にタイマーを停止し、戦闘員を全返却します
    /// </summary>
    /// <param name="context">スタイル効果コンテキスト</param>
    public void Cleanup(PlayerStyleEffectContext context)
    {
        _timer = 0f;
        _currentIntervalSeconds = InitialSummonDelaySeconds;
        context?.PirateCrewSummonController?.ClearAll();
    }
}
