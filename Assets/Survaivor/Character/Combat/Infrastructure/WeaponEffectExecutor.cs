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

    const int BULLET_POOL_SIZE = 20;
    const int THROWING_POOL_SIZE = 12;
    const int DAMAGE_FIELD_POOL_SIZE = 2;

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
        damageFieldController.SetSourcePow(request.SourcePow);
        damageFieldController.SetAreaScale(request.AreaScale);
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

    void PrepareSpawnedObject(Transform spawnedTransform, Vector3 position)
    {
        if (spawnedTransform == null)
        {
            return;
        }

        spawnedTransform.position = position;
        spawnedTransform.rotation = Quaternion.identity;
    }
}
