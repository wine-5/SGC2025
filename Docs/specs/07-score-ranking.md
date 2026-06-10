# 仕様書 — スコア・ランキングシステム

**対象スクリプト**: `Manager/ScoreManager.cs`, `Manager/RankingManager.cs`  
**最終更新**: 2026-06-07

---

## 概要

敵撃破とタイル緑化でスコアを獲得し、ゲーム終了後にローカルランキングへ記録する。  
Steam 版では緑化度（%）を主指標に刷新し、Steam Leaderboard と連携する予定。

---

## スコア構成（現行）

| 種別 | 加算タイミング | 基本値 |
|------|-------------|-------|
| 敵撃破スコア | 敵が死亡した時 | `enemyKillPoint`（デフォルト 50）|
| 緑化スコア | タイルが草地に変わった時 | `normalTilePoint`（デフォルト 100）/ `highScoreTileMultiplier` 倍 |

### スコア倍率

- `ItemManager.IsEffectActive(ScoreMultiplier)` が true の場合、スコアに `GetEffectValue(ScoreMultiplier)` の倍率を掛ける
- 倍率適用後の値が UI ポップアップ表示とランキングに使われる

---

## スコア取得フロー

### 敵撃破スコア

```
EnemyController.HandleDeath()
  └─ EnemyEvents.TriggerEnemyDestroyed（スコア値付き）
       └─ ScoreManager.OnEnemyScoreAdded（購読）
            ├─ ScoreMultiplier 倍率を適用
            ├─ scoreEnemy に加算
            └─ EnemyEvents.TriggerEnemyScoreAdded（UI 用）発火
```

### 緑化スコア

```
GroundManager.DrawSingleTile()
  └─ GroundEvents.TriggerGroundGreenified（スコア値付き）
       └─ ScoreManager.OnGreenScoreAdded（購読）
            ├─ ScoreMultiplier 倍率を適用
            ├─ scoreGreen に加算
            └─ GroundEvents.TriggerGreenScoreAdded（UI 用）発火
```

---

## スコアの種別

| メソッド | 内容 |
|---------|------|
| `GetEnemyScore()` | 敵撃破スコアの合計 |
| `GetGreenScore()` | 緑化スコアの合計 |
| `GetTotalScore()` | 上記2つの合計 |
| `SaveGreeningRate(float)` | 緑化度（0〜1）を保存（リザルト表示用）|
| `GetGreeningRate()` | 保存済み緑化度を返す |
| `ResetScore()` | 全スコアを 0 にリセット（ゲーム開始時）|

`UseDontDestroyOnLoad = true`（リザルト画面でのスコア参照のためシーンをまたいで保持）

---

## ローカルランキング（現行）

### 仕様

| 項目 | 内容 |
|------|------|
| 最大記録数 | 3件（`MAX_RANK = 3`）|
| 保存形式 | JSON（`Application.persistentDataPath/ranking.json`）|
| ソート順 | 緑化率降順 → スコア降順 |
| 記録データ | プレイヤー名・スコア・緑化率 |

### RankingManager フロー

```
ResulltUI → NameInputUI（名前入力）
  └─ NameInputUI.OnSubmit()
       └─ RankingManager.AddScore(name, score, greeningRate)
            ├─ ScoreData を追加
            ├─ 降順ソート
            ├─ MAX_RANK を超えるエントリを削除
            └─ JSON ファイルに保存
  └─ RankingUI.UpdateScore()（ランキング再描画）
```

---

## Steam 版への移行予定

| 現行 | Steam 版 |
|------|---------|
| スコア（`GetTotalScore()`）が主指標 | **緑化度（%）** が主指標に変更 |
| ローカル JSON ランキング（3件） | **Steam Leaderboard** にグローバル登録 |
| `RankingManager` でローカル保存 | `ISaveService` + `ILeaderboardService` に抽象化 |

---

## 依存関係

```
ScoreManager
  ├─ EnemyEvents（撃破スコア購読）
  ├─ GroundEvents（緑化スコア購読）
  └─ ItemManager（倍率取得）

RankingManager（単独）
  └─ Application.persistentDataPath（JSON 保存先）
```
