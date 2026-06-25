using UnityEngine;
using UnityEngine.VFX;
using Tyotyo.Core;

namespace Tyotyo.Effect
{
    public class GreeningController : MonoBehaviour
    {
        private const float DEFAULT_GAUGE_VIEWPORT_X = 0.08f;
        private const float DEFAULT_GAUGE_VIEWPORT_Y = 0.88f;

        [SerializeField] private VisualEffect greeningEffect;
        [SerializeField] private Vector2 gaugeViewportPoint = new Vector2(DEFAULT_GAUGE_VIEWPORT_X, DEFAULT_GAUGE_VIEWPORT_Y);

        void Update()
        {
            greeningEffect.SetVector3("UIPos", CameraUtil.ViewportToWorld(gaugeViewportPoint));
        }
    }
}
