# スタイル変更 + 画像切り替え機能 実装計画

## 実装方針
- 既存の `StyleChangeUiController` を拡張し、スタイル選択時に「ステータス適用」と「画像適用」を同時実行する。
- スタイル名と定数を要件に合わせて「巫女 / アイドル / セレブ」に統一する。
- 画像は各スタイルの右向きスプライトのみ保持し、左右向き表現は `SpriteRenderer.flipX` に一本化する。
- 向き変更時の画像再差し替えを廃止し、`SpriteDirectionController` を「向き状態管理 + flipX制御」の責務へ変更する。
- 既存のレベル表示ルールは `LevelUpUiDisplayRule`（4,7,10...）を維持し、ドキュメント/表示文言のみ整合させる。

## 変更対象ファイル一覧
- `Assets/Scripts/UI/StyleChangeUiController.cs`
  - `StyleCardType.Maid` を `StyleCardType.Idol` に変更
  - カードタイトル/説明文を「アイドル」へ更新
  - `_maidStyleStats` を `_idolStyleStats` に変更
  - 右向き画像参照（`Sprite`）を3スタイル分追加
  - プレイヤー `SpriteDirectionController` / `SpriteRenderer` 参照を解決し、スタイル選択時に画像適用
  - ステータス適用と画像適用を同一メソッドで実行し、どちらか失敗時のログを明確化
- `Assets/Scripts/Character/SpriteDirectionController.cs`
  - 左右個別画像の保持をやめ、右向き基準画像 + `flipX` 制御へ変更
  - `UpdateDirection(float)` で `sprite` 差し替えせず `flipX` のみ更新
  - スタイル変更用に「右向き画像を設定して右向きへ初期化」する公開メソッドを追加
  - `GetFacingDirection()` の互換性を維持
- `Assets/Main.unity`
  - `StyleChangeUiController` のシリアライズ済みフィールド名変更（`_maidStyleStats`→`_idolStyleStats`）反映
  - `StyleChangeUiController` に右向き画像3種（`miko.png`, `idol.png`, `celebrity.png`）を割り当て
  - `SpriteDirectionController` の初期設定を右向き基準に合わせて更新（必要なら旧 `_leftSprite` 参照を除去）
- `Documentation/要件書/style-change-requirements.md`
  - 要件確定内容（4,7,10...、アイドル名称、右向き基準 + flipX）を維持

## データフロー / 処理フロー
1. `GameEvents.OnPlayerLevelUp` を `StyleChangeUiController` が受信する。
2. `LevelUpUiDisplayRule.ShouldOpenStyleUi(newLevel)` が `true`（4,7,10...）ならUIを表示して `Time.timeScale=0`。
3. ユーザーがカードを押下すると、カード種別から `CharacterStatsData` と右向き画像を取得する。
4. `HealthComponent.ApplyStatsDataAndKeepConsumedHp(statsData)` を実行し、HP消費量を維持した再計算を行う。
5. 同一タイミングで `SpriteDirectionController` に右向き画像を適用し、向きを右へリセット（`flipX=false`）。
6. 以後の移動入力では `SpriteDirectionController.UpdateDirection(moveInput.x)` が `flipX` のみ更新する。
7. 選択完了後にUIを閉じ、`Time.timeScale` を復帰する。

## 実装詳細
### 1. StyleChangeUiControllerの適用単位
- `TryApplyStyle(int cardIndex)` を「ステータス+画像」の両方を扱う入口にする。
- スタイルごとに以下の対応を1箇所で定義する（switchまたは小さな構造体）。
  - 表示名
  - `CharacterStatsData`
  - 右向きスプライト
- `CharacterStatsData` が未設定のときは適用中止（既存仕様維持）。
- 画像未設定時はステータス適用を続行し、警告ログのみ出す（要件準拠）。

### 2. SpriteDirectionControllerの責務整理
- 現在の `_leftSprite` / `_rightSprite` を右向き基準設計に置き換える。
- 追加公開API案
  - `public void ApplyRightFacingSprite(Sprite rightSprite)`
  - 処理: `null` ガード -> `_spriteRenderer.sprite = rightSprite` -> `_spriteRenderer.flipX = false` -> `_isFacingLeft = false`
- `UpdateDirection(float horizontalInput)` は以下のみ実施。
  - `horizontalInput < 0` で `flipX = true`
  - `horizontalInput > 0` で `flipX = false`
  - 0 は状態維持

### 3. シーン参照更新
- `Assets/Main.unity` の `StyleChangeUiController` セクションに新規 `Sprite` 参照を追加する。
- 右向き画像のGUID
  - 巫女: `ac06f3df74fe34873ba623bab6758023`
  - アイドル: `f7c5e08c6d00d4c6980e5532d1ba9ed3`
  - セレブ: `04bc42d1639ee45d799be297d1b60170`
- 既存 `MaidStyleStatsData.asset` はフィールド名だけ `idol` に合わせる（アセット自体の改名は必須ではない）。

## リスクと対策
- リスク: フィールド名変更で `Main.unity` の参照が欠落する。
  - 対策: `StyleChangeUiController` の SerializeField 名変更後、`Main.unity` の該当キー更新を確認する。
- リスク: `flipX` 導入時に初期向きが左のまま残る。
  - 対策: `Awake` またはスタイル適用時に必ず `flipX=false` を明示設定する。
- リスク: 画像未設定時に `NullReferenceException` が発生する。
  - 対策: 画像適用経路は常に `null` チェックし、未設定時はログのみで継続する。
- リスク: 既存攻撃方向取得（`GetFacingDirection`）との不整合。
  - 対策: `_isFacingLeft` の更新ロジックを維持し、戻り値仕様を変えない。

## 検証方針
- レベル3到達時にスタイルUIが開かないことを確認する。
- レベル4到達時にスタイルUIが開くことを確認する。
- カード文言が「巫女 / アイドル / セレブ」になっていることを確認する。
- 各カード選択で対応 `CharacterStatsData` が適用されることを確認する。
- スタイル変更時に対応画像へ即時切り替わることを確認する。
- スタイル変更直後の表示が右向き（`flipX=false`）であることを確認する。
- 左移動で `flipX=true`、右移動で `flipX=false` になり、画像差し替えが発生しないことを確認する。
- 画像未設定時に処理が継続し、警告ログのみ出ることを確認する。
- HP消費量維持の挙動（例: `5/10 -> 10/15`）が維持されることを確認する。

## コードスニペット（主要変更イメージ）
```csharp
bool TryApplyStyle(int cardIndex)
{
    if (_playerHealthComponent == null)
    {
        Debug.LogWarning("StyleChangeUiController: HealthComponentが見つからないためスタイルを適用できません。");
        return false;
    }

    if (!TryGetStyleDefinition((StyleCardType)cardIndex, out CharacterStatsData statsData, out Sprite rightSprite))
    {
        return false;
    }

    _playerHealthComponent.ApplyStatsDataAndKeepConsumedHp(statsData);

    if (_spriteDirectionController != null)
    {
        _spriteDirectionController.ApplyRightFacingSprite(rightSprite);
    }
    else if (rightSprite == null)
    {
        Debug.LogWarning("StyleChangeUiController: スタイル画像が未設定です。現在の画像を維持します。");
    }

    return true;
}

public void UpdateDirection(float horizontalInput)
{
    if (_spriteRenderer == null)
    {
        return;
    }

    if (horizontalInput < 0f)
    {
        _spriteRenderer.flipX = true;
        _isFacingLeft = true;
    }
    else if (horizontalInput > 0f)
    {
        _spriteRenderer.flipX = false;
        _isFacingLeft = false;
    }
}
```
