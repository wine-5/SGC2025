using UnityEngine;
using SGC2025.Enemy;
using SGC2025.Player;

namespace SGC2025.Player
{
    /// <summary>
    /// エンティティのHPバーを制御するコンポーネント
    /// </summary>
    public class HPBarController : MonoBehaviour
    {
        [SerializeField] private GameObject entity;
        [SerializeField] private bool isPlayer;

        private float rate;

        private float maxHealth;
        private float currentHealth;
        private Vector3 offsetFromPlayer; // Playerからの相対オフセット（固定）
        private Vector3 originalScale;
        private Quaternion fixedRotation; // 親が回転しても固定したいワールド回転
        private PlayerController cachedPlayer;
        private EnemyController cachedEnemy;
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

            // HPBarの初期位置を相対オフセットとして保存（Inspectorで設定された値）
            offsetFromPlayer = parentTransform.position;

            // 初期のワールド回転を保存し、以降この向きで固定する
            fixedRotation = parentTransform.rotation;

            if (isPlayer)
            {
                cachedPlayer = entity.GetComponent<PlayerController>();
                if (cachedPlayer != null)
                    maxHealth = cachedPlayer.GetPlayerMaxHealth();
            }
            else
            {
                cachedEnemy = entity.GetComponent<EnemyController>();
                if (cachedEnemy != null)
                    maxHealth = cachedEnemy.MaxHealth;
            }
        }

        void Update()
        {
            if (entity == null) return;
            if (entityTransform == null) return;

            if (isPlayer)
            {
                if (cachedPlayer != null)
                    currentHealth = cachedPlayer.GetPlayerCurrentHealth();
            }
            else
            {
                if (cachedEnemy != null)
                    currentHealth = cachedEnemy.CurrentHealth;
            }

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
