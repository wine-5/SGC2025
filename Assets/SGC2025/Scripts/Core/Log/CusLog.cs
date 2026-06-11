using UnityEngine;
using System.Diagnostics;

namespace Polychroma.Core.Log
{
    /// <summary>
    /// カスタムロガーシステム
    /// ビルド時の出力制御とカテゴリ別の色分けに対応
    /// </summary>
    public static class CusLog
    {
        public static bool includeStackTrace = true;

        #region 基本ログメソッド

        /// <summary>
        /// 通常ログ（白色）
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            LogInternal("<color=white>[LOG]</color>", message);
        }

        /// <summary>
        /// 警告ログ（黄色）
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string message)
        {
            LogInternal("<color=yellow>[WARNING]</color>", message);
        }

        /// <summary>
        /// エラーログ（赤色）
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Error(string message)
        {
            LogInternal("<color=red>[ERROR]</color>", message);
        }

        #endregion

        #region カスタムカテゴリログメソッド

        /// <summary>
        /// カテゴリ付き通常ログ
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string category, string message)
        {
            string colorCode = LoggerSettingsSO.Instance.GetCategoryColor(category);
            string prefix = $"<color={colorCode}>[{category}]</color>";
            LogInternal(prefix, message);
        }

        /// <summary>
        /// カテゴリ付き警告ログ
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string category, string message)
        {
            string colorCode = LoggerSettingsSO.Instance.GetCategoryColor(category);
            string prefix = $"<color={colorCode}>[{category}]</color> <color=yellow>[WARNING]</color>";
            LogInternal(prefix, message);
        }

        /// <summary>
        /// カテゴリ付きエラーログ
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Error(string category, string message)
        {
            string colorCode = LoggerSettingsSO.Instance.GetCategoryColor(category);
            string prefix = $"<color={colorCode}>[{category}]</color> <color=red>[ERROR]</color>";
            LogInternal(prefix, message);
        }

        #endregion

        #region オブジェクト付きログメソッド

        /// <summary>
        /// UnityObjectを指定した通常ログ
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message, Object context)
        {
            LogInternal("<color=white>[LOG]</color>", message, context);
        }

        /// <summary>
        /// UnityObjectを指定したカテゴリ付きログ
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Log(string category, string message, Object context)
        {
            string colorCode = LoggerSettingsSO.Instance.GetCategoryColor(category);
            string prefix = $"<color={colorCode}>[{category}]</color>";
            LogInternal(prefix, message, context);
        }

        #endregion

        #region 内部ログ処理

        private static string GetStackTraceInfo()
        {
            if (!includeStackTrace)
                return "";

            var stackTrace = new StackTrace(2, true);
            var frames = stackTrace.GetFrames();

            var filteredFrames = new System.Collections.Generic.List<string>();
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method?.DeclaringType?.Namespace == "Polychroma.Core.Log")
                    continue;

                filteredFrames.Add(frame.ToString());
            }

            return filteredFrames.Count > 0 ? $"\n{string.Join("\n", filteredFrames)}" : "";
        }

        [Conditional("UNITY_EDITOR")]
        private static void LogInternal(string prefix, string message, Object context = null)
        {
            string trace = GetStackTraceInfo();
            string fullMessage = $"{prefix} {message}{trace}";

            if (context != null)
                UnityEngine.Debug.Log(fullMessage, context);
            else
                UnityEngine.Debug.Log(fullMessage);
        }

        #endregion
    }
}
