using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace SGC2025
{
    public class NameInputUI : UIBase
    {
        [SerializeField] private TMP_InputField nameInputField;

        private int lastScore;

        override public void Start()
        {
            nameInputField.onSelect.AddListener(OnInputFocus);

            // 文字数制限
            nameInputField.characterLimit = 5;

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
            {
                name = "ナナシ";
            }

            RankingManager.I.AddScore(name, lastScore);

            this.gameObject.SetActive(false); // シーンのロードで消えるので無効にしておくだけ
        }
       public void OnInputFocus(string text)
        {
            // 入力状態に切り替え
            nameInputField.ActivateInputField();
        }
    }

}


    
