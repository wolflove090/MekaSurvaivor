# 回復アイテム生成機能 ToDo

## Phase 1: 破壊可能オブジェクトと回復アイテムの土台
- [x] `Assets/Scripts/Item/BreakableObject.cs` を追加し、`IDamageable` 実装（HP1固定、破壊時コールバック）を実装
- [x] `Assets/Scripts/Item/HealPickup.cs` を追加し、経験値オーブ同等の追従/取得ロジックを実装
- [x] `Assets/Scripts/Item/HealPickupSpawner.cs` を追加し、回復アイテムのプール管理と生成APIを実装
- [x] 回復アイテム取得時に `HealthComponent.Heal(3)` を呼び出して回復する処理を接続

## Phase 2: オブジェクト生成管理
- [x] `Assets/Scripts/Item/BreakableObjectSpawner.cs` を追加し、30秒ごとの生成判定を実装
- [x] 同時存在上限3個の管理（生存リスト + nullクリーンアップ）を実装
- [x] カメラ画面外かつ外縁10px条件での生成位置算出ロジックを実装
- [x] 10pxオフセット方向をXZ平面ランダムにする補正ロジックを実装

## Phase 3: オートロック優先度と攻撃判定の接続
- [x] `Assets/Scripts/Wepon/BulletWeapon.cs` を更新し、ターゲット探索を「敵優先 -> オブジェクト」に変更
- [x] `Assets/Scripts/Wepon/ProjectileController.cs` を更新し、破壊可能オブジェクトへのダメージ適用を追加
- [ ] （必要に応じて）タグ/レイヤー/Prefab設定を追加し、オブジェクトが攻撃対象として認識される状態にする

## Phase 4: 動作確認
- [ ] 30秒ごとに生成判定が走ることを確認
- [ ] オブジェクトが3個存在時に4個目が生成されないことを確認
- [ ] 生成位置がカメラ外かつ外縁10px付近であることを確認
- [ ] オブジェクト破壊時に回復アイテムが1つ生成されることを確認
- [ ] 回復アイテム取得でHPが3回復し、最大HPを超えないことを確認
- [ ] 敵がいるときは敵を優先し、敵がいないときのみオブジェクトを自動照準することを確認
