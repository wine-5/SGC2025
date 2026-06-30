using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tyotyo.Core.Log
{
    /// <summary>
    /// ログカテゴリの設定データ
    /// </summary>
    [System.Serializable]
    public class LogCategory
    {
        public string categoryName;
        public Color color = Color.white;
        public bool isEnabled = true;

        public LogCategory(string name, Color color, bool enabled = true)
        {
            this.categoryName = name;
            this.color = color;
            this.isEnabled = enabled;
        }
    }

    /// <summary>
    /// CusLog設定を保存するScriptableObject
    /// </summary>
    public class LoggerSettingsSO : ScriptableObject
    {
        private static LoggerSettingsSO _instance;
        public static LoggerSettingsSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LoggerSettingsSO>("LoggerSettings");
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        _instance = CreateInstance<LoggerSettingsSO>();
                        _instance.InitializeDefaultCategories();

                        string path = "Assets/Resources";
                        if (!UnityEditor.AssetDatabase.IsValidFolder(path))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/LoggerSettings.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
#endif
                }
                return _instance;
            }
        }

        [SerializeField]
        private List<LogCategory> _categories = new List<LogCategory>();

        public List<LogCategory> Categories => _categories;

        private const float DEFAULT_PLAYER_COLOR_R = 0.3f;
        private const float DEFAULT_PLAYER_COLOR_G = 0.7f;
        private const float DEFAULT_PLAYER_COLOR_B = 1f;

        private const float DEFAULT_ENEMY_COLOR_R = 1f;
        private const float DEFAULT_ENEMY_COLOR_G = 0.3f;
        private const float DEFAULT_ENEMY_COLOR_B = 0.3f;

        private const float DEFAULT_UI_COLOR_R = 0.5f;
        private const float DEFAULT_UI_COLOR_G = 1f;
        private const float DEFAULT_UI_COLOR_B = 0.5f;

        private const float DEFAULT_AUDIO_COLOR_R = 1f;
        private const float DEFAULT_AUDIO_COLOR_G = 0.8f;
        private const float DEFAULT_AUDIO_COLOR_B = 0.3f;

        private const float DEFAULT_NETWORK_COLOR_R = 1f;
        private const float DEFAULT_NETWORK_COLOR_G = 0.5f;
        private const float DEFAULT_NETWORK_COLOR_B = 1f;

        private const float DEFAULT_SYSTEM_COLOR_R = 0.8f;
        private const float DEFAULT_SYSTEM_COLOR_G = 0.8f;
        private const float DEFAULT_SYSTEM_COLOR_B = 0.8f;

        private const int HEX_COLOR_MAX_VALUE = 255;

        private void InitializeDefaultCategories()
        {
            _categories = new List<LogCategory>
            {
                new LogCategory("Player", new Color(DEFAULT_PLAYER_COLOR_R, DEFAULT_PLAYER_COLOR_G, DEFAULT_PLAYER_COLOR_B)),
                new LogCategory("Enemy", new Color(DEFAULT_ENEMY_COLOR_R, DEFAULT_ENEMY_COLOR_G, DEFAULT_ENEMY_COLOR_B)),
                new LogCategory("UI", new Color(DEFAULT_UI_COLOR_R, DEFAULT_UI_COLOR_G, DEFAULT_UI_COLOR_B)),
                new LogCategory("Audio", new Color(DEFAULT_AUDIO_COLOR_R, DEFAULT_AUDIO_COLOR_G, DEFAULT_AUDIO_COLOR_B)),
                new LogCategory("Network", new Color(DEFAULT_NETWORK_COLOR_R, DEFAULT_NETWORK_COLOR_G, DEFAULT_NETWORK_COLOR_B)),
                new LogCategory("System", new Color(DEFAULT_SYSTEM_COLOR_R, DEFAULT_SYSTEM_COLOR_G, DEFAULT_SYSTEM_COLOR_B)),
            };
        }

        /// <summary>
        /// カテゴリの色を取得
        /// </summary>
        public string GetCategoryColor(string categoryName)
        {
            var category = _categories.FirstOrDefault(c => c.categoryName == categoryName);
            if (category != null)
            {
                return ColorToHex(category.color);
            }

            return "#FFFFFF";
        }

        /// <summary>
        /// カテゴリが有効かどうかを判定
        /// </summary>
        public bool IsCategoryEnabled(string categoryName)
        {
            var category = _categories.FirstOrDefault(c => c.categoryName == categoryName);
            return category?.isEnabled ?? true;
        }

        /// <summary>
        /// カテゴリの有効/無効を設定
        /// </summary>
        public void SetCategoryEnabled(string categoryName, bool enabled)
        {
            var category = _categories.FirstOrDefault(c => c.categoryName == categoryName);
            if (category != null)
            {
                category.isEnabled = enabled;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// カテゴリを追加
        /// </summary>
        public void AddCategory(string categoryName, Color color)
        {
            if (!_categories.Any(c => c.categoryName == categoryName))
                _categories.Add(new LogCategory(categoryName, color));
        }

        /// <summary>
        /// カテゴリを削除
        /// </summary>
        public void RemoveCategory(string categoryName)
        {
            _categories.RemoveAll(c => c.categoryName == categoryName);
        }

        /// <summary>
        /// ColorをHEXコードに変換
        /// </summary>
        private string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * HEX_COLOR_MAX_VALUE);
            int g = Mathf.RoundToInt(color.g * HEX_COLOR_MAX_VALUE);
            int b = Mathf.RoundToInt(color.b * HEX_COLOR_MAX_VALUE);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
