# Weapon Application責務分離 リファクタ ToDo

## Phase 1: 要求データ定義
- [ ] `Weapon` 発動要求の共通設計を決める
- [ ] `BulletFireRequest` を追加する
- [ ] `ThrowingFireRequest` を追加する
- [ ] `DamageFieldSpawnRequest` を追加する
- [ ] `IWeaponEffectExecutor` を追加する

## Phase 2: Infrastructure 実行アダプタ追加
- [ ] `WeaponEffectExecutor` を追加する
- [ ] 弾発射要求を `BulletController` 初期化へ変換する
- [ ] 投擲弾発射要求を `ThrowingBulletController` 初期化へ変換する
- [ ] ダメージフィールド生成要求を `DamageFieldController` 初期化へ変換する
- [ ] `BulletFactory` の責務を Prefab参照提供に限定する

## Phase 3: Weapon基底の純化
- [ ] `WeaponBase` から `Instantiate` / `GetComponent` / `ObjectPool` 依存を排除する
- [ ] `WeaponBase` のコンストラクタ依存を見直し、実行ポートを受け取れる形にする
- [ ] `WeaponBase` のクールダウン進行と武器チェーン挙動を維持する

## Phase 4: 個別Weaponの純化
- [ ] `BulletWeapon` から Prefab保持とプール初期化を削除する
- [ ] `BulletWeapon` をターゲット探索と `BulletFireRequest` 生成に限定する
- [ ] `DamageFieldWeapon` から Prefab保持とプール初期化を削除する
- [ ] `DamageFieldWeapon` を `DamageFieldSpawnRequest` 生成に限定する
- [ ] `ThrowingWeapon` から Prefab保持とプール初期化を削除する
- [ ] `ThrowingWeapon` を `ThrowingFireRequest` 生成に限定する

## Phase 5: 呼び出し側の接続変更
- [ ] `WeaponService` のビルダーに `IWeaponEffectExecutor` 注入を追加する
- [ ] `PlayerController` で `WeaponEffectExecutor` を初期化する
- [ ] `PlayerController` の既存武器生成経路を新しい注入経路へ切り替える
- [ ] 既存の武器強化フローが壊れていないことを確認する

## Phase 6: テスト更新
- [ ] `WeaponServiceTests` を新構造に合わせて更新する
- [ ] `BulletWeapon` の要求生成テストを追加する
- [ ] `ThrowingWeapon` の要求生成テストを追加する
- [ ] `DamageFieldWeapon` の要求生成テストを追加する
- [ ] クールダウン進行と強化後挙動の回帰テストを確認する

## Phase 7: 手動確認
- [ ] シューター武器の発射挙動を確認する
- [ ] 破壊可能オブジェクトへのターゲット切替を確認する
- [ ] 投擲武器の向き反映を確認する
- [ ] ダメージフィールドの追従とサイズ拡大を確認する
- [ ] 武器強化 UI 経由の追加・強化を確認する
