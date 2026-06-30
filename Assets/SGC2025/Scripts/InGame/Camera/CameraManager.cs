using UnityEngine;
using Tyotyo.Core;

namespace Tyotyo.Cam
{
    /// <summary>
    /// カメラ関連機能を統括するマネージャー
    /// CameraMovement（追従）と CameraShake（振動）を一元管理する
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField] private CameraMovement cameraMovement = new CameraMovement();

        [Header("シェイク設定")]
        [SerializeField] private CameraShake shakeSettings = new CameraShake();

        private float shakeTimer;
        private Vector3 currentShakeOffset;
        private float currentShakeMagnitude;

        private void Awake()
        {
            cameraMovement.Initialize(transform, GetComponent<UnityEngine.Camera>());
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamagedEvent);
        }

        private void Start()
        {
            cameraMovement.OnStart();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamagedEvent);
        }

        private void LateUpdate()
        {
            cameraMovement.OnLateUpdate();

            if (shakeTimer > 0f)
            {
                currentShakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
                currentShakeOffset.z = 0f;
                transform.position += currentShakeOffset;
                shakeTimer -= Time.deltaTime;
            }
            else if (currentShakeOffset != Vector3.zero)
            {
                currentShakeOffset = Vector3.zero;
            }
        }

        /// <summary>Playerがダメージを受けた時の処理</summary>
        private void OnPlayerDamagedEvent(PlayerDamagedEvent e) => TriggerShake(e.HpRate);

        /// <summary>カメラシェイクをトリガー</summary>
        private void TriggerShake(float hpRate)
        {
            currentShakeMagnitude = shakeSettings.GetMagnitudeByHpRate(hpRate);
            shakeTimer = shakeSettings.Duration;
        }
    }
}
