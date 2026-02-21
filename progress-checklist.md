# ゲーム画面UI実装 進捗チェックシート（クリア残り時間表示統合）

## 使い方
- 各項目の完了時に `- [x]` へ変更してください。
- 必要に応じて「メモ」に実施内容や懸念点を追記してください。

## 1. 事前準備
- [x] 要件書を確認した（`Documentation/game-screen-ui-requirements.md`）
- [x] 実装プランを作成した（`plan.md`）
- [ ] 影響ファイルの事前バックアップ方針を確認した
- メモ: 既存変更を巻き戻さない方針で対象ファイルのみ更新。

## 2. UXML更新（レイアウト統合）
- [x] `Assets/UI/PlayerStatus/PlayerStatusUI.uxml` に残り時間表示コンテナを追加した
- [x] `Assets/UI/PlayerStatus/PlayerStatusUI.uxml` に残り時間ラベル（例: `game-time-value`）を追加した
- [x] 既存プレイヤーステータス要素の `name` と構造を維持した
- メモ: `game-time-root` / `game-time-value` を追加し、既存ステータス要素の name は未変更。

## 3. USS更新（表示位置・見た目）
- [x] `Assets/UI/PlayerStatus/PlayerStatusUI.uss` に中央上部配置スタイルを追加した
- [x] 残り時間ラベルの視認性（サイズ・色・背景）を調整した
- [x] 既存プレイヤーステータスパネル配置を維持した
- [x] `pointer-events: none` の設計を維持した
- メモ: 時間表示は上中央の絶対配置、ステータスパネルは右上固定配置。

## 4. C#更新（UI制御）
- [x] `Assets/Scripts/UI/PlayerStatusUiController.cs` に残り時間ラベル参照を追加した
- [x] `Assets/Scripts/UI/PlayerStatusUiController.cs` で `GameManager.Instance` の参照解決を追加した
- [x] `Assets/Scripts/UI/PlayerStatusUiController.cs` に時間フォーマット（`MM:SS`）を実装した
- [x] `Assets/Scripts/UI/PlayerStatusUiController.cs` に未取得時表示（`--:--`）を実装した
- [ ] 既存ステータス更新処理（LV/HP/EXP/POW/DEF/SPD）への影響がないことを確認した
- メモ: `RefreshGameTimeUi` を追加し、既存ステータス更新ロジックは分離維持。

## 5. 更新最適化
- [x] 時間表示文字列の差分更新（変化時のみ反映）を実装した
- [ ] 毎フレーム不要なUI再描画が発生していないことを確認した
- メモ: `_cachedGameTimeText` を使い、文字列変化時のみ更新。

## 6. シーン接続確認
- [x] `UIDocument` が更新済み `PlayerStatusUI.uxml` を参照している
- [x] `UIDocument` が更新済み `PlayerStatusUI.uss` を参照している
- [ ] `Assets/UI/PanelSettings.asset` で表示スケーリングが実運用に問題ない
- メモ: `Assets/Main.unity` の `PlayerStatusUiController` と `UIDocument` のGUID参照で確認。

## 7. 手動テスト（要件受け入れ基準）
- [ ] プレイ中に画面中央上部へ残り時間が表示される
- [ ] 残り時間が進行に応じて更新される
- [ ] 既存プレイヤーステータスUIが従来どおり表示・更新される
- [ ] UI Toolkit（UXML/USS/C#）のみで要件を満たしている
- [ ] 複数解像度で重大なレイアウト崩れが発生しない
- メモ:

## 8. 回帰確認
- [ ] ダメージ時のHP表示更新が継続する
- [ ] 経験値獲得時のEXP表示更新が継続する
- [ ] レベルアップ時のステータス表示更新が継続する
- [ ] ゲーム操作（移動/攻撃/入力）へのUI干渉がない
- メモ:

## 9. 最終確認
- [x] `.kiro/steering/structure.md` のコーディング規約を満たしている
- [x] 不要な差分が含まれていない（対象ファイルのみ変更）
- [x] `plan.md` と進捗状態が一致している
- メモ: 手動確認項目（表示・回帰・解像度検証）は未完了。
