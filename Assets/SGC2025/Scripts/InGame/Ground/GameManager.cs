using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SGC2025.Audio;
using SGC2025.Core;

namespace SGC2025.Manager
{
    /// <summary>
    /// ゲーム全体のループと状態管理を行うマネージャー
    /// シーン間で維持され、ゲームの最上位の制御を行う
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        private const float PERCENT_MULTIPLIER = 100f;
        private const int TOTAL_SCORE_MULTIPLIER = 10; // 総スコア計算の倍率
        private const int TOTAL_SCORE_BASE = 100;      // 総スコアの基礎点

        [SerializeField]
        private GameModeConfig gameModeConfig; // 展示用/Steam用の動作モード設定

        protected override bool UseDontDestroyOnLoad => true;

        /// <summary>ゲーム終了時点の緑化率（0.0～1.0）。リザルト画面で参照する</summary>
        public float FinalGreeningRate { get; private set; }

        /// <summary>ゲーム終了時点の敵撃破数。リザルト画面で参照する</summary>
        public int FinalKillCount { get; private set; }

        /// <summary>ゲーム終了時点の総スコア。リザルト画面で参照する</summary>
        public int FinalTotalScore { get; private set; }

        /// <summary>緑化したセルを塗った順に記録したリスト。リザルトのマップ再生で参照する</summary>
        public IReadOnlyList<Vector2Int> GreenifiedSequence => greenifiedSequence;

        /// <summary>マップの列数（リザルトのマップ再生でテクスチャ生成に使う）</summary>
        public int MapColumns { get; private set; }

        /// <summary>マップの行数（リザルトのマップ再生でテクスチャ生成に使う）</summary>
        public int MapRows { get; private set; }

        private readonly List<Vector2Int> greenifiedSequence = new List<Vector2Int>();

        private int killCount;

        protected override void Awake()
        {
            base.Awake();

            // SO参照を持つインスタンスは（破棄される側でも）必ず設定を登録する
            if (gameModeConfig != null)
                GameModeConfig.Register(gameModeConfig);

            // 重複インスタンス（破棄される側）では購読しない
            if (I != this) return;

            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            EventBus.Subscribe<CountDownFinishedEvent>(OnCountDownFinished);
            EventBus.Subscribe<GroundGreenifiedEvent>(OnGroundGreenified);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            EventBus.Unsubscribe<CountDownFinishedEvent>(OnCountDownFinished);
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);

            // Time.timeScaleを確実にリセット（ポーズ中に破棄された場合に備えて）
            Time.timeScale = 1f;

            base.OnDestroy();
        }

        /// <summary>ゲーム開始時に撃破数と塗り順記録をリセットする</summary>
        private void OnCountDownFinished(CountDownFinishedEvent _)
        {
            killCount = 0;
            greenifiedSequence.Clear();
        }

        /// <summary>敵撃破ごとに撃破数を加算する</summary>
        private void OnEnemyDestroyed(EnemyDestroyedEvent _) => killCount++;

        /// <summary>緑化したセルを塗った順に記録する（リザルトのマップ再生用）</summary>
        private void OnGroundGreenified(GroundGreenifiedEvent e)
        {
            if (GroundManager.Exists)
                greenifiedSequence.Add(GroundManager.I.WorldToCell(e.Position));
        }

        /// <summary>
        /// 総スコアを計算する（敵撃破数 × 倍率 × 緑化％ + 基礎点）
        /// </summary>
        private int CalcTotalScore()
        {
            float greeningPercent = FinalGreeningRate * PERCENT_MULTIPLIER;
            return Mathf.RoundToInt(killCount * TOTAL_SCORE_MULTIPLIER * greeningPercent + TOTAL_SCORE_BASE);
        }

        /// <summary>
        /// 結果シーンへの遷移処理
        /// InGameManagerから呼び出される
        /// </summary>
        public void LoadResultScene()
        {
            // ポーズ中にゲームオーバーになった場合に備えてTime.timeScaleをリセット
            Time.timeScale = 1f;

            // GroundManagerはInGameシーンと共に破棄されるため、遷移前に緑化率とマップ寸法を確定させる
            if (GroundManager.Exists)
            {
                FinalGreeningRate = GroundManager.I.GetGreenificationRate();
                MapColumns = GroundManager.I.MapData.columns;
                MapRows = GroundManager.I.MapData.rows;
            }

            // 緑化率の確定後に総スコアを確定させる
            FinalKillCount = killCount;
            FinalTotalScore = CalcTotalScore();

            if (SceneController.I != null)
            {
                SceneController.I.LoadResultScene();
                if (AudioManager.I != null)
                    AudioManager.I.PlayBGM(BGMType.Result);
            }
        }
    }
}