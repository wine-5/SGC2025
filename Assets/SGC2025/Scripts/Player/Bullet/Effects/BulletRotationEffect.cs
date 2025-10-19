using UnityEngine;

namespace SGC2025.Player.Bullet.Effects
{
    /// <summary>
    /// 弾のスプライト回転エフェクトを管理するコンポーネント
    /// 弾の見た目の回転演出を提供し、BulletControllerから独立して動作
    /// </summary>
    public class BulletRotationEffect : MonoBehaviour
    {
        #region 設定フィールド
        
        [Header("回転設定")]
        [Tooltip("回転速度（度/秒）")]
        [SerializeField] private float rotationSpeed = 360f;
        
        [Tooltip("回転軸")]
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;
        
        [Tooltip("回転方向（1: 時計回り, -1: 反時計回り）")]
        [SerializeField] private float rotationDirection = 1f;
        
        [Header("詳細設定")]
        [Tooltip("弾がアクティブでない時も回転するか")]
        [SerializeField] private bool rotateWhenInactive = false;
        
        [Tooltip("ランダムな初期回転を適用するか")]
        [SerializeField] private bool randomInitialRotation = true;
        
        #endregion
        
        #region プライベートフィールド
        
        // キャッシュされたコンポーネント
        private Transform cachedTransform;
        private BulletController bulletController;
        
        // 回転状態
        private bool isRotating;
        private float currentRotationSpeed;
        
        #endregion
        
        #region プロパティ
        
        /// <summary>現在回転中かどうか</summary>
        public bool IsRotating => isRotating;
        
        /// <summary>現在の回転速度</summary>
        public float CurrentRotationSpeed => currentRotationSpeed;
        
        #endregion
        
        #region Unityライフサイクル
        
        private void Awake()
        {
            CacheComponents();
            InitializeRotation();
        }
        
        private void Update()
        {
            if (ShouldRotate())
            {
                PerformRotation();
            }
        }
        
        private void OnEnable()
        {
            StartRotation();
            
            if (randomInitialRotation)
            {
                ApplyRandomInitialRotation();
            }
        }
        
        private void OnDisable()
        {
            StopRotation();
        }
        
        #endregion
        
        #region パブリックメソッド
        
        /// <summary>
        /// 回転を開始
        /// </summary>
        public void StartRotation()
        {
            isRotating = true;
            currentRotationSpeed = rotationSpeed * rotationDirection;
        }
        
        /// <summary>
        /// 回転を停止
        /// </summary>
        public void StopRotation()
        {
            isRotating = false;
            currentRotationSpeed = 0f;
        }
        
        /// <summary>
        /// 回転速度を設定
        /// </summary>
        /// <param name="speed">回転速度（度/秒）</param>
        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
            if (isRotating)
            {
                currentRotationSpeed = rotationSpeed * rotationDirection;
            }
        }
        
        /// <summary>
        /// 回転方向を設定
        /// </summary>
        /// <param name="direction">回転方向（1: 時計回り, -1: 反時計回り）</param>
        public void SetRotationDirection(float direction)
        {
            rotationDirection = Mathf.Sign(direction);
            if (isRotating)
            {
                currentRotationSpeed = rotationSpeed * rotationDirection;
            }
        }
        
        /// <summary>
        /// 回転軸を設定
        /// </summary>
        /// <param name="axis">回転軸</param>
        public void SetRotationAxis(Vector3 axis)
        {
            rotationAxis = axis.normalized;
        }
        
        /// <summary>
        /// ランダムな初期回転を適用
        /// </summary>
        public void ApplyRandomInitialRotation()
        {
            if (cachedTransform != null)
            {
                float randomAngle = Random.Range(0f, 360f);
                cachedTransform.rotation = Quaternion.AngleAxis(randomAngle, rotationAxis);
            }
        }
        
        #endregion
        
        #region プライベートメソッド
        
        private void CacheComponents()
        {
            cachedTransform = transform;
            bulletController = GetComponent<BulletController>();
        }
        
        private void InitializeRotation()
        {
            currentRotationSpeed = rotationSpeed * rotationDirection;
            isRotating = true;
        }
        
        private bool ShouldRotate()
        {
            if (!isRotating) return false;
            
            // 弾がアクティブでない時の回転設定をチェック
            if (!rotateWhenInactive && bulletController != null && !bulletController.IsActive)
            {
                return false;
            }
            
            return true;
        }
        
        private void PerformRotation()
        {
            if (cachedTransform != null && currentRotationSpeed != 0f)
            {
                float rotationAmount = currentRotationSpeed * Time.deltaTime;
                cachedTransform.Rotate(rotationAxis, rotationAmount, Space.Self);
            }
        }
        
        #endregion
        
        #region エディター用メソッド
        
        #if UNITY_EDITOR
        /// <summary>
        /// Inspector上でのテスト用メソッド
        /// </summary>
        [ContextMenu("Test Rotation")]
        private void TestRotation()
        {
            if (Application.isPlaying)
            {
                if (isRotating)
                {
                    StopRotation();
                    Debug.Log("BulletRotationEffect: 回転停止");
                }
                else
                {
                    StartRotation();
                    Debug.Log("BulletRotationEffect: 回転開始");
                }
            }
        }
        
        [ContextMenu("Apply Random Rotation")]
        private void TestRandomRotation()
        {
            ApplyRandomInitialRotation();
            Debug.Log("BulletRotationEffect: ランダム回転適用");
        }
        #endif
        
        #endregion
    }
}