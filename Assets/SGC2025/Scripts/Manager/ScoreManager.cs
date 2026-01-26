using UnityEngine;
using SGC2025.Events;
using SGC2025.Item;

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
        private float greeningRate; // 緑化度（0.0～1.0）

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
            greeningRate = 0f;
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
            // スコア倍率を適用
            float multiplier = GetScoreMultiplier();
            int finalScore = Mathf.RoundToInt(score * multiplier);
            scoreEnemy += finalScore;
        }

        private void OnGroundGreenified(Vector3 position, int points)
        {
            // スコア倍率を適用
            float multiplier = GetScoreMultiplier();
            int finalPoints = Mathf.RoundToInt(points * multiplier);
            scoreGreen += finalPoints;
        }
        
        /// <summary>
        /// 現在のスコア倍率を取得
        /// </summary>
        private float GetScoreMultiplier()
        {
            if (ItemManager.I != null && ItemManager.I.IsEffectActive(ItemType.ScoreMultiplier))
            {
                return ItemManager.I.GetEffectValue(ItemType.ScoreMultiplier);
            }
            return 1f;
        }

        public int GetEnemyScore() => scoreEnemy;
        public int GetGreenScore() => scoreGreen;
        public int GetTotalScore() => scoreEnemy + scoreGreen;
        
        /// <summary>
        /// 緑化度を保存（InGameシーンから呼ばれる）
        /// </summary>
        public void SaveGreeningRate(float rate)
        {
            greeningRate = rate;
        }
        
        /// <summary>
        /// 保存された緑化度を取得（0.0～1.0）
        /// </summary>
        public float GetGreeningRate() => greeningRate;
    }
}
