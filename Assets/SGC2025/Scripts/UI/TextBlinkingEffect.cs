using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Tyotyo.UI
{
    /// <summary>
    /// TextMeshProのテキストをフェードイン・アウトさせる演出
    /// </summary>
    public class TextBlinkingEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private float stayDuration = 1.0f;
        [SerializeField] private float minAlpha = 0.3f;

        private bool _isActive;

        private void OnEnable()
        {
            _isActive = true;
            StartBlinkingAsync().Forget();
        }

        private void OnDisable()
        {
            _isActive = false;
        }

        private async UniTask StartBlinkingAsync()
        {
            if (targetText == null)
                return;

            while (_isActive)
            {
                await FadeOutAsync();
                await UniTask.Delay((int)(stayDuration * 1000));
                await FadeInAsync();
                await UniTask.Delay((int)(stayDuration * 1000));
            }
        }

        private async UniTask FadeOutAsync()
        {
            float elapsedTime = 0f;
            Color originalColor = targetText.color;

            while (elapsedTime < fadeDuration && _isActive)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, minAlpha, elapsedTime / fadeDuration);
                Color newColor = originalColor;
                newColor.a = alpha;
                targetText.color = newColor;

                await UniTask.Yield();
            }

            Color finalColor = originalColor;
            finalColor.a = minAlpha;
            targetText.color = finalColor;
        }

        private async UniTask FadeInAsync()
        {
            float elapsedTime = 0f;
            Color originalColor = targetText.color;

            while (elapsedTime < fadeDuration && _isActive)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(minAlpha, 1f, elapsedTime / fadeDuration);
                Color newColor = originalColor;
                newColor.a = alpha;
                targetText.color = newColor;

                await UniTask.Yield();
            }

            Color finalColor = originalColor;
            finalColor.a = 1f;
            targetText.color = finalColor;
        }
    }
}
