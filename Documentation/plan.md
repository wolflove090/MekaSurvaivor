# スタイル変更機能 実装計画

## 実装方針
- 既存の `WeaponUpgradeUiController`（通常レベルの武器強化）に対して、3の倍数レベル専用の `StyleChangeUiController` を追加する。
- どちらも同一UXML/USS（3カード）を使い回し、カード文言だけを用途別に設定する。
- プレイヤーステータス差し替えを可能にするため、`CharacterStats` に `CharacterStatsData` 適用APIを追加する。
- スタイル適用時に `HealthComponent` で「消費HP維持」の再計算を実行し、`PlayerController` で移動速度を再同期する。

## 変更対象ファイル一覧
- `Assets/Scripts/UI/StyleChangeUiController.cs`（新規）
  - レベル3ごとの表示制御
  - 巫女/メイド/セレブ文言の設定
  - 選択時のステータス適用
- `Assets/Scripts/UI/WeaponUpgradeUiController.cs`
  - 3の倍数レベルではUIを開かない分岐追加（通常レベルのみ表示）
- `Assets/Scripts/Character/Stats/CharacterStats.cs`
  - `CharacterStatsData` 差し替えAPI追加
  - ステータス更新イベント追加
- `Assets/Scripts/Character/HealthComponent.cs`
  - スタイル変更時のHP再計算API追加（消費HP維持）
- `Assets/Scripts/Character/Player/PlayerController.cs`
  - ステータス変更イベント購読
  - 移動速度再反映処理追加

## 処理フロー
1. `GameEvents.OnPlayerLevelUp(newLevel)` を `WeaponUpgradeUiController` と `StyleChangeUiController` が受信。
2. `newLevel % 3 != 0` の場合は武器強化UIのみ開く。`newLevel % 3 == 0` の場合はスタイル変更UIのみ開く。
3. スタイル変更UIカード押下でスタイル種別（巫女/メイド/セレブ）を決定。
4. 対応する `CharacterStatsData` を `CharacterStats.ApplyStatsData(data)` でプレイヤーへ適用。
5. 適用直前の `previousCurrentHp` / `previousMaxHp` と新 `newMaxHp` から消費HP維持で現在HPを再計算。
6. `PlayerController` がステータス変更イベントを受けて移動速度を再反映。
7. UIを閉じてゲーム進行を再開。

## リスクと対策
- リスク: スタイルアセット未設定で適用不能。
  - 対策: Inspector参照未設定時は警告を出して適用を中断する。
- リスク: MaxHP変更時の現在HP再計算ミス。
  - 対策: `currentHp = max(1, newMaxHp - (previousMaxHp - previousCurrentHp))` を共通APIで計算する。
- リスク: 既存武器強化文言との混在。
  - 対策: 武器強化UIとスタイル変更UIを別コンポーネントとして分離し、責務を固定する。

## 検証方針
- Play Modeでレベルを上げ、2→3→4の順にUI表示条件を確認。
- 各スタイルを1回ずつ選択し、GameScreen UIのHP/POW/SPD値反映を確認。
- HP再計算の確認: `5/10` から巫女選択後に `10/15` になることを確認。
- UI表示時の停止と、選択後の再開を確認。

## コードスニペット（変更イメージ）
```csharp
void OnPlayerLevelUp(int newLevel)
{
    if (newLevel <= 0 || newLevel % 3 == 0)
    {
        return;
    }

    OpenUpgradeUi();
}

public void ApplyStyleAndKeepConsumedHp(CharacterStatsData newStatsData)
{
    int previousCurrentHp = _currentHp;
    int previousMaxHp = MaxHp;

    _characterStats.ApplyStatsData(newStatsData);
    int consumedHp = Mathf.Max(0, previousMaxHp - previousCurrentHp);
    _currentHp = Mathf.Max(1, MaxHp - consumedHp);
}
```
