# 既存スクリプト責務マッピング表

## 目的
- 既存 `MonoBehaviour` と旧実装クラスが、どの責務を持ち、どのレイヤーへ移行したかを一覧化する。
- 今後「どこまで移行済みか」「どの責務が旧実装に残っているか」をコードレビュー時に判断しやすくする。

## レイヤー定義
- `Domain`: 純C#の状態モデル、値オブジェクト、ルールの保持先
- `Application`: ユースケース進行、状態遷移、通知の仲介先
- `Infrastructure`: Unity参照解決、Prefab生成、シーン配線
- `Presentation`: UI表示、入力通知、見た目反映
- `Legacy/Bridge`: 旧クラスに残している同期・委譲用の薄いアダプター

## 主要スクリプト

| 既存/現行スクリプト | 現在の主責務 | 主な移行先レイヤー | 現状 |
| --- | --- | --- | --- |
| `Assets/Scripts/Common/GameManager.cs` | ゲーム進行のUnityアダプター、`GameSessionService` 呼び出し、終了時の通知発火 | `Application`, `Domain`, `Legacy/Bridge` | 制限時間と終了状態の実体は `GameSessionState` / `GameSessionService` に移行済み。`MonoBehaviour` 側は更新入力と通知窓口のみ。 |
| `Assets/Scripts/Character/Player/PlayerController.cs` | 入力連携、移動速度反映、武器強化委譲、ゲームオーバー時の見た目停止 | `Presentation`, `Application`, `Legacy/Bridge` | 進行ロジックの多くは外出し済み。まだプレイヤー統合アダプターとして複数参照を束ねている。 |
| `Assets/Scripts/Character/Player/PlayerExperience.cs` | `PlayerProgressionService` への委譲、進行状態の同期公開、通知中継 | `Application`, `Domain`, `Legacy/Bridge` | 経験値計算本体は移行済み。旧API互換のためブリッジとして残している。 |
| `Assets/Scripts/Character/Stats/CharacterStats.cs` | `CharacterStatsData` の読み出しとランタイム参照値の公開 | `Domain`, `Legacy/Bridge` | 実計算は `CharacterStatValues` へ集約。参照用コンポーネントへ縮小済み。 |
| `Assets/Scripts/Character/HealthComponent.cs` | HP保持、被ダメージ/回復、死亡通知 | `Presentation`, `Legacy/Bridge` | Unityイベントや物理反応に近いアダプター。純C#化は未着手。 |
| `Assets/Scripts/Character/Player/PlayerMovement.cs` | 移動入力の反映、向き・速度のTransform適用 | `Presentation` | Unityの移動反映担当として残す想定。 |
| `Assets/Scripts/Character/Player/PlayerInput.cs` | Input System の入力取得 | `Presentation` | Unity入力の受信専用。移行対象外。 |
| `Assets/Scripts/Character/Enemy/EnemySpawner.cs` | `EnemySpawnService` 呼び出し、Prefab生成、ターゲット/レジストリ接続 | `Application`, `Infrastructure`, `Legacy/Bridge` | スポーン判定は移行済み。Unity生成アダプターとして残る。 |
| `Assets/Scripts/Character/Enemy/EnemyController.cs` | 敵の移動実行、死亡時の返却/破棄、ダメージ受付 | `Presentation`, `Infrastructure` | 追跡・見た目・プール返却のUnity寄り責務が中心。 |
| `Assets/Scripts/Infrastructure/Unity/EnemyRegistry.cs` | 敵の登録管理、近傍探索、空間グリッド更新 | `Infrastructure` | Unityオブジェクト検索のためInfrastructureに残す。 |
| `Assets/Scripts/Wepon/WeaponBase.cs` | クールダウン進行の抽象境界、発動チェーン制御 | `Application`, `Legacy/Bridge` | クールダウン計算は `WeaponService` に移行済み。発動抽象と装備チェーンだけ残す。 |
| `Assets/Scripts/Wepon/BulletWeapon.cs` | 対象探索、弾生成、攻撃力注入 | `Infrastructure`, `Legacy/Bridge` | 発射タイミングは `WeaponService` 管理。Unity生成と探索の橋渡しが主責務。 |
| `Assets/Scripts/Wepon/ThrowingWeapon.cs` | 投擲弾生成、向き反映 | `Infrastructure`, `Legacy/Bridge` | 発射間隔管理は移行済み。 |
| `Assets/Scripts/Wepon/DamageFieldWeapon.cs` | ダメージフィールド生成、追従設定、攻撃力注入 | `Infrastructure`, `Legacy/Bridge` | 発射タイミング管理は移行済み。 |
| `Assets/Scripts/Wepon/ProjectileController.cs` | 弾の移動、衝突、寿命管理 | `Presentation` | Unity物理・移動反映担当として残す。 |
| `Assets/Scripts/Wepon/DamageFieldController.cs` | 範囲追従、継続ダメージ、寿命管理 | `Presentation` | Unity物理・Transform反映担当として残す。 |
| `Assets/Scripts/UI/GameScreenUiController.cs` | HUDのUXML/USS構築、ラベル参照保持 | `Presentation` | 表示ロジックは `GameScreenPresenter` へ移行済み。 |
| `Assets/Scripts/Presentation/UI/GameScreenPresenter.cs` | HUD表示モデル作成、通知購読、差分更新 | `Presentation` | Presenter化済み。UIの状態収集主体。 |
| `Assets/Scripts/UI/WeaponUpgradeUiController.cs` | 武器強化UI表示、カード入力通知 | `Presentation` | プレイヤー直参照は除去済み。 |
| `Assets/Scripts/Presentation/UI/WeaponUpgradePresenter.cs` | 武器強化UI入力を `PlayerController` / メッセージバスへ橋渡し | `Presentation`, `Application` | Presenter化済み。 |
| `Assets/Scripts/UI/StyleChangeUiController.cs` | スタイル変更UI表示、選択結果の適用、スプライト切替 | `Presentation`, `Legacy/Bridge` | UI主導の適用処理がまだ残る。今後さらにPresenter分離の余地あり。 |
| `Assets/Scripts/Item/BreakableObjectSpawner.cs` | 破壊可能オブジェクトの生成、画面外スポーン位置計算 | `Infrastructure` | Unity生成と配置責務に限定。 |
| `Assets/Scripts/Item/BreakableObject.cs` | 破壊判定、回復アイテム生成要求 | `Presentation`, `Infrastructure` | `IDamageable` のUnity側実装として残す。 |
| `Assets/Scripts/Item/HealPickupSpawner.cs` | 回復アイテムのプール生成 | `Infrastructure` | Unity生成責務。 |
| `Assets/Scripts/Orb/ExperienceOrbSpawner.cs` | 敵死亡通知を受けた経験値オーブ生成 | `Infrastructure` | `GameMessageBus` 購読に移行済み。 |
| `Assets/Scripts/Orb/ExperienceOrb.cs` | プレイヤー追従、取得時の経験値加算 | `Presentation`, `Legacy/Bridge` | 純C#化は未着手。Unity上の挙動クラス。 |

## 新規レイヤー側の受け皿

| レイヤー | 主要スクリプト | 受け持つ責務 |
| --- | --- | --- |
| `Domain` | `GameSessionState`, `PlayerState`, `WeaponState`, `EnemySpawnState`, `CharacterStatValues` | ランタイム状態、値オブジェクト、倍率や残り時間の保持 |
| `Application` | `GameSessionService`, `PlayerProgressionService`, `WeaponService`, `EnemySpawnService`, `GameMessageBus` | 進行更新、状態遷移、通知、ユースケース制御 |
| `Infrastructure` | `GameBootstrapper`, `EnemyRegistry`, 各Spawner | Unity参照解決、生成、探索、シーン配線 |
| `Presentation` | `GameScreenPresenter`, `WeaponUpgradePresenter`, 各UiController, 移動/投射系 `MonoBehaviour` | 表示、入力、Transform/物理反映 |

## 未整理の主な残件
- `PlayerController` はまだ統合窓口として責務が広く、入力・見た目・装備管理の再分割余地がある。
- `StyleChangeUiController` は UI 表示と適用処理を同時に持っており、Presenter分離の対象として残る。
- `HealthComponent` と `ExperienceOrb` 系は純C#状態モデルへまだ分離していない。
- `GameManager` は薄くなったが、最終的にはさらにブートストラップ配下のアダプターへ縮退できる。

## 保守ルール
- 新規のゲームルールや状態遷移は、まず `Domain` または `Application` に追加し、`MonoBehaviour` に直接書かない。
- `MonoBehaviour` の役割は、入力受信、`deltaTime` 受け渡し、Prefab生成、UI反映、Unityイベント購読に限定する。
- 新しい参照配線は `GameBootstrapper` へ集約し、他クラスから `FindFirstObjectByType` を増やさない。
- `GameManager`、`PlayerController`、`EnemySpawner` など既存コンポーネントの静的 `Instance` 前提を復活させない。
- シーン内通知が必要な場合は `GameMessageBus` を優先し、新規コードで `GameEvents` を再導入しない。
- UI追加時は `UiController` に表示要素参照と入力通知だけを持たせ、状態収集や分岐は Presenter 側へ寄せる。
- 既存の `Legacy/Bridge` 扱いクラスを修正する場合は、「委譲だけ残す」のか「責務を新層へ移す」のかを先に決め、ロジックの二重化を避ける。
- 近傍探索やスポーン判定のように純C#化済みの責務は、Unity API へ再依存させず、値やコールバックを引数で渡す。
- アーキテクチャ変更時は、このファイルの責務マッピングと `Documentation/todo.md` の進捗を同じ変更セットで更新する。
- 回帰確認は最低でも EditMode テスト追加、またはメインシーン手動確認項目の更新をセットで行う。

## 変更時の確認観点
- 追加したロジックが `Domain` / `Application` / `Infrastructure` / `Presentation` のどこに属するか説明できること。
- 既存の公開 API を残す場合、その API が互換維持のためか、現役の責務かをコメントやレビューで明確にすること。
- `GameBootstrapper` に未接続の参照追加がないこと。
- UI更新が Presenter 主導のままになっており、`UiController` 側で状態問い合わせを再開していないこと。
