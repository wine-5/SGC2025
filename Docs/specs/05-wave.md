# 仕様書 — Wave システム

**対象スクリプト**: `Manager/WaveManager.cs`, `InGame/Enemy/Spawning/WaveDataSO.cs`  
**最終更新**: 2026-06-07

---

## 概要

ゲーム経過時間に応じて敵の強さ・スポーン速度を段階的に引き上げる仕組み。  
`WaveDataSO` に各 Wave の設定を定義し、`WaveManager` が時間経過を監視して切り替える。

---

## Wave の進行

### 切り替え条件

- `InGameManager.CurrentGameTime`（ゲーム開始からの経過時間）を監視
- `waveInterval`（デフォルト30秒）ごとに Wave レベルを 1 上昇
- `useTestMode = true` の場合は `testWaveInterval`（デフォルト10秒）を使用

> **既知問題**: `useTestMode` のデフォルト値が `true` になっているため、本番ビルドでも短縮間隔で Wave が進行する。→ `false` への修正が必要。

### 最終 Wave の扱い

`WaveDataSO.loopLastWave = true` の場合、最終 Wave に達した後もその Wave を繰り返す。

---

## WaveDataSO

各 Wave の設定を ScriptableObject で管理。

### WaveData クラス

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `waveName` | `string` | Wave の識別名 |
| `waveLevel` | `int` | Wave レベル番号 |
| `spawnInterval` | `float` | 敵スポーン間隔（秒）|
| `maxEnemyCount` | `int` | 同時最大出現数 |
| `enemyConfigs` | `List<EnemySpawnConfigSO>` | 出現する敵の設定リスト |

---

## WaveManager の動作

### Wave 変更フロー

```
Update() → InGameManager.CurrentGameTime から期待 Wave レベルを計算
  └─ 現在 Wave と異なる場合 → ChangeWave(newLevel)
       ├─ CurrentWaveLevel を更新
       ├─ OnWaveChanged(waveLevel) 発火
       ├─ OnWaveDataChanged(waveData) 発火
       └─ FindObjectsByType<EnemySpawner> で全 Spawner に SetWaveLevel 通知
```

### 一時停止・停止

| メソッド | 動作 |
|---------|------|
| `PauseWaveProgression()` | Wave 進行を一時停止（`PauseManager.OnPause` 購読）|
| `ResumeWaveProgression()` | Wave 進行を再開（`PauseManager.OnResume` 購読）|
| `StopWaveProgression()` | Wave 進行を完全停止（ゲームオーバー・タイムアップ時）|

---

## イベント

| イベント | 型 | タイミング |
|---------|-----|----------|
| `OnWaveChanged` | `static event Action<int>` | Wave レベル変更時（レベル番号） |
| `OnWaveDataChanged` | `static event Action<WaveData>` | Wave レベル変更時（新 Wave データ） |

---

## 依存関係

```
WaveManager
  ├─ InGameManager（ゲーム経過時間）
  ├─ PauseManager（一時停止制御）
  ├─ WaveDataSO（Wave 定義）
  └─ EnemySpawner（Wave レベル通知）
```
