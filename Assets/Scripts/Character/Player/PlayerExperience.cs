using UnityEngine;

/// <summary>
/// プレイヤーの経験値とレベルを管理するコンポーネント
/// </summary>
public class PlayerExperience : MonoBehaviour
{
    [Header("初期設定")]
    [SerializeField]
    [Tooltip("開始レベル")]
    int _startLevel = 1;

    [SerializeField]
    [Tooltip("レベル1から2に必要な基礎経験値")]
    int _baseExperienceRequired = 10;

    [SerializeField]
    [Tooltip("レベルごとの経験値増加率")]
    float _experienceScaling = 1.5f;

    int _currentLevel;
    int _currentExperience;
    int _experienceToNextLevel;

    /// <summary>
    /// 現在のレベルを取得します
    /// </summary>
    public int CurrentLevel => _currentLevel;

    /// <summary>
    /// 現在の経験値を取得します
    /// </summary>
    public int CurrentExperience => _currentExperience;

    /// <summary>
    /// 次のレベルまでに必要な経験値を取得します
    /// </summary>
    public int ExperienceToNextLevel => _experienceToNextLevel;

    void Awake()
    {
        _currentLevel = _startLevel;
        _currentExperience = 0;
        _experienceToNextLevel = CalculateExperienceForLevel(_currentLevel + 1);
    }

    /// <summary>
    /// 経験値を追加します
    /// </summary>
    /// <param name="amount">追加する経験値量</param>
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        _currentExperience += amount;
        GameEvents.RaiseExperienceGained(amount, _currentExperience, _experienceToNextLevel);

        // レベルアップ判定
        while (_currentExperience >= _experienceToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// レベルアップ処理を実行します
    /// </summary>
    void LevelUp()
    {
        _currentExperience -= _experienceToNextLevel;
        _currentLevel++;
        _experienceToNextLevel = CalculateExperienceForLevel(_currentLevel + 1);

        Debug.Log($"レベルアップ！ レベル {_currentLevel} に到達しました");
        GameEvents.RaisePlayerLevelUp(_currentLevel);
    }

    /// <summary>
    /// 指定レベルに到達するために必要な経験値を計算します
    /// </summary>
    /// <param name="level">目標レベル</param>
    /// <returns>必要な経験値</returns>
    int CalculateExperienceForLevel(int level)
    {
        // 経験値カーブ: baseExp * (scaling ^ (level - 2))
        // レベル2: 10, レベル3: 15, レベル4: 22, レベル5: 33...
        if (level <= 1) return 0;
        
        float experience = _baseExperienceRequired * Mathf.Pow(_experienceScaling, level - 2);
        return Mathf.RoundToInt(experience);
    }
}
