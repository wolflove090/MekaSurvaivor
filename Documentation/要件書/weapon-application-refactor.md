# Weapon Application責務分離 リファクタ要件書

## 背景と目的
- 現在の `Assets/Scripts/Wepon` 配下では、`WeaponBase`、`BulletWeapon`、`DamageFieldWeapon`、`ThrowingWeapon` が武器の発動ルールを持つ一方で、`Transform`、`GameObject.Instantiate`、`ObjectPool`、`BulletFactory` など Unity 依存の具体処理も同居している。
- その結果、武器の発動条件、強化、クールダウン進行と、Prefab生成や弾コンポーネント制御の責務境界が曖昧になっている。
- 本リファクタの目的は、`*Weapon` を `Application` 層に配置できる状態まで責務を純化し、武器ロジックと Unity 実装を分離すること。

## スコープ内
- `WeaponBase`、`BulletWeapon`、`DamageFieldWeapon`、`ThrowingWeapon` の責務を見直し、Unity 依存処理を切り出す。
- 武器発動に必要なデータの受け渡し方法を整理し、`Application` 層のクラスが Unity の生成APIに直接依存しない構造へ変更する。
- 発射体・ダメージフィールドの生成、初期化、プール返却を担当する `Infrastructure` 側アダプタを定義する。
- 既存の `WeaponService`、`WeaponState`、`PlayerController` と整合する形で、武器強化と発動フローを維持する。
- 既存挙動（発射間隔、強化、ターゲット探索、追従、ダメージ反映）を維持する。

## スコープ外
- 武器種の追加、既存武器性能のバランス調整。
- UI (`WeaponUpgradeUiController` など) の仕様変更。
- `BulletController`、`DamageFieldController`、`ProjectileController`、`ThrowingBulletController` の挙動仕様変更。
- `Wepon` ディレクトリ名のリネーム（`Weapon` への改名など）。
- Addressables 導入やアセットロード方式の刷新。

## 機能要件
- `WeaponBase` はクールダウン進行と発動判定の起点として機能しつつ、Unity の生成APIや `GameObject` 生成処理を直接持たない。
- `BulletWeapon` は「最寄りターゲットを選ぶ」「発射要求を組み立てる」責務に限定し、弾の生成実行は別責務へ委譲する。
- `DamageFieldWeapon` は「生成タイミング」「効果スケール計算」を担当し、ダメージフィールドの生成実行は別責務へ委譲する。
- `ThrowingWeapon` は「プレイヤー向きに基づく投擲要求の組み立て」を担当し、投擲弾の生成実行は別責務へ委譲する。
- 武器ごとに必要な生成依頼は、`Application` 層で表現可能な要求データ（位置、方向、攻撃力、スケールなど）として定義される。
- `Infrastructure` 層には、要求データを受けて Prefab 生成・プール取得・ `*Controller` 初期化を行うアダプタを配置する。
- `PlayerController` から見た `WeaponBase.Tick` と `WeaponService.ApplyUpgrade` の利用方法は大きく変えず、呼び出し側の影響を最小化する。
- 既存の `BulletFactory` が保持している Prefab 参照は、必要に応じて `Infrastructure` 側の生成アダプタへ移譲または再利用される。

## 非機能要件
- 既存の EditMode テスト資産を壊さず、武器ロジックは可能な限り Unity 非依存で単体テストしやすい構造にする。
- `Application` 層の武器ロジックは、`MonoBehaviour` 継承、`Instantiate`、`Destroy`、`GetComponent` に依存しない。
- リファクタ後もメインシーンの実行時に NullReference や参照設定漏れが発生しないよう、依存注入経路を明示する。
- 段階移行中に旧経路と新経路が二重発火しないよう、発射実行の責務は常に単一箇所に限定する。

## 受け入れ条件
- `WeaponBase`、`BulletWeapon`、`DamageFieldWeapon`、`ThrowingWeapon` が Unity のオブジェクト生成処理を直接呼ばない。
- `*Weapon` クラスが `ObjectPool<T>`、`GameObject.Instantiate`、`GetComponent` を直接使用しない。
- 弾武器は従来どおり、最寄りの敵または破壊可能オブジェクトに向かって発射される。
- ダメージフィールド武器は従来どおり、プレイヤー追従・攻撃力反映・エリア拡大が機能する。
- 投擲武器は従来どおり、プレイヤーの向きに応じて投擲弾を発射する。
- 武器強化後のクールダウン短縮、エリア拡大、武器チェーンの多重発動が維持される。
- 既存の武器関連テストを更新または追加し、主要な発動条件と要求データ生成を検証できる。

## 未確定事項・確認事項
- `Application` 層から `Transform` を完全排除するか、発動基準位置の取得だけは呼び出し元から値注入で許容するか。
- 生成依頼の表現を、武器種ごとの専用リクエスト型に分けるか、共通インターフェースで統一するか。
- `BulletFactory` をそのまま `Infrastructure` アダプタの依存として残すか、より明示的な `WeaponPrefabProvider` に分離するか。
- 既存の `WeaponService` に「生成アダプタの仲介責務」を持たせるか、`WeaponBase` から別の実行ポートを参照する形にするか。
- `Assets/Scripts/Application/Character/Combat` など既存配置との整合上、武器関連ファイルをどの最終パスへ寄せるか。
