using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace SGC2025
{
    /// <summary>
    /// ハイスコア達成時の名前入力UI
    /// </summary>
    public class NameInputUI : UIBase
    {
        private const int MAX_NAME_LENGTH = 5;
        private const string DEFAULT_NAME = "ナナシ";
        
        [SerializeField] private TMP_InputField nameInputField;

        private int lastScore;

        override public void Start()
        {
            nameInputField.onSelect.AddListener(OnInputFocus);
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            base.Start();
        }

        /// <summary>
        /// ハイスコア時に呼び出す（他スクリプトから）
        /// </summary>
        public void ShowInput(int score)
        {
            lastScore = score;
            nameInputField.text = "";
            nameInputField.ActivateInputField();
        }

        public void OnSubmit()
        {
            string name = nameInputField.text.Trim();
            if (string.IsNullOrEmpty(name))
                name = DEFAULT_NAME;

            RankingManager.I.AddScore(name, lastScore);
            this.gameObject.SetActive(false);
        }
        
        public void OnInputFocus(string text)
        {
            nameInputField.ActivateInputField();
        }
    }

}


    
