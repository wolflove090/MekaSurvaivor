# スタイル変更 + 画像切り替え機能 ToDo

## Phase 1: スタイル定義の整合
- [x] `Assets/Scripts/UI/StyleChangeUiController.cs` のスタイル定義を「巫女 / アイドル / セレブ」に更新
- [x] `StyleCardType.Maid` を `StyleCardType.Idol` に変更し、関連 `switch` と表示文言を修正
- [x] スタイルステータス参照フィールド名を `_idolStyleStats` に統一
- [x] カード説明テキストの命名整合（メイド表記の残存除去）

## Phase 2: 画像適用ロジック追加
- [x] `StyleChangeUiController` に右向き画像3種（巫女/アイドル/セレブ）の `SerializeField` を追加
- [x] プレイヤー `SpriteDirectionController` 参照解決処理を追加
- [x] スタイル適用処理で `CharacterStatsData` と右向き画像を同時解決する処理を実装
- [x] 画像未設定時は警告ログのみ出して既存画像を維持するフォールバックを実装

## Phase 3: 向き制御の flipX 化
- [x] `Assets/Scripts/Character/SpriteDirectionController.cs` を右向き基準 + `flipX` 制御へ変更
- [x] `UpdateDirection(float)` を「画像差し替えなし」の実装へ変更
- [x] 右向き画像適用と向きリセット（`flipX=false`）を行う公開メソッドを追加
- [x] `GetFacingDirection()` の既存戻り値仕様（左/右）を維持

## Phase 4: シーン参照更新
- [x] `Assets/Main.unity` の `StyleChangeUiController` 参照キーを更新（`_maidStyleStats` -> `_idolStyleStats`）
- [x] `Assets/Main.unity` に右向き画像3種（`miko.png`, `idol.png`, `celebrity.png`）を割り当て
- [x] 必要に応じて `SpriteDirectionController` の旧フィールド参照を削除/再設定

## Phase 5: 動作確認
- [ ] レベル3でスタイルUIが開かず、レベル4で開くことを確認
- [ ] UIカード文言が「巫女 / アイドル / セレブ」であることを確認
- [ ] 各カード選択でステータス適用と画像切替が同時に実行されることを確認
- [ ] スタイル変更直後が右向き表示（`flipX=false`）であることを確認
- [ ] 左右移動時に `flipX` のみが変化し、画像再差し替えがないことを確認
- [ ] 画像未設定時に例外なく継続し、警告ログのみ出ることを確認
- [ ] HP消費量維持ロジック（`5/10 -> 10/15`）が維持されることを確認
