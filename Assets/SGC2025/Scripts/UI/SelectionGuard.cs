using UnityEngine;
using UnityEngine.EventSystems;

namespace SGC2025.UI
{
    /// <summary>
    /// 選択中のUIがnullになったとき、直前に選択していた有効な要素へ自動で復帰させる。
    /// マウスで空白をクリックした際の選択解除や、マウス↔コントローラー切替で
    /// 操作不能になる定番の不具合を防ぐ。
    /// 常駐するEventSystemと同じGameObjectにアタッチして使う。
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class SelectionGuard : MonoBehaviour
    {
        private GameObject lastSelected;

        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return;

            GameObject current = eventSystem.currentSelectedGameObject;

            // 有効な選択中はそれを記憶しておく
            if (current != null && current.activeInHierarchy)
            {
                lastSelected = current;
                return;
            }

            // 選択が外れた → まだ画面に存在する直前の要素へ復帰させる。
            // 復帰対象が非表示（パネルを閉じた等）なら何もしないので、
            // ゲームプレイ中など「UIを選択すべきでない場面」では干渉しない。
            if (lastSelected != null && lastSelected.activeInHierarchy)
                eventSystem.SetSelectedGameObject(lastSelected);
        }
    }
}
