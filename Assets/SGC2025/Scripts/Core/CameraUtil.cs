using UnityEngine;

namespace Tyotyo.Core
{
    /// <summary>
    /// カメラ関連の共通ユーティリティ
    /// </summary>
    public static class CameraUtil
    {
        /// <summary>
        /// ビューポート座標をワールド座標に変換する
        /// </summary>
        /// <param name="viewportPoint">ビューポート座標（0〜1の範囲）</param>
        /// <returns>変換されたワールド座標（Z=0）</returns>
        public static Vector3 ViewportToWorld(Vector2 viewportPoint)
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector3.zero;

            Vector3 viewport = new Vector3(viewportPoint.x, viewportPoint.y, Mathf.Abs(cam.transform.position.z));
            Vector3 world = cam.ViewportToWorldPoint(viewport);
            world.z = 0f;
            return world;
        }
    }
}
