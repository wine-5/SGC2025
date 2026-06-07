# 蝶々反乱 — Steam リリース版 要件定義書

**バージョン**: 1.0  
**作成日**: 2026-06-07  
**ステータス**: Draft

---

## 目次

1. [プロジェクト概要](#1-プロジェクト概要)
2. [ゲームデザイン要件](#2-ゲームデザイン要件)
3. [機能要件](#3-機能要件)
4. [技術・アーキテクチャ要件](#4-技術アーキテクチャ要件)
5. [非機能要件](#5-非機能要件)
6. [開発フェーズ計画](#6-開発フェーズ計画)
7. [未解決事項（TBD）](#7-未解決事項tbd)

---

## 1. プロジェクト概要

| 項目 | 内容 |
|------|------|
| タイトル | 蝶々反乱 |
| ジャンル | 2D アクション |
| ターゲット | Steam（Windows） |
| コンセプト | 蝶が人間たちに奪われた自然を取り返していくゲーム |
| リリース戦略 | Early Access → 正式リリース |
| 開発体制 | 既存メンバー（一部）＋新規参入の可能性あり |

### コアバリュー

> 「緑化度（自然回復度）」という指標で自分の貢献を実感できる、かつ他プレイヤーとの比較・競争を楽しめる

---

## 2. ゲームデザイン要件

### 2.1 指標の刷新：スコア → 緑化度

現行の「スコア」を廃止し、**緑化度（%）** を主指標として再設計する。

| 現行 | Steam版 |
|------|---------|
| スコア（数値） | 緑化度（0〜100%） |
| スコアでランキング | 緑化度でランキング・報酬 |
| スコア = ゲーム内通貨 | 緑化度達成率 → ポイント変換 |

**緑化度の計算ロジック（案）**
- 敵撃破 → 緑化度が上昇
- ウェーブ進行・時間経過に応じた上昇量調整
- 100% 達成がパーフェクトクリア相当

**ポイント変換式（TBD）**
```
獲得ポイント = 緑化度(%) × 係数 + ボーナス（モード・難易度別）
```

---

### 2.2 ゲームモード

#### モード 1：ランキングモード（既存モードの刷新）

| 項目 | 内容 |
|------|------|
| 概要 | 制限時間内に最大緑化度を目指す |
| 制限時間 | 現行通り（約5分） |
| 勝利条件 | タイムアップ時点の緑化度 |
| ランキング対象 | Steam Leaderboard へ緑化度を送信 |
| ポイント報酬 | 緑化度 × 係数 |

#### モード 2：Boss Rush モード（新規）

| 項目 | 内容 |
|------|------|
| 概要 | ボスを連続撃破し、自然を完全回復させる |
| 勝利条件 | 全ボス撃破後の緑化度（100% 到達でパーフェクト） |
| スコア競争 | なし（緑化度 % で自己記録・クリア可否を評価） |
| ポイント報酬 | 緑化度 × 係数（ボス固有ボーナスあり） |
| 前提 | ボスの実装が完了していること |

> **NOTE**: ボスの実装・演出は別途設計ドキュメントで定義する

---

### 2.3 Shop システム

#### 概要

- タイトル画面からアクセス可能な常設ショップ
- **課金なし**。ゲーム内ポイントのみで購入
- 購入データは Steam Cloud Save で永続保持

#### 取り扱いカテゴリ

| カテゴリ | 内容 | 例 |
|----------|------|----|
| キャラクタースキン | 蝶の種類・色変更 | モナーク蝶、青い蝶、黒蝶 など |
| 弾スキン | 弾の形状・色・エフェクト変更 | 花びら弾、光弾、葉っぱ弾 など |

#### データ構造（概要）

```
PlayerProgressionData
├── TotalPoints (累積ポイント)
├── CurrentPoints (使用可能ポイント)
├── UnlockedCosmetics (List<CosmeticId>)
└── EquippedCosmetics
    ├── CharacterSkinId
    └── BulletSkinId
```

---

### 2.4 実績・解放条件（TBD）

Steam 実績と連動させるかどうかは未検討。要別途設計。

---

## 3. 機能要件

### 3.1 Steam Leaderboard（オンラインランキング）

| 項目 | 内容 |
|------|------|
| 使用 API | Steamworks SDK — `ISteamUserStats::UploadLeaderboardScore` |
| 対象モード | ランキングモードのみ |
| 送信値 | 緑化度（int, 0〜10000 スケール等で精度保持） |
| フィルタ | グローバル / フレンド（Steam 標準機能で自動対応） |
| オフライン時 | ローカルベストのみ保持。Steam 接続時に送信は行わない（上書きリスク回避） |
| 抽象化 | `ILeaderboardService` インターフェースを介して実装。テスト時はモック差し替え可 |

```csharp
// 抽象層（例）
public interface ILeaderboardService
{
    UniTask<bool> UploadScoreAsync(int score);
    UniTask<LeaderboardEntry[]> FetchRankingsAsync(RankingFilter filter);
}
```

---

### 3.2 Steam Cloud Save（セーブデータ）

| 項目 | 内容 |
|------|------|
| 使用 API | Steamworks SDK — `ISteamRemoteStorage` |
| 保存対象 | ポイント残高、購入済みコスメ一覧、装備中コスメ |
| 保存タイミング | ゲーム終了時、ショップ購入時 |
| フォーマット | JSON（型安全のため専用の `SaveData` クラスで管理） |
| 抽象化 | `ISaveService` インターフェース。ローカルフォールバック実装も用意 |

```csharp
// 抽象層（例）
public interface ISaveService
{
    UniTask SaveAsync(PlayerProgressionData data);
    UniTask<PlayerProgressionData> LoadAsync();
}
```

---

### 3.3 EventBus

現行の `PlayerEvents` / `EnemyEvents` / `GroundEvents` の静的イベント方式を廃止し、型安全な `EventBus` に一本化。

```csharp
// 使用イメージ
EventBus.Publish(new PlayerDamagedEvent(hpRate));
EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
```

**移行対象イベント一覧（既存）**

| 既存 | 新イベント型 |
|------|-------------|
| `PlayerEvents.OnPlayerDamaged` | `PlayerDamagedEvent` |
| `PlayerEvents.OnPlayerDied` | `PlayerDiedEvent` |
| `EnemyEvents.OnEnemyDied` | `EnemyDiedEvent` |
| `GroundEvents.OnGroundChanged` | `GroundChangedEvent` |
| スコア変更 | `GreenRateChangedEvent`（緑化度移行後） |

---

### 3.4 コスメシステム

- `CosmeticDataSO`（ScriptableObject）で各スキンのデータ管理
- 実行時に `ICosmeticApplier` を介してキャラクター・弾に適用
- Shop 購入 → `ISaveService` で即時保存 → シーンをまたいでも装備状態が維持

---

### 3.5 入力対応

| デバイス | ライブラリ |
|----------|-----------|
| キーボード + マウス | Unity Input System（現行維持） |
| ゲームパッド（XInput/DirectInput） | Unity Input System（現行維持） |

---

## 4. 技術・アーキテクチャ要件

### 4.1 レイヤー構成

```
┌─────────────────────────────────────┐
│  Presentation Layer                 │  ← Unity Scene / MonoBehaviour / UI
│  (View / Presenter)                 │
├─────────────────────────────────────┤
│  Application Layer                  │  ← Use Case / Game Flow / State Machine
│  (GameModeController, ShopService)  │
├─────────────────────────────────────┤
│  Domain Layer                       │  ← Pure C# / ゲームロジック / Entity
│  (GreenRate, PlayerProgression)     │     MonoBehaviour 非依存
├─────────────────────────────────────┤
│  Infrastructure Layer               │  ← Steam API / Save / 外部依存の実装
│  (SteamLeaderboard, SteamSave)      │
└─────────────────────────────────────┘
```

- **外部依存（Steam SDK）は Infrastructure Layer に閉じ込める**
- Domain / Application Layer は Steam に直接依存しない
- インターフェースを介して DI（手動 or VContainer 等）

---

### 4.2 MVP 適用範囲

| 対象 | View | Presenter | Model |
|------|------|-----------|-------|
| Player HUD | `PlayerHUDView` | `PlayerHUDPresenter` | `PlayerHealthModel` |
| 緑化度 UI | `GreenRateView` | `GreenRatePresenter` | `GreenRateModel` |
| Enemy HP バー | `EnemyHPBarView` | `EnemyHPBarPresenter` | `EnemyHealthModel` |
| Shop 画面 | `ShopView` | `ShopPresenter` | `ShopModel` |
| ランキング画面 | `LeaderboardView` | `LeaderboardPresenter` | `LeaderboardModel` |
| リザルト画面 | `ResultView` | `ResultPresenter` | `ResultModel` |

> **MVP を適用しない範囲**: 純粋なゲームロジック（移動・当たり判定・AI）は従来の MonoBehaviour 設計を維持

---

### 4.3 名前空間設計

```
SGC2025.Core          // ゲーム全体の基盤（EventBus, DI, Singleton）
SGC2025.Domain        // Pure C# ゲームロジック
SGC2025.Application   // ゲームフロー・Use Case
SGC2025.Infrastructure.Steam   // Steam API ラッパー
SGC2025.Infrastructure.Save    // セーブ実装
SGC2025.Presentation  // View / Presenter
SGC2025.InGame        // インゲームの MonoBehaviour 群
SGC2025.UI            // 汎用 UI コンポーネント
SGC2025.Editor        // エディタ拡張
```

---

### 4.4 既存コードのリファクタリング優先順位

| 優先度 | 対象 | 内容 |
|--------|------|------|
| 🔴 即時 | `Player.cs` | `OnPlayerDamaged` 二重発火バグを修正 |
| 🔴 即時 | `WaveManager.cs` | `useTestMode = true` → `false` に変更 |
| 🟠 Phase 1 | `EventBus` | 静的イベント群を EventBus に移行 |
| 🟠 Phase 1 | `PlayerHealthHandler` | `Player.cs` 内のインラインヘルスを移行完了 |
| 🟠 Phase 1 | `Player.cs` | 命名規則修正・責務分離 |
| 🟠 Phase 1 | `InGameManager` | ゲームモード概念の導入 |
| 🟡 Phase 2 | `HPBarController` | `IDamageable` 対応 + フォルダ移動 |
| 🟡 Phase 2 | `ResulltUI.cs` | ファイル名タイポ修正 |
| 🟢 Phase 3 | 空フォルダ・重複クラス | 整理 |

---

### 4.5 テスト方針

- **Domain Layer** は Unit Test を必須とする（NUnit / Unity Test Framework）
- **Infrastructure Layer** はインターフェースに対するモック実装でテスト可能にする
- **Presentation / InGame** はテスト対象外（手動確認）

---

## 5. 非機能要件

| 項目 | 要件 |
|------|------|
| ターゲット FPS | 60fps 固定 |
| 対応 OS | Windows 10 / 11 |
| Steam 要件 | Steam クライアントのインストールが必須 |
| オフライン動作 | ゲームプレイ自体はオフラインで可。ランキング送信は Steam 接続時のみ |
| セーブデータ安全性 | Steam Cloud Save を使用。ローカルバックアップも保持 |
| コード規約 | [Refactoring.md](../Refactoring.md) 準拠 |
| ブランチ運用 | [GitRule.md](GitRule.md) 準拠 |

---

## 6. 開発フェーズ計画

### Phase 1：基盤整備（目安：2〜3ヶ月）

- [ ] バグ修正（`OnPlayerDamaged` 二重発火、`useTestMode` デフォルト値）
- [ ] EventBus 実装・既存イベント移行
- [ ] レイヤー構成の整備（Infrastructure / Domain 分離）
- [ ] `ILeaderboardService` / `ISaveService` インターフェース定義
- [ ] `Player.cs` リファクタリング（責務分離・命名規則）
- [ ] ゲームモード基盤（`IGameMode` インターフェース）
- [ ] 緑化度指標への移行（スコア廃止）

### Phase 2：新機能実装（目安：3〜4ヶ月）

- [ ] Boss Rush モード実装
- [ ] Shop システム実装（UI 含む）
- [ ] コスメシステム実装（キャラスキン・弾スキン）
- [ ] Steam Leaderboard 統合
- [ ] Steam Cloud Save 統合
- [ ] MVP による UI リファクタリング

### Phase 3：ポリッシュ・Early Access 準備（目安：2〜3ヶ月）

- [ ] コスメのアセット追加
- [ ] Steam 実績（未確定）
- [ ] パフォーマンス最適化
- [ ] Steam ストアページ作成
- [ ] Early Access リリース

### Phase 4：Early Access 後（継続）

- [ ] フィードバック反映
- [ ] コンテンツ追加（コスメ・イベント等）
- [ ] 正式リリース

---

## 7. 未解決事項（TBD）

| # | 項目 | 優先度 | メモ |
|---|------|--------|------|
| 1 | Boss Rush のボス数・演出仕様 | 高 | Phase 2 開始前に確定必須 |
| 2 | 緑化度の計算式（係数） | 高 | バランス調整が必要 |
| 3 | ポイント → コスメ交換レート | 高 | Shop 実装前に確定必須 |
| 4 | Steam 実績の有無・内容 | 中 | Phase 3 で判断 |
| 5 | Boss Rush のランキング対応有無 | 中 | 現状は対象外だが検討余地あり |
| 6 | DI コンテナの採用可否（VContainer 等） | 中 | 手動 DI か外部ライブラリか |
| 7 | コスメアセットの制作担当・数量 | 高 | デザイナーとの調整が必要 |
| 8 | UniTask の導入可否 | 低 | 非同期処理の統一化に影響 |

---

*このドキュメントは開発進行に応じて随時更新する。*
