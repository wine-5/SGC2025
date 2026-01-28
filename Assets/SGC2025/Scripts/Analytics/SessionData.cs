using System;
using System.Collections.Generic;

namespace SGC2025.Analytics
{
    /// <summary>
    /// 1プレイセッションのデータを保持するクラス
    /// 展示会でのプレイヤー行動分析用
    /// </summary>
    [Serializable]
    public class SessionData
    {
        // セッション識別情報
        public string sessionId;
        public string timestamp;
        public string playerName;

        // ゲーム結果
        public GameResultData gameResult;

        // プレイ指標
        public PlayMetricsData playMetrics;

        // アイテム収集データ
        public Dictionary<string, int> itemsCollected;

        // セッション情報
        public SessionInfoData sessionInfo;

        public SessionData()
        {
            sessionId = Guid.NewGuid().ToString();
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            itemsCollected = new Dictionary<string, int>();
        }
    }

    [Serializable]
    public class GameResultData
    {
        public int score;
        public float greeningRate;
        public int reachedWave;
        public bool isCleared;
        public int deathWave;
    }

    [Serializable]
    public class PlayMetricsData
    {
        public float playTime;              // ゲームプレイ時間（秒）
        public float titleScreenTime;       // タイトル画面滞在時間（秒）
        public int inputCount;              // 総入力回数
        public int shotCount;               // 弾発射回数
        public float moveDistance;          // 移動距離
        public int enemyKillCount;          // 敵撃破数
        public int itemCollectCount;        // アイテム取得回数
    }

    [Serializable]
    public class SessionInfoData
    {
        public bool isCompletedSession;     // 最後までプレイしたか
        public string quitReason;           // 終了理由（cleared/gameover/abandoned）
        public int quitWave;                // 離脱時のWave
        public float quitTime;              // 離脱時のプレイ時間
        public int pauseCount;              // ポーズ回数
        public int settingsOpenCount;       // 設定画面を開いた回数
        public float avgFPS;                // 平均FPS
        public string buildVersion;         // ビルドバージョン
    }

    /// <summary>
    /// 全セッションデータのコンテナ
    /// </summary>
    [Serializable]
    public class SessionDataContainer
    {
        public List<SessionData> sessions = new List<SessionData>();
    }
}
