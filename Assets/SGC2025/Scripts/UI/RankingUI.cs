using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

namespace SGC2025
{
    /// <summary>
    /// ランキング表示UI
    /// </summary>
    public class RankingUI : UIBase
    {
        [SerializeField]
        private TextMeshProUGUI[] scoreTexts;
        [SerializeField]
        private TextMeshProUGUI[] nameTexts;
        
        override public void Start()
        {
            base.Start();
            UpdateScore();
        }

        /// <summary>
        /// ランキングスコアを更新
        /// </summary>
        public void UpdateScore()
        {
            List<ScoreData> ranking = RankingManager.I.GetRanking();
            if (ranking == null) return;
            for (int i = 0; i < scoreTexts.Length; i++)
            {
                if (i < ranking.Count)
                {
                    var data = ranking[i];
                    nameTexts[i].text = $"{data.playerName}";
                    scoreTexts[i].text = $" {data.score}";
                }
                else
                {
                    nameTexts[i].text = $"---";
                    scoreTexts[i].text = $"---";
                }
            }
        }

    }
}