using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタイル種別に応じた効果オブジェクトを生成します
/// </summary>
public class PlayerStyleEffectFactory
{
    readonly Dictionary<PlayerStyleType, Func<IPlayerStyleEffect>> _builders =
        new Dictionary<PlayerStyleType, Func<IPlayerStyleEffect>>
        {
            { PlayerStyleType.Miko, () => new MikoStyleEffect() },
            { PlayerStyleType.Idol, () => new IdolStyleEffect() },
            { PlayerStyleType.Celeb, () => new CelebStyleEffect() }
        };

    /// <summary>
    /// 指定スタイルに対応する効果オブジェクトを生成します
    /// </summary>
    /// <param name="styleType">スタイル種別</param>
    /// <returns>生成した効果オブジェクト</returns>
    public IPlayerStyleEffect Create(PlayerStyleType styleType)
    {
        if (_builders.TryGetValue(styleType, out Func<IPlayerStyleEffect> builder))
        {
            return builder();
        }

        Debug.LogWarning($"PlayerStyleEffectFactory: 未対応のスタイル種別です。 styleType={styleType}");
        return new MikoStyleEffect();
    }
}
