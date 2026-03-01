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

    /// <summary>
    /// ステータスデータが変更された時に発火するイベント
    /// </summary>
    public event Action OnStatsDataChanged;

    /// <summary>
    /// ステータスデータが設定済みかどうかを取得します
    /// </summary>
    public bool HasData => _statsData != null;

    /// <summary>
    /// 現在のランタイム参照用ステータス値を取得します。
    /// </summary>
    public CharacterStatValues CurrentValues => _statsData != null ? _statsData.ToRuntimeValues() : CharacterStatValues.Default;

    /// <summary>
    /// 最大HPを取得します
    /// </summary>
    public int MaxHp => CurrentValues.MaxHp;

    /// <summary>
    /// 攻撃力を取得します
    /// </summary>
    public int Pow => CurrentValues.Pow;

    /// <summary>
    /// 防御力を取得します
    /// </summary>
    public int Def => CurrentValues.Def;

    /// <summary>
    /// 移動速度を取得します
    /// </summary>
    public float Spd => CurrentValues.Spd;

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
