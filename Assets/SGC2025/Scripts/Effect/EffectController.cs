using UnityEngine;

namespace SGC2025.Effect
{
    /// <summary>
    /// エフェクトの挙動を制御するコンポーネント
    /// 2つのモードを持つ:
    ///   - Transform追従モード: エフェクト自身を追従対象へ移動させる（プレイヤー追従など）
    ///   - VFXターゲットモード: VFXの公開プロパティへターゲット座標を渡し、パーティクルをそこへ飛ばす（緑化パーティクル等）
    /// </summary>
    public class EffectController : MonoBehaviour
    {
        [Header("追従設定")]
        [SerializeField, Tooltip("追従／ターゲットへのオフセット位置")]
        private Vector3 followOffset = Vector3.zero;

        [SerializeField, Tooltip("追従の滑らかさ（0で即座に追従）")]
        private float followSmooth = 0f;

        [SerializeField, Tooltip("回転も追従するか")]
        private bool followRotation = true;

        [Header("VFXターゲットモード")]
        [SerializeField, Tooltip("ONにすると、エフェクト自身を生成位置からターゲット（ゲージ）へ向かって移動させる")]
        private bool sendTargetToVfx = false;

        [SerializeField, Tooltip("ターゲットがUI要素のとき、向かう先をカメラのビューポート座標で指定（左下0,0〜右上1,1）。緑化度ゲージは画面左上")]
        private Vector2 gaugeViewportPoint = new Vector2(0.08f, 0.88f);

        private Transform followTarget;
        private float duration;
        private float startTime;
        private Vector3 travelStartPosition;

        /// <summary>
        /// エフェクトを初期化
        /// </summary>
        /// <param name="target">追従対象／ターゲット（nullの場合は何もしない）</param>
        /// <param name="effectDuration">エフェクトの持続時間</param>
        public void Initialize(Transform target, float effectDuration)
        {
            followTarget = target;
            duration = effectDuration;
            startTime = Time.time;
            travelStartPosition = transform.position; // 生成位置（敵の位置）を移動の起点として記憶
        }

        private void Update()
        {
            if (sendTargetToVfx)
                UpdateTargetTravel();
            else
                UpdateTransformFollow();

            if (duration > 0f && Time.time - startTime >= duration)
                ReturnToPool();
        }

        /// <summary>Transform追従モード: エフェクト自身を追従対象へ移動させる</summary>
        private void UpdateTransformFollow()
        {
            if (followTarget == null) return;

            Vector3 targetPosition = followTarget.position + followOffset;

            if (followSmooth > 0f)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSmooth);
            else
                transform.position = targetPosition;

            if (followRotation)
            {
                if (followSmooth > 0f)
                    transform.rotation = Quaternion.Lerp(transform.rotation, followTarget.rotation, Time.deltaTime * followSmooth);
                else
                    transform.rotation = followTarget.rotation;
            }
        }

        /// <summary>VFXターゲットモード: 生成位置からゲージへ向かってエフェクト自身を移動させる</summary>
        private void UpdateTargetTravel()
        {
            if (followTarget == null) return;

            Vector3 worldTarget = ResolveWorldPosition(followTarget) + followOffset;

            // 持続時間をかけて起点（敵）→ターゲット（ゲージ）へ移動。終盤を少し加速させる
            float t = duration > 0f ? Mathf.Clamp01((Time.time - startTime) / duration) : 1f;
            float eased = t * t;
            transform.position = Vector3.Lerp(travelStartPosition, worldTarget, eased);
        }

        /// <summary>
        /// 追従対象のワールド座標を求める。
        /// UI(RectTransform)の場合は、Canvasの設定に左右されないよう
        /// カメラのビューポート座標から画面上の位置をワールド座標へ変換する。
        /// </summary>
        private Vector3 ResolveWorldPosition(Transform target)
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;

            if (target is RectTransform && cam != null)
            {
                Vector3 viewport = new Vector3(gaugeViewportPoint.x, gaugeViewportPoint.y, Mathf.Abs(cam.transform.position.z));
                Vector3 world = cam.ViewportToWorldPoint(viewport);
                world.z = 0f;
                return world;
            }

            return target.position;
        }

        /// <summary>
        /// エフェクトをプールに返却
        /// </summary>
        private void ReturnToPool()
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
