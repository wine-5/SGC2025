using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tyotyo.Manager;
using Tyotyo.Ranking;
using System;


namespace Tyotyo.UI
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
        [SerializeField] private GameObject duplicateWarning; // 同名使用済みの警告表示（任意）

        public event Action Submitted;

        private void Start()
        {
            if (nameInputField != null)
            {
                nameInputField.onSelect.AddListener(OnInputFocus);
                nameInputField.onValueChanged.AddListener(OnInputValueChanged);
                nameInputField.characterLimit = MAX_NAME_LENGTH;
            }

            UpdateSubmitButtonState();
        }
        
        /// <summary>
        /// 入力内容が変更されたときの処理
        /// </summary>
        private void OnInputValueChanged(string text)
        {
            UpdateSubmitButtonState();
        }
        
        /// <summary>
        /// ボタンの有効/無効を更新する。
        /// 未入力、または既にローカルランキングで使われている名前の場合は決定不可にする。
        /// </summary>
        private void UpdateSubmitButtonState()
        {
            if (submitButton == null || nameInputField == null) return;

            string name = nameInputField.text.Trim();
            bool hasInput = !string.IsNullOrWhiteSpace(name);
            bool isDuplicate = hasInput && IsNameTaken(name);
            bool canSubmit = hasInput && !isDuplicate;

            submitButton.interactable = canSubmit;
            submitButton.gameObject.SetActive(canSubmit);

            if (duplicateWarning != null)
                duplicateWarning.SetActive(isDuplicate);
        }

        /// <summary>
        /// 指定名がローカルランキングで使用済みかどうか
        /// </summary>
        private bool IsNameTaken(string name)
            => RankingManager.I != null && RankingManager.I.NameExists(name);

        public void OnSubmit()
        {
            if (nameInputField == null) return;

            string name = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                name = DEFAULT_NAME;

            // 同名は登録不可（Enterキー等でボタン無効化を迂回した場合の保険）
            if (IsNameTaken(name))
            {
                UpdateSubmitButtonState();
                return;
            }

            float greeningRate = GameManager.I.FinalGreeningRate * PERCENT_MULTIPLIER;
            int totalScore = GameManager.I.FinalTotalScore;

            RankingManager.I.AddResult(name, greeningRate, totalScore);

            Submitted?.Invoke();
            
            gameObject.SetActive(false);
        }

        private void OnInputFocus(string text)
        {
            if (nameInputField != null)
                nameInputField.ActivateInputField();
        }
    }
}