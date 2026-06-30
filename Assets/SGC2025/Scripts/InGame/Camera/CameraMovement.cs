using UnityEngine;

namespace Tyotyo.Cam
{
    /// <summary>
    /// プレイヤーをスムーズに追従するカメラ移動設定
    /// </summary>
    [System.Serializable]
    public class CameraMovement
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;

        [Header("カメラサイズ設定")]
        [SerializeField, Tooltip("カメラの視野の大きさ（Orthographicカメラの場合）値が大きいほど引きの画面")]
        private float orthographicSize = 12f;

        [SerializeField, Tooltip("Perspectiveカメラの場合の視野角 (Field of View)。値が大きいほど引きの画面")]
        private float fieldOfView = 60f;

        private Transform cameraTransform;
        private UnityEngine.Camera cam;

        /// <summary>
        /// カメラの Transform と Camera コンポーネントを渡して初期化
        /// </summary>
        public void Initialize(Transform cameraTf, UnityEngine.Camera camera)
        {
            cameraTransform = cameraTf;
            cam = camera;
        }

        /// <summary>
        /// Start に相当する初期配置処理
        /// </summary>
        public void OnStart()
        {
            if (target == null || cameraTransform == null) return;

            cameraTransform.position = new Vector3(
                target.position.x,
                target.position.y,
                cameraTransform.position.z);

            if (cam != null)
            {
                if (cam.orthographic)
                    cam.orthographicSize = orthographicSize;
                else
                    cam.fieldOfView = fieldOfView;
            }
        }

        /// <summary>
        /// LateUpdate に相当するスムーズ追従処理
        /// </summary>
        public void OnLateUpdate()
        {
            if (target == null || cameraTransform == null) return;

            Vector3 newPos = new Vector3(
                target.position.x,
                target.position.y,
                cameraTransform.position.z);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPos, smoothSpeed * Time.deltaTime);

            if (cam != null)
            {
                if (cam.orthographic && cam.orthographicSize != orthographicSize)
                    cam.orthographicSize = orthographicSize;
                else if (!cam.orthographic && cam.fieldOfView != fieldOfView)
                    cam.fieldOfView = fieldOfView;
            }
        }
    }
}
