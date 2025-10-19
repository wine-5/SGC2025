using UnityEngine;

using SGC2025.Enemy;

namespace SGC2025
{
    public class HPBarController : MonoBehaviour
    {
        [SerializeField] GameObject entity;
        

        [Range(0f, 1f)]
        [SerializeField] private float rate;
        private float maxHealth;
        private float currentHealth;
        private Vector3 originalPos;
        private Vector3 originalScale;

        [SerializeField] private bool isPlayer;

        void Start()
        {
            originalPos = transform.parent.position;
            originalScale = transform.localScale;

            if (isPlayer)
            {
                maxHealth = entity.GetComponent<PlayerCharacter>().GetPlayerMaxHealth();

            }
            else
            {
                //エネミーの最大体力
                maxHealth = entity.GetComponent<EnemyController>().MaxHealth;
            }
        }

        void Update()
        {
            transform.parent.position = entity.transform.position + originalPos;

            if (isPlayer)
            {
                 currentHealth = entity.GetComponent<PlayerCharacter>().GetPlayerCurrentHalth();
            }
            else
            {
                currentHealth = entity.GetComponent<EnemyController>().CurrentHealth;
            }


            rate = currentHealth / maxHealth;
            transform.localScale = new Vector3(originalScale.x * rate, originalScale.y);
        }
    }
}
