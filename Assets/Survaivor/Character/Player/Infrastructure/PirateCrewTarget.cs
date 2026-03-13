using UnityEngine;

/// <summary>
/// 敵の追跡候補となる海賊戦闘員を識別します
/// </summary>
public class PirateCrewTarget : MonoBehaviour
{
    /// <summary>
    /// 現在追跡候補として有効かどうかを取得します
    /// </summary>
    public bool IsAvailable => isActiveAndEnabled && gameObject.activeInHierarchy;
}
