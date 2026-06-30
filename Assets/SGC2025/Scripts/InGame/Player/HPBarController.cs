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
        [Tooltip("Playerの場合true。シーン再読込時にPlayerDataProviderからentityを再取得するために使用")]
        [SerializeField] private bool isPlayer;

        private float rate;

        private float maxHealth;
        private float currentHealth;
        private Vector3 offsetFromPlayer; // Playerからの相対オフセット（固定）
        private Vector3 originalScale;
        private Quaternion fixedRotation; // 親が回転しても固定したいワールド回転
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
            originalScale = transform.localScale;

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

            if (cachedDamageable != null)
                currentHealth = cachedDamageable.CurrentHealth;

            if (maxHealth > 0)
            {
                rate = currentHealth / maxHealth;
                transform.localScale = new Vector3(originalScale.x * rate, originalScale.y, originalScale.z);
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
