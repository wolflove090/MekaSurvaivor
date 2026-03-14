# メイドスタイル移動速度強化 実装計画

## 実装方針
- 既存のスタイル効果基盤をそのまま利用し、メイド固有効果は `MaidStyleEffect` の `ApplyParameters()` で `PlayerState.MoveSpeedMultiplier` を `1.5f` に設定して実現する。
- スタイル切り替え時の排他制御は、既存の `PlayerProgressionService.ChangeStyle()` にある `ResetStyleParameters() -> SetCurrentStyle() -> ApplyParameters()` の順序を維持し、特殊分岐は追加しない。
- 実効移動速度の反映は、既存の `PlayerController.ApplyMoveSpeedFromStats()` に委ねる。メイド専用の UI 層ロジックや追加イベントは作らない。
- 実装方針は `IdolStyleEffect` の移動速度倍率付与と揃え、メイドだけ例外的な経路を増やさない。
- 検証は EditMode テストを中心に行い、メイド選択時、他スタイルからの切り替え時、他スタイルへの切り替え時の 3 系統を明示的に確認する。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Player/Application/StyleEffects/MaidStyleEffect.cs`
  - 定数 `1.5f` を持たせ、`ApplyParameters()` で `context?.PlayerState?.SetMoveSpeedMultiplier(1.5f)` を設定する。
  - `Tick()` は現状どおり空実装を維持し、継続処理を追加しない。
- `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs`
  - 既存の「メイド切り替え時に前スタイル倍率が消える」テストの期待値を `1f` から `1.5f` へ更新する。
  - メイドから別スタイルへ変更した際に `MoveSpeedMultiplier` が変更先の期待値へ切り替わるテストを追加する。
  - メイド再適用や連続切り替えでも倍率が累積しないテストを追加する。

### 参照確認のみ
- `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs`
  - 変更時の倍率リセット順序と既存責務を維持する前提を確認する。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - `ApplyMoveSpeedFromStats()` が `CharacterStats.CurrentValues.Spd * PlayerState.MoveSpeedMultiplier` を反映していることを前提に再利用する。
- `Assets/Survaivor/Character/Player/Domain/PlayerState.cs`
  - `MoveSpeedMultiplier` の初期値、設定、リセット API を既存のまま利用する。
- `Assets/Survaivor/Character/Player/Application/StyleEffects/IdolStyleEffect.cs`
  - 移動速度倍率付与の実装パターンを揃える際の参照元とする。

## データフロー / 処理フロー

### 1. メイドスタイル適用
1. `PlayerController.ChangeStyle(PlayerStyleType.Maid)` が呼ばれる。
2. `PlayerExperience.ChangeStyle()` 経由で `PlayerProgressionService.ChangeStyle()` が呼ばれる。
3. `PlayerProgressionService.ResetStyleParameters()` が `PlayerState.MoveSpeedMultiplier` と `ExperienceMultiplier` を基準値へ戻す。
4. `PlayerProgressionService` が `PlayerStyleEffectFactory.Create(PlayerStyleType.Maid)` で `MaidStyleEffect` を生成し、`ApplyParameters()` を実行する。
5. `MaidStyleEffect.ApplyParameters()` が `PlayerState.MoveSpeedMultiplier = 1.5f` を設定する。
6. `PlayerController.ChangeStyle()` の末尾で `ApplyMoveSpeedFromStats()` が呼ばれ、`CharacterStats.CurrentValues.Spd * 1.5f` が `PlayerMovement.MoveSpeed` に反映される。

### 2. ステータス再反映時
1. `CharacterStats.OnStatsDataChanged` が発火する。
2. `PlayerController.OnStatsDataChanged()` から `ApplyMoveSpeedFromStats()` が呼ばれる。
3. 現在の `PlayerState.MoveSpeedMultiplier` がそのまま乗算されるため、メイド選択中は再計算後も `1.5f` が維持される。

### 3. メイドから別スタイルへ切り替え
1. `PlayerProgressionService.ChangeStyle()` 冒頭で倍率を基準値へリセットする。
2. 変更先スタイルの `ApplyParameters()` が必要な倍率だけを再設定する。
3. `PlayerController.ApplyMoveSpeedFromStats()` が最新の `MoveSpeedMultiplier` を用いて移動速度を再反映する。
4. その結果、メイド由来の `1.5f` は残存せず、変更先が速度倍率を持たない場合は `1f`、アイドルなら `1.2f` になる。

## 実装ステップ

### Phase 1: メイド効果の実装
- `MaidStyleEffect` に移動速度倍率定数を追加する。
- `ApplyParameters()` で `PlayerState.SetMoveSpeedMultiplier(1.5f)` を呼ぶ。
- 既存のドキュメントコメントは維持し、内容を実装に合わせて更新する。

### Phase 2: テスト更新
- `ChangeStyle_SwitchingToMaid_ClearsPreviousStyleMultipliers()` の期待値を、経験値倍率 `1f` 維持と移動速度倍率 `1.5f` 反映に更新する。
- メイドからアイドルなど速度倍率を持つ別スタイルへ切り替えた際、メイド倍率が残らないことを検証するテストを追加する。
- `Celeb -> Maid -> Maid` のような連続切り替え、または `Maid -> Maid` の再適用で `1.5f` を超えて累積しないことを検証する。

## リスクと対策
- リスク: `MaidStyleEffect` のみを更新して `PlayerController` 側の再反映呼び出しを見落とすと、実装意図と計画書の責務分担がずれる。
  - 対策: `ChangeStyle()` と `OnStatsDataChanged()` の既存フローを維持する前提を明記し、コード変更対象を最小化する。
- リスク: 既存テストが「メイドは空効果」という前提で書かれているため、期待値更新を漏らすと失敗する。
  - 対策: 既存メイドテストを先に更新し、メイド関連ケースをまとめて見直す。
- リスク: 連続切り替え時に倍率が累積する不具合を見落とすと、要件の受け入れ条件を満たせない。
  - 対策: 同一スタイル再適用と別スタイル往復の両方を EditMode テストに含める。
- リスク: UI や表示文言が未更新でも誤って本対応に含めるとスコープが広がる。
  - 対策: 今回の変更対象をゲームロジックと EditMode テストに限定し、UI 同期は別タスクとして扱う。

## 検証方針
- EditMode テスト
  - メイド選択直後に `PlayerState.MoveSpeedMultiplier == 1.5f` になること。
  - `Celeb -> Maid` 切り替え後に `ExperienceMultiplier == 1f` かつ `MoveSpeedMultiplier == 1.5f` になること。
  - `Maid -> Idol` 切り替え後に `MoveSpeedMultiplier == 1.2f` となり、メイド由来の `1.5f` が残らないこと。
  - `Maid -> Celeb` 切り替え後に `MoveSpeedMultiplier == 1f` かつ `ExperienceMultiplier == 2f` になること。
  - `Maid -> Maid` または連続切り替えを行っても `MoveSpeedMultiplier` が `1.5f` を超えて累積しないこと。
- 手動確認
  - 必要に応じて Unity 上でメイド選択中の移動速度変化を確認する。
- Unity 確認
  - `u tests run edit` で関連 EditMode テストが通ること。
  - `u console get -l E` で関連エラーが出ていないこと。

## コードスニペット（主要変更イメージ）
```csharp
public class MaidStyleEffect : IPlayerStyleEffect
{
    const float MOVE_SPEED_MULTIPLIER = 1.5f;

    public PlayerStyleType StyleType => PlayerStyleType.Maid;

    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        context?.PlayerState?.SetMoveSpeedMultiplier(MOVE_SPEED_MULTIPLIER);
    }

    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
    }
}
```

```csharp
[Test]
public void ChangeStyle_SwitchingToMaid_AppliesMaidMoveSpeedMultiplier()
{
    PlayerState state = new PlayerState(1);
    PlayerProgressionService service = new PlayerProgressionService(state, 10, 1.5f);
    PlayerStyleEffectContext context = new PlayerStyleEffectContext(null, state);

    service.ChangeStyle(PlayerStyleType.Celeb, context);
    service.ChangeStyle(PlayerStyleType.Maid, context);

    Assert.That(state.CurrentStyleType, Is.EqualTo(PlayerStyleType.Maid));
    Assert.That(state.MoveSpeedMultiplier, Is.EqualTo(1.5f));
    Assert.That(state.ExperienceMultiplier, Is.EqualTo(1f));
}
```
