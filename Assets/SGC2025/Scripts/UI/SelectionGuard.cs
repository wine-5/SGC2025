using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Tyotyo.UI
{
    /// <summary>
    /// 選択中のUIがnullになったとき、直前に選択していた有効な要素へ自動で復帰させる。
    /// コントローラー操作で「選択が外れて操作不能」になるのを防ぐ。
    /// マウス操作中は復帰を行わない（マウスとパッドの競合・誤フォーカスを防ぐ）。
    /// 常駐するEventSystemと同じGameObjectにアタッチして使う。
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class SelectionGuard : MonoBehaviour
    {
        [Tooltip("マウス操作中とみなすカーソル移動量のしきい値（二乗）")]
        [SerializeField] private float mouseMoveThreshold = 0.01f;

        private GameObject lastSelected;

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return;

            GameObject current = eventSystem.currentSelectedGameObject;

            // 有効な選択中は常にそれを記憶しておく（マウス操作中でも記録は続ける）
            if (current != null && current.activeInHierarchy)
            {
                lastSelected = current;
                return;
            }

            // マウス操作中は強制復帰しない。
            // マウスでメニューをクリックした瞬間に選択が一時的に外れても、
            // 直前の選択（右パネル等）へ引き戻さないようにする。
            if (IsMouseBeingUsed()) return;

            // 選択が外れた → まだ画面に存在する直前の要素へ復帰させる。
            // 復帰対象が非表示（パネルを閉じた等）なら何もしないので、
            // ゲームプレイ中など「UIを選択すべきでない場面」では干渉しない。
            if (lastSelected != null && lastSelected.activeInHierarchy)
                eventSystem.SetSelectedGameObject(lastSelected);
        }

        /// <summary>マウスが動いている／ボタンが押されている間はマウス操作中とみなす。</summary>
        private bool IsMouseBeingUsed()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return false;

            return mouse.delta.ReadValue().sqrMagnitude > mouseMoveThreshold
                || mouse.leftButton.isPressed
                || mouse.rightButton.isPressed;
        }
    }
}
