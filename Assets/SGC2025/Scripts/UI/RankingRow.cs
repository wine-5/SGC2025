using UnityEngine;
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

        /// <summary>
        /// 1行分の内容を設定する
        /// </summary>
        public void Set(int rank, string playerName, string score)
        {
            if (rankText != null)
                rankText.SetText($"{rank}");

            if (nameText != null)
                nameText.SetText(playerName);

            if (scoreText != null)
                scoreText.SetText(score);
        }
    }
}
