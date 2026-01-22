using UnityEngine;
using UnityEngine.SceneManagement;

namespace SGC2025
{
    /// <summary>
    /// スコア管理とゲーム時間管理を行うマネージャー
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        [Header("スコア設定")]
        [SerializeField]
        [Tooltip("通常タイルを緑化した際のポイント")]
        private int normalTilePoint = 100;
        
        [SerializeField]
        [Tooltip("ハイスコアタイルのポイント倍率")]
        private int highScoreTileMultiplier = 3;
        
        /// <summary>通常タイルポイントを取得</summary>
        public int NormalTilePoint => normalTilePoint;
        
        /// <summary>ハイスコア倍率を取得</summary>
        public int HighScoreTileMultiplier => highScoreTileMultiplier;
        
        [Header("タイマー設定")]
        [SerializeField] private float startCountDownTime;
        [SerializeField] private InGameUI gameScoreUI;
        
        private bool isCountDown = false;
        private float currentCountDownTimer = 0f;
        private float countGameTimer = 0f;
        private int scoreEnemy = 0;
        private int scoreGreen = 0;

        private void Start()
        {
            ResetValue();
        }

        private void Update()
        {
            CountDownTimer();
            CountGameTimer();
            if(CommonDef.GAME_MINIT <= countGameTimer) SceneManager.LoadScene("Result");
        }

        private void ResetValue()
        {
            isCountDown = false;
            currentCountDownTimer = startCountDownTime;
            countGameTimer = 0f;
            scoreEnemy = 0;
            scoreGreen = 0;
        }

        /// <summary>カウントダウン開始</summary>
        public void CountDownStart()
        {
            currentCountDownTimer = startCountDownTime;
            isCountDown = true;
        }

        private void CountDownTimer()
        {
            if (!isCountDown) return;
            currentCountDownTimer -= Time.deltaTime;
            if (currentCountDownTimer <= 0f) isCountDown = false;
        }

        /// <summary>カウントダウン残り時間取得</summary>
        public float GetCountDown() => currentCountDownTimer;

        private void CountGameTimer()
        {
            if (currentCountDownTimer > 0f || isCountDown) return;
                
            countGameTimer += Time.deltaTime;
        }

        /// <summary>ゲーム残り時間取得</summary>
        public float GetGameCount() => CommonDef.GAME_MINIT - countGameTimer;

        /// <summary>エネミースコア加算</summary>
        public void AddEnemyScore(int score)
        {
            scoreEnemy += score;
            CommonDef.currentEnemyScore = score;
            gameScoreUI.ShowScorePopup(score);
        }

        /// <summary>エネミースコア取得</summary>
        public int GetEnemyScore() => scoreEnemy;

        /// <summary>緑化スコア加算</summary>
        public void AddGreenScore(int score)
        {
            scoreGreen += score;
            CommonDef.currentGreeningScore = scoreGreen;
            gameScoreUI.ShowScorePopup(score);
        }

        /// <summary>緑化スコア取得</summary>
        public int GetGreenScore() => scoreGreen;
    }
}
