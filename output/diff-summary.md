# origin/main 差分要約

## 対象ブランチと比較対象
- 対象ブランチ: `feature/style-effect`
- 比較対象: `origin/main`
- 比較コマンド: `git diff origin/main...HEAD`

## 変更サマリ
- 変更ファイル数: 24
- 追加/削除行数: `+596 / -151`
- 主要カテゴリ:
  - 機能追加: プレイヤースタイル効果システム（Miko/Idol/Celeb）
  - 既存改修: `PlayerController` / `PlayerExperience` / `StyleChangeUiController`
  - UI調整: 武器アップグレードUIレイアウト調整（USS）
  - シーン設定: `Main.unity` の経験値スケール値変更
  - ドキュメント: 要件書追加、運用スキル文言更新、`plan.md`/`todo.md` 更新

## 対応内容の詳細

### 1) プレイヤースタイル効果の新規導入
- 目的:
  - スタイル選択に応じた固有効果をコード上で分離し、切替可能にする。
- 変更点:
  - `PlayerStyleType` enum を新規追加。
  - `IPlayerStyleEffect` と `PlayerStyleEffectContext` を新規追加。
  - `PlayerStyleEffectFactory` を新規追加し、スタイル種別から効果実装を生成。
  - `MikoStyleEffect`（30秒ごとに回復）、`IdolStyleEffect`（移動速度倍率）、`CelebStyleEffect`（経験値倍率）を追加。
- 影響範囲:
  - プレイヤーの戦闘中パラメータ（HP回復、移動速度、経験値獲得）
  - スタイル効果の実装拡張点
- 確認事項/懸念:
  - 要件書には「同一スタイル再適用で効果インスタンス1つ維持」とあるが、現実装は `ChangeStyle` 毎に新規生成するため要確認。

### 2) PlayerController への効果ライフサイクル統合
- 目的:
  - スタイル効果の適用・更新・リセットをプレイヤー制御に統合する。
- 変更点:
  - `PlayerExperience` 参照、`IPlayerStyleEffect`、`Context`、`Factory` を保持。
  - `Update()` で `Tick()` 実行。
  - `ChangeStyle(PlayerStyleType)` を追加し、切替時にリセット後新効果を適用。
  - `SetMoveSpeedMultiplier()` と `_moveSpeedMultiplier` を追加。
  - `OnDestroy()` でリセット実行。
- 影響範囲:
  - プレイヤーの移動速度算出式（`CharacterStats.Spd * multiplier`）
  - スタイル切替時の状態初期化
- 確認事項/懸念:
  - `_playerExperience == null` の場合、`BuildStyleEffectContext()` が `null` を返し、経験値倍率だけでなく全スタイル変更が不能になる実装。警告文の意図と挙動が不一致のため要確認。

### 3) UIからのスタイル適用接続
- 目的:
  - スタイルカード選択時にプレイヤー効果を確定適用する。
- 変更点:
  - `StyleChangeUiController` に `PlayerController` 参照保持を追加。
  - カード選択時に `TryChangePlayerStyle()` を実行。
  - `cardIndex -> PlayerStyleType` 変換ロジックを追加。
- 影響範囲:
  - スタイル選択UIの確定イベント
- 確認事項/懸念:
  - `StyleCardType` と `PlayerStyleType` の列挙順依存が継続するため、将来の列挙追加時はズレに注意が必要。

### 4) 関連調整（UI/シーン/ドキュメント）
- 目的:
  - 見た目調整および仕様明文化。
- 変更点:
  - `WeaponUpgradeUI.uss` のレスポンシブ調整。
  - `Main.unity` の `_experienceScaling` が `1.5 -> 1`。
  - `Documentation/要件書/style-effect-requirements.md` を追加。
  - `.codex/skills/diff-understanding-share/SKILL.md` に説明ルール追加。
- 影響範囲:
  - UI表示密度とカード幅挙動
  - 経験値進行バランス
- 確認事項/懸念:
  - `_experienceScaling` 変更はゲームテンポへ影響するため、意図変更か要確認。

## ユーザー向け段階共有の塊定義
1. 塊1（コア機能）
- 対象: `PlayerController.cs`, `PlayerExperience.cs`, `StyleChangeUiController.cs`
- 伝達ポイント: 効果適用フロー、更新ループ、UI接続、主要懸念

2. 塊2（新規効果実装群）
- 対象: `PlayerStyleType.cs`, `StyleEffects/*.cs`
- 伝達ポイント: 設計分離、各スタイル効果、将来拡張性

3. 塊3（周辺変更）
- 対象: `WeaponUpgradeUI.uss`, `Main.unity`, `Documentation/要件書/style-effect-requirements.md`, `.codex/skills/diff-understanding-share/SKILL.md`
- 伝達ポイント: UI/設定/文書の補助変更と影響
