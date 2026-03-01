using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 破壊可能オブジェクトの生成と検索を管理するシングルトンクラス
/// </summary>
public class BreakableObjectSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    [SerializeField]
    [Tooltip("破壊可能オブジェクトのプレハブ")]
    BreakableObject _breakableObjectPrefab;

    [SerializeField]
    [Tooltip("スポーン間隔（秒）")]
    float _spawnInterval = 30f;

    [SerializeField]
    [Tooltip("同時存在数の上限")]
    int _maxAliveCount = 3;

    [SerializeField]
    [Tooltip("画面外へ出すピクセル量")]
    float _offscreenOffsetPixels = 10f;

    [SerializeField]
    [Tooltip("スポーン位置を探す最大試行回数")]
    int _maxSpawnAttempts = 8;

    [SerializeField]
    [Tooltip("初期プールサイズ")]
    int _initialPoolSize = 6;

    ObjectPool<BreakableObject> _breakableObjectPool;
    List<GameObject> _activeBreakableObjects = new List<GameObject>();
    float _spawnTimer;
    Transform _spawnTarget;
    HealPickupSpawner _healPickupSpawner;

    static BreakableObjectSpawner _instance;

    /// <summary>
    /// BreakableObjectSpawnerのシングルトンインスタンスを取得します
    /// </summary>
    public static BreakableObjectSpawner Instance => _instance;

    /// <summary>
    /// シングルトン設定と内部状態の初期化を行います
    /// </summary>
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"BreakableObjectSpawnerが複数存在します。既存: {_instance.gameObject.name}, 新規: {gameObject.name}");
            enabled = false;
            return;
        }

        _instance = this;
    }

    /// <summary>
    /// 開始時にスポーンタイマーとプールを初期化します
    /// </summary>
    void Start()
    {
        _spawnTimer = _spawnInterval;
        InitializePool();
    }

    /// <summary>
    /// 毎フレームでスポーン判定と破棄済み参照のクリーンアップを行います
    /// </summary>
    void Update()
    {
        CleanupNullBreakableObjects();

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            TrySpawnBreakableObject();
            _spawnTimer = _spawnInterval;
        }
    }

    /// <summary>
    /// シーン破棄時にシングルトン参照を解除します
    /// </summary>
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// 指定位置から最も近い破壊可能オブジェクトを検索します
    /// </summary>
    /// <param name="position">基準位置</param>
    /// <param name="onlyVisible">カメラ可視範囲のオブジェクトのみを対象にするか</param>
    /// <returns>最寄りの破壊可能オブジェクト。存在しない場合はnull</returns>
    public GameObject FindNearestBreakableObject(Vector3 position, bool onlyVisible = false)
    {
        CleanupNullBreakableObjects();

        Camera mainCamera = onlyVisible ? Camera.main : null;
        GameObject nearestObject = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject breakableObject in _activeBreakableObjects)
        {
            if (breakableObject == null)
            {
                continue;
            }

            if (onlyVisible && mainCamera != null && !IsVisibleFromCamera(breakableObject, mainCamera))
            {
                continue;
            }

            float distance = Vector3.Distance(position, breakableObject.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestObject = breakableObject;
            }
        }

        return nearestObject;
    }

    /// <summary>
    /// 管理対象から破壊可能オブジェクトを解除します
    /// </summary>
    /// <param name="breakableObject">解除対象のGameObject</param>
    public void UnregisterBreakableObject(GameObject breakableObject)
    {
        _activeBreakableObjects.Remove(breakableObject);
    }

    /// <summary>
    /// スポーン位置計算に使用するターゲットを設定します。
    /// </summary>
    /// <param name="target">基準となるTransform</param>
    public void SetSpawnTarget(Transform target)
    {
        _spawnTarget = target;
    }

    /// <summary>
    /// 破壊時に使用する回復アイテムスポナーを設定します。
    /// </summary>
    /// <param name="healPickupSpawner">設定するスポナー</param>
    public void SetHealPickupSpawner(HealPickupSpawner healPickupSpawner)
    {
        _healPickupSpawner = healPickupSpawner;
    }

    /// <summary>
    /// 破壊可能オブジェクトのスポーンを試みます
    /// </summary>
    void TrySpawnBreakableObject()
    {
        if (_breakableObjectPrefab == null)
        {
            Debug.LogWarning("BreakableObjectSpawner: 破壊可能オブジェクトプレハブが設定されていません");
            return;
        }

        if (_activeBreakableObjects.Count >= _maxAliveCount)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (!TryCalculateSpawnPosition(mainCamera, out Vector3 spawnPosition))
        {
            return;
        }

        BreakableObject breakableObject = _breakableObjectPool != null
            ? _breakableObjectPool.Get()
            : Instantiate(_breakableObjectPrefab);

        breakableObject.transform.position = spawnPosition;
        breakableObject.transform.rotation = Quaternion.identity;
        breakableObject.Initialize(this, _healPickupSpawner);
        _activeBreakableObjects.Add(breakableObject.gameObject);
    }

    /// <summary>
    /// カメラ外縁付近の有効なスポーン位置を計算します
    /// </summary>
    /// <param name="camera">計算に使用するカメラ</param>
    /// <param name="spawnPosition">計算結果のスポーン位置</param>
    /// <returns>有効位置が見つかった場合はtrue</returns>
    bool TryCalculateSpawnPosition(Camera camera, out Vector3 spawnPosition)
    {
        float spawnPlaneY = _spawnTarget != null ? _spawnTarget.position.y : 0f;

        for (int i = 0; i < _maxSpawnAttempts; i++)
        {
            Vector2 edgePoint = PickRandomOffscreenEdgePoint(camera);
            if (!TryScreenPointToGround(camera, edgePoint, spawnPlaneY, out Vector3 worldPoint))
            {
                continue;
            }

            Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
            if (randomDirection2D == Vector2.zero)
            {
                randomDirection2D = Vector2.right;
            }

            float worldOffset = ConvertPixelToWorldDistance(camera, _offscreenOffsetPixels, spawnPlaneY);
            Vector3 randomOffset = new Vector3(randomDirection2D.x, 0f, randomDirection2D.y) * worldOffset;
            spawnPosition = worldPoint + randomOffset;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    /// <summary>
    /// ランダムな画面辺を選び、画面外へオフセットしたスクリーン座標を返します
    /// </summary>
    /// <param name="camera">基準カメラ</param>
    /// <returns>画面外のスクリーン座標</returns>
    Vector2 PickRandomOffscreenEdgePoint(Camera camera)
    {
        int edge = Random.Range(0, 4);
        float width = camera.pixelWidth;
        float height = camera.pixelHeight;
        float offset = Mathf.Max(1f, _offscreenOffsetPixels);

        switch (edge)
        {
            case 0:
                return new Vector2(-offset, Random.Range(0f, height));
            case 1:
                return new Vector2(width + offset, Random.Range(0f, height));
            case 2:
                return new Vector2(Random.Range(0f, width), -offset);
            default:
                return new Vector2(Random.Range(0f, width), height + offset);
        }
    }

    /// <summary>
    /// スクリーン座標を地面平面（Y固定）へ投影します
    /// </summary>
    /// <param name="camera">計算に使用するカメラ</param>
    /// <param name="screenPoint">投影するスクリーン座標</param>
    /// <param name="planeY">投影先平面のY座標</param>
    /// <param name="worldPoint">投影結果のワールド座標</param>
    /// <returns>投影に成功した場合はtrue</returns>
    bool TryScreenPointToGround(Camera camera, Vector2 screenPoint, float planeY, out Vector3 worldPoint)
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0f));
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            worldPoint = ray.GetPoint(enterDistance);
            return true;
        }

        worldPoint = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 指定ピクセル量に相当するワールド距離を地面平面上で算出します
    /// </summary>
    /// <param name="camera">計算に使用するカメラ</param>
    /// <param name="pixelOffset">換算したいピクセル量</param>
    /// <param name="planeY">換算先平面のY座標</param>
    /// <returns>ピクセル量に相当するワールド距離</returns>
    float ConvertPixelToWorldDistance(Camera camera, float pixelOffset, float planeY)
    {
        Vector2 center = new Vector2(camera.pixelWidth * 0.5f, camera.pixelHeight * 0.5f);
        Vector2 shifted = center + new Vector2(pixelOffset, 0f);

        bool hasCenter = TryScreenPointToGround(camera, center, planeY, out Vector3 centerWorldPoint);
        bool hasShifted = TryScreenPointToGround(camera, shifted, planeY, out Vector3 shiftedWorldPoint);

        if (hasCenter && hasShifted)
        {
            return Vector3.Distance(centerWorldPoint, shiftedWorldPoint);
        }

        return 0.5f;
    }

    /// <summary>
    /// 破棄済み参照を管理リストから除外します
    /// </summary>
    void CleanupNullBreakableObjects()
    {
        for (int i = _activeBreakableObjects.Count - 1; i >= 0; i--)
        {
            if (_activeBreakableObjects[i] == null)
            {
                _activeBreakableObjects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// オブジェクトがカメラに写っているか判定します
    /// </summary>
    /// <param name="targetObject">判定対象のオブジェクト</param>
    /// <param name="camera">判定に使用するカメラ</param>
    /// <returns>可視範囲内にある場合はtrue</returns>
    bool IsVisibleFromCamera(GameObject targetObject, Camera camera)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(targetObject.transform.position);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
               viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
               viewportPoint.z > 0f;
    }

    /// <summary>
    /// 破壊可能オブジェクトのプールを初期化します
    /// </summary>
    void InitializePool()
    {
        if (_breakableObjectPrefab == null)
        {
            return;
        }

        _breakableObjectPool = new ObjectPool<BreakableObject>(_breakableObjectPrefab, _initialPoolSize, transform);
    }
}
