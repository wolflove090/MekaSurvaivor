# デバッグ用トレーニングルーム 実装計画

## 実装方針
- `Assets/Survaivor/Editor/Sandbox.unity` を、`Main` シーンの移動確認と武器デバッグに必要な最小構成シーンとして扱う。敵スポーン、経験値、武器強化UIは外し、プレイヤー移動と攻撃検証に必要な参照だけを残す。
- プレイヤーの初期武器は現状 `PlayerController.InitializeWeaponSystems()` 内で `Shooter` を自動付与しているため、この初期付与を設定で無効化できるようにする。`Main` は既存挙動を維持し、`Sandbox` のみ未所持開始に切り替える。
- 武器の追加と強化はランタイムUIではなく Editor 拡張で行う。`Assets/Survaivor/Editor` 配下にサンドボックス専用 `EditorWindow` を追加し、Play 中の `PlayerController` を操作して既存の `ApplyWeaponUpgrade()` を呼び出す。
- EditorWindow は「武器追加トグル」と「レベル 1-5 スライダー」を武器ごとに持つ。トグル ON で未所持武器を取得し、スライダー増加で差分回数だけ `LevelUp` を適用する。トグル OFF とスライダー減少は未対応として扱い、武器状態を変更せずログを出す。
- 現在の `WeaponService` / `PlayerController` には「指定武器の所持有無」や「現在レベル」の読み取り API がないため、EditorWindow から参照できる最小限の公開 API を追加する。削除やレベルダウンの API は追加しない。
- サンドバッグ用エネミーは既存 `EnemyController` と `HealthComponent` を破壊せずに流用し、同一 GameObject に追加する専用コンポーネントで「無敵」「非移動」「非接触ダメージ」を実現する。武器のターゲットとしては既存 `EnemyRegistry` に載り続ける状態を維持する。
- `Sandbox.unity` 再生開始時の EditorWindow 自動表示は、`EditorApplication.playModeStateChanged` とアクティブシーン判定で行う。シーン名だけでなくパスも確認し、他シーン再生時に誤起動しないようにする。

## 変更対象ファイル一覧

### 改修対象
- `Assets/Survaivor/Character/Player/Infrastructure/PlayerController.cs`
  - 初期武器自動付与を ON/OFF できるシリアライズ設定を追加する。
  - 武器未所持でも `Update()` で安全に動作するよう、`_weapon` null 時のガードを入れる。
  - 指定武器の所持有無、現在レベル、全武器状態を EditorWindow から読める公開 API を追加する。
  - EditorWindow から目標レベルへ合わせる際に使う補助メソッド（例: `TryApplyWeaponLevel(type, targetLevel)`）を追加するか、既存 `ApplyWeaponUpgrade()` と読み取り API の組み合わせで完結できる構成に整理する。
- `Assets/Survaivor/Character/Combat/Application/WeaponService.cs`
  - `UpgradeCardType` から実武器 `Type` への対応表を、Apply 用だけでなく読み取りにも使えるよう整理する。
  - 指定武器の所持判定や現在レベル参照に必要な補助 API を追加する。
  - レベル低下や武器削除は未対応のままにし、既存の生成・レベルアップロジックを維持する。
- `Assets/Survaivor/Game/Infrastructure/GameBootstrapper.cs`
  - `Sandbox` で `WeaponUpgradeUiController`、`ExperienceOrbSpawner`、`EnemySpawner` が未配置でも、不要な警告や依存漏れで破綻しないことを確認し、必要ならシーン差分前提のガードを追加する。
  - `Sandbox` で最低限必要な依存だけを接続できるよう、既存の null 許容挙動を明文化したログへ整理する。
- `Assets/Survaivor/Character/Enemy/Infrastructure/EnemyController.cs`
  - サンドバッグ用コンポーネントが付いている場合に、追跡行動や接触ダメージ向けの通常挙動を抑止しやすい拡張ポイントを用意する。
  - 既存の `EnemyRegistry` 登録や武器ヒット対象としての扱いは維持する。
- `Assets/Survaivor/Character/Infrastructure/HealthComponent.cs`
  - サンドバッグ無敵化コンポーネントと協調するため、ダメージ適用前のキャンセル拡張ポイントを追加するか、外部からダメージを吸収できる構造に整理する。
  - 既存の敵・プレイヤーへの通常ダメージフローを壊さないよう影響範囲を限定する。
- `Assets/Survaivor/Editor/GameTimePlayToolbarExtension.cs`
  - 必要に応じて `Sandbox` 用 EditorWindow 自動起動コードをここへ統合するか、責務分離のため別ファイルへ切り出す際の共通フック元として使う。
  - 既存のツールバー機能とは責務を分け、再生開始イベント処理の競合を避ける。

### 新規作成候補
- `Assets/Survaivor/Editor/SandboxWeaponDebugWindow.cs`
  - サンドボックス専用 `EditorWindow` 本体。
  - 全武器分のトグルとスライダー描画、Play 中の `PlayerController` 解決、未対応操作時ログ出力を担当する。
- `Assets/Survaivor/Editor/SandboxPlayModeHook.cs`
  - PlayMode 遷移時に `Sandbox.unity` を判定し、`SandboxWeaponDebugWindow` を自動表示する。
  - 他シーンでは何もしない。
- `Assets/Survaivor/Character/Enemy/Infrastructure/SandboxDummyEnemy.cs`
  - サンドバッグ挙動を付与する専用コンポーネント。
  - HP 減少無効、移動停止、プレイヤーへの接触ダメージ無効を担当する。
- `Assets/Survaivor/Tests/EditMode/Editor/SandboxWeaponDebugWindowTests.cs`
  - EditorWindow の武器状態同期ロジック、未対応操作時の分岐を検証する。
- `Assets/Survaivor/Tests/EditMode/Player/PlayerControllerWeaponDebugTests.cs`
  - 初期武器なし設定、所持判定、目標レベルへの同期ロジックを検証する。

### シーン・プレハブ調整対象
- `Assets/Survaivor/Editor/Sandbox.unity`
  - プレイヤーの初期武器自動付与を OFF に設定する。
  - `EnemySpawner`、経験値関連、武器強化UI を配置しない構成へ整理する。
  - サンドバッグ用エネミーに `SandboxDummyEnemy` をアタッチし、必要なら `EnemyRegistry` へ登録される構成を確認する。
  - `GameBootstrapper` の参照を `Sandbox` 用最小構成に合わせて設定する。

## データフロー / 処理フロー

### 1. `Sandbox.unity` 再生開始時の初期化
1. Unity Editor で `Assets/Survaivor/Editor/Sandbox.unity` をアクティブシーンにした状態で Play を開始する。
2. `SandboxPlayModeHook` が `EditorApplication.playModeStateChanged` を受け取り、Play 開始直後に現在シーンパスを確認する。
3. シーンが `Sandbox.unity` の場合のみ、`SandboxWeaponDebugWindow` を開く。
4. `Sandbox` 側では `GameBootstrapper` が最低限の依存を解決し、`PlayerController` を通常どおり初期化する。
5. `PlayerController` は初期武器自動付与 OFF 設定により、武器未所持で開始する。

### 2. EditorWindow から武器を追加する
1. `SandboxWeaponDebugWindow` が Play 中の `PlayerController` を `FindFirstObjectByType<PlayerController>()` などで解決する。
2. Window は `PlayerController` から全武器の所持状況と現在レベルを読み取り、トグルとスライダーの初期表示を同期する。
3. ユーザーが未所持武器のトグルを OFF から ON にする。
4. Window は `PlayerController.ApplyWeaponUpgrade(type)` を1回呼び出し、対象武器を追加する。
5. 追加後に再読込して、トグル ON / スライダー 1 の表示へ同期する。

### 3. EditorWindow から武器レベルを上げる
1. ユーザーが所持済み武器のスライダーを現在値より大きい値へ変更する。
2. Window は `PlayerController` から現在レベルを読み取り、差分値を算出する。
3. 差分回数だけ `PlayerController.ApplyWeaponUpgrade(type)` を呼び出す。
4. 反映後に `PlayerController` から現在レベルを再読込し、Window 表示を実状態へ合わせる。

### 4. 未対応操作時の扱い
1. ユーザーがトグルを ON から OFF にする、またはスライダーを現在値未満へ下げる。
2. Window は武器削除・レベル低下を行わず、`Debug.Log` で未対応であることを出力する。
3. UI は即時に実際の武器状態へ差し戻し、表示と実状態の不整合を残さない。

### 5. サンドバッグへの攻撃
1. プレイヤー武器がサンドバッグ用エネミーへヒットする。
2. 通常どおり `IDamageable.TakeDamage()` 系の経路に入る。
3. `SandboxDummyEnemy` または `HealthComponent` 側のガードがダメージ消費と死亡遷移を抑止する。
4. エネミーは `EnemyRegistry` 上に残り続け、次の攻撃でもターゲット対象のままとなる。
5. サンドバッグはプレイヤー追跡と接触ダメージを行わない。

## 実装ステップ

### Phase 1: ランタイム側の武器状態 API 整備
- `PlayerController` に初期武器自動付与フラグを追加する。
- `PlayerController.Update()` の `_weapon` null ガードを追加する。
- `PlayerController` / `WeaponService` に、全武器の所持有無と現在レベルを読める API を追加する。
- 既存 `Main` シーンでは初期武器 ON のまま変化しないことを前提にする。

### Phase 2: EditorWindow と自動起動フックの実装
- `SandboxWeaponDebugWindow` を作成し、全武器分のトグルと 1-5 スライダーを描画する。
- Play 中の `PlayerController` 解決に失敗した場合は、操作無効と案内表示だけにする。
- トグル ON 時の追加、スライダー増加時の差分適用、未対応操作時ログと即時差し戻しを実装する。
- `SandboxPlayModeHook` で `Sandbox.unity` 再生時のみ Window を自動表示する。

### Phase 3: サンドバッグ挙動の実装
- `SandboxDummyEnemy` を作成し、無敵、移動停止、接触ダメージ無効を実装する。
- 既存 `EnemyController` / `HealthComponent` への影響が最小になる差し込みポイントを追加する。
- 武器ヒット時にターゲットとしては有効、死亡だけしない状態を確認する。

### Phase 4: `Sandbox.unity` の構成整理
- プレイヤーの初期武器自動付与を OFF に設定する。
- 経験値、武器強化UI、敵スポナーを外した最小構成にする。
- `GameBootstrapper` の参照不足がないようシーン上の参照を再接続する。
- サンドバッグ用エネミーへ専用コンポーネントを付与する。

### Phase 5: テストと確認
- EditMode テストで初期武器なし、武器追加、レベル同期、未対応操作時の分岐を確認する。
- Editor テストで `Sandbox.unity` 判定による Window 自動起動条件を確認する。
- 手動で `Sandbox.unity` を再生し、Window 自動起動、武器追加、レベル変更、サンドバッグの無敵を確認する。

## リスクと対策
- リスク: `PlayerController` の初期武器無効化変更で `Main` シーンまで武器なし開始になる。
  - 対策: シリアライズ設定を追加し、既存シーンのデフォルト値は ON に維持する。
- リスク: `_weapon` null 許容にした結果、既存コードの武器前提箇所が見落とされる。
  - 対策: `PlayerController.Update()` 以外にも `_weapon` 参照箇所を確認し、null ガードを明示的に揃える。
- リスク: EditorWindow の表示状態と実際の武器レベルがずれる。
  - 対策: 操作後は必ず `PlayerController` から再読込し、Window 表示を実状態へ再同期する。
- リスク: スライダー変更時の IMGUI 再描画で、同じ操作が複数回適用される。
  - 対策: 前回表示値と実反映値を分離して保持し、値変化イベント時のみ適用する。
- リスク: PlayMode フックが他シーン再生でも Window を開く。
  - 対策: シーンパスを厳密一致で判定し、`Sandbox.unity` 以外は即 return する。
- リスク: サンドバッグ無敵化の差し込み方次第で、通常の敵まで死亡しなくなる。
  - 対策: 無敵判定は `SandboxDummyEnemy` の有無に限定し、通常敵は既存挙動を維持する。
- リスク: 接触ダメージ無効のために `PlayerController.OnTriggerEnter()` を広く変えると、通常シーンの被弾判定が壊れる。
  - 対策: プレイヤー側ではなく、サンドバッグ側の Collider / Layer / 移動停止で接触ダメージが発生しない構成を優先する。

## 検証方針
- EditMode テスト
  - `PlayerController` が初期武器自動付与 OFF で武器なし開始できること。
  - `ApplyWeaponUpgrade()` と読み取り API の組み合わせで、目標レベルまで増加できること。
  - スライダー減少要求時にレベルが変わらず、未対応ログ経路へ入ること。
  - トグル OFF 要求時に武器が削除されず、未対応ログ経路へ入ること。
  - `SandboxDummyEnemy` がダメージを受けても死亡しないこと。
- Editor テスト
  - `Sandbox.unity` 再生時のみ Window を開くこと。
  - `PlayerController` 未検出時に Window が操作不能表示になること。
- 手動確認
  - `Sandbox.unity` を再生すると Window が自動表示されること。
  - 初期状態でプレイヤーが移動可能かつ攻撃しないこと。
  - 任意の武器トグル ON で武器が追加されること。
  - スライダーを 1 から 5 に上げると、その武器の攻撃頻度や挙動が強化後状態になること。
  - スライダーを下げた時にレベルは下がらず、ログが出ること。
  - トグルを OFF にしても武器は消えず、ログが出ること。
  - サンドバッグへ攻撃しても破壊されず、プレイヤーへダメージを返さないこと。
- Unity 確認
  - `u console get -l E` で `Sandbox` 再生時に Console エラーが発生していないこと。

## コードスニペット（主要変更イメージ）
```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("開始時に初期武器を自動付与するかどうか")]
    bool _grantDefaultWeaponOnStart = true;

    public bool TryGetWeaponLevel(WeaponUpgradeUiController.UpgradeCardType type, out int level)
    {
        return _playerWeaponService != null &&
            _playerWeaponService.TryGetWeaponLevel(type, _weapons, out level);
    }

    void Update()
    {
        if (IsGameOver)
        {
            return;
        }

        _weapon?.Tick(Time.deltaTime);
        _playerExperience?.TickStyleEffect(_styleEffectContext, Time.deltaTime);
    }

    void InitializeWeaponSystems()
    {
        // 既存初期化...

        if (_grantDefaultWeaponOnStart)
        {
            _weapon = _playerWeaponService.ApplyUpgrade(
                WeaponUpgradeUiController.UpgradeCardType.Shooter,
                null,
                _weapons);
        }
    }
}
```

```csharp
public class WeaponService
{
    public bool TryGetWeaponLevel(
        WeaponUpgradeUiController.UpgradeCardType type,
        Dictionary<Type, WeaponBase> weapons,
        out int level)
    {
        level = 0;

        if (!_weaponTypes.TryGetValue(type, out Type weaponType))
        {
            return false;
        }

        if (!weapons.TryGetValue(weaponType, out WeaponBase weapon))
        {
            return false;
        }

        level = weapon.UpgradeLevel;
        return true;
    }
}
```

```csharp
public class SandboxWeaponDebugWindow : EditorWindow
{
    void DrawWeaponControl(WeaponUpgradeUiController.UpgradeCardType type)
    {
        bool hasWeapon = _playerController.TryGetWeaponLevel(type, out int currentLevel);
        bool nextToggle = EditorGUILayout.ToggleLeft(type.ToString(), hasWeapon);

        if (!hasWeapon && nextToggle)
        {
            _playerController.ApplyWeaponUpgrade(type);
            return;
        }

        if (hasWeapon && !nextToggle)
        {
            Debug.Log($"SandboxWeaponDebugWindow: 武器削除は未対応です。 type={type}");
        }

        int requestedLevel = EditorGUILayout.IntSlider(currentLevel, 1, 5);
        if (requestedLevel < currentLevel)
        {
            Debug.Log($"SandboxWeaponDebugWindow: レベル低下は未対応です。 type={type}, current={currentLevel}, requested={requestedLevel}");
            return;
        }

        for (int i = currentLevel; i < requestedLevel; i++)
        {
            _playerController.ApplyWeaponUpgrade(type);
        }
    }
}
```

```csharp
[InitializeOnLoad]
public static class SandboxPlayModeHook
{
    static SandboxPlayModeHook()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }

        if (SceneManager.GetActiveScene().path != "Assets/Survaivor/Editor/Sandbox.unity")
        {
            return;
        }

        SandboxWeaponDebugWindow.Open();
    }
}
```
