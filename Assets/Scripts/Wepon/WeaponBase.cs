using UnityEngine;

/// <summary>
/// 武器のクールダウン制御を提供する基底クラス
/// </summary>
public abstract class WeaponBase
{
    protected Transform _transform;
    protected readonly WeaponState _weaponState;

    // デコレーションパターンで武器を増やす
    WeaponBase _rideWeapon;
    readonly WeaponService _weaponService;

    /// <summary>
    /// 現在の強化レベルを取得します
    /// </summary>
    public int UpgradeLevel => _weaponState.UpgradeLevel;

    /// <summary>
    /// 発動間隔（秒）を取得します
    /// </summary>
    protected abstract float CooldownDuration { get; }

    /// <summary>
    /// 武器を初期化します。
    /// </summary>
    /// <param name="transform">発動基準のTransform</param>
    /// <param name="rideWeapon">多重発動する下位武器</param>
    public WeaponBase(Transform transform, WeaponBase rideWeapon)
    {
        _transform = transform;
        _rideWeapon = rideWeapon;
        _weaponService = new WeaponService();
        _weaponState = new WeaponState(CooldownDuration);
    }

    /// <summary>
    /// 武器のクールダウンを進行し、必要なら発動します。
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public virtual void Tick(float deltaTime)
    {
        if(_rideWeapon != null)
        {
            // 別の武器が存在している場合はその処理も実行
            _rideWeapon.Tick(deltaTime);
        }

        if (!_weaponService.Tick(_weaponState, deltaTime, CooldownDuration))
        {
            return;
        }

        Fire();
    }

    /// <summary>
    /// 武器効果を発動します
    /// </summary>
    protected abstract void Fire();

    /// <summary>
    /// 武器を1段階強化します
    /// </summary>
    public abstract void LevelUp();

    /// <summary>
    /// クールダウン残り時間を現在の発動間隔以内に補正します
    /// </summary>
    protected void ClampCooldownTimerToDuration()
    {
        _weaponService.ClampCooldownToDuration(_weaponState, CooldownDuration);
    }
}
