using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SGC2025.UI
{
    /// <summary>
    /// ランキング1行分の表示（ScrollView 内に動的生成される行プレハブ用）
    /// </summary>
    public class RankingRow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI rankText;  // 順位（1〜）
        [SerializeField]
        private TextMeshProUGUI nameText;  // プレイヤー名
        [SerializeField]
        private TextMeshProUGUI scoreText; // スコア（緑化度／総スコア）
        [SerializeField]
        private Image backgroundImage;     // 本人ハイライト用の背景（四角）

        [Header("順位カラー（1/2/3位＝金/銀/銅）")]
        [SerializeField]
        private Color firstColor = new Color(1f, 0.843f, 0f);          // 金
        [SerializeField]
        private Color secondColor = new Color(0.753f, 0.753f, 0.753f); // 銀
        [SerializeField]
        private Color thirdColor = new Color(0.804f, 0.498f, 0.196f);  // 銅
        [SerializeField]
        private Color defaultRankColor = new Color(0.196f, 0.196f, 0.196f); // 4位以下（既定の文字色）

        [Header("本人ハイライト")]
        [SerializeField]
        private Color highlightColor = new Color(0.53f, 0.81f, 0.92f, 0.6f); // 水色（半透明）

        private static readonly Color TransparentColor = new Color(0f, 0f, 0f, 0f);

        /// <summary>
        /// 1行分の内容を設定する
        /// </summary>
        /// <param name="rank">順位（1〜）</param>
        /// <param name="playerName">プレイヤー名</param>
        /// <param name="score">スコア表示文字列</param>
        /// <param name="isCurrentUser">本人の行なら true（背景を水色でハイライト）</param>
        public void Set(int rank, string playerName, string score, bool isCurrentUser = false)
        {
            if (rankText != null)
            {
                rankText.SetText($"{rank}");
                rankText.color = GetRankColor(rank);
            }

            if (nameText != null)
                nameText.SetText(playerName);

            if (scoreText != null)
                scoreText.SetText(score);

            if (backgroundImage != null)
                backgroundImage.color = isCurrentUser ? highlightColor : TransparentColor;
        }

        /// <summary>順位に応じた文字色（1位=金, 2位=銀, 3位=銅, 4位以下=既定）を返す</summary>
        private Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return firstColor;
                case 2: return secondColor;
                case 3: return thirdColor;
                default: return defaultRankColor;
            }
        }
    }
}
