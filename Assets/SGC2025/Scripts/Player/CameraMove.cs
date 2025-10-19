using UnityEngine;

namespace SGC2025
{

    public class CameraMove : MonoBehaviour
    {
        [SerializeField]
        private Transform target;   // 追従する対象（プレイヤー）
        [SerializeField]
        private Vector3 offset = new Vector3(0, 5, -10); // カメラの位置ずれ
        [SerializeField]
        private float smoothSpeed = 5f; // スムーズな追従速度      

        public void Start()
        {
           transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
        
        void LateUpdate()
        {
            if (target == null) return;
            Vector3 newPos = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed * Time.deltaTime);
        }

    }
}
