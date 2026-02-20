# ステータス実装 進捗チェックシート

## 使い方
- 各項目の完了時に `- [x]` へ変更してください。
- 必要に応じて「メモ」に実施内容や懸念点を追記してください。

## 1. ステータス定義データ追加
- [ ] `Assets/Scripts/Character/Stats/CharacterStatsData.cs` を作成した
- [ ] `maxHp / pow / def / spd` を定義した
- [ ] `CreateAssetMenu` を付与した
- [ ] 値の下限（バリデーション）を実装した
- メモ:

## 2. ランタイム用ステータス参照コンポーネント追加
- [ ] `Assets/Scripts/Character/Stats/CharacterStats.cs` を作成した
- [ ] `CharacterStatsData` の参照保持を実装した
- [ ] `MaxHp / Pow / Def / Spd` の取得APIを実装した
- [ ] null時フェイルセーフ値を実装した
- メモ:

## 3. HealthComponent 更新（DEF反映）
- [ ] `Assets/Scripts/Character/HealthComponent.cs` の `_maxHp` 直持ちを見直した
- [ ] `CharacterStats` から `MaxHp` を参照するようにした
- [ ] `TakeDamage` で `max(1, damage - def)` を適用した
- [ ] `OnDamaged` が最終ダメージ通知になるようにした
- [ ] 初期化/リセット時のHP反映を確認した
- メモ:

## 4. SPD適用（移動速度）
- [ ] `Assets/Scripts/Character/Player/PlayerController.cs` で `Spd` を適用した
- [ ] `Assets/Scripts/Character/Enemy/EnemyController.cs` で `Spd` を適用した
- [ ] 有効化時の再初期化フローを確認した
- メモ:

## 5. POW適用（与ダメ）
- [ ] `Assets/Scripts/Wepon/ProjectileController.cs` でプレイヤー `Pow` を反映した
- [ ] `Assets/Scripts/Wepon/DamageFieldController.cs` でプレイヤー `Pow` を反映した
- [ ] `Assets/Scripts/Character/Player/PlayerController.cs` の接触ダメージを敵 `Pow` 参照に変更した
- メモ:

## 6. ScriptableObjectアセット作成と接続
- [ ] Player用ステータスアセットを作成した（`Assets/GameResources/Player/`）
- [ ] Enemy用ステータスアセットを作成した（`Assets/GameResources/Enemy/`）
- [ ] プレイヤープレハブへ割り当てた
- [ ] エネミープレハブへ割り当てた
- メモ:

## 7. 既存挙動との整合確認
- [ ] Player死亡時にGameOverが発火する
- [ ] Enemy死亡時にReturnToPool/Destroyが維持される
- [ ] ノックバック挙動が維持される
- メモ:

## 8. テスト観点（手動）
- [ ] `POW <= DEF` でも1ダメージになる
- [ ] PlayerのSPD変更が移動速度に反映される
- [ ] EnemyのSPD変更が移動速度に反映される
- [ ] Player HP 0でGameOverになる
- [ ] Enemy HP 0で除去される
- [ ] 武器ダメージと接触ダメージの双方でPOW/DEF計算が有効
- メモ:

## 9. 最終確認
- [ ] `.kiro/steering/structure.md` のコーディング規約を満たす
- [ ] 変更差分を確認し、不要な変更がない
- [ ] 必要なら `plan.md` の進捗を同期更新した
- メモ:
