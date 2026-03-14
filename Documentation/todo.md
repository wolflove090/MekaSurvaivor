# 武器レベル段階調整 ToDo

## Phase 1: 固定性能テーブルへ置き換える
- [x] `Assets/Survaivor/Character/Combat/Application/BulletWeapon.cs` の発射間隔をレベル固定値 `1.5 / 1.0 / 0.8 / 0.4 / 0.2` に変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/ThrowingWeapon.cs` の発射間隔をレベル固定値 `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/DamageFieldWeapon.cs` のサイズをレベル固定値 `3.0 / 3.5 / 4.0 / 4.5 / 5.0` に変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs` の発射間隔をレベル固定値 `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs` の投擲間隔をレベル固定値 `1.5 / 1.0 / 1.0 / 1.0 / 1.0` に変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs` の攻撃間隔をレベル固定値 `2.0 / 1.5 / 1.0 / 0.8 / 0.4` に変更する。
- [x] 各武器のコンストラクタ初期値がレベル1仕様と一致することを確認する。
- [x] 各武器の `LevelUp()` が「現在レベルから固定値を引く」処理になっており、割合短縮や累積加算を使わないことを確認する。
- [x] レベル5超過時に配列外参照せず、レベル5の値を維持する clamp を入れる。

## Phase 2: 武器ごとの特殊挙動を反映する
- [x] `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs` の発射方向を真下へ変更する。
- [x] `Assets/Survaivor/Character/Combat/Application/BoundBallWeapon.cs` にレベル別バウンド回数 `1 / 1 / 2 / 2 / 3` を反映する。
- [x] `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs` にレベル別投射物数 `1 / 1 / 2 / 3 / 4` を反映する。
- [x] `Assets/Survaivor/Character/Combat/Application/FlameBottleWeapon.cs` で扇状拡散の角度計算を追加する。
- [x] 火炎瓶の扇状拡散がプレイヤー前方中心の左右対称になるようにする。

## Phase 3: ドローン2機化へ対応する
- [x] `Assets/Survaivor/Character/Combat/Application/DroneSpawnRequest.cs` に位相オフセットまたは同等の識別情報を追加する。
- [x] `Assets/Survaivor/Character/Combat/Application/DroneWeapon.cs` がレベル5で2回 `DeployDrone()` を呼ぶようにする。
- [x] 2機ドローンに 0 度 / 180 度の位相を割り当てる。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/WeaponEffectExecutor.cs` の単一 `_activeDroneController` 前提を複数管理へ変更する。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/WeaponEffectExecutor.cs` のドローンプール数を2以上へ見直す。
- [x] `Assets/Survaivor/Character/Combat/Infrastructure/Drones/DroneController.cs` が初期位相を受け取り、同一半径で周回できるようにする。
- [x] ドローン関連の複雑な位相管理と再利用処理に、意図が分かる簡潔なコメントを追加する。
- [x] レベル5の2機が円周上の正反対に配置され、重ならないことを確認する。

## Phase 4: テストを追加・更新する
- [x] 武器レベル固定値を検証する EditMode テストを追加する。
- [x] `BulletWeapon` のレベル1-5発射間隔テストを追加する。
- [x] `ThrowingWeapon` のレベル1-5発射間隔テストを追加する。
- [x] `DamageFieldWeapon` のレベル1-5サイズテストを追加する。
- [x] `BoundBallWeapon` のレベル別発射間隔・バウンド回数・方向テストを追加する。
- [x] `FlameBottleWeapon` のレベル別投擲間隔・投射物数テストを追加する。
- [x] `DroneWeapon` のレベル別攻撃間隔テストを追加する。
- [x] `DroneWeapon` のレベル5で2機要求になることを検証するテストを追加する。
- [x] `PlayerController.TrySetWeaponLevel()` または `WeaponService.RebuildWeapons()` 経由でも固定値が崩れないことを検証する。

## Phase 5: 動作確認
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] 実機確認または PlayMode で、各武器がレベル1から5で指定どおりに変化することを確認する。
- [ ] レベル5ドローンで2機が正反対を周回することを確認する。
- [ ] レベル3-5火炎瓶で扇状拡散の本数が 2 / 3 / 4 になることを確認する。
- [ ] `u console get -l E` で関連エラーが出ていないことを確認する。
