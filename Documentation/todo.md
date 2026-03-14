# カウガール攻撃間隔短縮 ToDo

## Phase 1: プレイヤー状態とスタイル効果を拡張する
- [x] `Assets/Survaivor/Character/Player/Domain/PlayerState.cs` に `AttackIntervalMultiplier` と set/reset API を追加する。
- [x] `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs` の `ResetStyleParameters()` に攻撃間隔倍率 reset を追加する。
- [x] `Assets/Survaivor/Character/Player/Application/StyleEffects/CowgirlStyleEffect.cs` を空実装から `0.75f` 設定実装へ変更する。
- [x] `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs` にカウガール倍率の設定・解除テストを追加する。

## Phase 2: 武器へ倍率参照を配線する
- [x] `Assets/Survaivor/Character/Combat/Application/WeaponBase.cs` に攻撃間隔倍率 provider と共通適用ヘルパーを追加する。
- [x] `Assets/Survaivor/Character/Combat/Application/WeaponService.cs` の builder に攻撃間隔倍率 provider を通す。
- [x] `Assets/Survaivor/Character/Combat/Application/BulletWeapon.cs` の `CooldownDuration` を倍率込みへ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/ThrowingWeapon.cs` の `CooldownDuration` を倍率込みへ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/DamageFieldWeapon.cs` の `CooldownDuration` を倍率込みへ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs` の `CooldownDuration` を倍率込みへ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs` の `CooldownDuration` を倍率込みへ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs` の `ShotInterval` にのみ倍率を反映し、`_deployInterval` は維持する。

## Phase 3: 既存武器への即時反映を成立させる
- [x] `Assets/Survaivor/Character/Combat/Application/WeaponBase.cs` に武器チェーン全体のクールダウン再 clamp API を追加する。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` の `WeaponService` 初期化で `PlayerState.AttackIntervalMultiplier` provider を渡す。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` の `ChangeStyle()` 後に既存武器チェーンへ再 clamp を掛ける。
- [x] カウガール切替前から所持している武器が、切替直後から短縮間隔に入ることを確認する。

## Phase 4: テストと動作確認を整備する
- [x] `Assets/Survaivor/Tests/EditMode/Combat/WeaponLevelTuningTests.cs` に攻撃間隔倍率反映テストを追加する。
- [x] `Assets/Survaivor/Tests/EditMode/Combat/WeaponServiceTests.cs` に provider 配線テストを追加する。
- [x] `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs` にスタイル切替後の即時反映テストを追加する。
- [x] `DroneWeapon` だけが `DroneSpawnRequest.ShotInterval` のみ短縮されることをテストで固定化する。
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] `u console get -l E` で関連エラーが出ていないことを確認する。
