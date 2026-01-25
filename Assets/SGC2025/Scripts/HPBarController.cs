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
        private Vector3 originalPos;
        private Vector3 originalScale;
        private PlayerCharacter cachedPlayer;
        private EnemyController cachedEnemy;
        private Transform parentTransform;
        private Transform entityTransform;

        void Start()
        {
            if (entity == null) return;
            parentTransform = transform.parent;
            entityTransform = entity.transform;
            originalPos = parentTransform.position - entityTransform.position;
            originalScale = transform.localScale;

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
            Vector3 newPos = entityTransform.position + originalPos;
            newPos.x = entityTransform.position.x + originalPos.x;
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
