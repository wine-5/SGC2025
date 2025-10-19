using UnityEngine;

namespace SGC2025
{
    public class HPBarController : MonoBehaviour
    {
        [SerializeField] GameObject entity;
        [Range(0f, 1f)]
        [SerializeField] private float rate;
        private float maxHealth;
        private float currentHealth;
        private Vector3 originalScale;

        [SerializeField] private bool isPlayer;

        void Start()
        {
            originalScale = transform.localScale;

            if (isPlayer)
            {
                maxHealth = entity.GetComponent<Player>().GetPlayerMaxHealth();
            }
        }

        void Update()
        {
            if (isPlayer)
            {
                 currentHealth = entity.GetComponent<Player>().GetPlayerCurrentHalth();
            }


            rate = currentHealth / maxHealth;
            transform.localScale = new Vector3(originalScale.x * rate, originalScale.y);
        }
    }
}
