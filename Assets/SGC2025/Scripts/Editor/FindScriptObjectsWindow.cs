using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Tyotyo.Core.Log;

namespace Tyotyo.Editor
{
    public class FindScriptObjectsWindow : EditorWindow
    {
        private MonoScript targetScript; // 検索対象のスクリプト
        private List<GameObject> foundObjects = new List<GameObject>(); // 見つかったオブジェクトのリスト

        [MenuItem("Tools/Find Script Objects")]
        public static void OpenWindow()
        {
            GetWindow<FindScriptObjectsWindow>("Find Script Objects");
        }

        private void OnGUI()
        {
            GUILayout.Label("Find Objects with Script", EditorStyles.boldLabel);

            // スクリプトを選択するフィールド
            targetScript = (MonoScript)EditorGUILayout.ObjectField("Script", targetScript, typeof(MonoScript), false);

            if (GUILayout.Button("Find"))
            {
                if (targetScript != null)
                {
                    FindObjectsWithScript();
                }
                else
                {
                    CusLog.Warning("FindScriptObjectsWindow", "Please select a script to search for.");
                }
            }

            GUILayout.Space(10);

            // 検索結果のリスト表示
            if (foundObjects.Count > 0)
            {
                GUILayout.Label($"Found {foundObjects.Count} objects:", EditorStyles.boldLabel);

                foreach (GameObject obj in foundObjects)
                {
                    // 破棄されたオブジェクトをスキップ
                    if (obj == null) continue;

                    if (GUILayout.Button(obj.name))
                    {
                        Selection.activeGameObject = obj; // オブジェクトを選択
                    }
                }
            }
        }

        private void FindObjectsWithScript()
        {
            foundObjects.Clear();

            // スクリプトの型を取得
            System.Type scriptType = targetScript.GetClass();
            if (scriptType == null || !typeof(MonoBehaviour).IsAssignableFrom(scriptType))
            {
                CusLog.Error("FindScriptObjectsWindow", "Selected script is not a MonoBehaviour.");
                return;
            }

            // シーン内の全てのGameObjectを検索（非アクティブなオブジェクトも含める）
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);

            int activeCount = 0;
            int inactiveCount = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.GetComponent(scriptType) != null)
                {
                    foundObjects.Add(obj);
                    if (obj.activeInHierarchy)
                    {
                        activeCount++;
                    }
                    else
                    {
                        inactiveCount++;
                        CusLog.Log("FindScriptObjectsWindow", $"[INACTIVE] {obj.name}");
                    }
                }
            }

            CusLog.Log("FindScriptObjectsWindow", $"Found {foundObjects.Count} objects with the script {targetScript.name}. (Active: {activeCount}, Inactive: {inactiveCount})");
        }
    }
}
