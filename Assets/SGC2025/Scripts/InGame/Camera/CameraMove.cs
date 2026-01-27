using UnityEngine;

namespace SGC2025.Camera
{
    /// <summary>
    /// プレイヤーをスムーズに追従するカメラ制御
    /// </summary>
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        
        [Header("カメラサイズ設定")]
        [SerializeField]
        [Tooltip("カメラの視野の大きさ（Orthographicカメラの場合）値が大きいほど引きの画面")]
        private float orthographicSize = 12f;

        private UnityEngine.Camera cam;

        public void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
        }

        public void Start()
        {
           transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
           
           if (cam != null && cam.orthographic)
               cam.orthographicSize = orthographicSize;
        }
        
        void LateUpdate()
        {
            if (target == null) return;
            Vector3 newPos = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed * Time.deltaTime);
        }
    }
}
