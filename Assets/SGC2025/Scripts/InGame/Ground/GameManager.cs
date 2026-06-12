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
        protected override bool UseDontDestroyOnLoad => true;

        /// <summary>ゲーム終了時点の緑化率（0.0～1.0）。リザルト画面で参照する</summary>
        public float FinalGreeningRate { get; private set; }

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

            // GroundManagerはInGameシーンと共に破棄されるため、遷移前に緑化率を確定させる
            if (GroundManager.Exists)
                FinalGreeningRate = GroundManager.I.GetGreenificationRate();

            if (SceneController.I != null)
            {
                SceneController.I.LoadResultScene();
                if (AudioManager.I != null)
                    AudioManager.I.PlayBGM(BGMType.Result);
            }
        }
    }
}