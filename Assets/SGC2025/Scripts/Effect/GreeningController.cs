using UnityEngine;
using UnityEngine.VFX;
using Tyotyo.Core;

namespace Tyotyo.Effect
{
    public class GreeningController : MonoBehaviour
    {
        [SerializeField] private VisualEffect greeningEffect;
        [SerializeField] private Vector2 gaugeViewportPoint = new Vector2(0.08f, 0.88f);

        void Update()
        {
            greeningEffect.SetVector3("UIPos", CameraUtil.ViewportToWorld(gaugeViewportPoint));
        }
    }
}
