using System;
using UnityEngine;

/// <summary>
/// オブジェクトプールから取得・返却されるオブジェクトに付与するコンポーネント
/// </summary>
public class PooledObject : MonoBehaviour
{
    Action _returnAction;
    bool _isInPool = true;

    /// <summary>
    /// プール返却時に実行する処理を設定します
    /// </summary>
    /// <param name="returnAction">返却処理</param>
    public void SetReturnAction(Action returnAction)
    {
        _returnAction = returnAction;
    }

    /// <summary>
    /// プールへ返却します
    /// </summary>
    public void ReturnToPool()
    {
        if (_isInPool)
        {
            return;
        }

        if (_returnAction != null)
        {
            _returnAction.Invoke();
            return;
        }

        // 返却アクションが無い場合はそのまま削除
        Destroy(gameObject);
    }

    /// <summary>
    /// プールから取り出された状態としてマークします
    /// </summary>
    public void MarkSpawned()
    {
        _isInPool = false;
    }

    /// <summary>
    /// プール内にある状態としてマークします
    /// </summary>
    public void MarkReturned()
    {
        _isInPool = true;
    }
}
