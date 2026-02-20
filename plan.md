# ステータス実装計画（Player / Enemy）

## 目的
- プレイヤーとエネミーに `HP / POW / DEF / SPD` のステータスを導入する。
- ステータス値は ScriptableObject で管理し、各キャラクターが参照して適用する。
- ダメージ計算は `受けるダメージ = max(1, 攻撃力 - 防御力)` を満たすように統一する。

## 仕様整理（実装に落とす内容）
- HP: 0 で死亡。プレイヤーはゲームオーバー、エネミーは破壊（またはプール返却）。
- POW: 通常攻撃（武器）と接触ダメージに適用。
- DEF: 受けたダメージを軽減。最終ダメージは 1 未満にならない。
- SPD: 移動速度として適用。

## 実装ステップ
1. ステータス定義データを追加
- `Assets/Scripts/Character/Stats/CharacterStatsData.cs` を新規作成。
- ScriptableObject に `maxHp`, `pow`, `def`, `spd` を定義（Inspectorで編集可能）。
- `CreateAssetMenu` を付与して Player/Enemy 用アセットを作成しやすくする。
- 必要に応じて値の下限を `OnValidate` で保証（例: HP/POW/DEF/SPD の最低値）。

2. ランタイム用ステータス参照コンポーネントを追加
- `Assets/Scripts/Character/Stats/CharacterStats.cs` を新規作成。
- 役割:
  - `CharacterStatsData` の参照保持。
  - 現在値取得 API（`MaxHp`, `Pow`, `Def`, `Spd`）提供。
  - 将来バフ対応を見据えた拡張点を用意（初期は基礎値のみ）。

3. HealthComponent を防御考慮の受ダメ構造へ更新
- 対象: `Assets/Scripts/Character/HealthComponent.cs`
- 変更方針:
  - 既存 `_maxHp` 直持ちを廃止し、`CharacterStats` から `MaxHp` を読む構造へ移行。
  - `TakeDamage(int damage, ...)` で `DEF` を反映して最終ダメージを計算。
  - 最終ダメージ `max(1, damage - def)` を適用。
  - `OnDamaged` には最終ダメージを通知（UI/ログ整合）。
  - 初期化・リセット時に `MaxHp` を再反映。

4. 移動速度（SPD）の適用
- 対象: `Assets/Scripts/Character/Player/PlayerController.cs`
- 対象: `Assets/Scripts/Character/Enemy/EnemyController.cs`
- 変更方針:
  - 起動時（`Awake`/`OnEnable`）に `CharacterStats.Spd` を移動コンポーネントへ適用。
  - Enemy は既存 `_moveSpeed` をステータスで上書きする流れへ統一。

5. 攻撃力（POW）の適用経路を追加
- プレイヤー -> エネミー:
  - 対象: `Assets/Scripts/Wepon/ProjectileController.cs`
  - 対象: `Assets/Scripts/Wepon/DamageFieldController.cs`
  - 変更方針: 武器固有ダメージ値にプレイヤー `POW` を反映して与ダメージ化。
- エネミー -> プレイヤー（接触）:
  - 対象: `Assets/Scripts/Character/Player/PlayerController.cs`
  - 変更方針: 固定値 `1` ダメージを廃止し、接触したエネミーの `POW` を参照して渡す。

6. ScriptableObject アセット作成と参照接続
- `Assets/GameResources/Player/` にプレイヤー用ステータスアセットを作成。
- `Assets/GameResources/Enemy/` にエネミー用ステータスアセットを作成。
- プレイヤープレハブ/エネミープレハブへ `CharacterStatsData` を割り当て。

7. 既存挙動との整合確認
- 死亡時処理:
  - プレイヤー: 既存ゲームオーバーイベントが継続発火すること。
  - エネミー: 既存のプール返却/Destroy 分岐が維持されること。
- ノックバック: 既存処理がダメージ適用後も維持されること。

8. テスト観点（手動確認）
- `POW <= DEF` でも 1 ダメージ入ること。
- Player/Enemy の SPD 変更で移動速度が反映されること。
- Player HP 0 で GameOver、Enemy HP 0 で除去されること。
- 武器ダメージと接触ダメージの双方で POW/DEF 計算が効くこと。

## 影響ファイル（予定）
- 新規:
  - `Assets/Scripts/Character/Stats/CharacterStatsData.cs`
  - `Assets/Scripts/Character/Stats/CharacterStats.cs`
- 更新:
  - `Assets/Scripts/Character/HealthComponent.cs`
  - `Assets/Scripts/Character/Player/PlayerController.cs`
  - `Assets/Scripts/Character/Enemy/EnemyController.cs`
  - `Assets/Scripts/Wepon/ProjectileController.cs`
  - `Assets/Scripts/Wepon/DamageFieldController.cs`
- アセット追加（予定）:
  - `Assets/GameResources/Player/*Stats*.asset`
  - `Assets/GameResources/Enemy/*Stats*.asset`

## 実装時の注意
- `.kiro/steering/structure.md` の規約を順守（暗黙 private、`_camelCase`、ドキュメントコメント）。
- 既存の public API 変更は最小限にして、他コンポーネントへの影響を局所化する。
- 初期値未設定時のフェイルセーフ（null 時のデフォルト値）を入れて実行時エラーを防ぐ。

## 進捗メモ（2026-02-20）
- 完了:
  - `CharacterStatsData` / `CharacterStats` を追加。
  - `HealthComponent` を `CharacterStats` 参照へ移行し、`DEF` を考慮した受ダメ計算へ更新。
  - `PlayerController` / `EnemyController` で `SPD` 反映を追加。
  - `ProjectileController` / `DamageFieldController` でプレイヤー `POW` 反映を追加。
  - `Assets/GameResources/Player/PlayerStatsData.asset` と `Assets/GameResources/Enemy/EnemyStatsData.asset` を追加。
- 未完了:
  - Player/Enemy プレハブ（またはシーン上の実体）への `CharacterStatsData` 割り当て。
  - 手動プレイ確認（ダメージ最小値、SPD反映、死亡時挙動確認）。
