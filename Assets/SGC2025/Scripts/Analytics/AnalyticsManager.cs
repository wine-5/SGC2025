using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace SGC2025.Analytics
{
    /// <summary>
    /// ゲームプレイデータの収集と保存を管理するマネージャー
    /// 展示会用のプレイヤー行動分析データを記録
    /// </summary>
    public class AnalyticsManager : Singleton<AnalyticsManager>
    {
        private const string SESSION_FILE_NAME = "analytics_sessions.json";
        private const string REPORT_FILE_NAME = "analytics_report.md";

        private SessionData currentSession;
        private SessionDataContainer allSessions;
        private string savePath;
        private string reportPath;

        // データ収集用
        private float sessionStartTime;
        private float titleStartTime;
        private Vector3 lastPosition;
        private List<float> fpsHistory = new List<float>();

        protected override bool UseDontDestroyOnLoad => true;

        protected override void Init()
        {
            base.Init();

            savePath = Path.Combine(Application.persistentDataPath, SESSION_FILE_NAME);
            reportPath = Path.Combine(Application.persistentDataPath, REPORT_FILE_NAME);

            LoadAllSessions();
        }

        /// <summary>
        /// 新しいセッションを開始
        /// </summary>
        public void StartNewSession()
        {
            currentSession = new SessionData
            {
                gameResult = new GameResultData(),
                playMetrics = new PlayMetricsData(),
                sessionInfo = new SessionInfoData
                {
                    buildVersion = Application.version,
                    quitReason = "abandoned" // デフォルトは途中離脱
                }
            };

            sessionStartTime = Time.time;
            titleStartTime = Time.time;
            fpsHistory.Clear();

            Debug.Log($"[Analytics] Session started: {currentSession.sessionId}");
        }

        /// <summary>
        /// タイトル画面からゲーム開始時に呼び出す
        /// </summary>
        public void OnGameStart()
        {
            if (currentSession != null && currentSession.playMetrics != null)
            {
                currentSession.playMetrics.titleScreenTime = Time.time - titleStartTime;
            }
        }

        /// <summary>
        /// セッション終了時に呼び出す
        /// </summary>
        public void EndSession(string playerName, int score, float greeningRate, int reachedWave, bool isCleared, int deathWave)
        {
            if (currentSession == null) return;

            // プレイヤー名
            currentSession.playerName = string.IsNullOrEmpty(playerName) ? "Anonymous" : playerName;

            // ゲーム結果
            currentSession.gameResult.score = score;
            currentSession.gameResult.greeningRate = greeningRate;
            currentSession.gameResult.reachedWave = reachedWave;
            currentSession.gameResult.isCleared = isCleared;
            currentSession.gameResult.deathWave = deathWave;

            // プレイ時間
            currentSession.playMetrics.playTime = Time.time - sessionStartTime;

            // セッション情報
            currentSession.sessionInfo.isCompletedSession = true;
            currentSession.sessionInfo.quitReason = isCleared ? "cleared" : "gameover";
            currentSession.sessionInfo.quitWave = deathWave;
            currentSession.sessionInfo.quitTime = currentSession.playMetrics.playTime;
            currentSession.sessionInfo.avgFPS = CalculateAverageFPS();

            // セッションを保存
            SaveSession();
            
            Debug.Log($"[Analytics] Session ended: {currentSession.sessionId}");
        }

        /// <summary>
        /// 入力回数を記録
        /// </summary>
        public void RecordInput()
        {
            if (currentSession?.playMetrics != null)
                currentSession.playMetrics.inputCount++;
        }

        /// <summary>
        /// 弾発射を記録
        /// </summary>
        public void RecordShot()
        {
            if (currentSession?.playMetrics != null)
                currentSession.playMetrics.shotCount++;
        }

        /// <summary>
        /// 移動距離を記録（Updateで呼び出す）
        /// </summary>
        public void RecordMovement(Vector3 currentPosition)
        {
            if (currentSession?.playMetrics != null && lastPosition != Vector3.zero)
            {
                float distance = Vector3.Distance(lastPosition, currentPosition);
                currentSession.playMetrics.moveDistance += distance;
            }
            lastPosition = currentPosition;
        }

        /// <summary>
        /// 敵撃破を記録
        /// </summary>
        public void RecordEnemyKill()
        {
            if (currentSession?.playMetrics != null)
                currentSession.playMetrics.enemyKillCount++;
        }

        /// <summary>
        /// アイテム取得を記録
        /// </summary>
        public void RecordItemCollect(string itemType)
        {
            if (currentSession == null) return;

            currentSession.playMetrics.itemCollectCount++;

            if (currentSession.itemsCollected.ContainsKey(itemType))
                currentSession.itemsCollected[itemType]++;
            else
                currentSession.itemsCollected[itemType] = 1;
        }

        /// <summary>
        /// ポーズを記録
        /// </summary>
        public void RecordPause()
        {
            if (currentSession?.sessionInfo != null)
                currentSession.sessionInfo.pauseCount++;
        }

        /// <summary>
        /// 設定画面を開いたことを記録
        /// </summary>
        public void RecordSettingsOpen()
        {
            if (currentSession?.sessionInfo != null)
                currentSession.sessionInfo.settingsOpenCount++;
        }

        /// <summary>
        /// FPSを記録（Updateで呼び出す）
        /// </summary>
        public void RecordFPS()
        {
            if (fpsHistory.Count < 1000) // メモリ節約のため1000フレームまで
                fpsHistory.Add(1f / Time.unscaledDeltaTime);
        }

        private float CalculateAverageFPS()
        {
            if (fpsHistory.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (float fps in fpsHistory)
                sum += fps;
            
            return sum / fpsHistory.Count;
        }

        private void SaveSession()
        {
            if (currentSession == null) return;

            allSessions.sessions.Add(currentSession);

            try
            {
                string json = JsonUtility.ToJson(allSessions, true);
                File.WriteAllText(savePath, json);
                
                // レポートも自動生成
                GenerateReport();
                
                Debug.Log($"[Analytics] Session saved to: {savePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Analytics] Failed to save session: {e.Message}");
            }
        }

        private void LoadAllSessions()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    string json = File.ReadAllText(savePath);
                    allSessions = JsonUtility.FromJson<SessionDataContainer>(json);
                    Debug.Log($"[Analytics] Loaded {allSessions.sessions.Count} sessions");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Analytics] Failed to load sessions: {e.Message}");
                    allSessions = new SessionDataContainer();
                }
            }
            else
            {
                allSessions = new SessionDataContainer();
            }
        }

        /// <summary>
        /// Markdownレポートを生成
        /// </summary>
        private void GenerateReport()
        {
            var generator = new ReportGenerator(allSessions);
            string markdown = generator.GenerateMarkdownReport();

            try
            {
                File.WriteAllText(reportPath, markdown);
                Debug.Log($"[Analytics] Report generated: {reportPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Analytics] Failed to generate report: {e.Message}");
            }
        }

        /// <summary>
        /// 手動でレポートを再生成
        /// </summary>
        public void RegenerateReport()
        {
            GenerateReport();
        }
    }
}
