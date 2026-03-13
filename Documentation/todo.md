# 海賊スタイル召喚能力 ToDo

## Phase 1: スタイル効果の基盤を拡張する
- [x] `Assets/Survaivor/Character/Player/Application/StyleEffects/IPlayerStyleEffect.cs` に終了処理フックを追加する。
- [x] `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs` で旧スタイルのクリーンアップを呼ぶ。
- [x] `Assets/Survaivor/Character/Player/Application/StyleEffects/PlayerStyleEffectContext.cs` にプレイヤー Transform、向き、`EnemyRegistry`、戦闘員管理参照を追加する。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs` で戦闘員管理コンポーネントを初期化し、コンテキストへ注入する。

## Phase 2: 戦闘員管理とプレハブを追加する
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewSummonController.cs` を作成し、プール初期化と全返却を実装する。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewRegistry.cs` を作成し、アクティブ戦闘員一覧を管理する。
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewTarget.cs` を作成し、敵が識別できるマーカーを追加する。
- [ ] `Assets/Survaivor/Character/Player/Prefabs/PirateCrewMember.prefab` を追加する。
- [x] `Assets/GameResources/Player/PirateCrewMemberStatsData.asset` を追加し、HP30 を設定する。
- [x] 生成失敗時に警告ログだけ出して継続する処理を入れる。

## Phase 3: 戦闘員の挙動を実装する
- [x] `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewMemberController.cs` を作成する。
- [x] 最寄り敵追跡と敵不在時の前方直進を XZ 平面で実装する。
- [x] 戦闘員死亡時のレジストリ解除とプール返却を実装する。
- [x] 戦闘員が攻撃処理やダメージ生成を持たないことを確認する。

## Phase 4: 海賊スタイル効果を実装する
- [x] `Assets/Survaivor/Character/Player/Application/StyleEffects/PirateStyleEffect.cs` に 10 秒タイマーを実装する。
- [x] 1 回あたり 2 体召喚する処理を追加する。
- [x] `Time.timeScale == 0` の間はタイマーを停止する。
- [x] 再召喚時に前回分を返却する。
- [x] スタイル解除時に戦闘員を全破棄する。

## Phase 5: 敵ターゲット優先を実装する
- [x] `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyController.cs` に優先ターゲット解決処理を追加する。
- [x] 戦闘員がいる間はプレイヤーより戦闘員を優先する。
- [x] 戦闘員消滅後はプレイヤー追跡へ戻る。
- [x] 既存敵と新規スポーン敵で挙動差が出ないことを確認する。

## Phase 6: テストを追加する
- [x] 海賊スタイルの 10 秒召喚と停止条件の EditMode テストを追加する。
- [x] 戦闘員再召喚時の全返却とプール再利用テストを追加する。
- [x] 戦闘員の移動フォールバックのテストを追加する。
- [x] 敵ターゲット優先切替のテストを追加する。
- [x] スタイル解除時の全破棄テストを追加する。

## Phase 7: 動作確認を行う
- [ ] `u tests run edit` で EditMode テストを実行する。
- [ ] PlayMode で 10 秒ごとの 2 体召喚を確認する。
- [ ] PlayMode で敵がおとりへ向かうことを確認する。
- [ ] PlayMode でスタイル解除時に戦闘員が消えることを確認する。
- [ ] `u console get -l E` で関連エラーがないことを確認する。
