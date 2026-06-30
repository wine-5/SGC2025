using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tyotyo.Core.Log
{
    /// <summary>
    /// ビルドした実行ファイル上でもログを画面に重ねて表示するデバッグ用オーバーレイ。
    /// Application.logMessageReceived を購読し、直近のログを画面左上に描画する。
    /// 製品版（リリースビルド）では Debug.isDebugBuild が false になるため自動的に無効化される。
    /// </summary>
    public class OnScreenLogView : MonoBehaviour
    {
        // 表示/非表示の切り替えキー（固定）。シリアライズ値に依存させないことで型変更時の不整合を避ける
        private const Key ToggleKey = Key.F1;

        [SerializeField, Tooltip("画面に保持するログ行数")]
        private int maxLines = 20;

        [SerializeField, Tooltip("文字サイズ")]
        private int fontSize = 16;

        [SerializeField, Tooltip("起動時に表示状態にするか")]
        private bool visibleOnStart = true;

        private readonly Queue<string> lines = new Queue<string>();
        private bool isVisible;
        private GUIStyle style;
        private Vector2 scroll;

        private void Awake()
        {
            // 製品版では一切動作させない（エディタ・開発ビルドのみ有効）
            if (!Debug.isDebugBuild)
            {
                enabled = false;
                return;
            }

            DontDestroyOnLoad(gameObject);
            isVisible = visibleOnStart;
        }

        private void OnEnable()
        {
            if (!Debug.isDebugBuild) return;
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard[ToggleKey].wasPressedThisFrame)
                isVisible = !isVisible;
        }

        /// <summary>
        /// ログ受信時にバッファへ積む。エラー/例外は色を変えて見やすくする。
        /// </summary>
        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            string colored = type switch
            {
                LogType.Error or LogType.Exception or LogType.Assert => $"<color=red>{condition}</color>",
                LogType.Warning => $"<color=yellow>{condition}</color>",
                _ => condition,
            };

            lines.Enqueue(colored);
            while (lines.Count > maxLines)
                lines.Dequeue();
        }

        private void OnGUI()
        {
            if (!isVisible || lines.Count == 0) return;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = fontSize,
                    richText = true,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft,
                };
            }

            float width = Screen.width * 0.5f;
            float height = Screen.height * 0.6f;

            GUILayout.BeginArea(new Rect(10, 10, width, height), GUI.skin.box);
            scroll = GUILayout.BeginScrollView(scroll);

            foreach (string line in lines)
                GUILayout.Label(line, style);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
