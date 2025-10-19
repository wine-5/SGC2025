using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace SGC2025
{
    public class NameInputUI : UIBase
    {
        [SerializeField] private TMP_InputField nameInputField;

        private int lastScore;

        void Start()
        {

            // 文字数制限（最大6文字）
            nameInputField.characterLimit = 6;
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
                return;
            }

            RankingManager.I.AddScore(name, lastScore);
            Debug.Log($"スコア登録: {name} - {lastScore}");


            this.gameObject.SetActive(false); // シーンのロードで消えるので無効にしておくだけ
        }
    }
}
