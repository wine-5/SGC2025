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
| 勝利条件 | **タイムアップ時点の緑化度** |
| ランキング対象 | Steam Leaderboard へ緑化度を送信 |
| ポイント報酬 | 緑化度 × 係数 |
| 難易度別報酬 | Easy: 1.0倍 / Normal: 1.5倍 / Hard: 2.0倍 / Lunatic: 3.0倍 |

#### モード 2：地域解放モード（新規 - 段階的コンテンツ）

| 項目 | 内容 |
|------|------|
| 概要 | 難易度別に地域を解放し、緑化度達成を目指す |
| クリア条件 | **制限時間内に目標緑化度に到達** |
| 難易度別設定 | 後述「2.2.1 難易度システム」を参照 |
| 破壊者システム | 後述「2.2.2 破壊者（Destroyer）」を参照 |
| ポイント報酬 | 最終緑化度 × 係数 × 難易度倍率 |

#### 2.2.1 難易度システム

| 難易度 | 出現敵 | 破壊者 | 制限時間 | クリア緑化度 | 報酬倍率 |
|--------|-------|-------|---------|------------|---------|
| Easy | 少ない | 0体 | 10分 | 30% | 1.0倍 |
| Normal | 標準 | ウェーブ3から1体 | 7分 | 60% | 1.5倍 |
| Hard | 多い（強化） | ウェーブ3から2体 | 5分 | 100% | 2.0倍 |
| Lunatic | 超多い（超強化） | ウェーブ3から3体 | 3分 | 150% | 3.0倍 |

**注記**：
- Normal クリア → 次地域解放
- Hard クリア → 隠し地域追加解放
- Lunatic クリア → 特別リワード

#### 2.2.2 破壊者（Destroyer）

蝶から奪われた自然を破壊する敵。通った場所の緑化度を戻す脅威。

| 項目 | 内容 |
|------|------|
| 敵タイプ | 新規 EnemyType: `Destroyer` |
| 移動パターン | プレイヤーを直線的に追従 |
| ビジュアル | サイズが大きい / 赤色 / 周囲にエフェクト |
| HP バー | 画面上部に表示 |
| 基本攻撃 | プレイヤーにダメージ（2倍程度） |
| 破壊メカニクス | 通った場所を **3×3 範囲で砂地に戻す** |
| ダメージ回数 | 5～10回の攻撃で撃破（BossDataSO で調整） |
| 撃破ボーナス | 撃破位置を中心に **4×4 範囲が自動緑化** |
| ポイント報酬 | 通常敵の **×2.0** |

#### モード 3：Boss Rush モード（新規）

| 項目 | 内容 |
|------|------|
| 概要 | ボスを連続撃破し、自然を完全回復させる |
| 勝利条件 | 全ボス撃破後の緑化度（100% 到達でパーフェクト） |
| ボス数 | 複数（後日設計） |
| ポイント報酬 | 緑化度 × 係数（ボス固有ボーナスあり） |

> **NOTE**: ボスキャラの実装・演出は別途設計ドキュメントで定義する

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

### Phase 1：基盤整備 ✅完了

- [x] バグ修正（`OnPlayerDamaged` 二重発火、`useTestMode` デフォルト値）
- [x] EventBus 実装・既存イベント移行
- [x] レイヤー構成の整備（Infrastructure / Domain / Application 分離）
- [x] `ILeaderboardService` / `ISaveService` インターフェース定義
- [x] `Player.cs` / `PlayerController.cs` リファクタリング（責務分離・命名規則）
- [x] ゲームモード基盤（`IGameMode` インターフェース）
- [x] 緑化度指標への移行（スコア廃止）
- [x] BootstrapLoader 実装（Manager シーン自動ロード）
- [x] Ranking フォルダ整備・システム整理
- [x] コード品質向上（リファクタリング）

### Phase 2：新機能実装（進行中 - 目安：3〜4ヶ月）

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

## 7. 現在の実装状況（2026-06-11 更新）

### ✅ Phase 1 完了

Phase 1 基盤整備はすべて完了。以下の内容が実装済み：

- EventBus による型安全なイベントシステム
- BootstrapLoader による Manager シーン自動ロード
- 緑化度（GreenificationRate）中心の指標システム
- Ranking フォルダの整理と専門化
- Infrastructure / Domain / Application 層の分離
- ILeaderboardService, ISaveService インターフェース
- IGameMode ゲームモード基盤
- PlayerController のリファクタリング完了
- コード品質向上（不要なインポート削除、設定の最適化）

### ⏳ Phase 2 実装中

| # | 機能 | 実装状況 | 優先度 |
|----|------|--------|--------|
| 1 | Boss Rush モード | ❌未実装 | 🔴高 |
| 2 | Shop システム | ❌未実装 | 🔴高 |
| 3 | コスメシステム | ❌未実装 | 🔴高 |
| 4 | Steam Leaderboard 統合 | ❌未実装 | 🔴高 |
| 5 | Steam Cloud Save 統合 | ❌未実装 | 🔴高 |
| 6 | MVP による UI リファクタリング | ❌未実装 | 🟠中 |

---

## 8. Phase 2 実装前の決定事項（TBD）

| # | 項目 | 優先度 | ステータス |
|---|------|--------|-----------|
| 1 | Boss Rush のボス数・演出仕様 | 🔴高 | ⏳確定待ち |
| 2 | 緑化度の計算式（係数・ボーナス） | 🔴高 | ⏳確定待ち |
| 3 | ポイント → コスメ交換レート | 🔴高 | ⏳確定待ち |
| 4 | DI コンテナの採用可否（VContainer 等） | 🟠中 | ⏳確定待ち |
| 5 | Boss Rush のランキング対応有無 | 🟠中 | ⏳検討中 |
| 6 | コスメアセットの制作担当・数量 | 🟠中 | ⏳デザイナー調整待ち |
| 7 | Steam 実績の有無・内容 | 🟢低 | Phase 3 で判断 |

---

## 9. Phase 2 推奨実装順序

1. **Boss Rush モード** ← ゲームモード概念の最初の実装
2. **Shop システム + コスメシステム** ← ゲーム内経済の実装
3. **Steam 統合（Leaderboard + Cloud Save）** ← 外部連携機能
4. **MVP による UI リファクタリング** ← ポーランド・最適化

---

*ドキュメント更新日: 2026-06-11 - Phase 1 完了、Phase 2 開始準備中*
