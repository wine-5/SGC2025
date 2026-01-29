using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace SGC2025.Analytics
{
    /// <summary>
    /// 分析データからMarkdownレポートを生成するクラス
    /// </summary>
    public class ReportGenerator
    {
        private SessionDataContainer data;

        public ReportGenerator(SessionDataContainer sessionData)
        {
            data = sessionData;
        }

        public string GenerateMarkdownReport()
        {
            var sb = new StringBuilder();

            // ヘッダー
            sb.AppendLine("# ゲーム分析レポート");
            sb.AppendLine();
            sb.AppendLine($"**生成日時**: {DateTime.Now:yyyy年MM月dd日 HH時mm分}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            // 概要
            GenerateOverview(sb);

            // Wave別分析
            GenerateWaveAnalysis(sb);

            // プレイヤー行動分析
            GeneratePlayerBehaviorAnalysis(sb);

            // アイテム人気度
            GenerateItemPopularity(sb);

            // トッププレイヤー
            GenerateTopPlayers(sb);

            // パフォーマンス
            GeneratePerformanceMetrics(sb);

            // 全セッション詳細
            GenerateSessionDetails(sb);

            return sb.ToString();
        }

        private void GenerateOverview(StringBuilder sb)
        {
            sb.AppendLine("## 📊 概要");
            sb.AppendLine();

            int totalSessions = data.sessions.Count;
            int completedSessions = data.sessions.Count(s => s.sessionInfo.isCompletedSession);
            int clearedSessions = data.sessions.Count(s => s.gameResult.isCleared);
            
            float avgPlayTime = totalSessions > 0 
                ? data.sessions.Average(s => s.playMetrics.playTime) 
                : 0f;

            sb.AppendLine($"- **総プレイ回数**: {totalSessions}回");
            sb.AppendLine($"- **完走回数**: {completedSessions}回 ({GetPercentage(completedSessions, totalSessions)})");
            sb.AppendLine($"- **クリア回数**: {clearedSessions}回 ({GetPercentage(clearedSessions, totalSessions)})");
            sb.AppendLine($"- **平均プレイ時間**: {FormatTime(avgPlayTime)}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GenerateWaveAnalysis(StringBuilder sb)
        {
            sb.AppendLine("## 🌊 Wave別分析");
            sb.AppendLine();

            var waveData = data.sessions
                .GroupBy(s => s.gameResult.deathWave)
                .OrderBy(g => g.Key)
                .Select(g => new { Wave = g.Key, Count = g.Count() })
                .ToList();

            if (waveData.Any())
            {
                sb.AppendLine("| Wave | 死亡数 | 割合 |");
                sb.AppendLine("|------|--------|------|");

                foreach (var item in waveData)
                {
                    string percentage = GetPercentage(item.Count, data.sessions.Count);
                    sb.AppendLine($"| {item.Wave} | {item.Count} | {percentage} |");
                }
            }
            else
            {
                sb.AppendLine("*データなし*");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GeneratePlayerBehaviorAnalysis(StringBuilder sb)
        {
            sb.AppendLine("## 🎮 プレイヤー行動分析");
            sb.AppendLine();

            if (data.sessions.Count == 0)
            {
                sb.AppendLine("*データなし*");
                sb.AppendLine();
                return;
            }

            float avgInputs = (float)data.sessions.Average(s => s.playMetrics.inputCount);
            float avgShots = (float)data.sessions.Average(s => s.playMetrics.shotCount);
            float avgDistance = (float)data.sessions.Average(s => s.playMetrics.moveDistance);
            float avgKills = (float)data.sessions.Average(s => s.playMetrics.enemyKillCount);
            float avgItems = (float)data.sessions.Average(s => s.playMetrics.itemCollectCount);

            sb.AppendLine($"- **平均入力回数**: {avgInputs:F0}回");
            sb.AppendLine($"- **平均弾発射数**: {avgShots:F0}発");
            sb.AppendLine($"- **平均移動距離**: {avgDistance:F1}");
            sb.AppendLine($"- **平均敵撃破数**: {avgKills:F1}体");
            sb.AppendLine($"- **平均アイテム取得数**: {avgItems:F1}個");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GenerateItemPopularity(StringBuilder sb)
        {
            sb.AppendLine("## 🎁 アイテム人気度");
            sb.AppendLine();

            var itemStats = new Dictionary<string, int>();
            
            foreach (var session in data.sessions)
            {
                foreach (var item in session.itemsCollected)
                {
                    if (itemStats.ContainsKey(item.Key))
                        itemStats[item.Key] += item.Value;
                    else
                        itemStats[item.Key] = item.Value;
                }
            }

            if (itemStats.Any())
            {
                sb.AppendLine("| アイテム | 取得回数 |");
                sb.AppendLine("|----------|----------|");

                foreach (var item in itemStats.OrderByDescending(kv => kv.Value))
                {
                    sb.AppendLine($"| {item.Key} | {item.Value} |");
                }
            }
            else
            {
                sb.AppendLine("*データなし*");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GenerateTopPlayers(StringBuilder sb)
        {
            sb.AppendLine("## 🏆 トッププレイヤー");
            sb.AppendLine();

            var topPlayers = data.sessions
                .OrderByDescending(s => s.gameResult.greeningRate)
                .ThenByDescending(s => s.gameResult.score)
                .Take(10)
                .ToList();

            if (topPlayers.Any())
            {
                sb.AppendLine("| 順位 | プレイヤー名 | スコア | 緑化度 | 到達Wave |");
                sb.AppendLine("|------|--------------|--------|--------|----------|");

                for (int i = 0; i < topPlayers.Count; i++)
                {
                    var player = topPlayers[i];
                    sb.AppendLine($"| {i + 1} | {player.playerName} | {player.gameResult.score:N0} | {player.gameResult.greeningRate:F1}% | {player.gameResult.reachedWave} |");
                }
            }
            else
            {
                sb.AppendLine("*データなし*");
            }

            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GeneratePerformanceMetrics(StringBuilder sb)
        {
            sb.AppendLine("## ⚡ パフォーマンス指標");
            sb.AppendLine();

            if (data.sessions.Count == 0)
            {
                sb.AppendLine("*データなし*");
                sb.AppendLine();
                return;
            }

            float avgFPS = (float)data.sessions.Average(s => s.sessionInfo.avgFPS);
            float minFPS = (float)data.sessions.Min(s => s.sessionInfo.avgFPS);
            int pauseCount = data.sessions.Sum(s => s.sessionInfo.pauseCount);
            int settingsOpenCount = data.sessions.Sum(s => s.sessionInfo.settingsOpenCount);

            sb.AppendLine($"- **平均FPS**: {avgFPS:F1}");
            sb.AppendLine($"- **最低FPS**: {minFPS:F1}");
            sb.AppendLine($"- **総ポーズ回数**: {pauseCount}回");
            sb.AppendLine($"- **総設定変更回数**: {settingsOpenCount}回");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        private void GenerateSessionDetails(StringBuilder sb)
        {
            sb.AppendLine("## 📝 全セッション詳細");
            sb.AppendLine();

            if (data.sessions.Count == 0)
            {
                sb.AppendLine("*データなし*");
                return;
            }

            foreach (var session in data.sessions.OrderByDescending(s => s.timestamp))
            {
                sb.AppendLine($"### {session.playerName} - {session.timestamp}");
                sb.AppendLine();
                sb.AppendLine($"- **スコア**: {session.gameResult.score:N0}");
                sb.AppendLine($"- **緑化度**: {session.gameResult.greeningRate:F1}%");
                sb.AppendLine($"- **到達Wave**: {session.gameResult.reachedWave}");
                sb.AppendLine($"- **結果**: {GetResultText(session)}");
                sb.AppendLine($"- **プレイ時間**: {FormatTime(session.playMetrics.playTime)}");
                sb.AppendLine($"- **入力回数**: {session.playMetrics.inputCount}");
                sb.AppendLine($"- **弾発射数**: {session.playMetrics.shotCount}");
                sb.AppendLine($"- **敵撃破数**: {session.playMetrics.enemyKillCount}");
                sb.AppendLine($"- **平均FPS**: {session.sessionInfo.avgFPS:F1}");
                sb.AppendLine();
            }
        }

        private string GetPercentage(int count, int total)
        {
            if (total == 0) return "0%";
            return $"{(count * 100.0 / total):F1}%";
        }

        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{minutes}分{secs}秒";
        }

        private string GetResultText(SessionData session)
        {
            if (session.gameResult.isCleared)
                return "✅ クリア";
            else if (session.sessionInfo.quitReason == "gameover")
                return "💀 ゲームオーバー";
            else
                return "⚠️ 途中離脱";
        }
    }
}
