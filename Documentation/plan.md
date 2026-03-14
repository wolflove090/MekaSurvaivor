# ナース回復アイテム強化 実装計画

## 実装方針
- 回復アイテム倍率はプレイヤーのランタイム状態として `PlayerState` に保持する。既存の経験値倍率・移動速度倍率と同じ責務に揃え、スタイル効果の適用先を一元化する。
- ナース固有効果の付与と解除は既存の `PlayerController.ChangeStyle` -> `PlayerExperience.ChangeStyle` -> `PlayerProgressionService.ChangeStyle` フローに追従する。`NurseStyleEffect` は `ApplyParameters` で倍率を設定し、他スタイルへ切り替わる際は `ResetStyleParameters` で基準値 `1.0` に戻す。
- `HealPickup` はスタイル種別を判定せず、取得時点のプレイヤー側倍率のみ参照して回復量を計算する。これにより、生成済みアイテムでも取得時点のスタイルが反映される。
- 巫女の定期回復や `HealthComponent.Heal(int)` を直接呼ぶ他経路には影響を広げない。倍率適用は `HealPickup.CollectPickup` の中だけに限定する。
- 要件書本文は `Mathf.RoundToInt` を前提としているため、計画書もそれに合わせる。ただし要件書末尾に「切り捨てにして」のメモが残っているため、実装着手前に確定が必要な確認事項として扱う。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Player/Domain/PlayerState.cs`
  - 回復アイテム倍率プロパティを追加する。
  - 設定メソッドと初期値復帰メソッドを追加し、既存の倍率管理 API と並べる。
- `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs`
  - `ResetStyleParameters()` に回復アイテム倍率の初期化を追加する。
  - スタイル切替時に既存倍率が残らない前提をナース効果まで拡張する。
- `Assets/Survaivor/Character/Player/Application/StyleEffects/NurseStyleEffect.cs`
  - 空実装をやめ、`ApplyParameters` で回復アイテム倍率 `1.5f` を設定する。
  - `Tick` は引き続き noop のまま維持する。
- `Assets/Survaivor/Items/Infrastructure/HealPickup/HealPickup.cs`
  - `HealthComponent` だけでなく `PlayerExperience` または `PlayerState` へ到達できる参照も取得する。
  - 取得時に基礎回復量 `3` と倍率から最終回復量を算出し、`HealthComponent.Heal()` へ渡す。
  - スタイル判定は追加せず、倍率未取得時は `1.0` フォールバックにする。

### テスト追加・更新対象
- `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs`
  - ナース変更時に回復アイテム倍率が `1.5f` になることを検証する。
  - ナースから他スタイルへ切り替えた際に倍率が `1.0f` へ戻ることを検証する。
  - 既存の経験値倍率・移動速度倍率の回帰がないことを確認する。
- `Assets/Survaivor/Tests/EditMode/...` 配下の新規または既存テスト
  - `HealPickup` の取得時回復量を検証するテストを追加する。
  - ナース時は `3 * 1.5` が丸め後 `5` になることを確認する。
  - ナース以外では基礎値 `3` のままであることを確認する。
  - 最大HP超過時も `HealthComponent` 側の上限処理が維持されることを確認する。

### 変更不要想定
- `Assets/Survaivor/Character/Infrastructure/HealthComponent.cs`
  - 回復上限処理は既存実装で満たしているため変更しない。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - スタイル切替導線は既存のままで足りる想定。倍率保持先を `PlayerState` に寄せるため、変更は原則不要とする。
- `Assets/Survaivor/Character/Player/Application/StyleEffects/MikoStyleEffect.cs`
  - 巫女の定期回復は `HealthComponent.Heal(1)` を直接呼ぶ既存仕様を維持し、倍率適用対象外とする。

## データフロー / 処理フロー

### 1. スタイル変更時
1. `PlayerController.ChangeStyle(PlayerStyleType.Nurse)` が呼ばれる。
2. `PlayerExperience.ChangeStyle()` から `PlayerProgressionService.ChangeStyle()` へ到達する。
3. `PlayerProgressionService.ResetStyleParameters()` が先に走り、移動速度倍率・経験値倍率・回復アイテム倍率を基準値へ戻す。
4. `State.SetCurrentStyle(styleType)` 実行後、`PlayerStyleEffectFactory` が `NurseStyleEffect` を生成する。
5. `NurseStyleEffect.ApplyParameters()` が `PlayerState` の回復アイテム倍率を `1.5f` に設定する。

### 2. 回復アイテム取得時
1. `HealPickup.OnEnable()` でプレイヤー参照を取得する。
2. プレイヤー接触時に `CollectPickup()` が呼ばれる。
3. `HealPickup` が基礎回復量 `_healAmount` と、プレイヤーが持つ現在倍率を取得する。
4. 最終回復量を `Mathf.RoundToInt(_healAmount * multiplier)` で算出する。
5. 算出値を `HealthComponent.Heal(finalAmount)` に渡し、上限処理は `HealthComponent` へ委譲する。
6. アイテムは既存どおりプール返却または破棄される。

### 3. ナース解除時
1. ナース以外のスタイルへ変更すると、再度 `PlayerProgressionService.ChangeStyle()` が呼ばれる。
2. `ResetStyleParameters()` により回復アイテム倍率が `1.0f` へ戻る。
3. 新スタイルが回復アイテム倍率を設定しない限り、以後の `HealPickup` は基礎回復量 `3` を使用する。

## 実装ステップ

### Phase 1: 状態モデル拡張
- `PlayerState` に回復アイテム倍率プロパティと setter/resetter を追加する。
- 初期値は `1.0f` とし、既存コンストラクタで初期化する。
- `PlayerProgressionService.ResetStyleParameters()` から確実に初期値へ戻せるようにする。

### Phase 2: ナース効果実装
- `NurseStyleEffect.ApplyParameters()` で `PlayerState` に倍率 `1.5f` を設定する。
- `context` や `PlayerState` が未解決の場合は安全に何もしない。
- 同一スタイル再適用時も加算ではなく代入にすることで、倍率の重複適用を防ぐ。

### Phase 3: 回復アイテム適用
- `HealPickup` にプレイヤーの進行状態へアクセスする経路を追加する。
- 取得時点の倍率参照で最終回復量を算出する実装へ変更する。
- プレイヤー参照の一部が取得できない場合は `1.0f` を使い、既存挙動を壊さない。

### Phase 4: テスト整備
- `PlayerProgressionServiceTests` にナース倍率付与・解除のケースを追加する。
- `HealPickup` の回復量テストを追加し、丸め結果・最大HP上限・スタイル切替後の解除を検証する。
- 巫女の定期回復がナース倍率の影響を受けないことを回帰テストで担保する。

## リスクと対策
- リスク: `ResetStyleParameters()` に回復アイテム倍率の初期化を入れ忘れると、ナース解除後も倍率が残留する。
  - 対策: スタイル切替テストでナース -> 他スタイルの戻り値を明示的に検証する。
- リスク: `HealPickup` がスタイル種別や `PlayerController` に直接依存し始めると、責務が崩れて今後のスタイル追加で分岐が増える。
  - 対策: `HealPickup` では倍率値だけを読む実装に限定し、スタイル名ベースの条件分岐を禁止する。
- リスク: `OnEnable()` で `PlayerExperience` を取得できないケースがあると NullReference になりうる。
  - 対策: `HealthComponent` と同様に null 安全に扱い、倍率取得不可時は `1.0f` にフォールバックする。
- リスク: 端数処理方針が未確定のまま実装すると、要件書とテスト期待値が食い違う。
  - 対策: 計画段階では `Mathf.RoundToInt` を採用し、実装前に要件書の未確定事項を解消する。
- リスク: `HealPickup` テストで MonoBehaviour 初期化順に依存すると不安定になる。
  - 対策: EditMode テストでは最小構成の `GameObject` を組み立て、必要コンポーネントを明示的に追加して検証する。

## 検証方針
- EditMode テスト
  - `ChangeStyle(PlayerStyleType.Nurse)` 後に `PlayerState` の回復アイテム倍率が `1.5f` になること。
  - ナースから `Idol` や `Maid` に変更した後、回復アイテム倍率が `1.0f` に戻ること。
  - `HealPickup` 取得時に基礎回復量 `3` が通常時 `3`、ナース時 `5` になること。
  - HP が最大値付近でも回復後に `MaxHp` を超えないこと。
  - 巫女スタイルの定期回復がナース倍率を参照しないこと。
- 手動確認
  - ナース選択中に既存配置済みの回復アイテムを取得しても `5` 回復になること。
  - ナース解除後に新規・既存どちらの回復アイテムでも `3` 回復へ戻ること。
- Unity 確認
  - `u tests run edit` で関連 EditMode テストが通ること。
  - `u console get -l E` でスタイル切替および回復取得時のエラーが出ていないこと。

## コードスニペット（主要変更イメージ）
```csharp
public class PlayerState
{
    public float HealPickupMultiplier { get; private set; }

    public void SetHealPickupMultiplier(float multiplier)
    {
        HealPickupMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetHealPickupMultiplier()
    {
        HealPickupMultiplier = 1f;
    }
}
```

```csharp
public class NurseStyleEffect : IPlayerStyleEffect
{
    const float HEAL_PICKUP_MULTIPLIER = 1.5f;

    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        context?.PlayerState?.SetHealPickupMultiplier(HEAL_PICKUP_MULTIPLIER);
    }
}
```

```csharp
void CollectPickup()
{
    float multiplier = _playerState != null ? _playerState.HealPickupMultiplier : 1f;
    int finalHealAmount = Mathf.RoundToInt(_healAmount * multiplier);
    _playerHealthComponent?.Heal(finalHealAmount);
    ReturnToPoolOrDestroy();
}
```
