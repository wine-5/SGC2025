using UnityEngine;
using UnityEngine.EventSystems;

namespace SGC2025.UI
{
    /// <summary>
    /// UI要素のフォーカス（選択状態）管理を行うヘルパークラス
    /// コントローラー操作時のUI選択を簡潔に扱うための静的ユーティリティ
    /// </summary>
    public static class UIFocusHelper
    {
        /// <summary>
        /// 指定したUI要素にフォーカスを設定
        /// </summary>
        /// <param name="target">フォーカスするUI要素</param>
        public static void SetFocus(GameObject target)
        {
            if (EventSystem.current == null || target == null) return;
            
            EventSystem.current.SetSelectedGameObject(null); // 一度リセット
            EventSystem.current.SetSelectedGameObject(target);
        }

        /// <summary>
        /// 現在フォーカスされているUI要素を取得
        /// </summary>
        /// <returns>現在選択されているGameObject（nullの場合もある）</returns>
        public static GameObject GetCurrentFocus()
        {
            return EventSystem.current?.currentSelectedGameObject;
        }

        /// <summary>
        /// フォーカスをクリア
        /// </summary>
        public static void ClearFocus()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        /// 以前のフォーカスを復元
        /// </summary>
        /// <param name="previousFocus">復元するUI要素</param>
        public static void RestoreFocus(GameObject previousFocus)
        {
            if (previousFocus != null)
                SetFocus(previousFocus);
        }
    }
}
