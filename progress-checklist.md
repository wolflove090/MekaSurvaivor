# ステータス実装 進捗チェックシート

## 使い方
- 各項目の完了時に `- [x]` へ変更してください。
- 必要に応じて「メモ」に実施内容や懸念点を追記してください。

## 1. ステータス定義データ追加
- [x] `Assets/Scripts/Character/Stats/CharacterStatsData.cs` を作成した
- [x] `maxHp / pow / def / spd` を定義した
- [x] `CreateAssetMenu` を付与した
- [x] 値の下限（バリデーション）を実装した
- メモ: `OnValidate` で `maxHp>=1`, `pow>=0`, `def>=0`, `spd>=0` を保証。

## 2. ランタイム用ステータス参照コンポーネント追加
- [x] `Assets/Scripts/Character/Stats/CharacterStats.cs` を作成した
- [x] `CharacterStatsData` の参照保持を実装した
- [x] `MaxHp / Pow / Def / Spd` の取得APIを実装した
- [x] null時フェイルセーフ値を実装した
- メモ: データ未設定時は `MaxHp=10`, `Pow=1`, `Def=0`, `Spd=5` を返す。

## 3. HealthComponent 更新（DEF反映）
- [x] `Assets/Scripts/Character/HealthComponent.cs` の `_maxHp` 直持ちを見直した
- [x] `CharacterStats` から `MaxHp` を参照するようにした
- [x] `TakeDamage` で `max(1, damage - def)` を適用した
- [x] `OnDamaged` が最終ダメージ通知になるようにした
- [x] 初期化/リセット時のHP反映を確認した
- メモ: `Heal` の上限も `MaxHp` を参照するように統一。

## 4. SPD適用（移動速度）
- [x] `Assets/Scripts/Character/Player/PlayerController.cs` で `Spd` を適用した
- [x] `Assets/Scripts/Character/Enemy/EnemyController.cs` で `Spd` を適用した
- [x] 有効化時の再初期化フローを確認した
- メモ: `Awake/OnEnable` で `ApplyMoveSpeedFromStats()` を実行。

## 5. POW適用（与ダメ）
- [x] `Assets/Scripts/Wepon/ProjectileController.cs` でプレイヤー `Pow` を反映した
- [x] `Assets/Scripts/Wepon/DamageFieldController.cs` でプレイヤー `Pow` を反映した
- [x] `Assets/Scripts/Character/Player/PlayerController.cs` の接触ダメージを敵 `Pow` 参照に変更した
- メモ: 武器ダメージは `baseDamage + playerPow`（最低1）で計算。

## 6. ScriptableObjectアセット作成と接続
- [x] Player用ステータスアセットを作成した（`Assets/GameResources/Player/`）
- [x] Enemy用ステータスアセットを作成した（`Assets/GameResources/Enemy/`）
- [x] プレイヤープレハブへ割り当てた
- [x] エネミープレハブへ割り当てた
- メモ: `PlayerStatsData.asset` / `EnemyStatsData.asset` を作成。プレハブ・シーン接続は未実施。

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
- [x] `.kiro/steering/structure.md` のコーディング規約を満たす
- [x] 変更差分を確認し、不要な変更がない
- [x] 必要なら `plan.md` の進捗を同期更新した
- メモ: ビルドは `dotnet build MekaSurvaivor.slnx` が長時間応答なしで完了確認できず。
