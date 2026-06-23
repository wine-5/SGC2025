using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        private const string GREENING_HEADER = "緑化度";  // スコア列見出し（緑化度表示時）
        private const string TOTAL_HEADER = "総スコア";   // スコア列見出し（総スコア表示時）

        [SerializeField]
        private RankingRow rowPrefab; // 1行分のプレハブ
        [SerializeField]
        private Transform contentParent; // ScrollView の Content
        [SerializeField]
        private Button greeningButton; // 緑化度ランキング切替ボタン
        [SerializeField]
        private Button totalButton; // 総スコアランキング切替ボタン
        [SerializeField]
        private TextMeshProUGUI scoreHeaderText; // スコア列の見出し（緑化度／総スコアを切替表示）

        [Header("切替アニメーション")]
        [SerializeField]
        private GameObject animationTarget; // 切替演出をかける対象（未指定なら Content を使用）
        [SerializeField]
        private float fadeDuration = UIPanelAnimator.DefaultFadeDuration; // フェードイン時間（秒）
        [SerializeField]
        private float startScale = UIPanelAnimator.DefaultStartScale;     // 出現開始時のスケール倍率

        private readonly List<RankingRow> spawnedRows = new List<RankingRow>();
        private LeaderboardType currentType = LeaderboardType.GreeningRate;
        private Coroutine playingRoutine;

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
            UpdateHeader();
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
            UpdateHeader();
            UpdateScore();
            PlaySwitchAnimation();
        }

        /// <summary>
        /// ランキング切替時にフェード＋スケールのポップ演出を再生する
        /// </summary>
        private void PlaySwitchAnimation()
        {
            GameObject target = animationTarget != null
                ? animationTarget
                : (contentParent != null ? contentParent.gameObject : null);

            if (target == null || !isActiveAndEnabled) return;

            if (playingRoutine != null) StopCoroutine(playingRoutine);
            playingRoutine = StartCoroutine(PlayShowAnimation(target));
        }

        private IEnumerator PlayShowAnimation(GameObject target)
        {
            yield return UIPanelAnimator.PlayShow(target, fadeDuration, startScale);
            playingRoutine = null;
        }

        /// <summary>
        /// スコア列の見出しを現在の表示種別に合わせて切り替える
        /// </summary>
        private void UpdateHeader()
        {
            if (scoreHeaderText != null)
                scoreHeaderText.SetText(currentType == LeaderboardType.GreeningRate ? GREENING_HEADER : TOTAL_HEADER);
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
                    CreateRow().Set(i + 1, entries[i].PlayerName, FormatSteamScore(entries[i].Score), entries[i].IsCurrentUser);
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
            string currentName = GetCurrentPlayerName();

            // 実データが少なくても常に MAX_ENTRIES 行表示し、空きは「---」で埋める
            for (int i = 0; i < MAX_ENTRIES; i++)
            {
                if (i < ranking.Count)
                {
                    bool isCurrentUser = !string.IsNullOrEmpty(currentName) && ranking[i].playerName == currentName;
                    CreateRow().Set(i + 1, ranking[i].playerName, FormatLocalScore(ranking[i].score), isCurrentUser);
                }
                else
                {
                    CreateRow().Set(i + 1, EMPTY_TEXT, EMPTY_TEXT);
                }
            }
        }

        /// <summary>
        /// ローカル格納スコアを表示用文字列へ整形する
        /// </summary>
        private string FormatLocalScore(float score)
            => currentType == LeaderboardType.GreeningRate ? $"{score:F1}%" : Mathf.RoundToInt(score).ToString();

        /// <summary>
        /// 本人判定用の現在プレイヤー名を取得する（Steam時はログイン名、それ以外は空＝ハイライト無し）
        /// </summary>
        private string GetCurrentPlayerName()
        {
#if STEAMWORKS_NET
            if (SteamManager.Initialized)
                return Steamworks.SteamFriends.GetPersonaName();
#endif
            return string.Empty;
        }

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