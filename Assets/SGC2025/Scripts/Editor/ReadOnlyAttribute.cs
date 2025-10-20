using UnityEngine;

namespace SGC2025.Editor
{
    /// <summary>
    /// Inspector上でフィールドを読み取り専用として表示するアトリビュート
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        // UnityのPropertyAttributeを継承するだけで十分
    }
}