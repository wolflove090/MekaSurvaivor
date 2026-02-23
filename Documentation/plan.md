# 回復アイテム生成機能 実装計画

## 実装方針
- 破壊可能オブジェクト、回復アイテム、生成管理をそれぞれ独立コンポーネントとして追加し、責務を分離する。
- 回復アイテムの追従/取得ロジックは `ExperienceOrb` と同じ形式（接近で追従、接触距離で取得）で実装する。
- 既存の `ObjectPool<T>` / `PooledObject` を利用し、破壊可能オブジェクトと回復アイテムの生成/回収をプール運用に統一する。
- オートロック優先度は `BulletWeapon` のターゲット探索順を変更し、敵検索がヒットしない場合のみオブジェクト検索へフォールバックする。
- 攻撃ヒット判定は `ProjectileController` の対象判定を拡張し、エネミーに加えて破壊可能オブジェクトにも `IDamageable` 経由でダメージを通す。
- オブジェクトのスポーン位置はカメラ外縁10px外側を基準にし、10pxオフセット方向をXZ平面ランダムで付与する。

## 変更対象ファイル一覧
- `Assets/Scripts/Item/BreakableObject.cs`（新規）
  - `IDamageable` 実装、HP1固定、死亡時に回復アイテム生成、スポーナーへの登録解除通知
- `Assets/Scripts/Item/BreakableObjectSpawner.cs`（新規）
  - 30秒インターバル、同時存在3個制限、カメラ外縁10px条件でのスポーン位置算出、検索API提供
- `Assets/Scripts/Item/HealPickup.cs`（新規）
  - 経験値オーブ同等の追従/取得挙動、取得時 `HealthComponent.Heal(3)` 呼び出し
- `Assets/Scripts/Item/HealPickupSpawner.cs`（新規）
  - 回復アイテムのプール管理とスポーン窓口
- `Assets/Scripts/Wepon/BulletWeapon.cs`
  - ターゲット探索順を「敵優先、見つからなければ破壊可能オブジェクト」に変更
- `Assets/Scripts/Wepon/ProjectileController.cs`
  - 衝突対象判定を拡張し、破壊可能オブジェクトにもダメージ適用
- `Assets/Scripts/Character/Enemy/EnemySpawner.cs`
  - 既存の敵検索APIは維持（優先度制御の第一段として継続利用）
- `Assets/Scripts/Item/Prefabs/*.prefab`（新規想定）
  - 破壊可能オブジェクト用プレハブ、回復アイテム用プレハブ、必要なCollider/Tag設定

## データフロー / 処理フロー
1. `BreakableObjectSpawner` が30秒ごとに生成判定を実行する。
2. 現在の生存オブジェクト数が3未満の場合のみ、カメラ外縁10px外側の候補位置を計算してスポーンする。
3. プレイヤー武器（弾）が衝突すると `ProjectileController` が `IDamageable` へダメージを適用する。
4. `BreakableObject` のHPが0以下になると破壊処理を実行し、`HealPickupSpawner` に生成要求を送る。
5. `HealPickup` は待機し、プレイヤーが近づくと追従を開始、取得距離で回収される。
6. 回収時にプレイヤーの `HealthComponent.Heal(3)` を実行し、自身をプールへ返却する。
7. `BulletWeapon` は発射ごとに `EnemySpawner.FindNearestEnemy(..., onlyVisible: true)` を先に呼び、未検出時のみ `BreakableObjectSpawner.FindNearestBreakableObject(..., onlyVisible: true)` を呼ぶ。

## カメラ外縁10px位置の算出方針
- `Camera.main` とプレイヤー高度（XZ平面）を基準に、スクリーン境界外の点をサンプリングする。
- 画面端のランダム辺を選択し、画面外へ10pxオフセットしたスクリーン座標を作る。
- さらにXZ平面ランダム方向ベクトルで「10px相当のワールド距離」分だけ補正する。
- `Camera.ScreenPointToRay` と生成平面（Y固定Plane）の交点をワールド生成位置として採用する。
- 有効候補を一定回数試行して見つからない場合は、その周期の生成をスキップする。

## リスクと対策
- リスク: カメラ投影方式（Perspective/Orthographic）差で10pxのワールド換算誤差が出る。
  - 対策: 生成平面上で `ScreenPointToRay` 交点を用いて都度換算し、固定値のワールド距離を直接使わない。
- リスク: タグ依存の衝突判定が崩れると既存の敵ダメージ処理に影響。
  - 対策: `IDamageable` とタグ条件を併用し、敵処理の既存分岐を後方互換で残す。
- リスク: 破壊済みオブジェクト参照が残り、最大数制御が不正になる。
  - 対策: スポーナー側で `null` クリーンアップを毎周期実行し、破壊時に明示的な解除APIも呼ぶ。
- リスク: 回復アイテムがプレイヤー未取得で増殖し続ける。
  - 対策: 回復アイテム側に寿命タイマー（必要なら）を持たせる余地を残し、初期実装はプール返却経路を監視ログで確認する。

## 検証方針
- 30秒経過ごとにオブジェクト生成判定が行われることをPlay Modeで確認する。
- オブジェクトが3個存在する状態で、4個目が生成されないことを確認する。
- 生成位置がカメラ画面外であり、外縁近傍（約10px）に出現することを確認する。
- 弾でオブジェクトを攻撃し、1ヒットで破壊され回復アイテムが1つ生成されることを確認する。
- 回復アイテム取得でHPが3回復し、`MaxHp` を超えないことを確認する。
- 敵とオブジェクトが同時に可視範囲にいる状況で、弾が敵を優先して狙うことを確認する。
- 敵不在時にのみオブジェクトへ自動照準することを確認する。

## コードスニペット（主要変更イメージ）
```csharp
protected override void Fire()
{
    Vector3 shootPosition = transform.position + _shootOffset;

    GameObject target = EnemySpawner.Instance != null
        ? EnemySpawner.Instance.FindNearestEnemy(shootPosition, onlyVisible: true)
        : null;

    if (target == null && BreakableObjectSpawner.Instance != null)
    {
        target = BreakableObjectSpawner.Instance.FindNearestBreakableObject(shootPosition, onlyVisible: true);
    }

    if (target == null)
    {
        return;
    }

    Vector3 direction = (target.transform.position - shootPosition).normalized;
    direction.y = 0f;
    SpawnBullet(shootPosition, direction);
}

Vector3 CalculateOffscreenSpawnPosition(Camera camera)
{
    Vector2 edgePoint = PickRandomOffscreenEdgePoint(camera.pixelWidth, camera.pixelHeight, 10f);
    Vector3 worldPoint = ScreenPointToGround(camera, edgePoint, _spawnPlaneY);

    Vector2 randomDir2D = Random.insideUnitCircle.normalized;
    Vector3 randomDir = new Vector3(randomDir2D.x, 0f, randomDir2D.y);
    float worldOffset = ConvertPixelToWorldDistance(camera, 10f, _spawnPlaneY);

    return worldPoint + randomDir * worldOffset;
}
```
