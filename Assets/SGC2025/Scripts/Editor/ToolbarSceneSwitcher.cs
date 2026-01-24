using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace SGC2025.Editor
{
    /// <summary>
    /// Unityツールバーにシーン切り替えボタンを追加するエディタ拡張
    /// リフレクションを使用してUnity内部のToolbarにアクセス
    /// </summary>
    [InitializeOnLoad]
    public static class ToolbarSceneSwitcher
    {
        #region 定数

        private const string SCENE_FOLDER = "Assets/SGC2025/Scenes";
        private const string TOOLBAR_ZONE_LEFT_ALIGN = "ToolbarZoneLeftAlign";
        private const string SCENE_ASSET_ICON = "SceneAsset Icon";
        private const float TOOLBAR_BUTTON_X_OFFSET = 100f;
        private const float TOOLBAR_BUTTON_Y_POSITION = 5f;
        private const float TOOLBAR_BUTTON_WIDTH = 80f;
        private const float TOOLBAR_BUTTON_HEIGHT = 24f;
        private const int ICON_BUTTON_WIDTH = 32;

        #endregion

        #region フィールド

        private static readonly Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject currentToolbar;

        #endregion

        #region 初期化

        static ToolbarSceneSwitcher()
        {
            EditorApplication.update += OnUpdate;
        }

        #endregion

        #region プライベートメソッド - 更新

        /// <summary>
        /// エディタ更新時にToolbarを検索してフックを登録
        /// </summary>
        private static void OnUpdate()
        {
            if (currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
                currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                
                if (currentToolbar != null)
                {
                    // ToolbarのGUIイベントにフックを追加
                    var root = currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var rawRoot = root?.GetValue(currentToolbar);
                    var mRoot = rawRoot as VisualElement;
                    
                    if (mRoot != null)
                    {
                        RegisterCallback(mRoot);
                    }
                    else
                    {
                        // フォールバック: ToolbarのOnGUIをフック
                        FieldInfo onGuiHandler = currentToolbar.GetType()
                            .GetField("m_OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
                        
                        if (onGuiHandler != null)
                        {
                            var handler = (Action)onGuiHandler.GetValue(currentToolbar);
                            handler -= OnGUI;
                            handler += OnGUI;
                            onGuiHandler.SetValue(currentToolbar, handler);
                        }
                    }
                }
            }
        }

        #endregion

        #region プライベートメソッド - UI構築

        /// <summary>
        /// UIElements APIを使用してツールバーにボタンを登録
        /// </summary>
        /// <param name="root">ツールバーのルート要素</param>
        private static void RegisterCallback(VisualElement root)
        {
            var toolbarZoneLeftAlign = root.Q(TOOLBAR_ZONE_LEFT_ALIGN);
            
            if (toolbarZoneLeftAlign != null)
            {
                var sceneIcon = EditorGUIUtility.IconContent(SCENE_ASSET_ICON).image as Texture2D;
                var button = new ToolbarButton(ShowSceneMenu)
                {
                    text = sceneIcon != null ? "" : "Scene",
                    tooltip = "シーン切り替え"
                };
                
                if (sceneIcon != null)
                {
                    button.style.backgroundImage = sceneIcon;
                    button.style.width = ICON_BUTTON_WIDTH;
                }
                
                toolbarZoneLeftAlign.Add(button);
                EditorApplication.update -= OnUpdate;
            }
        }

        /// <summary>
        /// IMGUI方式でツールバーにボタンを描画（フォールバック用）
        /// </summary>
        private static void OnGUI()
        {
            var screenWidth = EditorGUIUtility.currentViewWidth;
            var rect = new Rect(
                screenWidth * 0.5f - TOOLBAR_BUTTON_X_OFFSET,
                TOOLBAR_BUTTON_Y_POSITION,
                TOOLBAR_BUTTON_WIDTH,
                TOOLBAR_BUTTON_HEIGHT
            );
            
            var sceneIcon = EditorGUIUtility.IconContent(SCENE_ASSET_ICON);
            var content = new GUIContent(" Scene", sceneIcon.image, "シーン一覧を表示");
            
            if (GUI.Button(rect, content, EditorStyles.toolbarButton))
            {
                ShowSceneMenu();
            }
        }

        #endregion

        #region プライベートメソッド - メニュー表示

        /// <summary>
        /// シーン切り替えメニューを表示
        /// </summary>
        private static void ShowSceneMenu()
        {
            var menu = new GenericMenu();
            
            // 現在のシーン
            var currentScene = SceneManager.GetActiveScene();
            menu.AddDisabledItem(new GUIContent($"● {currentScene.name} (現在)"));
            menu.AddSeparator("");
            
            // Build Settingsに登録されている全シーン
            var scenes = GetAllScenes();
            
            if (scenes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("シーンが見つかりません"));
            }
            else
            {
                foreach (var sceneInfo in scenes)
                {
                    var sceneName = sceneInfo.name;
                    var scenePath = sceneInfo.path;
                    var isCurrent = sceneName == currentScene.name;
                    
                    if (isCurrent)
                    {
                        // 現在のシーンはスキップ（既に上部に表示）
                        continue;
        #endregion

        #region プライベートメソッド - シーン管理

        /// <summary>
        /// 指定フォルダ内の全シーン情報を取得
        /// </summary>
        /// <returns>シーン情報のリスト</returns>
        private static List<SceneInfo> GetAllScenes()
        {
            var scenes = new List<SceneInfo>();
            
            var allScenePaths = AssetDatabase.FindAssets("t:Scene", new[] { SCENE_FOLDER })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            
            foreach (var path in allScenePaths)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(new SceneInfo { name = sceneName, path = path });
            }
            
            return scenes.OrderBy(s => s.name).ToList();
        }

        /// <summary>
        /// 指定パスのシーンを開く
        /// </summary>
        /// <param name="scenePath">シーンのパス</param>
        private static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        #endregion

        #region 内部構造体

        /// <summary>
        /// シーン情報を保持する構造体
        /// </summary>            var allScenePaths = AssetDatabase.FindAssets("t:Scene", new[] { sceneFolder })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid));
            
            foreach (var path in allScenePaths)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                scenes.Add(new SceneInfo { name = sceneName, path = path });
            }

        #endregion
            
            return scenes.OrderBy(s => s.name).ToList();
        }

        private static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        private struct SceneInfo
        {
            public string name;
            public string path;
        }
    }
}

// UIElements用の互換性クラス
namespace SGC2025.Editor
{
    using UnityEngine.UIElements;
    
    /// <summary>
    /// UIElements向けのToolbarButton拡張
    /// </summary>
    public class ToolbarButton : Button
    {
        public ToolbarButton(Action clickEvent) : base(clickEvent)
        {
            AddToClassList("unity-toolbar-button");
            style.height = 22;
            style.marginTop = 4;
            style.marginBottom = 4;
            style.paddingLeft = 8;
            style.paddingRight = 8;
        }
    }
    
    /// <summary>
    /// VisualElement拡張メソッド
    /// </summary>
    public static class VisualElementExtensions
    {
        public static T Q<T>(this VisualElement element, string name = null) where T : VisualElement
        {
            if (element == null) return null;
            
            try
            {
                var method = typeof(VisualElement).GetMethod("Q", new[] { typeof(string), typeof(string) });
                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(typeof(T));
                    return (T)genericMethod.Invoke(element, new object[] { name, null });
                }
            }
            catch
            {
                // リフレクション失敗時はnullを返す
            }
            
            return null;
        }
    }
}
