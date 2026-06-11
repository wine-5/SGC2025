# 仕様書 — 敵システム

**対象スクリプト**: `InGame/Enemy/` 以下全て  
**最終更新**: 2026-06-07

---

## 概要

敵の生成・移動・ダメージ・死亡・プール返却を管理する。  
移動アルゴリズムは **Strategy パターン** で差し替え可能な設計になっている。

---

## 敵の種類

| EnemyType | 移動タイプ | 特徴 |
|-----------|-----------|------|
| `NormalBoy` | `LinearChaser` | 直線的にプレイヤーを追う |
| `BigBoy` | `InertiaChaser` | 慣性をつけてゆっくり方向転換 |
| `OldMan` | `PredictiveChaser` | プレイヤーの進行先を予測して追う |
| `Excavator` | `ArcChaser` / `FixedDirection` | 円弧移動、または固定方向に直進 |
| `Destroyer`（ボス） | `LinearChaser` | プレイヤーを直線的に追う。通った場所の緑化を破壊する |

---

## データ定義（ScriptableObject）

### EnemyDataSO

各敵タイプのパラメータを ScriptableObject で管理。

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `enemyType` | `EnemyType` | 敵の種類 |
| `movementType` | `MovementType` | 移動アルゴリズムの種別 |
| `health` | `float` | 基本HP |
| `moveSpeed` | `float` | 基本移動速度 |
| `lifeTime` | `float` | 自動消滅までの時間（秒）|
| `baseScale` | `Vector3` | 基本スケール |
| `scaleGrowthRate` | `float` | Waveレベルごとのスケール上昇率 |

### BossDataSO（ボス専用）

破壊者（Destroyer）の専用パラメータを管理。

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `health` | `float` | 基本HP |
| `moveSpeed` | `float` | プレイヤーを追う速度 |
| `destructionRange` | `Vector2Int` | 緑化を破壊する範囲（デフォルト：3×3） |
| `damageToPlayer` | `float` | プレイヤーへのダメージ量 |
| `destroyColor` | `Color` | ビジュアル（赤色など） |
| `destroyScale` | `float` | サイズ倍率（通常敵より大きい） |
| `spawnWaveLevel` | `int` | スポーン開始ウェーブレベル（例：3） |
| `maxCountPerStage` | `int` | ステージ内の最大出現数 |

### パラメータのWaveスケーリング

`EnemyDataSO.GetScaledParameters(waveLevel)` で計算。

```
HP        = baseHP    × (1 + waveLevel × 0.1)  最大2倍まで
MoveSpeed = baseSpeed × (1 + waveLevel × 0.1)  最大2倍まで
Scale     = baseScale × (1 + waveLevel × scaleGrowthRate)
```

---

## 移動システム

### 移動戦略（Strategy パターン）

全戦略は `IMovementStrategy.Move(enemy, target, speed, deltaTime)` を実装。

| クラス | MovementType | 動作 |
|--------|-------------|------|
| `LinearMovementStrategy` | `LinearChaser` | プレイヤーへ直線移動 |
| `InertiaMovementStrategy` | `InertiaChaser` | `Vector3.Slerp` で徐々に方向転換（慣性感） |
| `PredictiveMovementStrategy` | `PredictiveChaser` | プレイヤーの速度ベクトルから移動先を予測して追従 |
| `ArcMovementStrategy` | `ArcChaser` | プレイヤー周囲を円弧状に周回 |
| ―（null）| `FixedDirection` | 戦略なし → `EnemyMovement` が下方向に直進 |

`MovementStrategyFactory.CreateStrategy(MovementType)` でインスタンスを生成。

### EnemyMovement

- 毎 `Update` で `CanMove` チェック後に以下の順で移動を決定：
  1. `targetPosition` が設定されている場合 → 固定目標へ直進（到達で停止・オーバーシュート時折り返し）
  2. `movementStrategy` が設定されている場合 → 戦略に委譲
  3. いずれもなし → 下方向に `moveSpeed` で直進

---

## 生成システム

### 生成フロー

```
EnemySpawner.SpawnEnemy()
  └─ EnemyFactory.CreateRandomEnemy(position, waveLevel)
       └─ EnemySpawnConfigManager.SelectRandomEnemyData()
            └─ EnemySpawnConfigSO.SelectRandomEnemy()（重み付きランダム）
  └─ MovementStrategyFactory.CreateStrategy(movementType)
  └─ EnemyMovement.SetMovementStrategy(strategy)
  └─ EnemyAutoReturn.Initialize()
```

### スポーン位置

`EnemySpawnPositionManager` がマップの4辺境界からランダムに選択。  
`FixedDirection` の場合は `GetOppositeEdgePosition()` で反対側の辺へ向かわせる。

### スポーン間隔

`WaveManager.CurrentWave.spawnInterval` を `EnemySpawner.GetCurrentSpawnInterval()` で取得して使用。  
カウントダウン中はスポーン停止。

---

## ダメージ・死亡

### ダメージフロー

```
BulletController.OnTriggerEnter2D（弾がEnemy接触）
  └─ IDamageable.TakeDamage(damage)
       └─ EnemyController.TakeDamage(damage)
            ├─ currentHp -= damage
            ├─ EnemyEvents.TriggerEnemyDamage 発火
            └─ HP ≤ 0 → HandleDeath()
                 ├─ EnemyEvents.TriggerEnemyDestroyed 発火（スコア付き）
                 └─ EnemyFactory.ReturnEnemy()（ObjectPool へ返却）
```

### 自動消滅（EnemyAutoReturn）

毎 `Update` で以下を確認し、いずれか該当したら ObjectPool へ返却：
- ライフタイム（`EnemyController.LifeTime`）超過
- マップ範囲外（`GroundManager` のマップ境界と比較）

---

## インターフェース

| インターフェース | 実装クラス | 定義内容 |
|---------------|-----------|---------|
| `IDamageable` | `EnemyController` | HP・ダメージ受付・死亡イベント |
| `IEnemyParameters` | `EnemyController` | 移動速度・タイプ・ライフタイムなどの読み取り |
| `IMovable` | `EnemyController` | 移動システムへの依存抽象 |
| `IMovementStrategy` | 各戦略クラス | `Move()` メソッド |
| `ISpawnPositionProvider` | `EnemySpawnPositionManager`, `DirectCoordinateSpawnProvider` | スポーン位置の提供 |

---

## EnemyEvents（イベント一覧）

| イベント | 型 | 用途 |
|---------|-----|------|
| `OnEnemyDestroyed` | `Action` | 撃破カウント（武器レベルアップ用） |
| `OnEnemyDestroyedAtPosition` | `Action<Vector3>` | エフェクト生成位置 |
| `OnEnemyDestroyedWithScore` | `Action<int, Vector3>` | スコア加算 |
| `OnEnemyScoreAdded` | `Action<int, Vector3>` | UI スコアポップアップ（倍率適用後） |
| `OnEnemyDamageTaken` | `Action<GameObject, float>` | HPバー更新 |
| `OnEnemyHealthChanged` | `Action<GameObject, float, float>` | HP変化通知 |
| `OnEnemySpawned` | `Action<GameObject>` | スポーン通知 |
| `OnEnemyReturnedToPool` | `Action<GameObject>` | プール返却通知 |

---

## 依存関係

```
EnemyController
  ├─ EnemyDataSO（パラメータ）
  ├─ EnemyEvents（撃破・ダメージ通知）
  ├─ ScoreManager（スコア値取得）
  └─ EnemyFactory（プール返却）

EnemyMovement
  ├─ EnemyController（IMovable）
  ├─ IMovementStrategy（移動戦略）
  └─ PlayerDataProvider（プレイヤー位置）

EnemySpawner
  ├─ EnemyFactory（生成・返却）
  ├─ WaveManager（Waveデータ取得）
  ├─ InGameManager（カウントダウン状態）
  └─ MovementStrategyFactory（戦略生成）
```
