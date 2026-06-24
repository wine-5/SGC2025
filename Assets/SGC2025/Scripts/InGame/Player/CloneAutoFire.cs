using UnityEngine;
using Tyotyo.InGame.Bullet;

namespace Tyotyo.InGame.Player
{
    /// <summary>
    /// クローン（蝶）の自動発射。
    /// アクティブな間、一定間隔で全方位（既定4方向＝90度刻みの縦横）に弾を撃つ。
    /// プレイヤーの入力や武器レベルには連動しない固定発射。
    /// </summary>
    public class CloneAutoFire : MonoBehaviour
    {
        [SerializeField, Tooltip("発射間隔（秒）")]
        private float fireInterval = 2f;

        [SerializeField, Tooltip("発射方向数（4 = 90度刻みの縦横）")]
        private int directions = 4;

        [SerializeField, Tooltip("使用する弾データ（プレイヤーと同じものでOK）")]
        private BulletDataSO bulletData;

        [Header("向き・位置の固定")]
        [SerializeField, Tooltip("ONにすると、親(Player)が回転しても相対位置を固定する（位置は一緒に回らない）")]
        private bool fixToWorld = true;

        [SerializeField, Tooltip("ONにすると、位置は固定したまま向きだけ親(Player)の回転に追従する（方向を見せたい場合）")]
        private bool followParentRotation = true;

        private float fireTimer;
        private Transform parentTransform;
        private Vector3 fixedOffset;       // 親からの相対位置（固定）
        private Quaternion fixedRotation;  // 親が回転しても保つワールド回転

        private void Awake()
        {
            // 設計時のローカル値を、固定すべきオフセット・向きとして保持する
            // （ローカル値は親の回転に影響されないため、有効化タイミングが途中でも設計値が得られる）
            parentTransform = transform.parent;
            fixedOffset = transform.localPosition;
            fixedRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            // 有効化されたタイミングから計測を開始する
            fireTimer = 0f;
        }

        private void Update()
        {
            fireTimer += Time.deltaTime;
            if (fireTimer < fireInterval) return;

            fireTimer -= fireInterval;
            Fire();
        }

        // Playerの移動・回転が確定した後に補正し、1フレームのズレ（チラつき）を防ぐ
        private void LateUpdate()
        {
            if (!fixToWorld || parentTransform == null) return;

            // 位置は親の回転を打ち消して相対位置を固定（一緒に回らない）。
            // 向きは followParentRotation が ON なら親の回転に追従させ、方向が分かるようにする。
            Quaternion rotation = followParentRotation
                ? parentTransform.rotation * fixedRotation
                : fixedRotation;
            transform.SetPositionAndRotation(parentTransform.position + fixedOffset, rotation);
        }

        private void Fire()
        {
            if (BulletFactory.I == null) return;

            BulletFactory.I.CreateCircularBullets(transform.position, directions, bulletData);
        }
    }
}
