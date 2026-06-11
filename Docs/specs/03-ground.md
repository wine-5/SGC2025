# 仕様書 — 地面・緑化システム

**対象スクリプト**: `InGame/Ground/GroundManager.cs`, `InGame/Ground/GroundDataSO.cs`  
**最終更新**: 2026-06-07

---

## 概要

グリッド状のマップタイルを管理し、敵撃破・アイテム効果をトリガーに **草地（緑化）** へ変換する。  
緑化度（0〜100%）がゲームの主要指標。

---

## マップ構成

### グリッド仕様

| パラメータ | デフォルト値 | 備考 |
|-----------|------------|------|
| `columns` | 60 | 横タイル数 |
| `rows` | 45 | 縦タイル数 |
| `cellWidth` / `cellHeight` | Inspector or 自動計算 | `autoCalculateSize = true` の場合は `tileAspect` から算出 |

### マップ座標

- `MapCenterPosition` : マップ中心ワールド座標（プレイヤースポーン位置）
- `MapWorldSize` : マップ全体のワールドサイズ
- `MapMaxWorldPosition` : マップ右上端のワールド座標（移動範囲制限・スポーン位置計算に使用）

---

## タイルの状態

| 状態 | 内容 |
|------|------|
| 初期（砂地など） | `tilePrefab` で生成 |
| 緑化済み（草地） | `grassTilePrefab` に置き換え |
| 破壊済み（砂地に戻す） | 破壊者が通った場所は砂地に戻る |

一度緑化したタイルは、破壊者が通るまでは変わらない。  
破壊者に破壊されたタイルは砂地に戻り、再度緑化可能。

---

## 緑化処理

### 1タイル緑化（`DrawGround`）

```
DrawGround(worldPosition)
  └─ SearchCellIndex(worldPosition) → グリッドインデックス算出
       └─ DrawSingleTile(index)
            ├─ grassTilePrefab に置き換え
            ├─ EffectFactory.CreateEffect（草地エフェクト生成）
            ├─ AudioManager.PlaySE（SE再生）
            └─ GroundEvents.TriggerGroundGreenified 発火
```

### 3×3範囲緑化（`DrawGroundArea`）

`DrawGroundArea(center)` : 指定座標を中心に 3×3 の9タイルを `DrawGround` で一括緑化。  
アイテム `AreaGreenify` が有効な場合、敵撃破時に呼ばれる。

---

## 緑化度の喪失（破壊者メカニクス）

### 破壊者による破壊

破壊者（Destroyer）敵が通った場所は、緑化済みのタイルが砂地に戻る。

```
DestroyerMovement.Update()
  └─ 毎フレーム現在位置の周辺タイルをチェック
       └─ 3×3 範囲内の緑化済みタイルを砂地に戻す
            ├─ tilePrefab に置き換え
            └─ GroundEvents.TriggerGroundDestroyed 発火
```

### 破壊されたタイルの再緑化

破壊者に破壊されたタイルは砂地状態となり、プレイヤーの敵撃破で再度緑化可能。  
緑化度には反映される。

### 破壊者撃破時のボーナス

破壊者を撃破した位置を中心に、4×4 の範囲が自動的に緑化される。

```
破壊者撃破位置を (x, y) とした場合
(x-2, y-2) から (x+1, y+1) の 4×4 タイルが一括緑化
  ├─ EffectFactory.CreateEffect で視覚的フィードバック
  └─ GroundEvents.TriggerGroundGreenified 発火
```

**ポイント報酬**：
```
破壊者撃破ポイント = 基本ポイント × 2.0（倍率予定）
```

---

## 緑化度

```csharp
GetGreenificationRate() → float（0.0〜1.0）
  = 緑化済みタイル数 / 総タイル数（columns × rows）
```

- `ScoreManager.SaveGreeningRate()` でリザルト画面に受け渡し
- Steam Leaderboard 送信値としても使用予定

---

## GroundEvents

| イベント | 型 | タイミング |
|---------|-----|----------|
| `OnGroundGreenified` | `Action<Vector3, int>` | 1タイル緑化時（位置・スコア値を渡す） |
| `OnGreenScoreAdded` | `Action<Vector3, int>` | UI スコアポップアップ用（倍率適用後） |

---

## プレイヤースポーン位置

`GroundManager.GetPlayerSpawnPosition()` → `groundData.MapCenterPosition` を返す。  
`Player.Start()` から呼ばれ、プレイヤーの初期位置をマップ中央に設定する。

---

## 依存関係

```
GroundManager
  ├─ GroundDataSO（マップ設定）
  ├─ EffectFactory（草地エフェクト生成）
  ├─ AudioManager（SE 再生）
  └─ GroundEvents（緑化通知発火）
```
