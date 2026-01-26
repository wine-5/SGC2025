using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Button submitButton; // 決定ボタン

        override public void Start()
        {
            nameInputField.onSelect.AddListener(OnInputFocus);
            nameInputField.onValueChanged.AddListener(OnInputValueChanged);
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            
            UpdateSubmitButtonState();
            
            base.Start();
        }
        
        /// <summary>
        /// 入力内容が変更されたときの処理
        /// </summary>
        private void OnInputValueChanged(string text)
        {
            UpdateSubmitButtonState();
        }
        
        /// <summary>
        /// ボタンの有効/無効を更新
        /// </summary>
        private void UpdateSubmitButtonState()
        {
            if (submitButton != null)
            {
                bool hasInput = !string.IsNullOrWhiteSpace(nameInputField.text);
                submitButton.interactable = hasInput;
            }
        }

        public void OnSubmit()
        {
            string name = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                name = DEFAULT_NAME;

            int totalScore = ScoreManager.I.GetTotalScore();
            float greeningRate = ScoreManager.I != null ? ScoreManager.I.GetGreeningRate() * 100f : 0f;
            
            RankingManager.I.AddScore(name, totalScore, greeningRate);
            
            this.gameObject.SetActive(false);
        }

        public void OnInputFocus(string text)
        {
            nameInputField.ActivateInputField();
        }
    }
}

