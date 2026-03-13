using System;
using UnityEngine;

/// <summary>
/// スタイル効果で利用するプレイヤー参照を集約します
/// </summary>
public class PlayerStyleEffectContext
{
    /// <summary>
    /// HP管理コンポーネントを取得します
    /// </summary>
    public HealthComponent HealthComponent { get; }

    /// <summary>
    /// プレイヤー進行状態を取得します
    /// </summary>
    public PlayerState PlayerState { get; }

    /// <summary>
    /// プレイヤーのTransformを取得します
    /// </summary>
    public Transform PlayerTransform { get; }

    /// <summary>
    /// 敵レジストリを取得します
    /// </summary>
    public EnemyRegistry EnemyRegistry { get; }

    /// <summary>
    /// 海賊戦闘員管理コンポーネントを取得します
    /// </summary>
    public IPirateCrewSummonController PirateCrewSummonController { get; }

    readonly Func<Vector3> _playerFacingDirectionProvider;

    /// <summary>
    /// 現在のプレイヤーの向きを取得します
    /// </summary>
    public Vector3 PlayerFacingDirection =>
        _playerFacingDirectionProvider != null ? _playerFacingDirectionProvider.Invoke() : Vector3.right;

    /// <summary>
    /// コンテキストを初期化します
    /// </summary>
    /// <param name="healthComponent">HP管理コンポーネント</param>
    /// <param name="playerState">プレイヤー進行状態</param>
    /// <param name="playerTransform">プレイヤーTransform</param>
    /// <param name="playerFacingDirectionProvider">プレイヤー向き取得関数</param>
    /// <param name="enemyRegistry">敵レジストリ</param>
    /// <param name="pirateCrewSummonController">海賊戦闘員管理コンポーネント</param>
    public PlayerStyleEffectContext(
        HealthComponent healthComponent,
        PlayerState playerState,
        Transform playerTransform = null,
        Func<Vector3> playerFacingDirectionProvider = null,
        EnemyRegistry enemyRegistry = null,
        IPirateCrewSummonController pirateCrewSummonController = null)
    {
        HealthComponent = healthComponent;
        PlayerState = playerState;
        PlayerTransform = playerTransform;
        _playerFacingDirectionProvider = playerFacingDirectionProvider;
        EnemyRegistry = enemyRegistry;
        PirateCrewSummonController = pirateCrewSummonController;
    }
}
