# 仕様書 — オーディオシステム

**対象スクリプト**: `Audio/AudioManager.cs`, `Audio/AudioDataSO.cs`, `Audio/AudioTypes.cs`  
**最終更新**: 2026-06-07

---

## 概要

BGM・SE の再生・停止・音量制御を一元管理する。  
BGM はクロスフェード対応、SE は AudioSource プールで多重再生をサポートする。

---

## BGM 一覧

| BGMType | 再生シーン |
|---------|-----------|
| `None` | 再生なし |
| `Title` | タイトル画面（`TitleUI.Start()` から呼び出し）|
| `InGame` | インゲーム中（`InGameManager` から呼び出し）|
| `Result` | リザルト画面 |

---

## SE 一覧

| SEType | 再生タイミング |
|--------|-------------|
| `None` | 再生なし |
| `PlayerShoot` | プレイヤーが弾を発射した時 |
| `Grass` | タイルが緑化された時 |
| `TimeUp` | 制限時間切れ時 |
| `ButtonClick` | UI ボタンを押した時 |
| `PlayerDamage` | プレイヤーがダメージを受けた時 |
| `GetItem` | アイテムを取得した時 |
| `CountDown` | カウントダウン音 |

---

## AudioManager の機能

### BGM 再生

| メソッド | 処理 |
|---------|------|
| `PlayBGM(BGMType)` | フェードイン/アウトつきで BGM を再生。クロスフェードに対応（AudioSource × 2 を交互使用）|
| `StopBGM(bool fadeOut)` | BGM を停止（`fadeOut = true` ならフェードアウト）|

### SE 再生

| メソッド | 処理 |
|---------|------|
| `PlaySE(SEType)` | プールから空き AudioSource を取得して再生（多重再生対応）|

SE 用 AudioSource は初期 10 個をプールとして保持。

### 音量制御

| プロパティ | 対応 Mixer グループ |
|-----------|-----------------|
| `masterVolume` | マスターグループ |
| `bgmVolume` | BGM グループ |
| `seVolume` | SE グループ |

スライダー変更時に即時 `AudioMixerGroup` に反映（`SettingsUI` 経由）。

---

## AudioDataSO

BGM・SE の全クリップとパラメータを ScriptableObject で管理。

### SEAudioData

| フィールド | 内容 |
|-----------|------|
| `SEType` | SE 種別 |
| `AudioClip` | 音声クリップ |
| `volumeMultiplier` | 音量倍率 |

### BGMAudioData

| フィールド | 内容 |
|-----------|------|
| `BGMType` | BGM 種別 |
| `AudioClip` | 音声クリップ |
| `volumeMultiplier` | 音量倍率 |
| `loop` | ループ再生するか |
| フェード設定 | フェードイン/アウトの曲線・時間 |

辞書（`seDataDict`, `bgmDataDict`）でキャッシュし O(1) アクセスを実現。

---

## 依存関係

```
AudioManager（Singleton・DontDestroyOnLoad）
  ├─ AudioDataSO（クリップデータ）
  └─ AudioMixerGroup（UnityEngine.Audio）

呼び出し元（主なもの）
  ├─ PlayerWeaponSystem（PlayerShoot SE）
  ├─ PlayerCharacter（PlayerDamage SE）
  ├─ ItemController（GetItem SE）
  ├─ GroundManager（Grass SE）
  ├─ InGameManager / GameManager（BGM 停止）
  ├─ TitleUI（Title BGM）
  └─ SettingsUI（音量変更）
```
