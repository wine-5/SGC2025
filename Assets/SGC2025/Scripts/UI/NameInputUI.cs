using UnityEngine;
using TMPro;
using SGC2025.Manager;


namespace SGC2025.UI
{
    /// <summary>
    /// ハイスコア達成時の名前入力UI
    /// </summary>
    public class NameInputUI : UIBase
    {
        private const int MAX_NAME_LENGTH = 5;
        private const string DEFAULT_NAME = "ナナシ";

        [SerializeField] private TMP_InputField nameInputField;

        override public void Start()
        {
            nameInputField.onSelect.AddListener(OnInputFocus);
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            base.Start();
        }

        public void OnSubmit()
        {
            string name = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                name = DEFAULT_NAME;

            // スコアと緑化度を取得して登録
            int totalScore = ScoreManager.I.GetTotalScore();
            float greeningRate = GroundManager.I != null ? GroundManager.I.GetGreenificationRate() * 100f : 0f;
            
            RankingManager.I.AddScore(name, totalScore, greeningRate);
            this.gameObject.SetActive(false);
        }

        public void OnInputFocus(string text)
        {
            nameInputField.ActivateInputField();
        }
    }
}

