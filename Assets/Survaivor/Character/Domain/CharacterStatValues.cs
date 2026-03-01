using UnityEngine;

/// <summary>
/// キャラクターのランタイム参照用ステータス値を保持します。
/// </summary>
public readonly struct CharacterStatValues
{
    /// <summary>
    /// 既定のステータス値を取得します。
    /// </summary>
    public static CharacterStatValues Default => new CharacterStatValues(10, 1, 0, 5f);

    /// <summary>
    /// 最大HPを取得します。
    /// </summary>
    public int MaxHp { get; }

    /// <summary>
    /// 攻撃力を取得します。
    /// </summary>
    public int Pow { get; }

    /// <summary>
    /// 防御力を取得します。
    /// </summary>
    public int Def { get; }

    /// <summary>
    /// 移動速度を取得します。
    /// </summary>
    public float Spd { get; }

    /// <summary>
    /// ランタイム参照用のステータス値を初期化します。
    /// </summary>
    /// <param name="maxHp">最大HP</param>
    /// <param name="pow">攻撃力</param>
    /// <param name="def">防御力</param>
    /// <param name="spd">移動速度</param>
    public CharacterStatValues(int maxHp, int pow, int def, float spd)
    {
        MaxHp = Mathf.Max(1, maxHp);
        Pow = Mathf.Max(0, pow);
        Def = Mathf.Max(0, def);
        Spd = Mathf.Max(0f, spd);
    }
}
