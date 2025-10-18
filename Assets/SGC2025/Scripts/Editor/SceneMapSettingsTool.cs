using UnityEngine;
using System.IO;
using SGC2025;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ===============================
// シーンマップ設定ウィンドウ
// ===============================
#if UNITY_EDITOR
public class SceneMapSettingsWindow : EditorWindow
{
    private MapSettings currentSettings;

    [MenuItem("Window/SGC2025/シーンマップ設定")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneMapSettingsWindow>("シーンマップ設定");
        window.minSize = new Vector2(300, 250);
        window.LoadOrCreateSettings();
    }

    private void LoadOrCreateSettings()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string folder = "Assets/Maps";
        string path = $"{folder}/{sceneName}_MapSettings.asset";

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        currentSettings = AssetDatabase.LoadAssetAtPath<MapSettings>(path);
        if (currentSettings == null)
        {
            currentSettings = ScriptableObject.CreateInstance<MapSettings>();
            currentSettings.sceneName = sceneName;
            AssetDatabase.CreateAsset(currentSettings, path);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        if (currentSettings == null)
        {
            if (GUILayout.Button("設定を読み込み / 初期化"))
            {
                LoadOrCreateSettings();
            }
            return;
        }

        EditorGUILayout.LabelField("シーン名: " + currentSettings.sceneName, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        currentSettings.columns = EditorGUILayout.IntField("列数", currentSettings.columns);
        currentSettings.rows = EditorGUILayout.IntField("行数", currentSettings.rows);

        EditorGUILayout.Space();

        currentSettings.aspectW = EditorGUILayout.FloatField("アスペクト比（横）", currentSettings.aspectW);
        currentSettings.aspectH = EditorGUILayout.FloatField("アスペクト比（縦）", currentSettings.aspectH);
        currentSettings.cellWidth = EditorGUILayout.FloatField("マス幅", currentSettings.cellWidth);
        currentSettings.cellHeight = EditorGUILayout.FloatField("マス高さ", currentSettings.cellHeight);
        currentSettings.spacing = EditorGUILayout.Vector2Field("マス間の間隔", currentSettings.spacing);

        EditorGUILayout.Space(15);

        if (GUILayout.Button("JSONとして保存"))
        {
            SaveToJson();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(currentSettings);
        }
    }

    private void SaveToJson()
    {
        string sceneName = currentSettings.sceneName;
        string folder = "Assets/Maps";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string json = JsonUtility.ToJson(currentSettings, true);
        string path = $"{folder}/{sceneName}_map.json";

        File.WriteAllText(path, json);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("保存完了", $"JSONを保存しました:\n{path}", "OK");
    }
}
#endif
