using UnityEngine;

namespace SGC2025.Effect
{
    /// <summary>
    /// エフェクトの挙動を制御するコンポーネント
    /// Player追従や自動返却などを管理
    /// </summary>
    public class EffectController : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField, Tooltip("追従のオフセット位置")]
        private Vector3 followOffset = Vector3.zero;
        
        [SerializeField, Tooltip("追従の滑らかさ（0で即座に追従）")]
        private float followSmooth = 0f;
        
        private Transform followTarget;
        private float duration;
        private float startTime;
        
        /// <summary>
        /// エフェクトを初期化
        /// </summary>
        /// <param name="target">追従対象（nullの場合は追従しない）</param>
        /// <param name="effectDuration">エフェクトの持続時間</param>
        public void Initialize(Transform target, float effectDuration)
        {
            followTarget = target;
            duration = effectDuration;
            startTime = Time.time;
            
            if (followTarget != null)
            {
                transform.position = followTarget.position + followOffset;
                transform.rotation = followTarget.rotation;
            }
        }
        
        private void Update()
        {
            if (followTarget != null)
            {
                Vector3 targetPosition = followTarget.position + followOffset;
                
                if (followSmooth > 0f)
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmooth);
                else
                    transform.position = targetPosition;
            }
            
            if (duration > 0f && Time.time - startTime >= duration)
                ReturnToPool();
        }
        
        /// <summary>
        /// エフェクトをプールに返却
        /// </summary>
        public void ReturnToPool()
        {
            if (EffectFactory.I != null)
                EffectFactory.I.ReturnEffect(gameObject);
            else
            {
                Debug.LogError("[EffectController] EffectFactory is not available! Cannot return effect to pool.");
                gameObject.SetActive(false);
            }
        }
        
        private void OnDisable()
        {
            // リセット
            followTarget = null;
        }
    }
}
