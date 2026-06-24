using UnityEngine;
using UnityEditor;

namespace Tyotyo.Editor
{
    /// <summary>
    /// ReadOnlyAttribute用のPropertyDrawer
    /// Inspector上でフィールドを無効化して読み取り専用にする
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 無効化してフィールドを描画
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // デフォルトの高さを返す
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}