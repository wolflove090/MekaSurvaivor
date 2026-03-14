# メイドスタイル移動速度強化 ToDo

## Phase 1: メイド効果を実装する
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/MaidStyleEffect.cs` に移動速度倍率定数 `1.5f` を追加する。
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/MaidStyleEffect.cs` の `ApplyParameters()` で `PlayerState.SetMoveSpeedMultiplier(1.5f)` を呼ぶ。
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/MaidStyleEffect.cs` のドキュメントコメントを実装内容に合わせて更新する。
- [ ] `MaidStyleEffect` に `Tick()` ベースの継続処理を追加せず、パラメータ適用だけで完結していることを確認する。

## Phase 2: スタイル切り替えの期待挙動をテストへ反映する
- [ ] `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs` の既存メイド切り替えテスト期待値を `MoveSpeedMultiplier == 1.5f` に更新する。
- [ ] `Celeb -> Maid` 切り替え後に `ExperienceMultiplier == 1f` かつ `MoveSpeedMultiplier == 1.5f` になるテストを維持または明示する。
- [ ] `Maid -> Idol` 切り替え後に `MoveSpeedMultiplier == 1.2f` となり、メイド倍率が残らないことを検証するテストを追加する。
- [ ] `Maid -> Celeb` 切り替え後に `MoveSpeedMultiplier == 1f` かつ `ExperienceMultiplier == 2f` になることを検証するテストを追加する。
- [ ] `Maid -> Maid` または連続切り替え時に `MoveSpeedMultiplier` が `1.5f` を超えて累積しないことを検証するテストを追加する。

## Phase 3: 既存責務との整合を確認する
- [ ] `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs` の `ResetStyleParameters() -> ApplyParameters()` 順序を崩していないことを確認する。
- [ ] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` の `ApplyMoveSpeedFromStats()` を追加変更せず再利用できることを確認する。
- [ ] `Assets/Survaivor/Character/Player/Domain/PlayerState.cs` の `MoveSpeedMultiplier` 初期値とリセット値が `1f` のままで問題ないことを確認する。
- [ ] UI 文言、演出、SE、VFX が今回の変更対象に含まれていないことを確認する。

## Phase 4: 動作確認を行う
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] 必要に応じて Unity 上でメイド選択直後の移動速度上昇を確認する。
- [ ] メイドから別スタイルへ変更した際に移動速度倍率が正しく切り替わることを確認する。
- [ ] `u console get -l E` で関連エラーが出ていないことを確認する。
