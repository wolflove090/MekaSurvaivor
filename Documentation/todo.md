# スタイル効果機能 ToDo

## Phase 1: 効果オブジェクト基盤作成
- [x] `Assets/Scripts/Character/Player/PlayerStyleType.cs` を新規作成する
- [x] `Assets/Scripts/Character/Player/StyleEffects/IPlayerStyleEffect.cs` を作成する
- [x] `Assets/Scripts/Character/Player/StyleEffects/PlayerStyleEffectContext.cs` を作成する
- [x] `Assets/Scripts/Character/Player/StyleEffects/PlayerStyleEffectFactory.cs` を作成する

## Phase 2: スタイル別効果クラス実装
- [x] `MikoStyleEffect` を実装し、30秒回復タイマーを組み込む
- [x] `IdolStyleEffect` を実装し、移動速度倍率1.2の適用/解除を実装する
- [x] `CelebStyleEffect` を実装し、経験値倍率2.0の適用/解除を実装する

## Phase 3: PlayerControllerへの統合
- [x] `PlayerController` に `_activeStyleEffect` と context を追加する
- [x] `ChangeStyle(PlayerStyleType styleType)` を実装する
- [x] `ChangeStyle` で `OnExit -> 生成差し替え -> OnEnter` を実行する
- [x] `Update` から `Tick` を委譲する
- [x] `SetMoveSpeedMultiplier(float)` を追加して `ApplyMoveSpeedFromStats` と連動させる

## Phase 4: PlayerExperience対応
- [x] `PlayerExperience` に経験値倍率フィールドを追加する
- [x] `SetExperienceMultiplier` / `ResetExperienceMultiplier` を追加する
- [x] `AddExperience` で倍率適用後の値を加算・通知する

## Phase 5: StyleChangeUiController接続
- [x] スタイル適用成功後に `PlayerController.ChangeStyle` を呼び出す
- [x] カード種別から `PlayerStyleType` へ変換する処理を追加する
- [x] プレイヤー参照未解決時の警告ログを追加する

## Phase 6: 動作確認
- [x] 巫女で30秒ごとにHPが1回復する
- [x] アイドルで移動速度が1.2倍になる
- [x] セレブで経験値獲得量が2倍になる
- [x] スタイル変更時に旧効果が解除される
- [x] 同一スタイル再選択で効果が再初期化される
