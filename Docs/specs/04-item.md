# 仕様書 — アイテムシステム

**対象スクリプト**: `InGame/Item/` 以下全て  
**最終更新**: 2026-06-07

---

## 概要

ゲーム中にランダムでフィールドにアイテムが出現し、プレイヤーが触れると一定時間の効果を得る。  
生成・返却は ObjectPool で管理する。

---

## アイテム種類

| ItemType | 効果 | 持続 |
|----------|------|------|
| `SpeedBoost` | 移動速度を `effectValue` 倍に上昇 | 時間制限あり |
| `ScoreMultiplier` | 獲得ポイントを `effectValue` 倍に増加 | 時間制限あり |
| `AreaGreenify` | 敵撃破時に周囲 3×3 タイルを緑化 | 時間制限あり |

---

## データ定義（ItemDataSO）

`ItemData` クラスで各アイテムのパラメータを定義。

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `itemType` | `ItemType` | アイテム種別 |
| `itemName` | `string` | 表示名 |
| `description` | `string` | 説明文 |
| `duration` | `float` | 効果持続時間（秒）|
| `effectValue` | `float` | 効果量（倍率など）|
| `itemPrefab` | `GameObject` | 外観プレハブ |
| `spawnWeight` | `float` | 出現重み（高いほど出やすい）|

---

## アイテム生成フロー

```
ItemManager（自動スポーン：spawnInterval ごと）
  └─ SpawnRandomItem()
       └─ ItemSpawnSelector.SelectRandom()（重み付きランダム選択）
            └─ ItemFactory.SpawnItem(itemData, position)
                 └─ ObjectPool からプレハブ取得
                      └─ ItemController.Initialize(itemData)
```

スポーン位置はプレイヤー位置またはランダム位置（`SpawnRandomItemAt(Vector3)`）。

---

## アイテム取得フロー

```
ItemController.OnTriggerEnter2D（プレイヤーLayerに接触）
  └─ ItemManager.CollectItem(itemData)
       ├─ activeEffects[itemType] = 新しい ItemEffect 登録
       ├─ OnItemEffectActivated 発火（PlayerCharacter が購読→速度倍率適用 等）
       └─ AudioManager.PlaySE(GetItem)
  └─ ItemFactory.ReturnItem()（プールに返却）
```

---

## 効果の管理

- 有効な効果は `activeEffects（Dictionary<ItemType, ItemEffect>）` で保持
- 毎 `Update` で `CheckEffectExpiration()` が時間経過をチェック
- 時間切れ → `activeEffects` から削除 → `OnItemEffectExpired` 発火

### 同種アイテムの重複取得

同じ `ItemType` のアイテムを取得した場合、**既存の効果を上書き**（時間リセット）する。

---

## 各効果の処理

### SpeedBoost

- `PlayerCharacter` が `OnItemEffectActivated` を購読
- `ItemManager.GetEffectValue(SpeedBoost)` で倍率を取得して `moveSpeed` に適用
- `OnItemEffectExpired` で元の速度に戻す

### ScoreMultiplier

- `ScoreManager` が `ItemManager.IsEffectActive(ScoreMultiplier)` と `GetEffectValue` を参照
- 敵撃破・タイル緑化時のスコア計算時に倍率を掛ける

### AreaGreenify

- `ItemManager` が `EnemyEvents.OnEnemyDestroyedAtPosition` を購読
- 効果有効中に敵撃破 → `GroundManager.DrawGroundArea(enemyPosition)` を呼び出し

---

## アイテムの挙動（ItemController）

- フィールド上での回転演出（`rotationSpeed` で制御）
- ライフタイム（デフォルト30秒）経過で自動消滅（プール返却）

---

## ItemManager のイベント

| イベント | 型 | タイミング |
|---------|-----|----------|
| `OnItemEffectActivated` | `Action<ItemType, float, float>` | 効果開始時（種別・effectValue・duration）|
| `OnItemEffectExpired` | `Action<ItemType>` | 効果終了時 |

---

## 依存関係

```
ItemManager
  ├─ ItemSpawnSelector（ランダム選択）
  ├─ ItemFactory（生成・返却）
  ├─ GroundManager（AreaGreenify 効果）
  ├─ EnemyEvents（敵撃破位置を取得）
  └─ EffectFactory（取得エフェクト生成）

ItemController
  ├─ ItemManager（取得通知）
  ├─ ItemFactory（プール返却）
  └─ AudioManager（SE 再生）
```
