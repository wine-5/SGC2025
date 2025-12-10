using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// Inspector上でフィールドを読み取り専用として表示するアトリビュート
    /// ランタイムスクリプト用
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        // UnityのPropertyAttributeを継承するだけで十分
    }
}