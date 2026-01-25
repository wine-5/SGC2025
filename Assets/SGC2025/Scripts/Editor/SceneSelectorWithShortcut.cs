using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

namespace SGC2025.Editor
{
    public class SceneSelectorWithShortcut : EditorWindow
{
    [MenuItem("Tools/Scene Selector %&s")] // Ctrl + Alt + Sで開く
    public static void ShowWindow()
    {
        GetWindow<SceneSelectorWithShortcut>("Scene Selector");
    }

    void OnGUI()
    {
        // Assets以下の全てのSceneファイルを取得
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        // GUIDをパスに変換して、ファイル名を取得
        var scenePaths = sceneGUIDs
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => new { path, name = Path.GetFileNameWithoutExtension(path) })
            .OrderBy(s => s.name) // 名前順で並べ替え（必要なら変更可）
            .ToList();

        // すべてのシーンを表示
        foreach (var scene in scenePaths)
        {
            if (GUILayout.Button(scene.name))
            {
                // 現在のシーンに保存が必要なら保存を促し、選択したシーンを開く
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    if (File.Exists(scene.path))
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                    else
                    {
                        Debug.LogError($"Scene file not found: {scene.path}");
                    }
                }
            }
        }
    }
}