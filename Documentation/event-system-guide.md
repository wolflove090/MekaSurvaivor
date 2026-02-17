# GameEventsシステム使用ガイド

## 概要

`GameEvents` は、ゲーム内のコンポーネント間で疎結合な通信を実現する静的イベントシステムです。

## 利用可能なイベント

### プレイヤー関連

```csharp
// プレイヤーが移動した時
GameEvents.OnPlayerMoved += (Vector3 position) => { };

// プレイヤーがダメージを受けた時
GameEvents.OnPlayerDamaged += (int damage) => { };

// プレイヤーが死亡した時
GameEvents.OnPlayerDied += () => { };
```

### 敵関連

```csharp
// 敵がスポーンした時
GameEvents.OnEnemySpawned += (GameObject enemy) => { };

// 敵が死亡した時
GameEvents.OnEnemyDied += (GameObject enemy) => { };
```

### ゲーム状態

```csharp
// ゲームオーバーになった時
GameEvents.OnGameOver += () => { };
```

## 使用方法

### イベントの購読（リスナー登録）

```csharp
public class MyComponent : MonoBehaviour
{
    void OnEnable()
    {
        // イベントを購読
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
    }

    void OnDisable()
    {
        // イベントの購読を解除（メモリリーク防止）
        GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
    }

    void OnPlayerDamaged(int damage)
    {
        Debug.Log($"プレイヤーが{damage}ダメージを受けました");
    }
}
```

### イベントの発火

```csharp
// プレイヤー移動イベントを発火
GameEvents.RaisePlayerMoved(transform.position);

// プレイヤーダメージイベントを発火
GameEvents.RaisePlayerDamaged(10);

// プレイヤー死亡イベントを発火
GameEvents.RaisePlayerDied();

// 敵スポーンイベントを発火
GameEvents.RaiseEnemySpawned(enemyGameObject);

// 敵死亡イベントを発火
GameEvents.RaiseEnemyDied(enemyGameObject);

// ゲームオーバーイベントを発火
GameEvents.RaiseGameOver();
```

## ベストプラクティス

### 1. 必ず購読解除する

```csharp
void OnEnable()
{
    GameEvents.OnPlayerDied += HandlePlayerDeath;
}

void OnDisable()
{
    // 必ず購読解除してメモリリークを防ぐ
    GameEvents.OnPlayerDied -= HandlePlayerDeath;
}
```

### 2. ラムダ式の注意点

```csharp
// ❌ 悪い例：ラムダ式は購読解除できない
void OnEnable()
{
    GameEvents.OnPlayerDied += () => Debug.Log("Player died");
}

// ✅ 良い例：メソッドを使用
void OnEnable()
{
    GameEvents.OnPlayerDied += HandlePlayerDeath;
}

void OnDisable()
{
    GameEvents.OnPlayerDied -= HandlePlayerDeath;
}

void HandlePlayerDeath()
{
    Debug.Log("Player died");
}
```

### 3. シーン遷移時のクリア

```csharp
// シーン遷移前に全イベントをクリア
void OnDestroy()
{
    if (isLastObjectInScene)
    {
        GameEvents.ClearAllEvents();
    }
}
```

## 実装例

### UIの更新

```csharp
public class HealthUI : MonoBehaviour
{
    [SerializeField] Text _healthText;

    void OnEnable()
    {
        GameEvents.OnPlayerDamaged += UpdateHealthDisplay;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDamaged -= UpdateHealthDisplay;
    }

    void UpdateHealthDisplay(int damage)
    {
        // プレイヤーのHPを取得して表示を更新
        if (PlayerController.Instance != null)
        {
            _healthText.text = $"HP: {PlayerController.Instance.CurrentHp}";
        }
    }
}
```

### サウンド再生

```csharp
public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioClip _damageSound;
    [SerializeField] AudioClip _deathSound;
    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        GameEvents.OnPlayerDamaged += PlayDamageSound;
        GameEvents.OnEnemyDied += PlayDeathSound;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDamaged -= PlayDamageSound;
        GameEvents.OnEnemyDied -= PlayDeathSound;
    }

    void PlayDamageSound(int damage)
    {
        _audioSource.PlayOneShot(_damageSound);
    }

    void PlayDeathSound(GameObject enemy)
    {
        _audioSource.PlayOneShot(_deathSound);
    }
}
```

### スコアシステム

```csharp
public class ScoreManager : MonoBehaviour
{
    int _score;

    void OnEnable()
    {
        GameEvents.OnEnemyDied += AddScore;
    }

    void OnDisable()
    {
        GameEvents.OnEnemyDied -= AddScore;
    }

    void AddScore(GameObject enemy)
    {
        _score += 10;
        Debug.Log($"Score: {_score}");
    }
}
```

### パーティクルエフェクト

```csharp
public class EffectManager : MonoBehaviour
{
    [SerializeField] GameObject _explosionPrefab;

    void OnEnable()
    {
        GameEvents.OnEnemyDied += SpawnExplosion;
    }

    void OnDisable()
    {
        GameEvents.OnEnemyDied -= SpawnExplosion;
    }

    void SpawnExplosion(GameObject enemy)
    {
        if (enemy != null)
        {
            Instantiate(_explosionPrefab, enemy.transform.position, Quaternion.identity);
        }
    }
}
```

## トラブルシューティング

### イベントが発火しない

1. イベントを発火するコードが実行されているか確認
2. `Raise` メソッドを使用しているか確認
3. デバッグログを追加して確認

```csharp
GameEvents.OnPlayerDamaged += (damage) => Debug.Log($"Event received: {damage}");
```

### メモリリーク

1. `OnDisable` または `OnDestroy` で必ず購読解除
2. ラムダ式ではなくメソッドを使用
3. シーン遷移時に `ClearAllEvents()` を呼び出す

### イベントが複数回発火する

1. 購読が重複していないか確認
2. `OnEnable` と `OnDisable` のペアが正しいか確認
3. 同じオブジェクトが複数存在していないか確認

## まとめ

`GameEvents` システムを使用することで：

- ✅ コンポーネント間の疎結合な通信
- ✅ 新機能の追加が容易
- ✅ テストが簡単
- ✅ コードの保守性向上

ただし、以下に注意：

- ⚠️ 必ず購読解除する
- ⚠️ ラムダ式の使用に注意
- ⚠️ シーン遷移時のクリアを忘れずに
