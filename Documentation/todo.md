# スタイル変更機能 ToDo

## Phase 1: ドメイン拡張
- [x] `CharacterStats` に `ApplyStatsData` と変更通知イベントを追加
- [x] `HealthComponent` に「消費HP維持」で現在HPを再計算するAPIを追加
- [x] `PlayerController` でステータス変更イベントを購読し、移動速度を再同期

## Phase 2: UIロジック変更
- [x] `StyleChangeUiController` を新規追加し、武器強化UIレイアウトを流用してスタイルカードを表示
- [x] `StyleChangeUiController` にレベル3ごとの表示制御と選択処理を実装
- [x] `WeaponUpgradeUiController` に「3の倍数レベルでは表示しない」分岐を追加
- [x] スタイル選択時に `CharacterStatsData` 適用 + HP再計算API呼び出しを接続

## Phase 3: 動作確認
- [ ] レベル3でスタイルUI、レベル2/4で武器強化UIが表示されることを確認
- [ ] 巫女/メイド/セレブの各選択でステータス反映を確認
- [ ] `5/10 -> 10/15` のHP再計算が成立することを確認
- [ ] UI表示中の停止と閉じた後の再開を確認
