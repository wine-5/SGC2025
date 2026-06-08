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

一度緑化したタイルは元に戻らない。

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
