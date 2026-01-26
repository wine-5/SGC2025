using UnityEngine;
using UnityEngine.SceneManagement;
using SGC2025.Audio;

namespace SGC2025.Manager
{
    /// <summary>
    /// ゲーム全体のループと状態管理を行うマネージャー
    /// シーン間で維持され、ゲームの最上位の制御を行う
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("ゲーム設定")]
        [SerializeField] private float gameOverDelay = 2f;

        protected override bool UseDontDestroyOnLoad => true;

        protected override void Init()
        {
            base.Init();
        }

        private void Start()
        {
        }

        protected override void OnDestroy()
        {
            // Time.timeScaleを確実にリセット（ポーズ中に破棄された場合に備えて）
            Time.timeScale = 1f;
            
            base.OnDestroy();
        }

        /// <summary>
        /// 結果シーンへの遷移処理
        /// InGameManagerから呼び出される
        /// </summary>
        public void LoadResultScene()
        {
            // ポーズ中にゲームオーバーになった場合に備えてTime.timeScaleをリセット
            Time.timeScale = 1f;
            
            if (ScoreManager.I != null && GroundManager.I != null)
            {
                float greeningRate = GroundManager.I.GetGreenificationRate();
                ScoreManager.I.SaveGreeningRate(greeningRate);
            }

            if (SceneController.I != null)
            {
                // 少し遅延してからシーン遷移（ゲームオーバー演出のため）
                Invoke(nameof(DoLoadResultScene), gameOverDelay);
            }
        }

        private void DoLoadResultScene()
        {
            if (SceneController.I != null)
            {
                SceneController.I.LoadResultScene();
                if (AudioManager.I != null)
                    AudioManager.I.PlayBGM(BGMType.Result);
            }
        }
    }
}