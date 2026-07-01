using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Tyotyo.Core.Log;
using Tyotyo.Manager;

namespace Tyotyo.UI
{
    /// <summary>
    /// UI画面がアクティブな時、デバイスに応じてフォーカスとカーソル表示を自動切り替える。
    /// コントローラー：最初のボタンに選択状態を維持、カーソル非表示。
    /// キーボード/マウス：フォーカスをクリア、カーソル表示（クリック操作に対応）。
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

        // イベント登録時の UIInputManager インスタンスをキャッシュ（登録解除時に使用）
        private UIInputManager cachedUIInputManager;

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

            // デバイスに応じてフォーカスとカーソル表示を制御
            // マウス操作時にフォーカスを残すと、クリックが「選択解除」に消費され
            // ボタンが反応しにくくなるため、コントローラー時のみフォーカスを付与する
            ApplyUIStateForDevice(target);
        }

        private void Start()
        {
            // UIInputManager のデバイス切り替え／決定イベントを購読
            // 登録したインスタンスをキャッシュして、登録解除時に同じインスタンスから削除するため
            cachedUIInputManager = UIInputManager.I;
            if (cachedUIInputManager != null)
            {
                cachedUIInputManager.OnDeviceSwitched += OnDeviceSwitched;
                cachedUIInputManager.OnSubmitPressed += OnSubmitPressed;
            }
        }

        private void OnDestroy()
        {
            // 登録したときと同じインスタンスから確実に登録解除
            if (cachedUIInputManager != null)
            {
                cachedUIInputManager.OnDeviceSwitched -= OnDeviceSwitched;
                cachedUIInputManager.OnSubmitPressed -= OnSubmitPressed;
            }
        }

        /// <summary>
        /// 決定（〇 / Shot）ボタンが押されたときの処理。
        /// どのボタンも選択されていない状態（マウス操作後など）でコントローラーの決定を押したら、
        /// アタッチされたボタンへフォーカスを復帰させる。
        /// すでに選択済みなら通常の決定に任せるため何もしない。
        /// </summary>
        private void OnSubmitPressed()
        {
            // このパネルがアクティブでないときは何もしない
            if (!gameObject.activeInHierarchy || !isActiveAndEnabled) return;

            // コントローラー操作時のみ対象
            if (Gamepad.current == null || !Gamepad.current.enabled) return;

            // すでにどこかのボタンが選択されているなら通常の決定に任せる
            if (UIFocusHelper.GetCurrentFocus() != null) return;

            GameObject target = firstSelected != null ? firstSelected : FindFirstSelectable();
            if (target != null)
                UIFocusHelper.SetFocus(target);
        }

        private void OnDisable()
        {
            // パネルを閉じたとき、開く前に選択していたボタンへフォーカスを戻す
            if (restoreFocusOnDisable)
                UIFocusHelper.RestoreFocus(previousSelected);
        }

        /// <summary>
        /// デバイスが切り替わった時に呼ばれるコールバック
        /// </のパネルが非アクティブ、またはこの GameObject が非アクティブなら何もしない
        /// </summary>
        private void OnDeviceSwitched(InputDeviceType deviceType)
        {
            // このパネル自体が非アクティブなら、フォーカス操作をしない
            if (!gameObject.activeInHierarchy) return;
            if (!isActiveAndEnabled) return;

            GameObject target = firstSelected != null ? firstSelected : FindFirstSelectable();
            if (target == null) return;

            ApplyUIStateForDevice(target);
        }

        /// <summary>
        /// デバイスの種類に応じてフォーカスとカーソル表示を制御
        /// コントローラー：target を選択状態にし、カーソル非表示（Navigation 操作に対応）
        /// マウス/キーボード：フォーカスをクリアし、カーソル表示（クリックが選択解除に消費されるのを防ぐ）
        /// </summary>
        private void ApplyUIStateForDevice(GameObject target)
        {
            if (target == null) return;

            if (UIInputManager.I == null) return; // UIInputManager が未初期化の場合はスキップ

            // ゲームパッド接続時：フォーカス付与＋カーソル非表示
            if (Gamepad.current != null && Gamepad.current.enabled)
            {
                UIFocusHelper.SetFocus(target);
                Cursor.visible = false;
            }
            // マウス/キーボード時：フォーカスをクリア＋カーソル表示
            // フォーカスを残すとクリックが選択解除に消費され、ボタンが反応しにくくなる
            else
            {
                UIFocusHelper.ClearFocus();
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// 子階層から最初の操作可能なSelectableを探す
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
