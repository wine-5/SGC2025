using UnityEngine;

namespace SGC2025
{
    public class HPBarController : MonoBehaviour
    {
        [Range(0,1f)]
        [SerializeField] private float rate;
        private Vector3 originalScale;

        void Start()
        {
            originalScale = transform.localScale;
        }

        void Update()
        {
            transform.localScale = new Vector3(originalScale.x * rate, originalScale.y);
        }
    }
}
