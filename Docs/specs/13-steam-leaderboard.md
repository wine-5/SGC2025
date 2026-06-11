# Steam Leaderboard 実装設計書

## Context

現在、ランキングはローカル JSON ファイルに保存されているのみ（`RankingManager.cs`）。
Steam 上でプレイヤー全員のスコア（緑化率）をグローバルランキングとして記録・表示し、
ゲーム終了時にランクインした場合にメッセージを表示、OKを押したら Steam に送信する仕組みを実装する。

---

## フォルダ構成（追加・変更）

```
Assets/SGC2025/Scripts/
├── Ranking/
│   ├── ScoreData.cs              ← 既存（変更なし）
│   ├── RankingData.cs            ← 既存（変更なし）
│   ├── RankingManager.cs         ← 既存 → Steam 版に完全置き換え
│   └── Steam/
│       ├── SteamLeaderboardManager.cs   ← 新規：Steam API の窓口
│       └── SteamLeaderboardEntry.cs     ← 新規：ランキングエントリー struct
├── Core/
│   └── InGameEvents.cs           ← 既存：LeaderboardRankedInEvent を追加
└── UI/
    ├── RankingUI.cs              ← 既存 → Steam 版データを表示するよう改修
    └── ResulltUI.cs              ← 既存 → ランクイン判定 + 確認ダイアログ追加
```

---

## クラス設計

### 1. SteamLeaderboardManager（新規）
`Assets/SGC2025/Scripts/Ranking/Steam/SteamLeaderboardManager.cs`

- `Singleton<SteamLeaderboardManager>` を継承
- `UseDontDestroyOnLoad => true`（シーン跨ぎで保持）
- Steam 初期化・Leaderboard 取得・スコア送信・ランキング取得を担当

```csharp
public class SteamLeaderboardManager : Singleton<SteamLeaderboardManager>
{
    private const string LEADERBOARD_NAME = "GreenificationRate";
    private const int LEADERBOARD_MAX_ENTRIES = 10;

    // Steam Leaderboard ハンドル
    private SteamLeaderboard_t leaderboardHandle;
    private bool isLeaderboardReady = false;

    // 取得済みエントリーキャッシュ
    public List<SteamLeaderboardEntry> CachedEntries { get; private set; }

    // Init() で Steam 初期化 & Leaderboard 取得
    // FindOrCreateLeaderboard() でハンドル取得
    // UploadScore(int score) でスコア送信
    // FetchLeaderboard(int count) でランキング取得
}
```

### 2. SteamLeaderboardEntry（新規）
`Assets/SGC2025/Scripts/Ranking/Steam/SteamLeaderboardEntry.cs`

```csharp
public struct SteamLeaderboardEntry
{
    public string PlayerName;   // Steam 表示名
    public int Score;           // 緑化率 × 100（例：8500 = 85.00%）
    public int Rank;            // 順位（1始まり）
}
```

### 3. InGameEvents.cs（追加）
`LeaderboardRankedInEvent` を追加

```csharp
/// <summary>Steam Leaderboard にランクインした</summary>
public struct LeaderboardRankedInEvent : IGameEvent
{
    public int Rank;
    public int Score;
    public LeaderboardRankedInEvent(int rank, int score)
    {
        Rank = rank;
        Score = score;
    }
}
```

### 4. RankingManager.cs（既存 → 改修）
- ローカル JSON 保存ロジックを削除し、`SteamLeaderboardManager` のラッパーに変更
- `IsNewRecord()` 判定を Steam のキャッシュ済みエントリーと比較する形に改修
- ランクイン判定後に `LeaderboardRankedInEvent` を Publish

### 5. ResulltUI.cs（既存 → 改修）
- `LeaderboardRankedInEvent` を Subscribe
- ランクインメッセージ用 UI（テキスト + OK ボタン）を追加
- OK ボタン押下時に `SteamLeaderboardManager.I.UploadScore()` を呼び出す

### 6. RankingUI.cs（既存 → 改修）
- `SteamLeaderboardManager.I.FetchLeaderboard()` でデータ取得
- `SteamLeaderboardManager.I.CachedEntries` から UI に表示

---

## データフロー

```
ゲーム終了
    ↓
GameTimeUpEvent / PlayerDiedEvent 発火
    ↓
ResulltUI.cs がスコア（greeningRate）を計算
    ↓
RankingManager.IsNewRecord(score) で判定
    ↓ ランクインなら
LeaderboardRankedInEvent を Publish
    ↓
ResulltUI.cs が「〇位にランクイン！」メッセージ表示
    ↓
プレイヤーが OK を押す
    ↓
SteamLeaderboardManager.I.UploadScore((int)(greeningRate * 100))
    ↓
Steam Leaderboard に送信
```

---

## Steamworks.NET のインストール手順

`Packages/manifest.json` に以下を追加：
```json
"com.rlabrecque.steamworks.net": "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net"
```

また `steam_appid.txt` をプロジェクトルートに配置（テスト用 AppID: `480`）

---

## テストコード（Debug.Log 確認フロー）

```csharp
// SteamLeaderboardManager.cs 内のテスト用ログ
private void Init()
{
    CusLog.Log("[SteamLeaderboardManager] Initializing...");
    // Steam 初期化確認
    if (!SteamManager.Initialized)
    {
        CusLog.Log("[SteamLeaderboardManager] Steam is NOT initialized. Test mode.");
        return;
    }
    FindOrCreateLeaderboard();
}

private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure)
{
    if (bIOFailure || result.m_bLeaderboardFound == 0)
    {
        CusLog.Log("[SteamLeaderboardManager] Leaderboard NOT found!");
        return;
    }
    leaderboardHandle = result.m_hSteamLeaderboard;
    isLeaderboardReady = true;
    CusLog.Log("[SteamLeaderboardManager] Leaderboard found and ready.");
    FetchLeaderboard(LEADERBOARD_MAX_ENTRIES);
}

public void UploadScore(int score)
{
    CusLog.Log($"[SteamLeaderboardManager] UploadScore called: {score}");
    if (!isLeaderboardReady)
    {
        CusLog.Log("[SteamLeaderboardManager] Leaderboard not ready. Upload skipped.");
        return;
    }
    // Steam API 呼び出し
    SteamUserStats.UploadLeaderboardScore(leaderboardHandle,
        ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
    CusLog.Log($"[SteamLeaderboardManager] Score {score} uploaded to Steam.");
}

private void OnLeaderboardEntriesLoaded(LeaderboardScoresDownloaded_t result, bool bIOFailure)
{
    CusLog.Log($"[SteamLeaderboardManager] Fetched {result.m_cEntryCount} entries.");
    CachedEntries = new List<SteamLeaderboardEntry>();
    for (int i = 0; i < result.m_cEntryCount; i++)
    {
        LeaderboardEntry_t entry;
        SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, null, 0);
        CachedEntries.Add(new SteamLeaderboardEntry
        {
            PlayerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser),
            Score = entry.m_nScore,
            Rank = entry.m_nGlobalRank
        });
        CusLog.Log($"[SteamLeaderboardManager] Entry {i + 1}: {CachedEntries[i].PlayerName} - {CachedEntries[i].Score}");
    }
}
```

---

## 作業タスク

| # | タスク | 担当 | 備考 |
|---|---|---|---|
| 1 | Steamworks.NET インストール・初期設定 | 新人 | manifest.json 追加、SteamManager 確認 |
| 2 | SteamLeaderboardEntry struct 作成 | 新人 | シンプルな struct |
| 3 | SteamLeaderboardManager 実装 | 新人 | Init / Find / Upload / Fetch |
| 4 | InGameEvents に LeaderboardRankedInEvent 追加 | ユーザー | 既存ファイルの改修 |
| 5 | RankingManager を Steam 版に改修 | 新人 | IsNewRecord → EventBus.Publish |
| 6 | ResulltUI にランクイン UI 追加 | ユーザー | UI 配置 + OK ボタン処理 |
| 7 | RankingUI を Steam データ表示に改修 | 新人 | CachedEntries を UI に反映 |

---

## 検証方法

1. **Steam なし環境（テスト）**
   - `SteamManager.Initialized` が false の場合、CusLog でスキップログが出ること
   - ゲーム終了時に `[SteamLeaderboardManager] Upload skipped.` が Console に表示される

2. **Steam あり環境（本番テスト）**
   - AppID `480`（SpaceWar）で Steam 起動中に Play
   - `[SteamLeaderboardManager] Leaderboard found and ready.` が表示される
   - ゲーム終了後、OK 押下で `Score uploaded to Steam.` が表示される
   - `FetchLeaderboard()` 後に `Entry 1: PlayerName - Score` がログに出る

3. **RankingUI 確認**
   - ランキング画面を開いた際に上位 10 件が表示される
   - Steam 表示名が正しく出ている
