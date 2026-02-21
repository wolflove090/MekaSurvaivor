using System;
using UnityEngine;

/// <summary>
/// キャラクターのランタイムステータス参照を提供するコンポーネント
/// </summary>
public class CharacterStats : MonoBehaviour
{
    [SerializeField]
    [Tooltip("参照するステータスデータ")]
    CharacterStatsData _statsData;

    const int DEFAULT_MAX_HP = 10;
    const int DEFAULT_POW = 1;
    const int DEFAULT_DEF = 0;
    const float DEFAULT_SPD = 5f;

    /// <summary>
    /// ステータスデータが変更された時に発火するイベント
    /// </summary>
    public event Action OnStatsDataChanged;

    /// <summary>
    /// ステータスデータが設定済みかどうかを取得します
    /// </summary>
    public bool HasData => _statsData != null;

    /// <summary>
    /// 最大HPを取得します
    /// </summary>
    public int MaxHp => _statsData != null ? _statsData.MaxHp : DEFAULT_MAX_HP;

    /// <summary>
    /// 攻撃力を取得します
    /// </summary>
    public int Pow => _statsData != null ? _statsData.Pow : DEFAULT_POW;

    /// <summary>
    /// 防御力を取得します
    /// </summary>
    public int Def => _statsData != null ? _statsData.Def : DEFAULT_DEF;

    /// <summary>
    /// 移動速度を取得します
    /// </summary>
    public float Spd => _statsData != null ? _statsData.Spd : DEFAULT_SPD;

    /// <summary>
    /// ステータスデータを差し替えて変更通知を発火します
    /// </summary>
    /// <param name="statsData">適用するステータスデータ</param>
    public void ApplyStatsData(CharacterStatsData statsData)
    {
        if (statsData == null)
        {
            Debug.LogWarning("CharacterStats: 適用するCharacterStatsDataがnullです。");
            return;
        }

        _statsData = statsData;
        OnStatsDataChanged?.Invoke();
    }
}
