# スタイル効果機能 ToDo

## Phase 1: 効果オブジェクト基盤作成
- [ ] `Assets/Scripts/Character/Player/PlayerStyleType.cs` を新規作成する
- [ ] `Assets/Scripts/Character/Player/StyleEffects/IPlayerStyleEffect.cs` を作成する
- [ ] `Assets/Scripts/Character/Player/StyleEffects/PlayerStyleEffectContext.cs` を作成する
- [ ] `Assets/Scripts/Character/Player/StyleEffects/PlayerStyleEffectFactory.cs` を作成する

## Phase 2: スタイル別効果クラス実装
- [ ] `MikoStyleEffect` を実装し、30秒回復タイマーを組み込む
- [ ] `IdolStyleEffect` を実装し、移動速度倍率1.2の適用/解除を実装する
- [ ] `CelebStyleEffect` を実装し、経験値倍率2.0の適用/解除を実装する

## Phase 3: PlayerControllerへの統合
- [ ] `PlayerController` に `_activeStyleEffect` と context を追加する
- [ ] `ChangeStyle(PlayerStyleType styleType)` を実装する
- [ ] `ChangeStyle` で `OnExit -> 生成差し替え -> OnEnter` を実行する
- [ ] `Update` から `Tick` を委譲する
- [ ] `SetMoveSpeedMultiplier(float)` を追加して `ApplyMoveSpeedFromStats` と連動させる

## Phase 4: PlayerExperience対応
- [ ] `PlayerExperience` に経験値倍率フィールドを追加する
- [ ] `SetExperienceMultiplier` / `ResetExperienceMultiplier` を追加する
- [ ] `AddExperience` で倍率適用後の値を加算・通知する

## Phase 5: StyleChangeUiController接続
- [ ] スタイル適用成功後に `PlayerController.ChangeStyle` を呼び出す
- [ ] カード種別から `PlayerStyleType` へ変換する処理を追加する
- [ ] プレイヤー参照未解決時の警告ログを追加する

## Phase 6: 動作確認
- [ ] 巫女で30秒ごとにHPが1回復する
- [ ] アイドルで移動速度が1.2倍になる
- [ ] セレブで経験値獲得量が2倍になる
- [ ] スタイル変更時に旧効果が解除される
- [ ] 同一スタイル再選択で効果が再初期化される
