using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器発動要求をUnityオブジェクト生成へ変換します。
/// </summary>
public class WeaponEffectExecutor : IWeaponEffectExecutor
{
    readonly BulletFactory _bulletFactory;
    readonly Transform _poolRoot;

    ObjectPool<BulletController> _bulletPool;
    ObjectPool<ThrowingBulletController> _throwingBulletPool;
    ObjectPool<DamageFieldController> _damageFieldPool;
    ObjectPool<DroneController> _dronePool;
    ObjectPool<BoundBallController> _boundBallPool;
    ObjectPool<FlameBottleProjectileController> _flameBottlePool;
    ObjectPool<DamageFieldController> _flameAreaPool;
    readonly Dictionary<int, DroneController> _activeDroneControllersByPhase = new Dictionary<int, DroneController>();

    const int BULLET_POOL_SIZE = 20;
    const int THROWING_POOL_SIZE = 12;
    const int DAMAGE_FIELD_POOL_SIZE = 2;
    const int DRONE_POOL_SIZE = 2;
    const int BOUND_BALL_POOL_SIZE = 8;
    const int FLAME_BOTTLE_POOL_SIZE = 6;
    const int FLAME_AREA_POOL_SIZE = 4;

    /// <summary>
    /// 武器実行アダプタを初期化します。
    /// </summary>
    /// <param name="bulletFactory">武器用プレハブ参照の提供元</param>
    /// <param name="poolRoot">プール配置先のTransform</param>
    public WeaponEffectExecutor(BulletFactory bulletFactory, Transform poolRoot)
    {
        _bulletFactory = bulletFactory;
        _poolRoot = poolRoot;
    }

    /// <summary>
    /// 通常弾の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    public void FireBullet(BulletFireRequest request)
    {
        if (request == null)
        {
            return;
        }

        BulletController bulletController = GetOrCreateBulletController();
        if (bulletController == null)
        {
            return;
        }

        PrepareSpawnedObject(bulletController.transform, request.Origin);
        bulletController.SetSourcePow(request.SourcePow);
        bulletController.SetDirection(request.Direction);
    }

    /// <summary>
    /// 投擲弾の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    public void FireThrowing(ThrowingFireRequest request)
    {
        if (request == null)
        {
            return;
        }

        ThrowingBulletController bulletController = GetOrCreateThrowingBulletController();
        if (bulletController == null)
        {
            return;
        }

        PrepareSpawnedObject(bulletController.transform, request.Origin);
        bulletController.SetSourcePow(request.SourcePow);
        bulletController.SetDirection(request.Direction);
    }

    /// <summary>
    /// ダメージフィールド生成要求を実行します。
    /// </summary>
    /// <param name="request">実行する生成要求</param>
    public void SpawnDamageField(DamageFieldSpawnRequest request)
    {
        if (request == null)
        {
            return;
        }

        DamageFieldController damageFieldController = GetOrCreateDamageFieldController();
        if (damageFieldController == null)
        {
            return;
        }

        PrepareSpawnedObject(damageFieldController.transform, request.Origin);
        damageFieldController.SetFollowTarget(request.FollowTarget);
        damageFieldController.SetFollowEnabled(true);
        damageFieldController.SetSourcePow(request.SourcePow);
        damageFieldController.SetAreaScale(request.AreaScale);
    }

    /// <summary>
    /// ドローン展開要求を実行します。
    /// </summary>
    /// <param name="request">実行する展開要求</param>
    public void DeployDrone(DroneSpawnRequest request)
    {
        if (request == null)
        {
            return;
        }

        int phaseKey = NormalizePhaseKey(request.PhaseOffsetDegrees);
        if (_activeDroneControllersByPhase.TryGetValue(phaseKey, out DroneController activeDroneController) &&
            activeDroneController != null &&
            activeDroneController.gameObject.activeSelf)
        {
            PrepareSpawnedObject(activeDroneController.transform, request.Origin);
            activeDroneController.Initialize(
                request.FollowTarget,
                _bulletFactory != null ? _bulletFactory.EnemyRegistry : null,
                this,
                request.SourcePow,
                request.OrbitRadius,
                request.ShotInterval,
                request.PhaseOffsetDegrees);
            return;
        }

        DroneController droneController = GetOrCreateDroneController();
        if (droneController == null)
        {
            return;
        }

        // 位相ごとにドローンを保持し、Lv5の2機展開時も同じ個体を安定再利用する。
        _activeDroneControllersByPhase[phaseKey] = droneController;
        PrepareSpawnedObject(droneController.transform, request.Origin);
        droneController.Initialize(
            request.FollowTarget,
            _bulletFactory != null ? _bulletFactory.EnemyRegistry : null,
            this,
            request.SourcePow,
            request.OrbitRadius,
            request.ShotInterval,
            request.PhaseOffsetDegrees);
    }

    /// <summary>
    /// バウンドボールの発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    public void FireBoundBall(BoundBallFireRequest request)
    {
        if (request == null)
        {
            return;
        }

        BoundBallController boundBallController = GetOrCreateBoundBallController();
        if (boundBallController == null)
        {
            return;
        }

        PrepareSpawnedObject(boundBallController.transform, request.Origin);
        boundBallController.SetSourcePow(request.SourcePow);
        boundBallController.SetDirection(request.Direction);
        boundBallController.SetBounceCount(request.MaxBounceCount);
    }

    /// <summary>
    /// 火炎瓶の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    public void FireFlameBottle(FlameBottleFireRequest request)
    {
        if (request == null)
        {
            return;
        }

        FlameBottleProjectileController flameBottleController = GetOrCreateFlameBottleController();
        if (flameBottleController == null)
        {
            return;
        }

        PrepareSpawnedObject(flameBottleController.transform, request.Origin);
        flameBottleController.Initialize(
            request.InitialVelocity,
            request.SourcePow,
            request.GroundY,
            request.FlameDuration,
            request.FlameRadius,
            this);
    }

    /// <summary>
    /// 炎エリア生成要求を実行します。
    /// </summary>
    /// <param name="request">実行する生成要求</param>
    public void SpawnFlameArea(FlameAreaSpawnRequest request)
    {
        if (request == null)
        {
            return;
        }

        DamageFieldController flameAreaController = GetOrCreateFlameAreaController();
        if (flameAreaController == null)
        {
            return;
        }

        PrepareSpawnedObject(flameAreaController.transform, request.Origin);
        flameAreaController.SetFollowTarget(null);
        flameAreaController.SetFollowEnabled(false);
        flameAreaController.SetSourcePow(request.SourcePow);
        flameAreaController.SetDuration(request.Duration);
        flameAreaController.SetDamageInterval(request.TickInterval);
        flameAreaController.SetAreaScale(request.Radius);
    }

    BulletController GetOrCreateBulletController()
    {
        GameObject bulletPrefab = _bulletFactory != null ? _bulletFactory.BulletPrefab : null;
        if (bulletPrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: BulletPrefabが設定されていません。");
            return null;
        }

        if (_bulletPool == null)
        {
            BulletController bulletPrefabController = bulletPrefab.GetComponent<BulletController>();
            if (bulletPrefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: 弾プレハブにBulletControllerがアタッチされていません。");
                return null;
            }

            _bulletPool = new ObjectPool<BulletController>(bulletPrefabController, BULLET_POOL_SIZE, _poolRoot);
        }

        return _bulletPool.Get();
    }

    ThrowingBulletController GetOrCreateThrowingBulletController()
    {
        GameObject throwingBulletPrefab = _bulletFactory != null ? _bulletFactory.ThrowingBulletPrefab : null;
        if (throwingBulletPrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: ThrowingBulletPrefabが設定されていません。");
            return null;
        }

        if (_throwingBulletPool == null)
        {
            ThrowingBulletController prefabController = throwingBulletPrefab.GetComponent<ThrowingBulletController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: 投擲弾プレハブにThrowingBulletControllerがアタッチされていません。");
                return null;
            }

            _throwingBulletPool = new ObjectPool<ThrowingBulletController>(prefabController, THROWING_POOL_SIZE, _poolRoot);
        }

        return _throwingBulletPool.Get();
    }

    DamageFieldController GetOrCreateDamageFieldController()
    {
        GameObject damageFieldPrefab = _bulletFactory != null ? _bulletFactory.DamageFieldPrefab : null;
        if (damageFieldPrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: DamageFieldPrefabが設定されていません。");
            return null;
        }

        if (_damageFieldPool == null)
        {
            DamageFieldController prefabController = damageFieldPrefab.GetComponent<DamageFieldController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: ダメージフィールドプレハブにDamageFieldControllerがアタッチされていません。");
                return null;
            }

            _damageFieldPool = new ObjectPool<DamageFieldController>(prefabController, DAMAGE_FIELD_POOL_SIZE, _poolRoot);
        }

        return _damageFieldPool.Get();
    }

    DroneController GetOrCreateDroneController()
    {
        GameObject dronePrefab = _bulletFactory != null ? _bulletFactory.DronePrefab : null;
        if (dronePrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: DronePrefabが設定されていません。");
            return null;
        }

        if (_dronePool == null)
        {
            DroneController prefabController = dronePrefab.GetComponent<DroneController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: ドローンプレハブにDroneControllerがアタッチされていません。");
                return null;
            }

            _dronePool = new ObjectPool<DroneController>(prefabController, DRONE_POOL_SIZE, _poolRoot);
        }

        return _dronePool.Get();
    }

    BoundBallController GetOrCreateBoundBallController()
    {
        GameObject boundBallPrefab = _bulletFactory != null ? _bulletFactory.BoundBallPrefab : null;
        if (boundBallPrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: BoundBallPrefabが設定されていません。");
            return null;
        }

        if (_boundBallPool == null)
        {
            BoundBallController prefabController = boundBallPrefab.GetComponent<BoundBallController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: バウンドボールプレハブにBoundBallControllerがアタッチされていません。");
                return null;
            }

            _boundBallPool = new ObjectPool<BoundBallController>(prefabController, BOUND_BALL_POOL_SIZE, _poolRoot);
        }

        return _boundBallPool.Get();
    }

    FlameBottleProjectileController GetOrCreateFlameBottleController()
    {
        GameObject flameBottlePrefab = _bulletFactory != null ? _bulletFactory.FlameBottlePrefab : null;
        if (flameBottlePrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: FlameBottlePrefabが設定されていません。");
            return null;
        }

        if (_flameBottlePool == null)
        {
            FlameBottleProjectileController prefabController = flameBottlePrefab.GetComponent<FlameBottleProjectileController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: 火炎瓶プレハブにFlameBottleProjectileControllerがアタッチされていません。");
                return null;
            }

            _flameBottlePool = new ObjectPool<FlameBottleProjectileController>(prefabController, FLAME_BOTTLE_POOL_SIZE, _poolRoot);
        }

        return _flameBottlePool.Get();
    }

    DamageFieldController GetOrCreateFlameAreaController()
    {
        GameObject flameAreaPrefab = _bulletFactory != null && _bulletFactory.FlameAreaPrefab != null
            ? _bulletFactory.FlameAreaPrefab
            : _bulletFactory != null ? _bulletFactory.DamageFieldPrefab : null;
        if (flameAreaPrefab == null)
        {
            Debug.LogWarning("WeaponEffectExecutor: FlameAreaPrefabまたはDamageFieldPrefabが設定されていません。");
            return null;
        }

        if (_flameAreaPool == null)
        {
            DamageFieldController prefabController = flameAreaPrefab.GetComponent<DamageFieldController>();
            if (prefabController == null)
            {
                Debug.LogWarning("WeaponEffectExecutor: 炎エリアプレハブにDamageFieldControllerがアタッチされていません。");
                return null;
            }

            _flameAreaPool = new ObjectPool<DamageFieldController>(prefabController, FLAME_AREA_POOL_SIZE, _poolRoot);
        }

        return _flameAreaPool.Get();
    }

    void PrepareSpawnedObject(Transform spawnedTransform, Vector3 position)
    {
        if (spawnedTransform == null)
        {
            return;
        }

        spawnedTransform.position = position;
        spawnedTransform.rotation = Quaternion.identity;
    }

    int NormalizePhaseKey(float phaseOffsetDegrees)
    {
        float normalized = Mathf.Repeat(phaseOffsetDegrees, 360f);
        return Mathf.RoundToInt(normalized);
    }
}
