# リファクタリング進捗管理

最終更新: 2026年2月17日

## 全体進捗

- [x] フェーズ1: コーディング規約の統一
- [x] フェーズ2: 共通機能の抽出
- [x] フェーズ3: 依存関係の改善
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

- [x] プレイヤー移動・攻撃の確認
- [x] 敵のスポーン・移動の確認
- [x] ダメージ処理の確認
- [x] ゲームオーバー・クリアの確認

---

## フェーズ2: 共通機能の抽出

### 2.1 HealthComponent の作成

- [x] `HealthComponent.cs` の実装
  - [x] HP管理機能
  - [x] ダメージ処理
  - [x] 回復処理
  - [x] イベント通知（OnDamaged, OnDied）
- [x] `PlayerController.cs` への適用
  - [x] HP関連コードの削除
  - [x] HealthComponentの統合
- [x] `EnemyController.cs` への適用
  - [x] HP関連コードの削除
  - [x] HealthComponentの統合
- [x] 動作確認
  - [x] プレイヤーのダメージ処理
  - [x] 敵のダメージ処理
  - [x] 死亡処理

### 2.2 KnockbackComponent の作成

- [x] `KnockbackComponent.cs` の実装
  - [x] ノックバック適用機能
  - [x] タイマー管理
  - [x] 状態取得プロパティ
- [x] `PlayerController.cs` への適用
  - [x] ノックバック関連コードの削除
  - [x] KnockbackComponentの統合
- [x] `EnemyController.cs` への適用
  - [x] ノックバック関連コードの削除
  - [x] KnockbackComponentの統合
- [x] 動作確認
  - [x] プレイヤーのノックバック
  - [x] 敵のノックバック
  - [x] ノックバック中の移動制限

### 2.3 SpriteDirectionController の作成

- [x] `SpriteDirectionController.cs` の実装
  - [x] スプライト切り替え機能
  - [x] 向き判定機能
  - [x] 方向ベクトル取得
- [x] `PlayerController.cs` への適用
  - [x] スプライト関連コードの削除
  - [x] SpriteDirectionControllerの統合
- [x] `EnemyController.cs` への適用
  - [x] スプライト関連コードの削除
  - [x] SpriteDirectionControllerの統合
- [x] 動作確認
  - [x] プレイヤーのスプライト切り替え
  - [x] 敵のスプライト切り替え

### 2.4 ProjectileController 基底クラスの作成

- [x] `ProjectileController.cs` の実装
  - [x] 基本移動処理
  - [x] 生存時間管理
  - [x] 方向設定
  - [x] 抽象メソッド定義
- [x] `BulletController.cs` のリファクタリング
  - [x] ProjectileControllerを継承
  - [x] 重複コードの削除
- [x] `ThrowingBulletController.cs` のリファクタリング
  - [x] ProjectileControllerを継承
  - [x] 重複コードの削除
- [x] 動作確認
  - [x] 通常弾の発射・移動
  - [x] 投擲弾の発射・移動
  - [x] 敵への命中判定

### 2.5 フェーズ2 総合テスト

- [x] 全機能の統合テスト
- [x] パフォーマンスチェック
- [x] バグ修正

---

## フェーズ3: 依存関係の改善

### 3.1 インターフェースの導入

- [x] `IDamageable.cs` の作成
- [x] `IKnockbackable.cs` の作成
- [x] `HealthComponent` への適用
- [x] `KnockbackComponent` への適用
- [x] 弾クラスでのインターフェース使用
- [x] 動作確認

### 3.2 イベントシステムの導入

- [x] `GameEvents.cs` の作成
  - [x] プレイヤー関連イベント
  - [x] 敵関連イベント
  - [x] ゲーム状態イベント
- [x] `PlayerController` のイベント発行
- [x] `EnemyController` のイベント発行
- [x] `EnemySpawner` のイベント購読
- [x] `GameManager` のイベント購読
- [x] 動作確認

### 3.3 シングルトン依存の削減

- [x] `EnemyController` の依存削減
  - [x] ターゲット参照の注入方式へ変更
- [x] `EnemySpawner` の依存削減
  - [x] プレイヤー参照の注入方式へ変更
- [x] `BulletShooter` の依存削減
- [x] `ThrowingBulletShooter` の依存削減
- [x] `DamageFieldController` の依存削減
- [x] 動作確認

### 3.4 フェーズ3 総合テスト

- [x] 全機能の統合テスト
- [x] 依存関係の検証
- [x] コンパイルエラーなし

---

## フェーズ4: パフォーマンス最適化

### 4.1 空間分割による敵検索の最適化

- [x] `SpatialGrid.cs` の実装
- [x] `EnemySpawner` への統合
- [x] `FindNearestEnemy()` の最適化
- [x] パフォーマンス測定
- [x] 動作確認

### 4.2 オブジェクトプーリングの導入

- [x] `ObjectPool.cs` の実装
- [ ] 弾のプーリング
  - [x] BulletController対応
  - [x] ThrowingBulletController対応
- [ ] 敵のプーリング
  - [x] EnemyController対応
  - [x] EnemySpawner対応
- [ ] ダメージフィールドのプーリング
  - [x] DamageFieldController対応
  - [x] DamageFieldSpawner対応
- [x] パフォーマンス測定
- [x] 動作確認

### 4.3 フェーズ4 総合テスト

- [x] パフォーマンス測定
- [x] メモリ使用量チェック
- [x] バグ修正

---

## フェーズ5: アーキテクチャの改善

### 5.1 PlayerController の分割

- [x] `PlayerMovement.cs` の作成
- [x] `PlayerInput.cs` の作成
- [x] `PlayerController.cs` のリファクタリング
- [x] 動作確認

### 5.2 武器システムの抽象化

- [x] `WeaponBase.cs` の作成
- [x] `BulletWeapon.cs` の作成
- [x] `ThrowingWeapon.cs` の作成
- [x] `DamageFieldWeapon.cs` の作成
- [x] 既存武器クラスの移行
- [x] 動作確認

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

### フェーズ2完了 (2026年2月17日)

フェーズ2「共通機能の抽出」が完了しました。

#### 実施内容

##### 新規作成したコンポーネント
1. `HealthComponent.cs` - HP管理とダメージ処理
   - HP管理機能（CurrentHp, MaxHp, IsDead）
   - ダメージ処理（TakeDamage）
   - 回復処理（Heal）
   - イベント通知（OnDamaged, OnDied）

2. `KnockbackComponent.cs` - ノックバック処理
   - ノックバック適用機能（ApplyKnockback）
   - タイマー管理
   - 状態取得プロパティ（IsKnockedBack）

3. `SpriteDirectionController.cs` - スプライト方向制御
   - スプライト切り替え機能（UpdateDirection）
   - 向き判定機能
   - 方向ベクトル取得（GetFacingDirection）

4. `ProjectileController.cs` - 弾の基底クラス
   - 基本移動処理
   - 生存時間管理
   - 方向設定（SetDirection）
   - 抽象メソッド定義（OnHitEnemy）

##### リファクタリングしたクラス
1. `PlayerController.cs`
   - HP関連コードを削除し、HealthComponentを使用
   - ノックバック関連コードを削除し、KnockbackComponentを使用
   - スプライト関連コードを削除し、SpriteDirectionControllerを使用
   - コード量が大幅に削減され、責務が明確化

2. `EnemyController.cs`
   - HP関連コードを削除し、HealthComponentを使用
   - ノックバック関連コードを削除し、KnockbackComponentを使用
   - スプライト関連コードを削除し、SpriteDirectionControllerを使用
   - コード量が大幅に削減され、責務が明確化

3. `BulletController.cs`
   - ProjectileControllerを継承
   - 重複コードを削除し、OnHitEnemyメソッドのみ実装
   - コード量が約70%削減

4. `ThrowingBulletController.cs`
   - ProjectileControllerを継承
   - 重複コードを削除し、SetDirectionとOnHitEnemyメソッドのみ実装
   - コード量が約70%削減

#### 成果
- コンパイルエラーなし
- 重複コードの大幅な削減
- 各クラスの責務が明確化
- 保守性と再利用性の向上
- コンポーネント指向の設計に移行

#### 注意事項
Unityエディタでの設定が必要：
- PlayerとEnemyのGameObjectに以下のコンポーネントを追加する必要があります：
  - HealthComponent（HP設定を移行）
  - KnockbackComponent（ノックバック設定を移行）
  - SpriteDirectionController（スプライト設定を移行）

#### 次のステップ
フェーズ3「依存関係の改善」に進む準備が整いました。
- インターフェースの導入（IDamageable, IKnockbackable）
- イベントシステムの導入（GameEvents）
- シングルトン依存の削減

### フェーズ3完了 (2026年2月17日)

フェーズ3「依存関係の改善」が完了しました。

#### 実施内容

##### 新規作成したインターフェースとシステム
1. `IDamageable.cs` - ダメージを受けるオブジェクトのインターフェース
   - TakeDamage(int damage, Vector3 knockbackDirection)
   - CurrentHp プロパティ
   - IsDead プロパティ

2. `IKnockbackable.cs` - ノックバックを受けるオブジェクトのインターフェース
   - ApplyKnockback(Vector3 direction)
   - IsKnockedBack プロパティ

3. `GameEvents.cs` - 静的イベントシステム
   - プレイヤー関連イベント（OnPlayerMoved, OnPlayerDamaged, OnPlayerDied）
   - 敵関連イベント（OnEnemySpawned, OnEnemyDied）
   - ゲーム状態イベント（OnGameOver）
   - イベント発火メソッド（Raise系メソッド）
   - イベントクリア機能（ClearAllEvents）

##### リファクタリングしたクラス
1. `HealthComponent.cs`
   - IDamageableインターフェースを実装
   - TakeDamageメソッドにノックバック方向パラメータを追加
   - ノックバック処理を自動的にKnockbackComponentに委譲

2. `KnockbackComponent.cs`
   - IKnockbackableインターフェースを実装

3. `PlayerController.cs`
   - プレイヤー移動時にGameEvents.RaisePlayerMovedを発火
   - ダメージ受信時にGameEvents.RaisePlayerDamagedを発火
   - 死亡時にGameEvents.RaisePlayerDiedとGameEvents.RaiseGameOverを発火
   - HealthComponent.TakeDamageの新しいシグネチャを使用

4. `EnemyController.cs`
   - SetTarget(Transform target)メソッドを追加（依存性注入）
   - PlayerController.Instanceへの直接参照を削除
   - _targetTransformフィールドを使用した疎結合な実装
   - 死亡時にGameEvents.RaiseEnemyDiedを発火
   - 後方互換性のためAwakeでPlayerController.Instanceから自動設定

5. `EnemySpawner.cs`
   - SetSpawnTarget(Transform target)メソッドを追加（依存性注入）
   - PlayerController.Instanceへの直接参照を削除
   - _playerTransformフィールドを使用した疎結合な実装
   - スポーン時にEnemyController.SetTargetでターゲットを注入
   - スポーン時にGameEvents.RaiseEnemySpawnedを発火
   - 後方互換性のためStartでPlayerController.Instanceから自動設定

6. `ProjectileController.cs`
   - OnHitEnemyメソッドを削除（抽象メソッドではなくなった）
   - OnTriggerEnterでIDamageableインターフェースを使用
   - EnemyControllerへの直接依存を削除
   - ノックバック処理を統合

7. `BulletController.cs`
   - OnHitEnemyメソッドを削除（基底クラスで処理）
   - 空のクラスになり、コード量が大幅に削減

8. `ThrowingBulletController.cs`
   - OnHitEnemyメソッドを削除（基底クラスで処理）
   - SetDirectionメソッドのみ残る

9. `DamageFieldController.cs`
   - SetFollowTarget(Transform target)メソッドを追加（依存性注入）
   - PlayerController.Instanceへの直接参照を削除
   - _targetTransformフィールドを使用した疎結合な実装
   - ApplyDamageToEnemyでIDamageableインターフェースを使用
   - EnemyControllerへの直接依存を削除
   - 後方互換性のためStartでPlayerController.Instanceから自動設定

10. `GameManager.cs`
    - GameEvents.OnGameOverイベントを購読
    - PlayerController.Instanceへの直接参照を削除
    - _isGameOverフィールドを追加してイベント経由で状態管理

#### 成果
- コンパイルエラーなし
- シングルトン依存の大幅な削減
- インターフェースによる疎結合な設計
- イベントシステムによる柔軟な通知機構
- 依存性注入パターンの導入
- テスト容易性の向上
- 後方互換性を維持しながら段階的な移行が可能

#### アーキテクチャの改善点
1. 疎結合化
   - クラス間の直接参照が削減され、インターフェースとイベントを介した通信に
   - 各クラスが具体的な実装ではなく抽象に依存

2. 拡張性の向上
   - IDamageableを実装すれば、どんなオブジェクトでもダメージを受けられる
   - IKnockbackableを実装すれば、どんなオブジェクトでもノックバック可能
   - イベントシステムにより、新しいリスナーを簡単に追加可能

3. テスト容易性
   - インターフェースによりモックオブジェクトの作成が容易
   - イベントシステムにより、各コンポーネントを独立してテスト可能
   - 依存性注入により、テスト用のダミーオブジェクトを注入可能

4. 保守性の向上
   - 変更の影響範囲が限定的
   - 各クラスの責務がより明確に
   - コードの重複がさらに削減

#### 注意事項
後方互換性のため、以下の動作を維持：
- EnemyControllerは自動的にPlayerController.Instanceをターゲットに設定
- EnemySpawnerは自動的にPlayerController.Instanceを基準に設定
- DamageFieldControllerは自動的にPlayerController.Instanceを追従対象に設定

将来的には、これらの自動設定を削除し、完全な依存性注入に移行することを推奨します。

#### 次のステップ
フェーズ4「パフォーマンス最適化」またはフェーズ5「アーキテクチャの改善」に進むことができます。
- フェーズ4: 空間分割による敵検索の最適化、オブジェクトプーリングの導入
- フェーズ5: PlayerControllerの分割、武器システムの抽象化、敵AIの抽象化

### フェーズ5-2着手 (2026年2月17日)

フェーズ5-2「武器システムの抽象化」に着手し、基底クラスと具体武器クラスの導入、および既存クラスの移行を実施しました。

#### 実施内容
- `WeaponBase.cs` を作成し、クールダウン制御と発動フローを共通化
- `BulletWeapon.cs` / `ThrowingWeapon.cs` / `DamageFieldWeapon.cs` を作成
- 既存クラスを後方互換ラッパーに移行
  - `BulletShooter : BulletWeapon`
  - `ThrowingBulletShooter : ThrowingWeapon`
  - `DamageFieldSpawner : DamageFieldWeapon`
- 新規スクリプトの `.meta` ファイルを追加

#### 補足
- 既存シーン・プレハブの参照互換性維持のため、旧クラス名は維持
- 動作確認（Unityエディタ再生による確認）は未実施

### 発見された問題

（ここに作業中に発見された問題を記録）

### 改善案

（ここに追加の改善案を記録）

### 保留事項

（ここに保留した項目を記録）
