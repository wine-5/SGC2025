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

        private const float _defaultPlayerColorR = 0.3f;
        private const float _defaultPlayerColorG = 0.7f;
        private const float _defaultPlayerColorB = 1f;

        private const float _defaultEnemyColorR = 1f;
        private const float _defaultEnemyColorG = 0.3f;
        private const float _defaultEnemyColorB = 0.3f;

        private const float _defaultUiColorR = 0.5f;
        private const float _defaultUiColorG = 1f;
        private const float _defaultUiColorB = 0.5f;

        private const float _defaultAudioColorR = 1f;
        private const float _defaultAudioColorG = 0.8f;
        private const float _defaultAudioColorB = 0.3f;

        private const float _defaultNetworkColorR = 1f;
        private const float _defaultNetworkColorG = 0.5f;
        private const float _defaultNetworkColorB = 1f;

        private const float _defaultSystemColorR = 0.8f;
        private const float _defaultSystemColorG = 0.8f;
        private const float _defaultSystemColorB = 0.8f;

        private const int _hexColorMaxValue = 255;

        private void InitializeDefaultCategories()
        {
            _categories = new List<LogCategory>
            {
                new LogCategory("Player", new Color(_defaultPlayerColorR, _defaultPlayerColorG, _defaultPlayerColorB)),
                new LogCategory("Enemy", new Color(_defaultEnemyColorR, _defaultEnemyColorG, _defaultEnemyColorB)),
                new LogCategory("UI", new Color(_defaultUiColorR, _defaultUiColorG, _defaultUiColorB)),
                new LogCategory("Audio", new Color(_defaultAudioColorR, _defaultAudioColorG, _defaultAudioColorB)),
                new LogCategory("Network", new Color(_defaultNetworkColorR, _defaultNetworkColorG, _defaultNetworkColorB)),
                new LogCategory("System", new Color(_defaultSystemColorR, _defaultSystemColorG, _defaultSystemColorB)),
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
            int r = Mathf.RoundToInt(color.r * _hexColorMaxValue);
            int g = Mathf.RoundToInt(color.g * _hexColorMaxValue);
            int b = Mathf.RoundToInt(color.b * _hexColorMaxValue);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}
