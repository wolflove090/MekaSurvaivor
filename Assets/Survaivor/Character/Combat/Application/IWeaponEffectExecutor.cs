/// <summary>
/// 武器発動要求の実行を担当するポートです。
/// </summary>
public interface IWeaponEffectExecutor
{
    /// <summary>
    /// 通常弾の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    void FireBullet(BulletFireRequest request);

    /// <summary>
    /// 投擲弾の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    void FireThrowing(ThrowingFireRequest request);

    /// <summary>
    /// ダメージフィールド生成要求を実行します。
    /// </summary>
    /// <param name="request">実行する生成要求</param>
    void SpawnDamageField(DamageFieldSpawnRequest request);

    /// <summary>
    /// ドローン展開要求を実行します。
    /// </summary>
    /// <param name="request">実行する展開要求</param>
    void DeployDrone(DroneSpawnRequest request);

    /// <summary>
    /// バウンドボールの発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    void FireBoundBall(BoundBallFireRequest request);

    /// <summary>
    /// 火炎瓶の発射要求を実行します。
    /// </summary>
    /// <param name="request">実行する発射要求</param>
    void FireFlameBottle(FlameBottleFireRequest request);

    /// <summary>
    /// 炎エリア生成要求を実行します。
    /// </summary>
    /// <param name="request">実行する生成要求</param>
    void SpawnFlameArea(FlameAreaSpawnRequest request);
}
