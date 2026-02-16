# リファクタリング進捗管理

最終更新: 2026年2月16日

## 全体進捗

- [x] フェーズ1: コーディング規約の統一
- [ ] フェーズ2: 共通機能の抽出
- [ ] フェーズ3: 依存関係の改善
- [ ] フェーズ4: パフォーマンス最適化
- [ ] フェーズ5: アーキテクチャの改善

---

## フェーズ1: コーディング規約の統一

### 1.1 暗黙privateへの変更

#### コントローラー系
- [x] `PlayerController.cs`
- [x] `EnemyController.cs`
- [x] `GameManager.cs`

#### 弾・武器系
- [x] `BulletController.cs`
- [x] `BulletShooter.cs`
- [x] `ThrowingBulletController.cs`
- [x] `ThrowingBulletShooter.cs`

#### スポーナー・フィールド系
- [x] `EnemySpawner.cs`
- [x] `DamageFieldController.cs`
- [x] `DamageFieldSpawner.cs`

#### その他
- [x] `CameraController.cs`

### 1.2 ドキュメントコメントの追加・修正

#### コントローラー系
- [x] `PlayerController.cs`
- [x] `EnemyController.cs`
- [x] `GameManager.cs`

#### 弾・武器系
- [x] `BulletController.cs`
- [x] `BulletShooter.cs`
- [x] `ThrowingBulletController.cs`
- [x] `ThrowingBulletShooter.cs`

#### スポーナー・フィールド系
- [x] `EnemySpawner.cs`
- [x] `DamageFieldController.cs`
- [x] `DamageFieldSpawner.cs`

#### その他
- [x] `CameraController.cs`

### 1.3 コメントの最適化

- [x] 冗長なコメントの削除
- [x] 必要箇所への適切なコメント追加
- [x] 全ファイルのレビュー完了

### 1.4 動作確認

- [ ] プレイヤー移動・攻撃の確認
- [ ] 敵のスポーン・移動の確認
- [ ] ダメージ処理の確認
- [ ] ゲームオーバー・クリアの確認

---

## フェーズ2: 共通機能の抽出

### 2.1 HealthComponent の作成

- [ ] `HealthComponent.cs` の実装
  - [ ] HP管理機能
  - [ ] ダメージ処理
  - [ ] 回復処理
  - [ ] イベント通知（OnDamaged, OnDied）
- [ ] `PlayerController.cs` への適用
  - [ ] HP関連コードの削除
  - [ ] HealthComponentの統合
- [ ] `EnemyController.cs` への適用
  - [ ] HP関連コードの削除
  - [ ] HealthComponentの統合
- [ ] 動作確認
  - [ ] プレイヤーのダメージ処理
  - [ ] 敵のダメージ処理
  - [ ] 死亡処理

### 2.2 KnockbackComponent の作成

- [ ] `KnockbackComponent.cs` の実装
  - [ ] ノックバック適用機能
  - [ ] タイマー管理
  - [ ] 状態取得プロパティ
- [ ] `PlayerController.cs` への適用
  - [ ] ノックバック関連コードの削除
  - [ ] KnockbackComponentの統合
- [ ] `EnemyController.cs` への適用
  - [ ] ノックバック関連コードの削除
  - [ ] KnockbackComponentの統合
- [ ] 動作確認
  - [ ] プレイヤーのノックバック
  - [ ] 敵のノックバック
  - [ ] ノックバック中の移動制限

### 2.3 SpriteDirectionController の作成

- [ ] `SpriteDirectionController.cs` の実装
  - [ ] スプライト切り替え機能
  - [ ] 向き判定機能
  - [ ] 方向ベクトル取得
- [ ] `PlayerController.cs` への適用
  - [ ] スプライト関連コードの削除
  - [ ] SpriteDirectionControllerの統合
- [ ] `EnemyController.cs` への適用
  - [ ] スプライト関連コードの削除
  - [ ] SpriteDirectionControllerの統合
- [ ] 動作確認
  - [ ] プレイヤーのスプライト切り替え
  - [ ] 敵のスプライト切り替え

### 2.4 ProjectileController 基底クラスの作成

- [ ] `ProjectileController.cs` の実装
  - [ ] 基本移動処理
  - [ ] 生存時間管理
  - [ ] 方向設定
  - [ ] 抽象メソッド定義
- [ ] `BulletController.cs` のリファクタリング
  - [ ] ProjectileControllerを継承
  - [ ] 重複コードの削除
- [ ] `ThrowingBulletController.cs` のリファクタリング
  - [ ] ProjectileControllerを継承
  - [ ] 重複コードの削除
- [ ] 動作確認
  - [ ] 通常弾の発射・移動
  - [ ] 投擲弾の発射・移動
  - [ ] 敵への命中判定

### 2.5 フェーズ2 総合テスト

- [ ] 全機能の統合テスト
- [ ] パフォーマンスチェック
- [ ] バグ修正

---

## フェーズ3: 依存関係の改善

### 3.1 インターフェースの導入

- [ ] `IDamageable.cs` の作成
- [ ] `IKnockbackable.cs` の作成
- [ ] `HealthComponent` への適用
- [ ] `KnockbackComponent` への適用
- [ ] 弾クラスでのインターフェース使用
- [ ] 動作確認

### 3.2 イベントシステムの導入

- [ ] `GameEvents.cs` の作成
  - [ ] プレイヤー関連イベント
  - [ ] 敵関連イベント
  - [ ] ゲーム状態イベント
- [ ] `PlayerController` のイベント発行
- [ ] `EnemyController` のイベント発行
- [ ] `EnemySpawner` のイベント購読
- [ ] `GameManager` のイベント購読
- [ ] 動作確認

### 3.3 シングルトン依存の削減

- [ ] `EnemyController` の依存削減
  - [ ] ターゲット参照の注入方式へ変更
- [ ] `EnemySpawner` の依存削減
  - [ ] プレイヤー参照の注入方式へ変更
- [ ] `BulletShooter` の依存削減
- [ ] `ThrowingBulletShooter` の依存削減
- [ ] `DamageFieldController` の依存削減
- [ ] 動作確認

### 3.4 フェーズ3 総合テスト

- [ ] 全機能の統合テスト
- [ ] 依存関係の検証
- [ ] バグ修正

---

## フェーズ4: パフォーマンス最適化

### 4.1 空間分割による敵検索の最適化

- [ ] `SpatialGrid.cs` の実装
- [ ] `EnemySpawner` への統合
- [ ] `FindNearestEnemy()` の最適化
- [ ] パフォーマンス測定
- [ ] 動作確認

### 4.2 オブジェクトプーリングの導入

- [ ] `ObjectPool.cs` の実装
- [ ] 弾のプーリング
  - [ ] BulletController対応
  - [ ] ThrowingBulletController対応
- [ ] 敵のプーリング
  - [ ] EnemyController対応
  - [ ] EnemySpawner対応
- [ ] ダメージフィールドのプーリング
  - [ ] DamageFieldController対応
  - [ ] DamageFieldSpawner対応
- [ ] パフォーマンス測定
- [ ] 動作確認

### 4.3 フェーズ4 総合テスト

- [ ] パフォーマンス測定
- [ ] メモリ使用量チェック
- [ ] バグ修正

---

## フェーズ5: アーキテクチャの改善

### 5.1 PlayerController の分割

- [ ] `PlayerMovement.cs` の作成
- [ ] `PlayerInput.cs` の作成
- [ ] `PlayerController.cs` のリファクタリング
- [ ] 動作確認

### 5.2 武器システムの抽象化

- [ ] `WeaponBase.cs` の作成
- [ ] `BulletWeapon.cs` の作成
- [ ] `ThrowingWeapon.cs` の作成
- [ ] `DamageFieldWeapon.cs` の作成
- [ ] 既存武器クラスの移行
- [ ] 動作確認

### 5.3 敵AIの抽象化

- [ ] `IEnemyBehavior.cs` の作成
- [ ] `ChasePlayerBehavior.cs` の作成
- [ ] `EnemyController` への統合
- [ ] 動作確認

### 5.4 フェーズ5 総合テスト

- [ ] 全機能の統合テスト
- [ ] 拡張性の検証
- [ ] バグ修正

---

## 最終確認

- [ ] 全フェーズの完了確認
- [ ] 総合動作テスト
- [ ] パフォーマンステスト
- [ ] コードレビュー
- [ ] ドキュメント更新
  - [ ] クラス図の更新
  - [ ] README更新（必要に応じて）

---

## メモ・課題

### フェーズ1完了 (2026年2月16日)

フェーズ1「コーディング規約の統一」が完了しました。

#### 実施内容
- 全ファイルから暗黙`private`への変更（`private`修飾子の削除）
- 冗長なコメントの削除と最適化
- ドキュメントコメントの確認と整理
- コンパイルエラーなし

#### 次のステップ
フェーズ2「共通機能の抽出」に進む準備が整いました。
- HealthComponent の作成
- KnockbackComponent の作成
- SpriteDirectionController の作成
- ProjectileController 基底クラスの作成

### 発見された問題

（ここに作業中に発見された問題を記録）

### 改善案

（ここに追加の改善案を記録）

### 保留事項

（ここに保留した項目を記録）
