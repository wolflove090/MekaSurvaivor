using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 海賊戦闘員の生成、再召喚、返却を管理します
/// </summary>
public class PirateCrewSummonController : MonoBehaviour, IPirateCrewSummonController
{
    const int CrewCountPerSummon = 2;
    const int CrewMaxHp = 30;
    const float CrewPow = 1f;
    const int CrewDef = 0;
    const float CrewMoveSpeed = 4.5f;

    [SerializeField]
    [Tooltip("戦闘員プレハブ。未設定時はランタイムで生成します")]
    PirateCrewMemberController _crewMemberPrefab;

    [SerializeField]
    [Tooltip("戦闘員ステータスデータ。未設定時はランタイムで生成します")]
    CharacterStatsData _crewStatsData;

    [SerializeField]
    [Tooltip("プールの初期生成数")]
    int _initialPoolSize = 2;

    [SerializeField]
    [Tooltip("プレイヤー前方への基本召喚距離")]
    float _forwardSpawnDistance = 1.5f;

    [SerializeField]
    [Tooltip("プレイヤー右方向への召喚距離")]
    float _rightSpawnDistance = 1.5f;

    ObjectPool<PirateCrewMemberController> _crewPool;
    readonly List<PirateCrewMemberController> _activeCrewMembers = new List<PirateCrewMemberController>();
    PirateCrewRegistry _pirateCrewRegistry;
    Transform _playerTransform;
    EnemyRegistry _enemyRegistry;

    /// <summary>
    /// アクティブな戦闘員数を取得します
    /// </summary>
    public int ActiveCrewCount => _activeCrewMembers.Count;

    /// <summary>
    /// アクティブな戦闘員一覧を取得します
    /// </summary>
    public IReadOnlyList<PirateCrewMemberController> ActiveCrewMembers => _activeCrewMembers;

    void Awake()
    {
        EnsureDependencies();
        EnsurePoolInitialized();
    }

    /// <summary>
    /// 依存参照を設定します
    /// </summary>
    /// <param name="playerTransform">プレイヤーTransform</param>
    /// <param name="enemyRegistry">敵レジストリ</param>
    public void Configure(Transform playerTransform, EnemyRegistry enemyRegistry)
    {
        _playerTransform = playerTransform;
        _enemyRegistry = enemyRegistry;
        EnsureDependencies();
        EnsurePoolInitialized();
    }

    /// <summary>
    /// 既存戦闘員を返却し、前方と右方向へ1体ずつ再召喚します
    /// </summary>
    /// <param name="playerPosition">プレイヤー位置</param>
    /// <param name="playerFacingDirection">プレイヤーの向き</param>
    public void ResummonCrew(Vector3 playerPosition, Vector3 playerFacingDirection)
    {
        ClearAll();
        EnsurePoolInitialized();

        if (_crewPool == null)
        {
            Debug.LogWarning("PirateCrewSummonController: 戦闘員プールを初期化できなかったため召喚できません。");
            return;
        }

        Vector3 forward = ProjectToGround(playerFacingDirection);
        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            forward = Vector3.right;
        }

        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3[] summonPositions =
        {
            playerPosition + forward * _forwardSpawnDistance,
            playerPosition + right * _rightSpawnDistance
        };

        for (int i = 0; i < CrewCountPerSummon; i++)
        {
            PirateCrewMemberController crewMember = null;

            try
            {
                crewMember = _crewPool.Get();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"PirateCrewSummonController: 戦闘員の取得に失敗しました。 index={i}, message={ex.Message}");
                continue;
            }

            if (crewMember == null)
            {
                Debug.LogWarning($"PirateCrewSummonController: 戦闘員の取得に失敗しました。 index={i}");
                continue;
            }

            crewMember.transform.position = summonPositions[i];
            crewMember.transform.rotation = Quaternion.identity;
            crewMember.Initialize(
                _playerTransform,
                _enemyRegistry,
                _pirateCrewRegistry,
                _crewStatsData,
                ProjectToGround(summonPositions[i] - playerPosition),
                OnCrewMemberReleased);
            _activeCrewMembers.Add(crewMember);
        }
    }

    /// <summary>
    /// すべてのアクティブ戦闘員を返却します
    /// </summary>
    public void ClearAll()
    {
        for (int i = _activeCrewMembers.Count - 1; i >= 0; i--)
        {
            PirateCrewMemberController crewMember = _activeCrewMembers[i];
            if (crewMember == null)
            {
                _activeCrewMembers.RemoveAt(i);
                continue;
            }

            crewMember.ReturnToPool();
        }

        _activeCrewMembers.Clear();
    }

    /// <summary>
    /// 依存コンポーネントを補完します
    /// </summary>
    void EnsureDependencies()
    {
        if (_pirateCrewRegistry == null)
        {
            _pirateCrewRegistry = GetComponent<PirateCrewRegistry>();
            if (_pirateCrewRegistry == null)
            {
                _pirateCrewRegistry = gameObject.AddComponent<PirateCrewRegistry>();
            }
        }

        if (_crewStatsData == null)
        {
            _crewStatsData = ScriptableObject.CreateInstance<CharacterStatsData>();
            _crewStatsData.SetValues(CrewMaxHp, CrewPow, CrewDef, CrewMoveSpeed);
        }

        if (_crewMemberPrefab == null)
        {
            _crewMemberPrefab = CreateRuntimeCrewPrefab();
        }
    }

    /// <summary>
    /// プールを初期化します
    /// </summary>
    void EnsurePoolInitialized()
    {
        if (_crewPool != null || _crewMemberPrefab == null)
        {
            return;
        }

        _crewPool = new ObjectPool<PirateCrewMemberController>(_crewMemberPrefab, _initialPoolSize, transform);
    }

    /// <summary>
    /// ランタイム用の戦闘員プレハブを生成します
    /// </summary>
    /// <returns>生成したプレハブコンポーネント</returns>
    PirateCrewMemberController CreateRuntimeCrewPrefab()
    {
        GameObject prefabRoot = new GameObject("PirateCrewMemberRuntimePrefab");
        prefabRoot.transform.SetParent(transform, false);
        prefabRoot.SetActive(false);
        prefabRoot.AddComponent<CharacterStats>();
        prefabRoot.AddComponent<HealthComponent>();
        prefabRoot.AddComponent<PirateCrewTarget>();
        PirateCrewMemberController memberController = prefabRoot.AddComponent<PirateCrewMemberController>();
        return memberController;
    }

    /// <summary>
    /// 戦闘員返却時に一覧から除外してプールへ戻します
    /// </summary>
    /// <param name="crewMember">返却対象</param>
    void OnCrewMemberReleased(PirateCrewMemberController crewMember)
    {
        _activeCrewMembers.Remove(crewMember);
        _crewPool?.Return(crewMember);
    }

    /// <summary>
    /// ベクトルをXZ平面へ射影します
    /// </summary>
    /// <param name="vector">射影対象</param>
    /// <returns>XZ平面ベクトル</returns>
    Vector3 ProjectToGround(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }
}
