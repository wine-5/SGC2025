using UnityEngine;
using Tyotyo.Core;

namespace Tyotyo.InGame.Player
{
    /// <summary>
    /// エンティティのHPバーを制御するコンポーネント
    /// </summary>
    public class HPBarController : MonoBehaviour
    {
        [SerializeField] private GameObject entity;
        [Tooltip("HPバーの塗り部分。このTransformのScaleを変化させてHPを表現する")]
        [SerializeField] private Transform hpBarFill;
        [Tooltip("Playerの場合true。シーン再読込時にPlayerDataProviderからentityを再取得するために使用")]
        [SerializeField] private bool isPlayer;

        private float maxHealth;
        private float currentHealth;
        private Vector3 offsetFromPlayer;
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Quaternion fixedRotation;
        private IDamageable cachedDamageable;
        private Transform parentTransform;
        private Transform entityTransform;

        void Start()
        {
            // isPlayerの場合、シーン再読み込みでentity参照が切れている可能性があるため、PlayerDataProviderから取得
            if (isPlayer && (entity == null || !entity.activeInHierarchy))
            {
                if (PlayerDataProvider.I != null && PlayerDataProvider.I.IsPlayerRegistered)
                {
                    entity = PlayerDataProvider.I.PlayerTransform.gameObject;
                }
            }

            if (entity == null) return;

            parentTransform = transform.parent;
            entityTransform = entity.transform;

            // HPバーの塗り部分の初期スケール・位置を基準値としてキャッシュする
            originalScale = hpBarFill.localScale;
            originalPosition = hpBarFill.localPosition;

            // HPBarの初期位置を「Playerからの相対オフセット」として保存
            // （絶対座標を保存すると、原点から離れた位置にスポーンしたときにズレる）
            offsetFromPlayer = parentTransform.position - entityTransform.position;

            // 初期のワールド回転を保存し、以降この向きで固定する
            fixedRotation = parentTransform.rotation;

            cachedDamageable = entity.GetComponent<IDamageable>();
            if (cachedDamageable != null)
                maxHealth = cachedDamageable.MaxHealth;
        }

        void Update()
        {
            if (entity == null) return;
            if (entityTransform == null) return;
            if (cachedDamageable == null) return;

            currentHealth = cachedDamageable.CurrentHealth;

            if (maxHealth > 0)
            {
                float rate = Mathf.Clamp01(currentHealth / maxHealth);

                // Startでキャッシュした元のスケール(1.9等)を基準に割合をかける
                // ※ 現在のlocalScale.xを基準にすると前フレームの縮小値が累積し0に収束して消える
                float scaledX = originalScale.x * rate;
                hpBarFill.localScale = new Vector3(scaledX, originalScale.y, originalScale.z);

                // 右端を固定するため、減った分だけ左にシフト
                float positionShift = originalScale.x * (1 - rate) / 2;
                hpBarFill.localPosition = new Vector3(originalPosition.x - positionShift, originalPosition.y, originalPosition.z);
            }
        }

        // Playerの移動・回転が確定した後に補正することで、1フレームのズレ（チラつき）を防ぐ
        void LateUpdate()
        {
            if (entity == null) return;
            if (entityTransform == null) return;

            // Playerの位置 + 固定オフセット
            Vector3 newPos = entityTransform.position + offsetFromPlayer;
            // 親の回転を打ち消して向きを固定（位置と回転をまとめて設定）
            parentTransform.SetPositionAndRotation(newPos, fixedRotation);
        }
    }
}
