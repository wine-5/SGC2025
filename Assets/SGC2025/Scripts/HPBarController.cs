using UnityEngine;
using SGC2025.Enemy;
using SGC2025.Player;

namespace SGC2025
{
    /// <summary>
    /// エンティティのHPバーを制御するコンポーネント
    /// </summary>
    public class HPBarController : MonoBehaviour
    {
        [SerializeField] private GameObject entity;
        [Range(0f, 1f)]
        [SerializeField] private float rate;
        [SerializeField] private bool isPlayer;

        private float maxHealth;
        private float currentHealth;
        private Vector3 offsetFromPlayer; // Playerからの相対オフセット（固定）
        private Vector3 originalScale;
        private PlayerCharacter cachedPlayer;
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

            if (isPlayer)
            {
                cachedPlayer = entity.GetComponent<PlayerCharacter>();
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
            
            // Playerの位置 + 固定オフセット
            Vector3 newPos = entityTransform.position + offsetFromPlayer;
            parentTransform.position = newPos;

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
    }
}
