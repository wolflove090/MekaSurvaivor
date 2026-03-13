using UnityEngine;

/// <summary>
/// 海賊戦闘員の再召喚と全返却を行う窓口を定義します
/// </summary>
public interface IPirateCrewSummonController
{
    /// <summary>
    /// 現在の戦闘員を返却し、新たな戦闘員を召喚します
    /// </summary>
    /// <param name="playerPosition">プレイヤー位置</param>
    /// <param name="playerFacingDirection">プレイヤーの向き</param>
    void ResummonCrew(Vector3 playerPosition, Vector3 playerFacingDirection);

    /// <summary>
    /// すべての戦闘員を返却します
    /// </summary>
    void ClearAll();
}
