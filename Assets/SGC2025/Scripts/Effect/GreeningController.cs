using UnityEngine;
using UnityEngine.VFX;

namespace Tyotyo.Effect
{
    public class GreeningController : MonoBehaviour
    {
        [SerializeField] private VisualEffect greeningEffect;
        [SerializeField] private Vector2 gaugeViewportPoint = new Vector2(0.08f, 0.88f);

        void Update()
        {
            greeningEffect.SetVector3("UIPos",ResolveWorldPosition());
        }

        private Vector3 ResolveWorldPosition()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;

            if (cam != null)
            {
                Vector3 viewport = new Vector3(gaugeViewportPoint.x, gaugeViewportPoint.y, Mathf.Abs(cam.transform.position.z));
                Vector3 world = cam.ViewportToWorldPoint(viewport);
                world.z = 0f;
                return world;
            }

            return new (0,0,0);
        }
    }
}
