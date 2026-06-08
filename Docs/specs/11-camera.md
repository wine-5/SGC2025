# 仕様書 — カメラシステム

**対象スクリプト**: `InGame/Camera/` 以下全て  
**最終更新**: 2026-06-07

---

## 概要

プレイヤーを追従するカメラと、ダメージ時のシェイク演出を管理する。

---

## カメラ追従（CameraMovement）

| パラメータ | 内容 |
|-----------|------|
| `target` | 追従対象の Transform（通常はプレイヤー）|
| `smoothSpeed` | 追従の滑らかさ（Lerp の補間係数）|
| `orthographicSize` | 正投影カメラのサイズ |
| `fieldOfView` | 透視投影カメラの視野角 |

- `LateUpdate` で `Vector3.Lerp(current, target, smoothSpeed × deltaTime)` によりスムーズ追従
- カメラの `orthographicSize` / `fieldOfView` は毎フレーム Inspector の値に同期

---

## カメラシェイク（CameraShake）

### シェイク設定（Inspector）

| パラメータ | 内容 |
|-----------|------|
| `duration` | シェイク持続時間（秒）|
| `magnitude` | シェイクの振幅 |
| `lowHpMultiplier` | 低 HP 時の振幅倍率 |
| `lowHpThreshold` | 低 HP の閾値（HP 率）|

### シェイク強度の計算

```
GetMagnitudeByHpRate(hpRate)
  → hpRate ≤ lowHpThreshold の場合: magnitude × lowHpMultiplier
  → それ以外: magnitude
```

### シェイク発動フロー

```
PlayerCharacter.OnPlayerDamaged（HP 率を渡す）
  └─ CameraManager.TriggerShake(hpRate)
       └─ CameraShake.GetMagnitudeByHpRate(hpRate) で強度決定
            └─ シェイクコルーチン開始
                 └─ LateUpdate でランダムオフセットを transform.position に加算
```

---

## CameraManager

`CameraMovement` と `CameraShake` を統合し、プレイヤーダメージイベントへの購読を担う。

| メソッド | 処理 |
|---------|------|
| `TriggerShake(float hpRate)` | HP 率に応じた強度でシェイク開始 |

- `PlayerCharacter.OnPlayerDamaged` を `OnEnable/OnDisable` で購読・解除

---

## 依存関係

```
CameraManager
  ├─ CameraMovement（追従処理）
  ├─ CameraShake（シェイク設定・強度計算）
  └─ PlayerCharacter（ダメージイベント購読）
```
