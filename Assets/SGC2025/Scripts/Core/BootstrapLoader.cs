using UnityEngine;
using UnityEngine.SceneManagement;

namespace SGC2025.Core.Scene
{
    /// <summary>
    /// どのシーンから開始しても必要なManagerシーンを自動的にロードするBootstrap
    /// InGameシーンから直接テストする場合でも、Managerが正しく初期化されます
    /// </summary>
    public class BootstrapLoader
    {
        private const string _managerSceneName = "Manager";
        private static bool _isBootstrapped = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (_isBootstrapped)
            {
                Debug.Log("[Bootstrap] 既に初期化済みです");
                return;
            }

            _isBootstrapped = true;

            bool managerSceneLoaded = IsSceneLoaded(_managerSceneName);

            if (managerSceneLoaded)
            {
                Debug.Log($"[Bootstrap] {_managerSceneName}シーンは既にロードされています");
                return;
            }

            try
            {
                SceneManager.LoadScene(_managerSceneName, LoadSceneMode.Additive);
                Debug.Log($"[Bootstrap] {_managerSceneName}シーンをロードしました");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bootstrap] {_managerSceneName}シーンのロードに失敗しました: {e.Message}");
                Debug.LogError($"[Bootstrap] Build Settingsに'{_managerSceneName}'シーンが追加されているか確認してください");
            }
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                    return true;
            }
            return false;
        }
    }
}
