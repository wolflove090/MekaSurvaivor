# 海賊スタイル召喚能力 実装計画

## 実装方針
- `PirateStyleEffect` 自体は「10秒ごとの召喚要求を出す責務」に絞り、戦闘員の生成・管理は専用ランタイムコンポーネントへ分離する。既存の `MikoStyleEffect` と同様にタイマーはスタイル効果側で持つが、召喚実体を直接生成しない。
- 現状の `PlayerStyleEffectContext` は `HealthComponent` と `PlayerState` しか持たず、召喚や敵探索に必要な参照を渡せない。プレイヤー Transform、`EnemyRegistry`、戦闘員管理サービスをコンテキストへ追加し、`PlayerController.BuildStyleEffectContext()` で組み立てる。
- 戦闘員はプレイヤー武器と同じく `ObjectPool<T>` を使って再利用し、スタイル切替時と次回召喚時に明示的に全返却する。短周期生成のたびに `Instantiate/Destroy` を繰り返さない構成にする。
- 敵の追跡先は `EnemyController.SetTarget()` が単一 Transform 前提で、`EnemySpawner` も常にプレイヤーを直接セットしている。これを維持しつつ、敵側に「おとりターゲット優先解決」を追加し、戦闘員が存在する間だけプレイヤーより戦闘員を優先する。
- 戦闘員は既存の敵レジストリを流用して最寄り敵を探索し、敵不在時はプレイヤーの向きに従って XZ 平面を直進する。攻撃コンポーネントやダメージ判定は持たせず、`Player` タグにも `Enemy` タグにも属さない専用識別を用意する。
- 接触ダメージは現在 `PlayerController.OnTriggerEnter()` が `Enemy` タグに直接反応している。戦闘員が敵へダメージを与えないことを保証するため、戦闘員側にはプレイヤーと同じ接触ダメージ処理を持たせず、敵からのみダメージを受ける構成に限定する。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Player/Application/StyleEffects/PirateStyleEffect.cs`
  - 10秒タイマー、`Time.timeScale == 0` 停止、2体召喚要求、初期化時の内部状態リセットを実装する。
  - 次回召喚前の既存戦闘員クリアを、コンテキスト経由の管理コンポーネント呼び出しに置き換える。
- `Assets/Survaivor/Character/Player/Application/StyleEffects/PlayerStyleEffectContext.cs`
  - 戦闘員召喚に必要な `Transform`、`EnemyRegistry`、戦闘員管理窓口を保持できるよう拡張する。
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - 戦闘員管理コンポーネントの取得または生成を追加し、`BuildStyleEffectContext()` へ注入する。
  - `OnDestroy()` でスタイル解除相当のクリーンアップが走るよう、戦闘員破棄経路を確保する。
- `Assets/Survaivor/Character/Player/Application/PlayerProgressionService.cs`
  - スタイル切替時に旧スタイルの後始末を確実に行うため、`ResetStyleParameters()` だけでなくアクティブ効果の停止フック追加を検討し、海賊スタイル解除時に戦闘員破棄が漏れない構造へ調整する。
- `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyController.cs`
  - 「既定ターゲット」と「戦闘員優先ターゲット」を分離し、毎フレーム有効なおとりを解決して追跡対象へ反映する。
  - 無効化済み戦闘員参照を保持し続けない null 安全処理を追加する。
- `Assets/Survaivor/Character/Enemy/Infrastructure/EnemySpawner.cs`
  - 新規スポーン敵にも戦闘員優先ロジックが効くよう、プレイヤー既定ターゲット設定と敵登録順の整合を確認し、必要なら新しい優先解決 API 初期化を追加する。
- `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyRegistry.cs`
  - 戦闘員移動用の最寄り敵探索をそのまま利用する。必要であれば可視条件なし検索の使い分けをコメントで補足する。

### 新規作成想定
- `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewSummonController.cs`
  - 戦闘員プール初期化、2体同時召喚、全戦闘員返却、アクティブ戦闘員一覧管理を担当する。
  - 召喚失敗時は警告ログを出して処理継続する。
- `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewMemberController.cs`
  - 戦闘員の初期化、HP30設定、敵追跡移動、敵不在時の前方移動、死亡時のプール返却を担当する。
- `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewRegistry.cs`
  - アクティブ戦闘員一覧と「最寄りの有効戦闘員」検索を管理し、敵 AI から参照できるようにする。
- `Assets/Survaivor/Character/Player/Infrastructure/PirateCrewTarget.cs`
  - 敵が追跡対象として扱うための軽量コンポーネントまたはマーカー。戦闘員識別と有効判定の責務を持たせる。
- `Assets/Survaivor/Character/Player/Prefabs/PirateCrewMember.prefab`
  - 戦闘員用プレハブ。`HealthComponent`、`CharacterStats`、必要な Collider / Rigidbody、プール返却対応を設定する。
- `Assets/GameResources/Player/PirateCrewMemberStatsData.asset`
  - HP30 を持つ戦闘員用の `CharacterStatsData`。攻撃力は使わないため最小値で保持する。

### テスト追加・更新対象
- `Assets/Survaivor/Tests/EditMode/Player/PlayerProgressionServiceTests.cs`
  - 海賊スタイル切替時に召喚タイマーが開始され、他スタイルへ変更後は継続しないことを検証する。
- `Assets/Survaivor/Tests/EditMode/Enemy/EnemyRegistryTests.cs`
  - 既存レジストリ探索を戦闘員移動へ流用した際の前提を崩していないことを確認する。
- `Assets/Survaivor/Tests/EditMode/...` 配下の新規テスト
  - `PirateStyleEffect` の10秒召喚、2体生成、停止条件を検証する。
  - `PirateCrewSummonController` の再召喚時全破棄、警告ログ、プール再利用を検証する。
  - `PirateCrewRegistry` / `EnemyController` の優先ターゲット切替を検証する。

## データフロー / 処理フロー

### 1. 海賊スタイル開始
1. `PlayerController.ChangeStyle(PlayerStyleType.Pirate)` が呼ばれる。
2. `PlayerProgressionService.ChangeStyle()` が既存スタイルの補正をリセットし、海賊スタイル効果を生成する。
3. `PlayerController.BuildStyleEffectContext()` で組み立てたコンテキストが `PirateStyleEffect.ApplyParameters()` に渡される。
4. `PirateStyleEffect` は内部タイマーを 0 に戻し、戦闘員管理側にも残存戦闘員クリアを要求する。

### 2. 10秒ごとの召喚
1. `PlayerController.Update()` から `PlayerExperience.TickStyleEffect()` が毎フレーム呼ばれる。
2. `PirateStyleEffect.Tick()` は `Time.timeScale == 0` の間はタイマーを進めない。
3. ゲーム内時間で 10 秒経過すると、戦闘員管理コンポーネントへ「既存戦闘員を返却してから2体召喚」を依頼する。
4. 戦闘員管理はプレイヤー前方の扇状オフセットを計算し、プールから 2 体取得して初期化する。
5. 生成失敗時はその個体だけ警告ログを出し、他個体とゲーム進行は継続する。

### 3. 戦闘員の行動
1. 各 `PirateCrewMemberController` は `EnemyRegistry.FindNearestEnemy(transform.position)` で最寄り敵を探す。
2. 敵が見つかればその方向へ XZ 平面移動する。
3. 敵がいなければプレイヤーの向きベクトルを基準に直進する。
4. 戦闘員は攻撃生成処理を持たず、接触時にも敵へダメージを与えない。
5. HP が 0 になった個体はレジストリから外れ、プールへ返却される。

### 4. 敵の優先ターゲット切替
1. `EnemySpawner` が敵を生成し、既定ターゲットとしてプレイヤー Transform をセットする。
2. `EnemyController.Update()` は毎フレーム `PirateCrewRegistry` から「最も近い有効戦闘員」を問い合わせる。
3. 見つかればその戦闘員 Transform を追跡対象に採用する。
4. 見つからなければ既定ターゲットのプレイヤー Transform に戻る。
5. 戦闘員が死亡・返却された場合も、次フレームで自動的に次候補またはプレイヤーへ復帰する。

### 5. 海賊スタイル終了
1. プレイヤーが別スタイルへ変更される。
2. `PlayerProgressionService.ChangeStyle()` の旧スタイル終了処理、または `PirateStyleEffect` 側の停止フックで、戦闘員管理へ全返却を指示する。
3. 以後は召喚タイマーを進めず、新規召喚も行わない。
4. 敵は有効な戦闘員がいなくなった時点で既定ターゲットのプレイヤーへ戻る。

## 実装ステップ

### Phase 1: 参照注入とライフサイクル整備
- `PlayerStyleEffectContext` を拡張し、プレイヤー Transform、`EnemyRegistry`、戦闘員管理窓口を保持できるようにする。
- `PlayerController` で必要コンポーネントを初期化し、スタイル効果コンテキスト再構築時に注入する。
- スタイル切替時に旧スタイルの後始末を呼べる設計へ `PlayerProgressionService` と `IPlayerStyleEffect` を拡張する。

### Phase 2: 戦闘員生成・管理基盤
- `PirateCrewSummonController`、`PirateCrewRegistry`、戦闘員プレハブを追加する。
- `ObjectPool<PirateCrewMemberController>` で 2 体以上を再利用できる構成を作る。
- 召喚位置計算、全返却、再召喚時の既存戦闘員クリア、生成失敗ログを実装する。

### Phase 3: 戦闘員の移動と被ダメージ
- `PirateCrewMemberController` に最寄り敵追跡と前方直進を実装する。
- `CharacterStatsData` で HP30 を設定し、`HealthComponent` と組み合わせて死亡時返却を実装する。
- 戦闘員が攻撃機能を持たないこと、既存プレイヤー武器が戦闘員を誤って攻撃対象にしないことを確認する。

### Phase 4: 敵ターゲット優先
- `EnemyController` に優先ターゲット解決処理を追加する。
- `PirateCrewRegistry` から最寄りの有効戦闘員を引けるようにする。
- 既存敵・新規敵ともに同じ優先ルールで動作することを確認する。

### Phase 5: テストと確認
- EditMode テストでスタイル効果、戦闘員管理、敵ターゲット切替を網羅する。
- `u tests run edit` で回帰確認する。
- 必要に応じて PlayMode で海賊スタイル選択後 10 秒周期、解除時破棄、敵のおとり切替を目視確認する。

## リスクと対策
- リスク: `IPlayerStyleEffect` に終了フックがないままだと、海賊スタイル解除時の後始末が `ChangeStyle()` の外に散らばる。
  - 対策: 効果の開始・更新・終了ライフサイクルを明示し、海賊固有の破棄処理を共通経路へ乗せる。
- リスク: `EnemyController` が毎フレーム全戦闘員を総当たりすると、敵数増加時に負荷が伸びる。
  - 対策: `PirateCrewRegistry` で有効戦闘員一覧を管理し、少数前提でも不要なアロケーションなしで探索する。
- リスク: 戦闘員プレハブに `Enemy` タグを付けると、プレイヤー武器やダメージフィールドの攻撃対象になってしまう。
  - 対策: 専用タグまたはマーカーを使い、敵攻撃対象の識別とプレイヤー武器対象の識別を分離する。
- リスク: 既存の `PlayerController.OnTriggerEnter()` は `Enemy` タグ接触で直接プレイヤーへダメージを与えるため、戦闘員をおとりにしても敵がすり抜けるとプレイヤー被弾が残る。
  - 対策: まずは追跡先変更で行動を戦闘員へ誘導し、必要なら敵側の接触ダメージ担当を将来的に `EnemyController` 側へ寄せやすい構造を意識して分離する。
- リスク: プール返却時にレジストリ解除漏れがあると、敵が無効オブジェクトを追跡する。
  - 対策: 戦闘員の `OnEnable/OnDisable` または明示的な `Initialize/Release` で登録解除を対にする。
- リスク: 10 秒判定を `Time.deltaTime` ベースでそのまま積むと、一時停止明けや大きなフレーム落ちで複数回召喚が走る可能性がある。
  - 対策: while で複数回消化するか、今回は 1 フレーム 1 回に制限するかを方針化し、要件どおり「10秒ごとに1回」を保つ実装にする。

## 検証方針
- EditMode テスト
  - 海賊スタイル選択時のみ 10 秒ごとに召喚要求が発生すること。
  - `Time.timeScale == 0` 相当では召喚タイマーが進まないこと。
  - 1 回の召喚で 2 体生成され、再召喚時に前回分が返却されること。
  - 別スタイルへ変更すると既存戦闘員が破棄され、新規召喚が止まること。
  - 戦闘員が最寄り敵へ向かい、敵不在時は前方移動へフォールバックすること。
  - 敵が戦闘員存在中はプレイヤーではなく戦闘員を優先し、全消滅後にプレイヤーへ戻ること。
- 手動確認
  - 海賊スタイル選択後、約10秒ごとにプレイヤー前方へ 2 体ずつ出現すること。
  - 戦闘員が敵に近づくだけで攻撃しないこと。
  - スタイル解除時に画面上の戦闘員が消え、以後増えないこと。
  - 複数敵がいても、おとりがいる間は敵群が戦闘員へ引き寄せられること。
- Unity 確認
  - `u tests run edit` で関連テストが通ること。
  - `u console get -l E` で海賊スタイル関連の例外が出ていないこと。

## コードスニペット（主要変更イメージ）
```csharp
public interface IPlayerStyleEffect
{
    PlayerStyleType StyleType { get; }
    void ApplyParameters(PlayerStyleEffectContext context);
    void Tick(PlayerStyleEffectContext context, float deltaTime);
    void Cleanup(PlayerStyleEffectContext context);
}
```

```csharp
public class PirateStyleEffect : IPlayerStyleEffect
{
    const float SUMMON_INTERVAL_SECONDS = 10f;
    float _timer;

    public void ApplyParameters(PlayerStyleEffectContext context)
    {
        _timer = 0f;
        context?.PirateCrewSummonController?.ClearAll();
    }

    public void Tick(PlayerStyleEffectContext context, float deltaTime)
    {
        if (context?.PirateCrewSummonController == null || Time.timeScale <= 0f)
        {
            return;
        }

        _timer += deltaTime;
        while (_timer >= SUMMON_INTERVAL_SECONDS)
        {
            context.PirateCrewSummonController.ResummonCrew(
                context.PlayerTransform.position,
                context.PlayerFacingDirection);
            _timer -= SUMMON_INTERVAL_SECONDS;
        }
    }

    public void Cleanup(PlayerStyleEffectContext context)
    {
        context?.PirateCrewSummonController?.ClearAll();
    }
}
```

```csharp
public class EnemyController : MonoBehaviour
{
    Transform _defaultTargetTransform;
    PirateCrewRegistry _pirateCrewRegistry;

    void Update()
    {
        Transform resolvedTarget = _pirateCrewRegistry != null
            ? _pirateCrewRegistry.FindNearestActiveCrew(transform.position)
            : null;

        _targetTransform = resolvedTarget != null ? resolvedTarget : _defaultTargetTransform;
        _enemyBehavior?.Execute(this);
    }
}
```
