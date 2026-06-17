using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SGC2025.Ranking;
using SGC2025.Core;
#if STEAMWORKS_NET
using SGC2025.Ranking.Steam;
#endif

namespace SGC2025.UI
{
    /// <summary>
    /// ランキング表示UI（緑化度・総スコアを切替表示。ScrollView 内に行を動的生成）
    /// </summary>
    public class RankingUI : UIBase
    {
        private const float GREENING_DISPLAY_DIVISOR = 100f; // Steam格納値(%×100)を%へ戻す除数
        private const int MAX_ENTRIES = 10;
        private const string EMPTY_TEXT = "---"; // 空きスロットの表示

        [SerializeField]
        private RankingRow rowPrefab; // 1行分のプレハブ
        [SerializeField]
        private Transform contentParent; // ScrollView の Content
        [SerializeField]
        private Button greeningButton; // 緑化度ランキング切替ボタン
        [SerializeField]
        private Button totalButton; // 総スコアランキング切替ボタン

        private readonly List<RankingRow> spawnedRows = new List<RankingRow>();
        private LeaderboardType currentType = LeaderboardType.GreeningRate;

        private void Awake()
        {
            if (greeningButton != null)
                greeningButton.onClick.AddListener(ShowGreening);

            if (totalButton != null)
                totalButton.onClick.AddListener(ShowTotal);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LeaderboardEntriesUpdatedEvent>(OnEntriesUpdated);
            UpdateScore();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LeaderboardEntriesUpdatedEvent>(OnEntriesUpdated);
        }

        /// <summary>
        /// Steam の取得完了通知を受けて、表示中の種別なら再描画する
        /// </summary>
        private void OnEntriesUpdated(LeaderboardEntriesUpdatedEvent updated)
        {
            if (updated.Type == currentType)
                UpdateScore();
        }

        /// <summary>緑化度ランキングへ切り替える（切替ボタンから呼ぶ）</summary>
        public void ShowGreening() => SetType(LeaderboardType.GreeningRate);

        /// <summary>総スコアランキングへ切り替える（切替ボタンから呼ぶ）</summary>
        public void ShowTotal() => SetType(LeaderboardType.TotalScore);

        private void SetType(LeaderboardType type)
        {
            currentType = type;
            UpdateScore();
        }

        public void UpdateScore()
        {
            if (rowPrefab == null || contentParent == null) return;

#if STEAMWORKS_NET
            if (GameModeConfig.UseSteam)
            {
                UpdateFromSteam();
                return;
            }
#endif
            UpdateFromLocal();
        }

#if STEAMWORKS_NET
        /// <summary>
        /// Steam のキャッシュ済みエントリーから行を生成する
        /// </summary>
        private void UpdateFromSteam()
        {
            ClearRows();

            List<SteamLeaderboardEntry> entries = SteamLeaderboardManager.I.GetCachedEntries(currentType);

            // 実データが少なくても常に MAX_ENTRIES 行表示し、空きは「---」で埋める
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                if (entries != null && i < entries.Count)
                    CreateRow().Set(i + 1, entries[i].PlayerName, FormatSteamScore(entries[i].Score));
                else
                    CreateRow().Set(i + 1, EMPTY_TEXT, EMPTY_TEXT);
            }
        }

        /// <summary>
        /// Steam格納スコアを表示用文字列へ整形する
        /// </summary>
        private string FormatSteamScore(int score)
            => currentType == LeaderboardType.GreeningRate ? $"{score / GREENING_DISPLAY_DIVISOR:F1}%" : score.ToString();
#endif

        /// <summary>
        /// ローカルランキングから行を生成する
        /// </summary>
        private void UpdateFromLocal()
        {
            ClearRows();

            List<ScoreData> ranking = RankingManager.I.GetRanking(currentType);

            // 実データが少なくても常に MAX_ENTRIES 行表示し、空きは「---」で埋める
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                if (i < ranking.Count)
                    CreateRow().Set(i + 1, ranking[i].playerName, FormatLocalScore(ranking[i].score));
                else
                    CreateRow().Set(i + 1, EMPTY_TEXT, EMPTY_TEXT);
            }
        }

        /// <summary>
        /// ローカル格納スコアを表示用文字列へ整形する
        /// </summary>
        private string FormatLocalScore(float score)
            => currentType == LeaderboardType.GreeningRate ? $"{score:F1}%" : Mathf.RoundToInt(score).ToString();

        /// <summary>
        /// 生成済みの行を全て破棄する
        /// </summary>
        private void ClearRows()
        {
            foreach (RankingRow row in spawnedRows)
                if (row != null) Destroy(row.gameObject);

            spawnedRows.Clear();
        }

        /// <summary>
        /// 行プレハブを1つ生成する
        /// </summary>
        private RankingRow CreateRow()
        {
            RankingRow row = Instantiate(rowPrefab, contentParent);
            spawnedRows.Add(row);
            return row;
        }
    }
}
