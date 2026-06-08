# 仕様書 — プレイヤーシステム

**対象スクリプト**: `InGame/Player/` 以下全て  
**最終更新**: 2026-06-07

---

## 概要

プレイヤーキャラクター（蝶）の移動・射撃・ダメージ・ステート管理を担う。  
`PlayerCharacter`（`Player.cs`）が中核で、内部に **ステートマシン** を持ち状態を管理する。

---

## ステートマシン

### 状態一覧

| 状態 | クラス | 遷移条件 |
|------|--------|---------|
| 待機（Idle） | `PlayerIdleState` | 移動入力がゼロ |
| 移動（Move） | `PlayerMoveState` | 移動入力が入力された時 |

### 継承構造

```
EntityState（abstract）
  └─ PlayerFlyingState
        ├─ PlayerIdleState
        └─ PlayerMoveState
```

- `EntityState.Enter()` / `Exit()` で Animator の BoolParameter を ON/OFF
- `StateMachine.ChangeState()` で現ステートの `Exit` → 新ステートの `Enter` を呼ぶ

---

## 移動

### 仕様

- **WASD / 左スティック** で上下左右の自由移動
- `Rigidbody2D.linearVelocity` に `moveSpeed × 正規化入力` を設定
- 移動方向に `transform.up` が向くよう自動回転（`rb.linearVelocity` 方向を参照）
- マップ端に達した場合、`GroundManager` のマップ範囲で位置をクランプ（マージン `0.5f`）
- **カウントダウン中は操作不能**（`InGameManager.IsCountingDown` チェック）
- **ポーズ中は操作不能**（`PauseManager` で `timeScale = 0` による停止）

### パラメータ

| パラメータ | デフォルト値 | 備考 |
|-----------|-------------|------|
| `moveSpeed` | Inspector 設定 | SpeedBoost アイテム適用時に倍率変更 |
| マップ端マージン | `0.5f` | `PlayerMoveState.BOUNDARY_MARGIN` |

---

## 射撃

詳細は [武器システム仕様書](07_WeaponSystem.md) を参照。

- **スペース / 右ボタン** で発射
- 弾は `firePoint` から `BulletFactory` 経由で生成
- **カウントダウン中は射撃不能**

---

## HP・ダメージ

### 仕様

| 項目 | 内容 |
|------|------|
| 最大HP | `maxHealth`（Inspector 設定、デフォルト 100） |
| ダメージ量 | `damage`（Inspector 設定、デフォルト 10） |
| 無敵時間 | `mutekiTime`（Inspector 設定）秒間 |
| ダメージ判定 | Enemy レイヤーのオブジェクトに触れた瞬間（`OnTriggerEnter2D`） |

### ダメージフロー

```
OnTriggerEnter2D（Enemy接触）
  └─ Damage()
       ├─ 無敵中なら無視（早期 return）
       ├─ TakeDamage(damage)
       │    ├─ currentHealth -= damage
       │    └─ HP ≤ 0 → OnPlayerDeath 発火
       ├─ nowMutekiTime = mutekiTime（無敵開始）
       └─ OnPlayerDamaged?.Invoke(hpRate)  ※要修正：二重発火バグあり
```

> **既知バグ**: `Damage()` → `TakeDamage()` 内でも `OnPlayerDamaged` が発火するため二重発火が発生する。`Damage()` 内の `OnPlayerDamaged?.Invoke` を削除する必要がある。

### イベント

| イベント | 型 | タイミング |
|---------|-----|-----------|
| `PlayerCharacter.OnPlayerDamaged` | `static event Action<float>` | ダメージ時（HP割合を渡す） |
| `PlayerCharacter.OnPlayerDeath` | `static event Action` | HP が 0 以下になった時 |

---

## 視覚フィードバック

### ScreenFlashEffect

- `PlayerCharacter.OnPlayerDamaged` を購読
- ダメージ時に画面をフラッシュ
- HP が `lowHpThreshold` 以下の場合は `lowHpFlashColor`、通常は `normalFlashColor` を使用

### SpriteBlinkEffect

- `PlayerCharacter.OnPlayerDamaged` を購読
- 無敵時間中（`player.IsInvincible == true`）にスプライトを点滅
- 無敵時間終了後は元の状態に戻す

---

## アイテム効果

| アイテム | 効果 |
|---------|------|
| `SpeedBoost` | `moveSpeed` に `effectValue` の倍率を適用 |

- `ItemManager.OnItemEffectActivated` を購読して効果を適用
- `ItemManager.OnItemEffectExpired` を購読して効果を解除

---

## PlayerDataProvider

- `Singleton` で全シーンから `PlayerTransform` を参照可能にするプロバイダー
- `Player.Start()` で `RegisterPlayer(transform)` を呼び、自身の Transform を登録
- `Player.OnDisable()` で `UnregisterPlayer()` を呼び解除
- `OnPlayerRegistered / OnPlayerUnregistered` イベントで変化を通知

---

## 実装済みインターフェース

| インターフェース | 用途 |
|---------------|------|
| `IPlayerHealth` | ヘルスシステム（`PlayerHealthHandler` で実装済みだが未統合）|
| `IPlayerInput` | 入力抽象化（定義済み・未使用）|
| `IPlayerMovement` | 移動抽象化（定義済み・未使用）|

> **NOTE**: `PlayerHealthHandler` は `IPlayerHealth` を正しく実装しているが、`Player.cs` がインラインでヘルスを管理しており未統合。リファクタリング Phase 1 で統合予定。

---

## 依存関係

```
PlayerCharacter
  ├─ StateMachine / PlayerIdleState / PlayerMoveState
  ├─ PlayerWeaponSystem（射撃）
  ├─ PlayerDataProvider（Transform 共有）
  ├─ GroundManager（スポーン位置・移動範囲）
  ├─ InGameManager（カウントダウン状態）
  ├─ PauseManager（ポーズ状態）
  ├─ ItemManager（アイテム効果）
  └─ AudioManager（SE 再生）
```
