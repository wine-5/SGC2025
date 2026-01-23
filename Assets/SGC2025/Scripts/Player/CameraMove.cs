using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// プレイヤーをスムーズに追従するカメラ制御
    /// </summary>
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);
        [SerializeField] private float smoothSpeed = 5f;
        
        [Header("カメラサイズ設定")]
        [SerializeField]
        [Tooltip("カメラの視野の大きさ（Orthographicカメラの場合）")]
        private float orthographicSize = 8f;

        private Camera cam;

        public void Awake()
        {
            cam = GetComponent<Camera>();
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
