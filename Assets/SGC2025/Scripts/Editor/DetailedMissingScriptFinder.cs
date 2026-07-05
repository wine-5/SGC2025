using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tyotyo.Editor
{
    /// <summary>
    /// Missing Scripts/Components の詳細検索ツール
    /// 複数シーンをスキャンし、詳細な情報を表示
    /// </summary>
    public class DetailedMissingScriptFinder : EditorWindow
    {
        private Vector2 scrollPos;
        private List<MissingScriptInfo> results = new List<MissingScriptInfo>();
        private bool searchAllScenes = true;
        private bool includeInactiveObjects = true;
        private bool showDebugLog = true;

        private struct MissingScriptInfo
        {
            public GameObject gameObject;
            public string scenePath;
            public string gameObjectPath;
            public int componentIndex;
        }

        [MenuItem("Tools/Finder/Detailed Missing Script Finder")]
        public static void ShowWindow()
        {
            GetWindow<DetailedMissingScriptFinder>("Detailed Missing Finder");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("詳細 Missing Script Finder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 検索オプション
            using (new EditorGUILayout.VerticalScope("box"))
            {
                searchAllScenes = EditorGUILayout.ToggleLeft("全シーンを検索", searchAllScenes);
                includeInactiveObjects = EditorGUILayout.ToggleLeft("非アクティブオブジェクトを含める", includeInactiveObjects);
                showDebugLog = EditorGUILayout.ToggleLeft("デバッグログを表示", showDebugLog);
            }

            EditorGUILayout.Space();

            // 検索ボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("検索開始", GUILayout.Height(30)))
                {
                    FindMissingScripts();
                }

                if (GUILayout.Button("クリア", GUILayout.Width(80), GUILayout.Height(30)))
                {
                    results.Clear();
                }
            }

            EditorGUILayout.Space();

            // 結果表示
            EditorGUILayout.LabelField($"検索結果: {results.Count}個のGameObjectで Missing Script を検出", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("Missing Script は見つかりません", MessageType.Info);
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < results.Count; i++)
            {
                DrawMissingScriptResult(results[i], i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawMissingScriptResult(MissingScriptInfo info, int index)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                string label = $"[{index + 1}] {info.gameObjectPath} (シーン: {info.scenePath})";

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label, EditorStyles.wordWrappedLabel);
                }

                if (info.gameObject != null)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("選択", GUILayout.Width(60)))
                        {
                            SelectGameObject(info.gameObject);
                        }

                        EditorGUILayout.ObjectField(info.gameObject, typeof(GameObject), true);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("GameObjectが無効化されているため選択できません", MessageType.Warning);
                }
            }
        }

        private void FindMissingScripts()
        {
            results.Clear();
            int totalGameObjects = 0;
            int totalComponents = 0;
            int totalMissing = 0;

            if (showDebugLog)
            {
                Debug.Log("[DetailedMissingScriptFinder] 検索を開始します");
            }

            List<string> scenePaths = new List<string>();

            if (searchAllScenes)
            {
                // ビルド設定のシーンを取得
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    scenePaths.Add(EditorBuildSettings.scenes[i].path);
                }
            }
            else
            {
                // 現在のアクティブシーンのみ
                scenePaths.Add(SceneManager.GetActiveScene().path);
            }

            // 各シーンをスキャン
            foreach (string scenePath in scenePaths)
            {
                if (string.IsNullOrEmpty(scenePath))
                    continue;

                if (showDebugLog)
                {
                    Debug.Log($"[DetailedMissingScriptFinder] シーンをスキャン: {scenePath}");
                }

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                totalMissing += ScanScene(scene, scenePath, ref totalGameObjects, ref totalComponents);
            }

            if (showDebugLog)
            {
                Debug.Log($"[DetailedMissingScriptFinder] 検索完了\n" +
                    $"  検査対象: {totalGameObjects}個のGameObject, {totalComponents}個のコンポーネント\n" +
                    $"  Missing Script: {totalMissing}個");
            }
        }

        private int ScanScene(Scene scene, string scenePath, ref int totalGameObjects, ref int totalComponents)
        {
            int missingCount = 0;
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects)
            {
                Transform[] allTransforms = rootObject.GetComponentsInChildren<Transform>(includeInactiveObjects);

                foreach (Transform transform in allTransforms)
                {
                    GameObject go = transform.gameObject;
                    totalGameObjects++;

                    Component[] components = go.GetComponents<Component>();
                    totalComponents += components.Length;

                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i] == null)
                        {
                            missingCount++;

                            MissingScriptInfo info = new MissingScriptInfo
                            {
                                gameObject = go,
                                scenePath = scenePath,
                                gameObjectPath = GetGameObjectPath(go),
                                componentIndex = i
                            };

                            results.Add(info);

                            if (showDebugLog)
                            {
                                Debug.LogWarning($"[DetailedMissingScriptFinder] Missing Script 検出: {info.gameObjectPath} (コンポーネント #{i})", go);
                            }
                        }
                    }
                }
            }

            return missingCount;
        }

        private string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private void SelectGameObject(GameObject go)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }
    }
}
