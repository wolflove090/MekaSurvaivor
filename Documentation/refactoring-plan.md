# リファクタリング計画書

## プロジェクト概要

MekaSurvaivor - Unity 2Dサバイバルゲームのリファクタリング

作成日: 2026年2月16日

## 現状分析

### 1. コーディング規約違反

- `private`修飾子が明示的に記述されている箇所がある
- 一部のメソッドにドキュメントコメントが不足
- コメントの量が最適化されていない箇所がある

### 2. アーキテクチャの問題

#### シングルトンパターンの多用
- `PlayerController.Instance`
- `GameManager.Instance`
- `EnemySpawner.Instance`

これにより、以下の問題が発生：
- テストが困難
- 密結合な依存関係
- 拡張性の低下

#### 密結合な依存関係
多くのクラスが`PlayerController.Instance`に直接依存：
- `EnemyController` - プレイヤー位置の取得
- `EnemySpawner` - スポーン位置の計算
- `GameManager` - ゲームオーバー判定
- `BulletShooter` - 発射位置の基準
- `ThrowingBulletShooter` - 発射方向の取得
- `DamageFieldController` - 追従処理

#### 責務の分離が不十分
例：`PlayerController`が以下を全て担当
- 入力処理
- 移動制御
- HP管理
- ノックバック処理
- スプライト制御
- ゲームオーバー処理

### 3. 重複コード

#### ノックバック処理
`PlayerController`と`EnemyController`で同様の実装：
```csharp
bool _isKnockedBack;
float _knockbackTimer;
void UpdateKnockback() { ... }
void ApplyKnockback(Vector3 direction) { ... }
```

#### スプライト更新処理
`PlayerController`と`EnemyController`で同様の実装：
```csharp
Sprite _leftSprite;
Sprite _rightSprite;
SpriteRenderer _spriteRenderer;
void UpdateSprite() { ... }
```

#### 弾の制御ロジック
`BulletController`と`ThrowingBulletController`で重複：
```csharp
float _speed;
float _lifetime;
Vector3 _direction;
void SetDirection(Vector3 direction) { ... }
void Update() { ... }
void OnTriggerEnter(Collider other) { ... }
```

### 4. パフォーマンス懸念

#### 全件検索
`EnemySpawner.FindNearestEnemy()`で全エネミーを毎回検索：
```csharp
// TODO 全件検索のため数が多くなると重たくなる
foreach (GameObject enemy in _enemies) { ... }
```

#### 毎フレームのリスト生成
`DamageFieldController.Update()`で辞書のキーをリスト化：
```csharp
List<GameObject> enemiesToUpdate = new List<GameObject>(_enemiesInField.Keys);
```

### 5. 拡張性の問題

- ダメージ処理が各弾クラスに散在（統一的な処理が困難）
- HP管理が各コントローラーに埋め込まれている（共通化できない）
- 新しい武器や敵タイプの追加が困難

## リファクタリング計画

### フェーズ1: コーディング規約の統一

**目的**: プロジェクト全体のコードスタイルを統一

**作業内容**:
1. 暗黙`private`への変更
   - 全ての`private`修飾子を削除
   - フィールド名が`_camelCase`であることを確認

2. ドキュメントコメントの追加・修正
   - 全てのpublicメソッド・プロパティにXMLコメント追加
   - 既存コメントの見直し

3. コメントの最適化
   - 冗長なコメントの削除
   - 必要な箇所への適切なコメント追加

**影響範囲**: 全ファイル

**リスク**: 低（動作に影響なし）

**所要時間**: 2-3時間

### フェーズ2: 共通機能の抽出

**目的**: 重複コードを削減し、保守性を向上

#### 2.1 HealthComponent の作成

**責務**: HP管理とダメージ処理

```csharp
public class HealthComponent : MonoBehaviour
{
    [SerializeField] int _maxHp;
    int _currentHp;
    
    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;
    public bool IsDead => _currentHp <= 0;
    
    public event System.Action<int> OnDamaged;
    public event System.Action OnDied;
    
    public void TakeDamage(int damage) { ... }
    public void Heal(int amount) { ... }
}
```

**適用対象**:
- `PlayerController`
- `EnemyController`

#### 2.2 KnockbackComponent の作成

**責務**: ノックバック処理

```csharp
public class KnockbackComponent : MonoBehaviour
{
    [SerializeField] float _knockbackDistance;
    [SerializeField] float _knockbackDuration;
    
    bool _isKnockedBack;
    float _knockbackTimer;
    
    public bool IsKnockedBack => _isKnockedBack;
    
    public void ApplyKnockback(Vector3 direction) { ... }
    void Update() { ... }
}
```

**適用対象**:
- `PlayerController`
- `EnemyController`

#### 2.3 SpriteDirectionController の作成

**責務**: 移動方向に応じたスプライト制御

```csharp
public class SpriteDirectionController : MonoBehaviour
{
    [SerializeField] Sprite _leftSprite;
    [SerializeField] Sprite _rightSprite;
    SpriteRenderer _spriteRenderer;
    
    public void UpdateDirection(float horizontalInput) { ... }
    public Vector3 GetFacingDirection() { ... }
}
```

**適用対象**:
- `PlayerController`
- `EnemyController`

#### 2.4 ProjectileController 基底クラスの作成

**責務**: 弾の共通処理

```csharp
public abstract class ProjectileController : MonoBehaviour
{
    [SerializeField] float _speed;
    [SerializeField] float _lifetime;
    [SerializeField] int _damage = 1;
    
    protected Vector3 _direction;
    
    public void SetDirection(Vector3 direction) { ... }
    protected virtual void Update() { ... }
    protected abstract void OnHitEnemy(EnemyController enemy);
}
```

**適用対象**:
- `BulletController` → `ProjectileController`を継承
- `ThrowingBulletController` → `ProjectileController`を継承

**影響範囲**: 中（既存クラスの大幅な変更）

**リスク**: 中（動作確認が必要）

**所要時間**: 4-6時間

### フェーズ3: 依存関係の改善

**目的**: シングルトン依存を削減し、疎結合な設計へ

#### 3.1 インターフェースの導入

```csharp
public interface IDamageable
{
    void TakeDamage(int damage, Vector3 knockbackDirection = default);
    int CurrentHp { get; }
    bool IsDead { get; }
}

public interface IKnockbackable
{
    void ApplyKnockback(Vector3 direction);
    bool IsKnockedBack { get; }
}
```

#### 3.2 イベントシステムの導入

シングルトンの代わりにイベントで通知：

```csharp
public static class GameEvents
{
    public static event System.Action<Vector3> OnPlayerMoved;
    public static event System.Action<int> OnPlayerDamaged;
    public static event System.Action OnPlayerDied;
    public static event System.Action<GameObject> OnEnemySpawned;
    public static event System.Action<GameObject> OnEnemyDied;
}
```

#### 3.3 依存性注入の活用

コンストラクタやSetterで依存を注入：

```csharp
public class EnemyController : MonoBehaviour
{
    Transform _targetTransform; // PlayerController.Instanceの代わり
    
    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }
}
```

**影響範囲**: 大（多くのクラスが影響を受ける）

**リスク**: 高（慎重な実装とテストが必要）

**所要時間**: 6-8時間

### フェーズ4: パフォーマンス最適化

**目的**: ゲームのパフォーマンス向上

#### 4.1 空間分割による敵検索の最適化

グリッドベースの空間分割を導入：

```csharp
public class SpatialGrid
{
    Dictionary<Vector2Int, List<GameObject>> _grid;
    float _cellSize;
    
    public void Register(GameObject obj) { ... }
    public void Unregister(GameObject obj) { ... }
    public List<GameObject> GetNearbyObjects(Vector3 position, float radius) { ... }
}
```

`EnemySpawner.FindNearestEnemy()`を最適化。

#### 4.2 オブジェクトプーリングの導入

頻繁に生成・破棄されるオブジェクトをプール化：
- 弾（Bullet, ThrowingBullet）
- エネミー
- ダメージフィールド

```csharp
public class ObjectPool<T> where T : Component
{
    Queue<T> _pool;
    T _prefab;
    
    public T Get() { ... }
    public void Return(T obj) { ... }
}
```

**影響範囲**: 中

**リスク**: 中

**所要時間**: 4-6時間

### フェーズ5: アーキテクチャの改善

**目的**: より保守性・拡張性の高い設計へ

#### 5.1 責務の明確な分離

`PlayerController`を分割：
- `PlayerMovement` - 移動処理
- `PlayerInput` - 入力処理
- `PlayerHealth` - HP管理（HealthComponent使用）
- `PlayerController` - 統合管理

#### 5.2 武器システムの抽象化

```csharp
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] float _cooldown;
    float _cooldownTimer;
    
    public abstract void Fire();
    protected virtual void Update() { ... }
}

public class BulletWeapon : WeaponBase { ... }
public class ThrowingWeapon : WeaponBase { ... }
public class DamageFieldWeapon : WeaponBase { ... }
```

#### 5.3 敵AIの抽象化

```csharp
public interface IEnemyBehavior
{
    void Execute(EnemyController enemy);
}

public class ChasePlayerBehavior : IEnemyBehavior { ... }
public class PatrolBehavior : IEnemyBehavior { ... }
```

**影響範囲**: 大

**リスク**: 高

**所要時間**: 8-10時間

## 実施順序

### 推奨順序

1. **フェーズ1: コーディング規約の統一**
   - 影響範囲が小さく安全
   - 後続作業の基盤となる

2. **フェーズ2: 共通機能の抽出**
   - 重複コード削減により後続作業が楽になる
   - 段階的に実施可能

3. **フェーズ3: 依存関係の改善**
   - アーキテクチャ改善の基盤
   - テストしながら慎重に進める

4. **フェーズ4: パフォーマンス最適化**（必要に応じて）
   - 現時点でパフォーマンス問題がなければ後回し可

5. **フェーズ5: アーキテクチャの改善**（必要に応じて）
   - 大規模な変更のため、必要性を見極めて実施

### 段階的実施の例

#### ステップ1: 安全な改善（フェーズ1-2）
- コーディング規約統一
- 共通コンポーネントの作成と適用

#### ステップ2: 構造改善（フェーズ3）
- インターフェース導入
- イベントシステム導入
- シングルトン依存の削減

#### ステップ3: 最適化・拡張（フェーズ4-5）
- パフォーマンス最適化
- より高度なアーキテクチャへの移行

## リスク管理

### リスクと対策

1. **動作不良のリスク**
   - 対策: 各フェーズ後に動作確認
   - 対策: Gitでこまめにコミット

2. **スコープクリープ**
   - 対策: 各フェーズの目標を明確に
   - 対策: 完璧を求めすぎない

3. **時間超過**
   - 対策: フェーズ1-2を優先
   - 対策: フェーズ4-5は必要に応じて実施

### テスト戦略

各フェーズ後に以下を確認：
- プレイヤーの移動・攻撃
- 敵のスポーン・移動・攻撃
- ダメージ処理
- ゲームオーバー・クリア判定

## 期待される効果

### 短期的効果（フェーズ1-2）
- コードの可読性向上
- 保守性の向上
- バグ修正の容易化

### 中期的効果（フェーズ3）
- テスト容易性の向上
- 拡張性の向上
- チーム開発の効率化

### 長期的効果（フェーズ4-5）
- パフォーマンスの向上
- 新機能追加の容易化
- コードベースの持続可能性

## まとめ

このリファクタリング計画は、段階的に実施することで、リスクを最小限に抑えながら、コードベースの品質を向上させることを目指しています。

まずはフェーズ1-2を完了させ、その後の必要性を評価しながら進めることを推奨します。
