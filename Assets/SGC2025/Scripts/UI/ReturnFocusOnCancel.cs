using UnityEngine;
using Tyotyo.Manager;

namespace Tyotyo.UI
{
    /// <summary>
    /// このパネル内の要素にフォーカスがある状態でCancel（×ボタン）が押されたら、
    /// 指定した要素（左メニューのボタン等）へフォーカスを移す。パネル自体は閉じない。
    /// 常に表示されたまま、フォーカスだけメニューへ戻したい側パネル向け。
    /// </summary>
    public class ReturnFocusOnCancel : MonoBehaviour
    {
        [Tooltip("×を押したときにフォーカスを戻す先（左メニューのボタンなど）。")]
        [SerializeField] private GameObject returnTarget;

        private void OnEnable()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnCancelPressed += ReturnFocus;
        }

        private void OnDisable()
        {
            if (UIInputManager.I != null)
                UIInputManager.I.OnCancelPressed -= ReturnFocus;
        }

        private void ReturnFocus()
        {
            GameObject current = UIFocusHelper.GetCurrentFocus();

            // このパネル内にフォーカスがある時だけ戻す（他画面の操作に干渉しない）
            if (current == null || !current.transform.IsChildOf(transform)) return;

            UIFocusHelper.SetFocus(returnTarget);
        }
    }
}
