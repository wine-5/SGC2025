using UnityEngine;
using SGC2025.Player;
using SGC2025.Core;

namespace SGC2025.Camera
{
    /// <summary>
    /// カメラ関連機能を統括するマネージャー
    /// CameraMovementとCameraShakeのFacadeとして機能
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("コンポーネント参照")]
        [SerializeField] private CameraMovement CameraMovement;
        
        [Header("シェイク設定")]
        [SerializeField] private CameraShake shakeSettings = new CameraShake();
        
        private float shakeTimer;
        private Vector3 currentShakeOffset;
        private float currentShakeMagnitude;

        private void Awake()
        {
            // CameraMovementの参照を取得
            if (CameraMovement == null)
                CameraMovement = GetComponent<CameraMovement>();
            
            // Playerのダメージイベントを購読
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamagedEvent);
        }

        private void OnDestroy()
        {
            // イベント購読解除
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamagedEvent);
        }

        private void LateUpdate()
        {
            if (shakeTimer > 0f)
            {
                // CameraMovementが計算した位置を保存
                Vector3 targetPosition = transform.position;
                
                currentShakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
                currentShakeOffset.z = 0f;
                
                // シェイクオフセットを適用
                transform.position = targetPosition + currentShakeOffset;
                
                shakeTimer -= Time.deltaTime;
            }
            else if (currentShakeOffset != Vector3.zero)
            {
                // シェイク終了時にオフセットをクリア
                currentShakeOffset = Vector3.zero;
            }
        }

        /// <summary>Playerがダメージを受けた時の処理</summary>
        private void OnPlayerDamagedEvent(PlayerDamagedEvent e) => TriggerShake(e.HpRate);

        /// <summary>カメラシェイクをトリガー</summary>
        public void TriggerShake(float hpRate)
        {
            currentShakeMagnitude = shakeSettings.GetMagnitudeByHpRate(hpRate);
            shakeTimer = shakeSettings.Duration;
        }
    }
}
