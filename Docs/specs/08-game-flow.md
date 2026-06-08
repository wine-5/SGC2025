# 仕様書 — ゲーム進行管理

**対象スクリプト**: `Manager/InGameManager.cs`, `Manager/GameManager.cs`, `Manager/PauseManager.cs`, `Manager/SceneController.cs`  
**最終更新**: 2026-06-07

---

## 概要

インゲームの開始〜終了までの進行フローを管理する。  
カウントダウン・タイマー・ゲームオーバー・シーン遷移を各 Manager が担当する。

---

## ゲームの進行フロー

```
InGame シーン読み込み
  └─ InGameManager.Init()
       ├─ PlayerCharacter.OnPlayerDeath を購読
       └─ カウントダウン開始（startCountDownTime: デフォルト 4 秒）

カウントダウン中
  ├─ プレイヤー操作不能（IsCountingDown = true）
  ├─ 敵スポーン停止
  └─ カウントダウン完了 → OnCountDownFinished 発火 → ゲーム開始

ゲームプレイ中
  ├─ CurrentGameTime が加算される
  ├─ WaveManager が経過時間を参照して Wave を更新
  └─ RemainingGameTime が 0 になる → タイムアップ

タイムアップ
  └─ InGameManager.UpdateGameTimer()
       ├─ AudioManager.StopBGM()
       ├─ OnGameTimeUp 発火
       └─ GameManager.LoadResultScene()（遅延あり）

プレイヤー死亡
  └─ InGameManager.HandlePlayerDeath()
       ├─ AudioManager.StopBGM()
       ├─ OnGameOver 発火
       └─ GameManager.LoadResultScene()（gameOverDelay 秒後）
```

---

## InGameManager

| パラメータ | デフォルト値 | 内容 |
|-----------|------------|------|
| `startCountDownTime` | 4 秒 | ゲーム開始前のカウントダウン時間 |
| `gameTimeLimit` | 300 秒（5分）| ゲームの制限時間 |

### プロパティ

| プロパティ | 内容 |
|-----------|------|
| `IsGameOver` | ゲームオーバー状態かどうか |
| `IsCountingDown` | カウントダウン中かどうか |
| `GameTimeLimit` | ゲーム制限時間 |
| `CurrentGameTime` | ゲーム開始からの経過時間 |
| `RemainingGameTime` | 残り時間 |
| `CountDownTimer` | カウントダウン残り時間 |

### イベント

| イベント | タイミング |
|---------|----------|
| `OnGameOver` | プレイヤー死亡時 |
| `OnCountDownFinished` | カウントダウン完了時 |
| `OnGameTimeUp` | 制限時間切れ時 |

`UseDontDestroyOnLoad = false`

---

## GameManager

シーン遷移のトリガーとなる薄いマネージャー。

| メソッド | 処理 |
|---------|------|
| `LoadResultScene()` | `GroundManager.GetGreenificationRate()` を `ScoreManager.SaveGreeningRate()` で保存後、`gameOverDelay` 秒後に Result シーンへ遷移 |

`UseDontDestroyOnLoad = true`

---

## PauseManager

| メソッド | 処理 |
|---------|------|
| `PauseGame()` | ポーズパネル表示・`Time.timeScale = 0`・`OnPause` 発火 |
| `ResumeGame()` | ポーズパネル非表示・`Time.timeScale = 1`・`OnResume` 発火 |
| `TogglePause()` | 現在の状態に応じて切り替え |

- ポーズ時は `firstPauseButton` にフォーカスを設定（ゲームパッド対応）
- **ESC / Option ボタン** でポーズ（入力は `PlayerInputSet` 経由）

### イベント

| イベント | タイミング |
|---------|----------|
| `OnPause` | ポーズ時 |
| `OnResume` | 再開時 |

`UseDontDestroyOnLoad = false`

---

## SceneController

シーン遷移を一元管理するシングルトン。

| シーン名 | enum 値 |
|---------|--------|
| タイトル画面 | `SceneName.Title` |
| インゲーム | `SceneName.InGame` |
| リザルト画面 | `SceneName.Result` |

| メソッド | 処理 |
|---------|------|
| `LoadScene(SceneName)` | `SceneManager.LoadScene` を呼び出す |
| `LoadResultScene()` | `LoadScene(SceneName.Result)` のショートカット |

`UseDontDestroyOnLoad = true`

---

## 依存関係

```
InGameManager
  ├─ PlayerCharacter（死亡イベント購読）
  ├─ PauseManager（ポーズ状態参照）
  └─ AudioManager（BGM 停止）

GameManager
  ├─ GroundManager（緑化度取得）
  ├─ ScoreManager（緑化度保存）
  ├─ SceneController（シーン遷移）
  └─ AudioManager（BGM 停止）
```
