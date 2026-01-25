using UnityEngine;
using SGC2025.Events;

namespace SGC2025.Manager
{
    /// <summary>
    /// スコア管理を行うマネージャー
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        [Header("スコア設定")]
        [SerializeField, Tooltip("敵を倒した際のポイント")]
        private int enemyKillPoint = 50;
        
        [SerializeField, Tooltip("通常タイルを緑化した際のポイント")]
        private int normalTilePoint = 100;

        [SerializeField, Tooltip("ハイスコアタイルのポイント倍率")]
        private int highScoreTileMultiplier = 3;

        public int EnemyKillPoint => enemyKillPoint;
        public int NormalTilePoint => normalTilePoint;
        public int HighScoreTileMultiplier => highScoreTileMultiplier;

        private int scoreEnemy;
        private int scoreGreen;

        protected override void Awake()
        {
            base.Awake();
            ResetScore();
        }

        /// <summary>
        /// スコアを初期化（0にリセット）
        /// </summary>
        public void ResetScore()
        {
            scoreEnemy = 0;
            scoreGreen = 0;
        }

        private void OnEnable()
        {
            EnemyEvents.OnEnemyDestroyedWithScore += OnEnemyDestroyedWithScore;
            GroundEvents.OnGroundGreenified += OnGroundGreenified;
        }

        private void OnDisable()
        {
            EnemyEvents.OnEnemyDestroyedWithScore -= OnEnemyDestroyedWithScore;
            GroundEvents.OnGroundGreenified -= OnGroundGreenified;
        }

        private void OnEnemyDestroyedWithScore(int score, Vector3 position)
        {
            scoreEnemy += score;
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            scoreGreen += points;
        }

        public int GetEnemyScore() => scoreEnemy;
        public int GetGreenScore() => scoreGreen;
        public int GetTotalScore() => scoreEnemy + scoreGreen;
    }
}
