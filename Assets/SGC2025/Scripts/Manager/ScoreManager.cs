using UnityEngine;
using SGC2025.Events;

namespace SGC2025
{
    /// <summary>
    /// スコア管理を行うマネージャー
    /// </summary>
    public class ScoreManager : Singleton<ScoreManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

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

        private int scoreEnemy = 0;
        private int scoreGreen = 0;

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

        private void Start()
        {
            ResetScores();
        }

        private void ResetScores()
        {
            scoreEnemy = 0;
            scoreGreen = 0;
        }

        /// <summary>エネミースコア取得</summary>
        public int GetEnemyScore() => scoreEnemy;

        /// <summary>緑化スコア取得</summary>
        public int GetGreenScore() => scoreGreen;

        /// <summary>総合スコア取得</summary>
        public int GetTotalScore() => scoreEnemy + scoreGreen;
    }
}
