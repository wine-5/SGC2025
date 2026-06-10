# 仕様書 — エフェクトシステム / ObjectPool

**対象スクリプト**: `Effect/` 以下全て、`System/ObjectPool.cs`  
**最終更新**: 2026-06-07

---

## エフェクトシステム

### 概要

ゲーム中のビジュアルエフェクト（草地回復・アイテム効果など）を ObjectPool で生成・返却する。

### エフェクト種別

| EffectType | 使用シーン |
|-----------|-----------|
| `SpeedBoostEffect` | SpeedBoost アイテム取得時 |
| `GrassRestorationEffect` | タイル緑化時 |
| `AreaGreenifyEffect` | AreaGreenify アイテム効果発動時 |

### EffectDataSO

各エフェクトの prefab を ScriptableObject で一元管理。  
`EffectData` クラス（`EffectType` + `GameObject effectPrefab`）のリストを保持。

### EffectFactory（`Singleton`）

| メソッド | 処理 |
|---------|------|
| `CreateEffect(type, position, duration, followTarget)` | 辞書からプレハブを取得 → ObjectPool から取り出し → `EffectController.Initialize()` |

`UseDontDestroyOnLoad = false`

### EffectController

エフェクト GameObject に付くコンポーネント。

| パラメータ | 内容 |
|-----------|------|
| `followOffset` | 追従対象からのオフセット |
| `followSmooth` | 追従の滑らかさ |
| `followRotation` | 対象の回転に追従するか |

| メソッド | 処理 |
|---------|------|
| `Initialize(target, duration)` | 追従対象と持続時間を設定 |
| `ReturnToPool()` | `EffectFactory` 経由で ObjectPool に返却 |

`Update` 毎に追従処理を実行し、持続時間が切れたら自動で `ReturnToPool()`。

---

## ObjectPool（`TechC.ObjectPool`）

### 概要

Prefab を事前にインスタンス化してプールに蓄え、生成・破棄のコストを削減する汎用プールシステム。  
Bullet・Enemy・Item・Effect すべてがこのシステムを使用する。

### 設定（Inspector）

`ObjectPoolItem` のリストで設定。

| フィールド | 内容 |
|-----------|------|
| `name` | プール識別名 |
| `prefab` | 生成するプレハブ |
| `parent` | 生成先の親 Transform |
| `initialSize` | 初期生成数 |

### 主なメソッド

| メソッド | 処理 |
|---------|------|
| `AddToPool(name, parent, prefab, size)` | 新しいプレハブをプールに追加 |
| `GetObject(prefab, position, rotation)` | プールから取り出してアクティブ化（空なら自動拡張）|
| `GetObjectByName(name)` | 名前でプレハブを特定して取得 |
| `ReturnObject(gameObject)` | 非アクティブ化してプールに返却 |

### 自動拡張

`autoExpand = true` の場合、プールが空の時に `expandSize` 個を追加生成する。

---

## Singleton 基底クラス

すべての Manager・Factory が継承する汎用シングルトン。

### 挙動

| 状況 | 動作 |
|------|------|
| インスタンスが未登録 | 自身を Instance に設定 → `Init()` 呼び出し → `DontDestroyOnLoad`（`UseDontDestroyOnLoad = true` の場合）|
| 同名インスタンスが既に存在・`DontDestroyOnLoad` あり | 新しい自身を破棄（既存を維持）|
| 同名インスタンスが既に存在・`DontDestroyOnLoad` なし | 古いインスタンスを破棄して自身に置き換え |

### サブクラスでのカスタマイズ

| virtual メンバー | 内容 |
|----------------|------|
| `UseDontDestroyOnLoad` | `true` でシーンをまたいで保持（デフォルト `true`）|
| `DestroyTargetGameObject` | 重複時に GameObject ごと破棄するか（デフォルト `false`）|
| `Init()` | Awake 時の初期化処理（サブクラスでオーバーライド）|
| `OnDestroy()` | 破棄時の後処理 |

### シングルトンの DontDestroyOnLoad 設定一覧

| クラス | DontDestroyOnLoad |
|--------|------------------|
| `GameManager` | ✅ true |
| `SceneController` | ✅ true |
| `ScoreManager` | ✅ true |
| `AudioManager` | ✅ true |
| `InGameManager` | ❌ false |
| `WaveManager` | ❌ false |
| `GroundManager` | ❌ false |
| `PauseManager` | ❌ false |
| `ItemManager` | ❌ false |
| `EnemyFactory` | ❌ false |
| `BulletFactory` | ❌ false |
| `EffectFactory` | ❌ false |
| `ItemFactory` | ❌ false |
| `PlayerDataProvider` | ❌ false |
