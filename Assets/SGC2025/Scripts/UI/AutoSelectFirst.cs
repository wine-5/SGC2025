using UnityEngine;
using UnityEngine.UI;
using Polychroma.Core.Log;

namespace SGC2025.UI
{
    /// <summary>
    /// 有効化されたときに、指定したUI要素（未指定なら最初の操作可能な子）へ自動でフォーカスを設定する。
    /// コントローラー操作でカーソルが無くても「最初の選択先」を保証するためのコンポーネント。
    /// 画面ルートや開閉するパネルのルートにアタッチして使う。
    /// </summary>
    public class AutoSelectFirst : MonoBehaviour
    {
        [Tooltip("最初に選択するUI要素。未指定の場合は子階層から最初の操作可能なSelectableを自動で探す。")]
        [SerializeField] private GameObject firstSelected;

        [Tooltip("無効化されたとき、有効化する直前に選択していた要素へフォーカスを戻す。パネルを閉じたら開いた元のボタンに戻したい場合はON。")]
        [SerializeField] private bool restoreFocusOnDisable = true;

        // 有効化される直前に選択されていた要素（閉じたときの戻り先）
        private GameObject previousSelected;

        private void OnEnable()
        {
            // 戻り先として、開く直前の選択を記憶しておく
            previousSelected = UIFocusHelper.GetCurrentFocus();

            GameObject target = firstSelected != null ? firstSelected : FindFirstSelectable();
            if (target == null)
            {
                CusLog.Error("UI", $"[AutoSelectFirst] 選択できるUI要素が見つかりません。'{name}' に firstSelected を割り当てるか、操作可能なSelectableを子に配置してください。");
                return;
            }

            UIFocusHelper.SetFocus(target);
        }

        private void OnDisable()
        {
            // パネルを閉じたとき、開く前に選択していたボタンへフォーカスを戻す
            if (restoreFocusOnDisable)
                UIFocusHelper.RestoreFocus(previousSelected);
        }

        /// <summary>
        /// 子階層から最初の操作可能なSelectableを探す。
        /// </summary>
        private GameObject FindFirstSelectable()
        {
            foreach (Selectable selectable in GetComponentsInChildren<Selectable>(false))
            {
                if (selectable.interactable && selectable.navigation.mode != Navigation.Mode.None)
                    return selectable.gameObject;
            }
            return null;
        }
    }
}
