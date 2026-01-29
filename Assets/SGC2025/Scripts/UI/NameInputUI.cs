using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SGC2025.Manager;
using System;


namespace SGC2025.UI
{
    /// <summary>
    /// ハイスコア達成時の名前入力UI
    /// </summary>
    public class NameInputUI : UIBase
    {
        private const int MAX_NAME_LENGTH = 5;
        private const string DEFAULT_NAME = "ナナシ";
        private const float PERCENT_MULTIPLIER = 100f;

        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button submitButton; // 決定ボタン

        public event Action Submitted;

        override public void Start()
        {
            if (nameInputField != null)
            {
                nameInputField.onSelect.AddListener(OnInputFocus);
                nameInputField.onValueChanged.AddListener(OnInputValueChanged);
                nameInputField.characterLimit = MAX_NAME_LENGTH;
            }
            
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
            if (submitButton != null && nameInputField != null)
            {
                bool hasInput = !string.IsNullOrWhiteSpace(nameInputField.text);
                submitButton.interactable = hasInput;
                submitButton.gameObject.SetActive(hasInput);
            }
        }

        public void OnSubmit()
        {
            if (nameInputField == null) return;

            string name = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                name = DEFAULT_NAME;

            int totalScore = ScoreManager.I.GetTotalScore();
            float greeningRate = ScoreManager.I != null ? ScoreManager.I.GetGreeningRate() * PERCENT_MULTIPLIER : 0f;
            
            RankingManager.I.AddScore(name, totalScore, greeningRate);

            Submitted?.Invoke();
            
            gameObject.SetActive(false);
        }

        public void OnInputFocus(string text)
        {
            if (nameInputField != null)
                nameInputField.ActivateInputField();
        }
    }
}