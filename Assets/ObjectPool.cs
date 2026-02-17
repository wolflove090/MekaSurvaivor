using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componentを対象にしたシンプルなオブジェクトプール
/// </summary>
/// <typeparam name="T">プール対象のComponent型</typeparam>
public class ObjectPool<T> where T : Component
{
    readonly Queue<T> _pool = new Queue<T>();
    readonly T _prefab;
    readonly Transform _poolRoot;

    /// <summary>
    /// オブジェクトプールを作成します
    /// </summary>
    /// <param name="prefab">生成元プレハブ</param>
    /// <param name="initialSize">初期生成数</param>
    /// <param name="poolRoot">プール内オブジェクトの親Transform</param>
    public ObjectPool(T prefab, int initialSize = 0, Transform poolRoot = null)
    {
        _prefab = prefab;
        _poolRoot = poolRoot;

        int prewarmCount = Mathf.Max(0, initialSize);
        for (int i = 0; i < prewarmCount; i++)
        {
            T instance = CreateInstance();
            Return(instance);
        }
    }

    /// <summary>
    /// オブジェクトを取得します
    /// </summary>
    /// <returns>利用可能なオブジェクト</returns>
    public T Get()
    {
        T instance = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();

        instance.transform.SetParent(null, true);
        instance.gameObject.SetActive(true);

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.MarkSpawned();
        }

        return instance;
    }

    /// <summary>
    /// オブジェクトをプールへ返却します
    /// </summary>
    /// <param name="instance">返却対象</param>
    public void Return(T instance)
    {
        if (instance == null)
        {
            return;
        }

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.MarkReturned();
        }

        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_poolRoot, false);
        _pool.Enqueue(instance);
    }

    /// <summary>
    /// 新しいインスタンスを生成し、プール返却フックを設定します
    /// </summary>
    /// <returns>初期化済みインスタンス</returns>
    T CreateInstance()
    {
        T instance = Object.Instantiate(_prefab, _poolRoot);
        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = instance.gameObject.AddComponent<PooledObject>();
        }

        pooledObject.SetReturnAction(() => Return(instance));
        pooledObject.MarkReturned();
        return instance;
    }
}
