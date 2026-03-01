using UnityEngine;

/// <summary>
/// キャラクターの基礎ステータスを保持するデータアセット
/// </summary>
[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "MekaSurvaivor/Character Stats Data")]
public class CharacterStatsData : ScriptableObject
{
    [SerializeField]
    [Tooltip("最大HP")]
    int _maxHp = 10;

    [SerializeField]
    [Tooltip("攻撃力")]
    int _pow = 1;

    [SerializeField]
    [Tooltip("防御力")]
    int _def = 0;

    [SerializeField]
    [Tooltip("移動速度")]
    float _spd = 5f;

    /// <summary>
    /// 最大HPを取得します
    /// </summary>
    public int MaxHp => _maxHp;

    /// <summary>
    /// 攻撃力を取得します
    /// </summary>
    public int Pow => _pow;

    /// <summary>
    /// 防御力を取得します
    /// </summary>
    public int Def => _def;

    /// <summary>
    /// 移動速度を取得します
    /// </summary>
    public float Spd => _spd;

    /// <summary>
    /// ランタイム参照用のステータス値へ変換します。
    /// </summary>
    /// <returns>ランタイム参照用ステータス値</returns>
    public CharacterStatValues ToRuntimeValues()
    {
        return new CharacterStatValues(_maxHp, _pow, _def, _spd);
    }

    void OnValidate()
    {
        _maxHp = Mathf.Max(1, _maxHp);
        _pow = Mathf.Max(0, _pow);
        _def = Mathf.Max(0, _def);
        _spd = Mathf.Max(0f, _spd);
    }
}
