# ナース回復アイテム強化 ToDo

## Phase 0: 実装前確認
- [ ] `Documentation/要件書/nurse-style-heal-item-requirements.md` の端数処理方針を確定する。
- [ ] `Mathf.RoundToInt` を採用するか、切り捨てへ変更するかを要件書へ反映する。
- [ ] 要件書・計画書・ToDo の用語が一致していることを確認する。

## Phase 1: プレイヤー状態に回復アイテム倍率を追加する
- [ ] `Assets/Survaivor/Character/Player/Domain/PlayerState.cs` に回復アイテム倍率プロパティを追加する。
- [ ] `Assets/Survaivor/Character/Player/Domain/PlayerState.cs` に倍率設定メソッドを追加する。
- [ ] `Assets/Survaivor/Character/Player/Domain/PlayerState.cs` に倍率初期化メソッドを追加する。
- [ ] 回復アイテム倍率の初期値が `1.0f` になるようにコンストラクタを更新する。

## Phase 2: スタイル切替時の付与と解除を実装する
- [ ] `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs` の `ResetStyleParameters()` に回復アイテム倍率の初期化を追加する。
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/NurseStyleEffect.cs` の空実装を置き換える。
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/NurseStyleEffect.cs` の `ApplyParameters()` で回復アイテム倍率 `1.5f` を設定する。
- [ ] `Assets/Survaivor/Character/Player/Application/StyleEffects/NurseStyleEffect.cs` が `context` 未解決時でも安全に動作することを確認する。
- [ ] 同一スタイル再適用時に倍率が重複加算されないことを確認する。

## Phase 3: 回復アイテム取得処理へ倍率を適用する
- [ ] `Assets/Survaivor/Items/Infrastructure/HealPickup/HealPickup.cs` でプレイヤーの進行状態へアクセスする参照を取得する。
- [ ] `Assets/Survaivor/Items/Infrastructure/HealPickup/HealPickup.cs` の `CollectPickup()` で取得時点の倍率を参照する。
- [ ] `Assets/Survaivor/Items/Infrastructure/HealPickup/HealPickup.cs` で基礎回復量 `3` と倍率から最終回復量を算出する。
- [ ] 最終回復量の算出に要件で確定した端数処理を適用する。
- [ ] プレイヤー参照が不足している場合は倍率 `1.0f` にフォールバックする。
- [ ] `HealPickup` にスタイル種別の直接判定を追加していないことを確認する。

## Phase 4: 回帰テストを追加・更新する
- [ ] `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs` にナース変更時の倍率 `1.5f` 検証を追加する。
- [ ] `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs` にナース解除後の倍率 `1.0f` 検証を追加する。
- [ ] `Assets/Survaivor/Tests/EditMode/...` 配下に `HealPickup` 回復量検証テストを追加または更新する。
- [ ] 通常時の回復量が基礎値 `3` であることを検証する。
- [ ] ナース時の回復量が要件どおり `5` になることを検証する。
- [ ] 最大HPを超えて回復しないことを検証する。
- [ ] ナース選択前に生成された回復アイテムでも、取得時点の倍率が使われることを検証する。
- [ ] 巫女の定期回復がナース倍率の影響を受けないことを回帰テストで確認する。

## Phase 5: 動作確認
- [ ] `u tests run edit` で関連 EditMode テストを実行する。
- [ ] ナース選択中に回復アイテム取得で `5` 回復することを手動確認する。
- [ ] ナースから別スタイルへ変更後に回復量が `3` へ戻ることを手動確認する。
- [ ] 既存配置済み回復アイテムでも、取得時点のスタイルに応じた回復量になることを手動確認する。
- [ ] `u console get -l E` で関連エラーが出ていないことを確認する。

## 完了条件
- [ ] ナーススタイルの固有効果として回復アイテム倍率 `1.5f` が実装されている。
- [ ] 回復アイテム以外の回復経路へ倍率が波及していない。
- [ ] スタイル切替時の倍率解除が機能している。
- [ ] テストと手動確認結果が要件書の受け入れ条件と一致している。
