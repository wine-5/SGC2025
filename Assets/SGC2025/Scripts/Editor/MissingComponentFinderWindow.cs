using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tyotyo.Editor
{
    public class MissingComponentFinderWindow : EditorWindow
    {
        private readonly List<GameObject> results = new List<GameObject>();
        private Vector2 scroll;
        private bool includeInactive = true;

        [MenuItem("Tools/Missing Component Finder")]
        public static void Open()
        {
            GetWindow<MissingComponentFinderWindow>("Missing Finder");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Missing Component Finder", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            includeInactive = EditorGUILayout.ToggleLeft("Include Inactive Objects", includeInactive);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Search Missing Components", GUILayout.Height(28)))
                {
                    Search();
                }

                if (GUILayout.Button("Clear", GUILayout.Height(28), GUILayout.Width(80)))
                {
                    results.Clear();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Found: {results.Count}", EditorStyles.helpBox);

            EditorGUILayout.Space();
            DrawResults();
        }

        private void Search()
        {
            results.Clear();

            // シーン内全ルート取得
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                var all = root.GetComponentsInChildren<Transform>(includeInactive);

                foreach (var t in all)
                {
                    var go = t.gameObject;

                    // Missing Script/Component の判定
                    // GetComponents<Component>() に null が混ざっていたら Missing
                    var components = go.GetComponents<Component>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i] == null)
                        {
                            results.Add(go);
                            break;
                        }
                    }
                }
            }

            // 見つかったらすぐ視認できるように
            Repaint();
        }

        private void DrawResults()
        {
            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("Missing Component is not found (or press Search).", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i];
                if (go == null) continue;

                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    // クリックで選択してヒエラルキー上で見えるようにする
                    if (GUILayout.Button(go.name, GUILayout.Height(22)))
                    {
                        SelectAndPing(go);
                    }

                    // 右側に ObjectField も置く（見やすい）
                    EditorGUILayout.ObjectField(go, typeof(GameObject), true, GUILayout.Width(220));
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void SelectAndPing(GameObject go)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            // ヒエラルキー上でちゃんとフォーカスさせる
            // Pingだけだと見失うことがあるので、Hierarchy Windowにフォーカスを当てる
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }
    }
}