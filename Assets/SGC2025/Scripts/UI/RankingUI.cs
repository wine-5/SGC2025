using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SGC2025.Ranking;

namespace SGC2025.UI
{
    /// <summary>
    /// ランキング表示UI
    /// </summary>
    public class RankingUI : UIBase
    {
        [SerializeField]
        private TextMeshProUGUI[] nameTexts;
        [SerializeField]
        private TextMeshProUGUI[] greeningRateTexts;
        
        override public void Start()
        {
            base.Start();
            UpdateScore();
        }

        public void UpdateScore()
        {
            List<ScoreData> ranking = RankingManager.I.GetRanking();
            if (ranking == null) return;
            
            for (int i = 0; i < nameTexts.Length; i++)
            {
                if (i < ranking.Count)
                {
                    var data = ranking[i];
                    nameTexts[i].text = data.playerName;
                    
                    if (greeningRateTexts != null && i < greeningRateTexts.Length)
                        greeningRateTexts[i].text = $"{data.greeningRate:F1}%";
                }
                else
                {
                    nameTexts[i].text = "---";
                    
                    if (greeningRateTexts != null && i < greeningRateTexts.Length)
                        greeningRateTexts[i].text = "---";
                }
            }
        }
    }
}