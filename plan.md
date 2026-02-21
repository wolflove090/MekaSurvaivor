# ゲーム画面UI実装プラン（クリア残り時間表示統合）

## 1. 目的
- 画面中央上部にゲームクリアまでの残り時間を表示する。
- 既存プレイヤーステータスUIと統合し、単一のゲーム画面UIとして運用する。

## 2. 前提と実装方針
- 残り時間のデータソースは `GameManager.RemainingTime` を利用する。
- UIは既存 `PlayerStatusUiController` を拡張し、別UIドキュメントへ分離しない。
- レイアウト定義は `PlayerStatusUI.uxml`、スタイル定義は `PlayerStatusUI.uss` を更新する。
- 値更新は既存ステータス同様、差分更新（表示値が変わった時のみラベル更新）で実装する。

## 3. 実装ステップ
1. UIレイアウト拡張（UXML）
- 対象: `Assets/UI/PlayerStatus/PlayerStatusUI.uxml`
- 追加内容:
  - ゲーム画面全体を内包できるルートレイアウトを維持。
  - 画面中央上部用のコンテナ（例: `game-time-root`）を追加。
  - 残り時間表示ラベル（例: `game-time-value`）を追加。
- 注意点:
  - 既存プレイヤーステータス領域の構造と `name` は変更しない。

2. UIスタイル追加（USS）
- 対象: `Assets/UI/PlayerStatus/PlayerStatusUI.uss`
- 追加内容:
  - `game-time-root` を「上寄せ + 水平中央」に配置するスタイル。
  - `game-time-value` の視認性を担保するスタイル（文字サイズ・色・背景・余白）。
  - プレイヤーステータスパネルの既存配置（右上寄り）を維持。
- 注意点:
  - `pointer-events: none` を維持してゲーム操作を阻害しない。
  - 複数解像度で崩れにくい相対レイアウトを使う。

3. コントローラー拡張（C#）
- 対象: `Assets/Scripts/UI/PlayerStatusUiController.cs`
- 追加内容:
  - ラベル参照 `Label _gameTimeValueLabel` を追加。
  - `GameManager` 参照キャッシュ（例: `GameManager _gameManager`）を追加。
  - `CacheElements()` で `game-time-value` を取得。
  - `ResolvePlayerReferences()` と同等の参照解決で `GameManager.Instance` を取得。
  - `RefreshStatusUi(bool force)` に残り時間更新処理を統合。
  - 表示フォーマット関数（`MM:SS`）を追加。
  - 未取得時の初期表示 `--:--` を反映。
- 注意点:
  - `.kiro/steering/structure.md` に従い、公開メンバーとメソッドへドキュメントコメントを維持。
  - 既存イベント購読/解除ロジックは壊さない。

4. 時間表示仕様の実装
- フォーマット:
  - `remainingTime >= 0` のとき `MM:SS` 表示。
  - 0秒未満は `00:00` 扱い（`Mathf.Max` で下限固定）。
- 更新最適化:
  - 秒単位文字列をキャッシュし、文字列が変化した時のみラベル更新。

5. シーン適用確認
- 対象: ゲーム画面で使用中の `UIDocument` 設定
- 確認内容:
  - `PlayerStatusUiController` が参照する UXML/USS が更新版を向いていること。
  - 既存 `PanelSettings`（`Assets/UI/PanelSettings.asset`）で意図通り表示されること。

## 4. テスト計画（手動）
1. 表示確認
- プレイ開始直後に画面中央上部へ残り時間が表示される。
- プレイヤーステータスUI（LV/HP/EXP/POW/DEF/SPD）が従来どおり表示される。

2. 更新確認
- 残り時間が進行に合わせて 1 秒単位で減少表示される。
- `GameManager` が未解決の状況で `--:--` が表示され、例外が発生しない。

3. レイアウト確認
- 16:9 と縦横比が異なる解像度で時間表示が中央上部を維持する。
- 他UI（ステータス/アップグレードUI）との重なりが許容範囲である。

4. 回帰確認
- ダメージ、経験値、レベルアップ時の既存ステータス更新が継続する。
- クリック/キー入力などゲーム操作にUIが干渉しない。

## 5. 影響ファイル（予定）
- 更新:
  - `Assets/UI/PlayerStatus/PlayerStatusUI.uxml`
  - `Assets/UI/PlayerStatus/PlayerStatusUI.uss`
  - `Assets/Scripts/UI/PlayerStatusUiController.cs`

## 6. 完了条件
- 要件書 `Documentation/game-screen-ui-requirements.md` の受け入れ基準 1-5 を満たす。
- エラー/警告なくプレイ可能で、既存プレイヤーステータス表示に回帰がない。
