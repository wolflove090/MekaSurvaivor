# プロジェクト再構築リファクタ 実装計画

## 実装方針
- 現在の挙動を維持しながら、既存クラスを一度に置き換えず、責務ごとに段階移行する。
- `MonoBehaviour` は Unityイベント受信と見た目反映に限定し、ゲームルールは純C#クラスへ移す。
- 新規コードは `Domain` / `Application` / `Infrastructure` / `Presentation` の4層構成で追加し、既存コードを少しずつ移設する。
- シングルトン (`GameManager.Instance`、`PlayerController.Instance`、`EnemySpawner.Instance`) と静的イベント (`GameEvents`) は、新規実装で依存を増やさず、移行完了後に縮退または置換する。
- UIは UI Toolkit を継続利用しつつ、表示更新と操作通知に責務を絞る。
- `ScriptableObject` は設定データの保持に限定し、ロジック本体は純C#側に寄せる。
- 優先順は以下とする。
  1. ブートストラップと依存解決の整理
  2. プレイヤー進行・ゲーム進行の純C#化
  3. 武器・敵生成の責務分離
  4. UIのPresenter化
  5. 旧依存の撤去とテスト整備

## 変更対象ファイル一覧

### 新規追加予定
- `Assets/Scripts/Domain/Game/GameSessionState.cs`
  - ゲーム時間、クリア、ゲームオーバー状態を保持する。
- `Assets/Scripts/Domain/Player/PlayerState.cs`
  - HP、経験値、レベル、移動倍率、装備武器状態、スタイル状態を保持する。
- `Assets/Scripts/Domain/Combat/WeaponState.cs`
  - 武器レベル、クールダウン、発射判定のランタイム状態を保持する。
- `Assets/Scripts/Domain/Enemy/EnemySpawnState.cs`
  - スポーンタイマーや出現制御に必要な状態を保持する。
- `Assets/Scripts/Application/Game/GameSessionService.cs`
  - 時間経過、ゲームクリア、ゲームオーバー遷移を制御する。
- `Assets/Scripts/Application/Player/PlayerProgressionService.cs`
  - 経験値加算、レベルアップ、スタイル変更、ステータス反映を制御する。
- `Assets/Scripts/Application/Combat/WeaponService.cs`
  - 武器アップグレード、発射タイミング判定、武器状態更新を制御する。
- `Assets/Scripts/Application/Enemy/EnemySpawnService.cs`
  - スポーン間隔更新、スポーン要求生成、探索条件判定を制御する。
- `Assets/Scripts/Application/Events/GameMessageBus.cs`
  - 局所的な通知ハブ。静的イベントの代替として利用する。
- `Assets/Scripts/Infrastructure/Unity/Bootstrap/GameBootstrapper.cs`
  - シーン起動時に依存関係を組み立てる。
- `Assets/Scripts/Presentation/UI/GameScreenPresenter.cs`
  - HUD表示用の表示モデル生成と UI 更新窓口を担当する。
- `Assets/Scripts/Presentation/UI/WeaponUpgradePresenter.cs`
  - 武器強化UIの入力を Application 層へ橋渡しする。

### 既存ファイルの改修対象
- `Assets/Scripts/Common/GameManager.cs`
  - 段階1では新 `GameSessionService` への委譲に変更し、最終的には薄いUnityアダプター化または役割解消する。
- `Assets/Scripts/Character/Player/PlayerController.cs`
  - ゲームルール保持をやめ、入力・見た目・アプリケーション層呼び出しに限定する。
- `Assets/Scripts/Character/Player/PlayerExperience.cs`
  - 計算と状態保持を `PlayerState` / `PlayerProgressionService` へ移し、当面は同期専用アダプターに縮小する。
- `Assets/Scripts/Character/Stats/CharacterStats.cs`
  - `ScriptableObject` 読み出しと表示用参照の責務に限定し、実ステータス計算はドメインへ移す。
- `Assets/Scripts/Character/Enemy/EnemySpawner.cs`
  - スポーン判定と生成処理を分離し、`EnemySpawnService` と Unity生成アダプターへ再編する。
- `Assets/Scripts/Wepon/WeaponBase.cs`
  - クールダウン計算と `Time.deltaTime` 依存をドメイン/アプリケーションへ移し、実行側の抽象境界を見直す。
- `Assets/Scripts/UI/GameScreenUiController.cs`
  - `FindFirstObjectByType` とシングルトン参照を排し、Presenter経由で表示更新する。
- `Assets/Scripts/UI/WeaponUpgradeUiController.cs`
  - `PlayerController` 直参照をやめ、選択通知のみを担う構成に変更する。
- `Assets/Scripts/Common/GameEvents.cs`
  - 新規依存を止め、段階的に `GameMessageBus` または明示的イベント購読へ置換する。

## フェーズ別実装計画

### Phase 1: フォルダ構成とブートストラップ整備
- 4層構成のディレクトリを新設する。
- `GameBootstrapper` を追加し、シーン内の参照解決とサービス生成を一元化する。
- 既存コンポーネント同士の `Instance` 参照を、ブートストラップ経由の明示参照へ移せる経路を作る。
- この段階では既存クラスの public API を急に壊さず、委譲先だけ差し替えられる状態にする。

### Phase 2: ゲーム進行とプレイヤー進行の純C#化
- `GameSessionState` と `GameSessionService` を作成し、残り時間、ゲームクリア、ゲームオーバー判定を移す。
- `PlayerState` と `PlayerProgressionService` を作成し、経験値、レベル、倍率、スタイル適用を移す。
- `PlayerExperience` のレベルアップ計算、`GameManager` のタイマー更新をサービスへ委譲する。
- `PlayerController` は `Update` でサービス更新を呼び、Transform反映や入力受付だけを担当する。

### Phase 3: 武器・敵生成の責務分離
- `WeaponBase` 由来のクールダウン更新と `Time.deltaTime` 依存を `WeaponService` に集約する。
- 武器の種類管理は `enum` や `switch` に閉じず、定義データまたは登録テーブルへ移行する。
- `EnemySpawner` のスポーンタイマー、スポーン条件、探索補助ロジックを `EnemySpawnService` に移す。
- Unity側は「スポーン要求を受けてPrefabを出す」「ターゲットを接続する」役割だけにする。

### Phase 4: UIのPresenter化
- `GameScreenUiController` は UXML/USS 構築と要素参照保持だけに寄せる。
- ステータスの表示モデル作成、更新差分判定、購読イベント管理を `GameScreenPresenter` に移す。
- `WeaponUpgradeUiController` はボタン入力通知のみを担当し、武器強化適用は Presenter または Application 層に委譲する。
- 既存の `GameEvents` 購読は、局所バスまたはサービス通知に置換する。

### Phase 5: 旧依存の撤去とテスト整備
- `Instance` 前提の参照、`FindFirstObjectByType`、不要になった静的イベント依存を削減する。
- EditMode テストを追加し、経験値計算、ゲーム進行、武器クールダウン、スポーン判定、スタイル適用を固定する。
- PlayMode では最低限、ブートストラップと UI 接続の統合確認を行う。

## データフロー / 処理フロー

### 1. ゲーム開始
1. `GameBootstrapper` がシーンの `MonoBehaviour` 参照を収集する。
2. `GameSessionState`、`PlayerState`、各Serviceを生成し、必要な依存を接続する。
3. UI Presenter に状態取得元と通知経路を渡す。
4. 既存 `MonoBehaviour` は以後、Serviceへの委譲先として動作する。

### 2. 毎フレーム更新
1. Unity側の `MonoBehaviour.Update` が入力や `deltaTime` を取得する。
2. `GameSessionService.Tick(deltaTime)` が制限時間と終了状態を更新する。
3. `PlayerProgressionService` と `WeaponService` が必要な状態更新を行う。
4. `EnemySpawnService.Tick(deltaTime)` がスポーン要求の有無を判定する。
5. Infrastructure層が必要なPrefab生成やTransform同期を行う。
6. Presenterが最新状態をUI表示へ反映する。

### 3. レベルアップと武器強化
1. 経験値取得時に `PlayerProgressionService.AddExperience(amount)` を呼ぶ。
2. サービスがレベルアップを確定し、必要なら武器強化UI表示イベントを通知する。
3. `WeaponUpgradeUiController` が選択結果を Presenterへ通知する。
4. Presenterが `WeaponService.ApplyUpgrade(selection)` を呼ぶ。
5. 武器状態が更新され、以後の `Tick` で新挙動が反映される。

### 4. スタイル変更
1. スタイル選択UIまたは既存導線から `PlayerProgressionService.ChangeStyle(style)` を呼ぶ。
2. サービスが旧スタイル効果を解除し、新スタイル効果を適用する。
3. `PlayerState` の倍率や継続効果状態を更新する。
4. Presenter と Unityアダプターが表示・移動速度・経験値計算へ反映する。

## リスクと対策
- リスク: 段階移行中に旧ロジックと新ロジックが二重で動き、挙動が重複する。
  - 対策: 各フェーズで「単一の責務は片方だけが更新する」境界を明確にし、委譲化してから旧処理を空にする。
- リスク: `GameEvents` と新通知基盤が混在し、通知漏れや二重通知が起きる。
  - 対策: 移行対象ごとに通知の唯一の発火元を決め、新規機能は静的イベントへ追加しない。
- リスク: `MonoBehaviour` から純C#へ切り出す際に、`Time.deltaTime` や `Transform` 依存が残る。
  - 対策: Service入力を `deltaTime` や座標値の引数化で受け取り、Unity型への依存を限定する。
- リスク: UI参照解決の変更で初期化順不整合が起きる。
  - 対策: `GameBootstrapper` に初期化順を集約し、`Awake` / `Start` の責務を明確化する。
- リスク: データ駆動化途中で `enum + switch` と定義テーブルが混在し、保守箇所が増える。
  - 対策: 武器、スタイルの順で対象を絞って移行し、切り替え完了単位ごとに旧分岐を削除する。

## 検証方針
- EditMode テスト
  - 制限時間経過でゲームクリアになる。
  - ゲームオーバー後は進行更新が停止する。
  - 経験値倍率込みで経験値加算とレベルアップが正しく計算される。
  - スタイル変更時に旧効果が解除され、新効果だけが残る。
  - 武器クールダウンとレベルアップ後の発動条件が正しく更新される。
  - 敵スポーンタイマーとスポーン要求判定が正しく動作する。
- PlayMode / 手動確認
  - メインシーン起動時に参照解決エラーが発生しない。
  - HUDがHP、経験値、時間を正しく表示する。
  - レベルアップ時に武器強化UIが表示され、選択結果が適用される。
  - プレイヤー移動、攻撃、敵スポーン、ゲームクリア/ゲームオーバーが既存通りに動作する。

## コードスニペット（主要変更イメージ）
```csharp
/// <summary>
/// ゲーム進行の純C#状態
/// </summary>
public class GameSessionState
{
    public float TimeLimit { get; }
    public float ElapsedTime { get; private set; }
    public bool IsGameClear { get; private set; }
    public bool IsGameOver { get; private set; }

    public GameSessionState(float timeLimit)
    {
        TimeLimit = timeLimit;
    }

    public void Advance(float deltaTime)
    {
        if (IsGameClear || IsGameOver)
        {
            return;
        }

        ElapsedTime += deltaTime;
        if (ElapsedTime >= TimeLimit)
        {
            IsGameClear = true;
        }
    }

    public void MarkGameOver()
    {
        IsGameOver = true;
    }
}

/// <summary>
/// Unity側の薄い委譲コンポーネント
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField]
    float _timeLimit = 30f;

    GameSessionService _gameSessionService;

    void Update()
    {
        _gameSessionService.Tick(Time.deltaTime);
    }
}
```
