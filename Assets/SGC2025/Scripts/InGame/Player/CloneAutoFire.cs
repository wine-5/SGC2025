using UnityEngine;
using SGC2025.Bullet;

namespace SGC2025.Player
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

        private float fireTimer;

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

        private void Fire()
        {
            if (BulletFactory.I == null) return;

            BulletFactory.I.CreateCircularBullets(transform.position, directions, bulletData);
        }
    }
}
