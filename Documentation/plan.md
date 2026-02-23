# スタイル効果機能 実装計画

## 実装方針
- スタイル効果は `switch` で分岐せず、スタイルごとの効果オブジェクトへ責務分離する。
- `StyleChangeUiController` は「どのスタイルを選んだか」を通知するだけに留め、効果の詳細ロジックは持たない。
- `PlayerController` は現在のスタイル効果オブジェクトを1つ保持し、スタイル変更時に旧オブジェクトを解除して新オブジェクトへ差し替える。
- SPD倍率はプレイヤー移動速度反映時（`PlayerController`）に適用する。
- 経験値倍率は `PlayerExperience.AddExperience` 内で適用する。

## 変更対象ファイル一覧
- `Assets/Scripts/UI/StyleChangeUiController.cs`
  - スタイル選択成功時に `PlayerController.ChangeStyle(styleType)` を呼び出す。
  - カードインデックスから `PlayerStyleType` へ変換する。
- `Assets/Scripts/Character/Player/PlayerController.cs`
  - 現在のスタイル効果オブジェクト参照を保持する。
  - スタイル変更API（差し替え処理）を追加する。
  - 差し替え時に `OnExit -> 参照置換 -> OnEnter` を実行する。
  - スタイル効果オブジェクトへ `Tick(deltaTime)` を委譲する。
- `Assets/Scripts/Character/Player/PlayerExperience.cs`
  - 経験値倍率フィールドと設定APIを追加する。
  - `AddExperience` で倍率適用後の値を加算する。
- `Assets/Scripts/Character/Player/PlayerStyleType.cs`（新規）
  - `Miko` / `Idol` / `Celeb` を定義する。
- `Assets/Scripts/Character/Player/StyleEffects/IPlayerStyleEffect.cs`（新規）
  - 効果オブジェクトの共通契約を定義する（`OnEnter`, `OnExit`, `Tick`）。
- `Assets/Scripts/Character/Player/StyleEffects/MikoStyleEffect.cs`（新規）
  - 30秒ごとHP1回復を実装する。
- `Assets/Scripts/Character/Player/StyleEffects/IdolStyleEffect.cs`（新規）
  - `PlayerController` の移動速度倍率を1.2に設定/解除する。
- `Assets/Scripts/Character/Player/StyleEffects/CelebStyleEffect.cs`（新規）
  - `PlayerExperience` の経験値倍率を2.0に設定/解除する。
- `Assets/Scripts/Character/Player/StyleEffects/PlayerStyleEffectFactory.cs`（新規）
  - `PlayerStyleType` から対応する効果オブジェクトを生成する。

## データフロー / 処理フロー
1. プレイヤーがスタイルカードを選択し、既存のステータス/画像適用が成功する。
2. `StyleChangeUiController` が `PlayerController.ChangeStyle(selectedStyle)` を呼ぶ。
3. `PlayerController` が現在効果を保持していれば `OnExit(context)` を呼ぶ。
4. `PlayerStyleEffectFactory` で新効果オブジェクトを生成し、`_activeStyleEffect` に差し替える。
5. 新効果の `OnEnter(context)` を呼ぶ。
6. 毎フレーム `PlayerController.Update` で `_activeStyleEffect.Tick(context, Time.deltaTime)` を呼ぶ。
7. スタイル再変更時は同じ手順で旧効果が解除され、新効果へ置換される。

## 実装詳細
### 1. 効果オブジェクト共通契約
- `IPlayerStyleEffect` は以下を持つ。
  - `PlayerStyleType StyleType { get; }`
  - `void OnEnter(PlayerStyleEffectContext context)`
  - `void OnExit(PlayerStyleEffectContext context)`
  - `void Tick(PlayerStyleEffectContext context, float deltaTime)`
- `Tick` が不要な効果（アイドル/セレブ）は空実装でよい。

### 2. Context設計
- 効果オブジェクトへ渡す依存参照を `PlayerStyleEffectContext` に集約する。
- 例: `HealthComponent`, `PlayerExperience`, `PlayerController`。
- 効果オブジェクトは `MonoBehaviour` 参照探索を持たず、contextのみを使う。

### 3. 各スタイル効果クラス
- `MikoStyleEffect`
  - 内部に `_timer` を保持し、`Tick` で加算。
  - 30秒到達ごとに `Heal(1)` を実行し、`_timer -= 30f` で繰り越し処理。
  - `OnExit` で `_timer = 0f`。
- `IdolStyleEffect`
  - `OnEnter` で `PlayerController.SetMoveSpeedMultiplier(1.2f)`。
  - `OnExit` で `SetMoveSpeedMultiplier(1f)`。
- `CelebStyleEffect`
  - `OnEnter` で `PlayerExperience.SetExperienceMultiplier(2f)`。
  - `OnExit` で `ResetExperienceMultiplier()`。

### 4. Factory + 差し替え制御
- `PlayerStyleEffectFactory.Create(styleType)` で具象クラスを生成する。
- `PlayerController.ChangeStyle` は以下順序を固定。
  - `_activeStyleEffect?.OnExit(context)`
  - `_activeStyleEffect = factory.Create(styleType)`
  - `_activeStyleEffect.OnEnter(context)`
- 同一スタイル再選択時も再生成するかは仕様で固定する。
  - 本計画では「再生成して再適用（初期化）」とする。

## リスクと対策
- リスク: 効果オブジェクト差し替え時に `OnExit` 呼び忘れで旧効果が残る。
  - 対策: `ChangeStyle` に処理順を集約し、他経路から直接差し替えない。
- リスク: context参照が未解決で `NullReferenceException`。
  - 対策: `PlayerController` 初期化時にcontext構築し、未解決時は警告ログを出して安全に無効動作。
- リスク: 同一スタイル再適用時にタイマー・倍率状態が意図せず継続。
  - 対策: 再生成方針で毎回初期化し、状態持ち越しを防ぐ。
- リスク: オブジェクト生成コスト増。
  - 対策: スタイル変更頻度は低いため許容。必要時は将来プール化を検討。

## 検証方針
- 巫女選択後、30秒ごとにHPが1回復する。
- 巫女から他スタイルへ変更後、回復が停止する。
- アイドル選択中のみ移動速度が1.2倍になる。
- セレブ選択中のみ経験値獲得量が2倍になる。
- スタイルを連続で切り替えても、前スタイル効果が残存しない。
- 同一スタイルを再選択したとき、効果状態が再初期化される。

## コードスニペット（主要変更イメージ）
```csharp
public interface IPlayerStyleEffect
{
    PlayerStyleType StyleType { get; }
    void OnEnter(PlayerStyleEffectContext context);
    void OnExit(PlayerStyleEffectContext context);
    void Tick(PlayerStyleEffectContext context, float deltaTime);
}

public void ChangeStyle(PlayerStyleType styleType)
{
    _activeStyleEffect?.OnExit(_styleEffectContext);
    _activeStyleEffect = _styleEffectFactory.Create(styleType);
    _activeStyleEffect.OnEnter(_styleEffectContext);
}

void Update()
{
    _activeStyleEffect?.Tick(_styleEffectContext, Time.deltaTime);
}
```
