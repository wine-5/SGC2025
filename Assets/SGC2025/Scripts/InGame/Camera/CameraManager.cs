using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// カメラ関連機能を統括するマネージャー
    /// CameraMoveとCameraShakeのFacadeとして機能
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("コンポーネント参照")]
        [SerializeField] private CameraMove cameraMove;
        
        [Header("シェイク設定")]
        [SerializeField] private CameraShake shakeSettings = new CameraShake();
        
        private float shakeTimer;
        private Vector3 currentShakeOffset;
        private float currentShakeMagnitude; // 現在のシェイク強度

        private void Awake()
        {
            // CameraMoveの参照を取得
            if (cameraMove == null)
                cameraMove = GetComponent<CameraMove>();
            
            // Playerのダメージイベントを購読
            PlayerCharacter.OnPlayerDamaged += HandlePlayerDamaged;
        }

        private void OnDestroy()
        {
            // イベント購読解除
            PlayerCharacter.OnPlayerDamaged -= HandlePlayerDamaged;
        }

        private void LateUpdate()
        {
            // シェイク処理（CameraMoveのLateUpdateの後に実行される）
            if (shakeTimer > 0)
            {
                // CameraMoveが計算した位置を保存
                Vector3 targetPosition = transform.position;
                
                // ランダムなシェイクオフセットを生成
                currentShakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
                currentShakeOffset.z = 0; // Z軸は固定（2Dゲームのため）
                
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

        /// <summary>
        /// Playerがダメージを受けた時の処理
        /// </summary>
        private void HandlePlayerDamaged(float hpRate)
        {
            TriggerShake(hpRate);
        }

        /// <summary>
        /// カメラシェイクをトリガー
        /// </summary>
        public void TriggerShake(float hpRate)
        {
            // HP率に応じてシェイクの強さを調整
            currentShakeMagnitude = shakeSettings.GetMagnitudeByHpRate(hpRate);
            shakeTimer = shakeSettings.Duration;
        }
    }
}
