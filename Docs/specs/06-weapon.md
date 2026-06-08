# 仕様書 — 武器システム

**対象スクリプト**: `InGame/Player/Weapon/` 以下全て  
**最終更新**: 2026-06-07

---

## 概要

プレイヤーが敵を撃破するほど武器が自動強化され、発射方向数が増加する仕組み。  
弾の生成は ObjectPool + Factory パターンで管理する。

---

## 武器レベルアップ

### レベルアップ条件

- 敵撃破数が `enemiesPerUpgrade`（`WeaponUpgradeDataSO`）の倍数に達するたびにレベルアップ
- レベルアップ時に `OnWeaponLevelUp(level)` イベントを発火

### WeaponUpgradeDataSO

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `enemiesPerUpgrade` | `int` | レベルアップに必要な撃破数 |
| `levelData` | `WeaponLevelData[]` | 各レベルの設定配列 |

### WeaponLevelData

| フィールド | 型 | 内容 |
|-----------|-----|------|
| `level` | `int` | レベル番号 |
| `bulletDirections` | `int` | 発射方向数（均等に円周配置）|

---

## 発射処理

### 発射フロー

```
PlayerInputSet（スペース / ボタン入力）
  └─ PlayerWeaponSystem.Fire()
       ├─ BulletFactory.CreateCircularBullets(firePoint, directions, bulletData)
       │    └─ directions 方向に均等角度で弾を生成
       └─ AudioManager.PlaySE(PlayerShoot)
```

- `firePoint`（Transform）から弾を生成
- カウントダウン中は射撃不能（`InGameManager.IsCountingDown` チェック）

---

## 弾の仕様（BulletController）

### BulletDataSO パラメータ

| パラメータ | 内容 |
|-----------|------|
| `MoveSpeed` | 弾の移動速度 |
| `Damage` | ダメージ量 |
| `LifeTime` | 弾の生存時間（秒）|
| `BulletSize` | 弾のサイズ |
| `EnableRotation` | 回転演出の有無 |
| `RotationSpeed` | 回転速度 |
| `RotationDirection` | 回転方向（1 or -1）|

### 弾の消滅条件

| 条件 | 処理 |
|------|------|
| Enemy レイヤーのオブジェクトに接触 | `IDamageable.TakeDamage()` 呼び出し後に消滅 |
| ライフタイム経過 | 自動消滅 |
| マップ範囲外に出た | 自動消滅 |

消滅時はすべて ObjectPool に返却（`BulletFactory.ReturnBullet()`）。

### 回転演出（BulletRotationEffect）

`BulletController` とは独立したコンポーネントで回転を担当。

| パラメータ | 内容 |
|-----------|------|
| `rotationSpeed` | 回転速度 |
| `rotationAxis` | 回転軸 |
| `rotationDirection` | 方向（1 or -1）|
| `randomInitialRotation` | 初期角度のランダム化 |
| `rotateWhenInactive` | 非アクティブ中も回転するか |

---

## BulletFactory

| メソッド | 処理 |
|---------|------|
| `CreateBullet(position, direction, bulletData)` | 単発弾を生成 |
| `CreateCircularBullets(position, count, bulletData)` | 360° 均等割りで `count` 本の弾を一斉生成 |

---

## イベント

| イベント | 型 | タイミング |
|---------|-----|----------|
| `PlayerWeaponSystem.OnWeaponLevelUp` | `static event Action<int>` | レベルアップ時（新レベル番号）|
| `PlayerWeaponSystem.OnEnemyKilled` | `static event Action<int, int>` | 敵撃破時（撃破数・現レベル）|

---

## 依存関係

```
PlayerWeaponSystem
  ├─ WeaponUpgradeDataSO（レベルアップ設定）
  ├─ BulletDataSO（弾パラメータ）
  ├─ BulletFactory（弾生成）
  ├─ EnemyEvents（撃破数カウント）
  └─ AudioManager（SE 再生）

BulletController
  ├─ BulletDataSO（弾パラメータ）
  ├─ IDamageable（ダメージ適用）
  ├─ EnemyEvents（撃破通知）
  └─ GroundManager（マップ範囲チェック）
```
