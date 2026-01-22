using UnityEngine;

namespace SGC2025
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "SGC2025/Map Settings", order = 0)]
    public class MapSettings : ScriptableObject
    {
        [Header("Scene Info")]
        public string sceneName;
    
        [Header("Grid Size")]
        public int columns = 10;
        public int rows = 8;
    
        [Header("Tile Aspect (W:H)")]
        public float aspectW = 1f;
        public float aspectH = 1f;
    
        [Header("Tile Size")]
        public float cellWidth = 1f;
        public float cellHeight = 1f;
    
        [Header("Spacing Between Tiles")]
        public Vector2 spacing = Vector2.zero;
    }
    
}
