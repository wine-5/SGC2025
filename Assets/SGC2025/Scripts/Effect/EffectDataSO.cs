using UnityEngine;

namespace SGC2025.Effect
{
    /// <summary>
    /// エフェクトデータ
    /// </summary>
    [System.Serializable]
    public class EffectData
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private GameObject effectPrefab;
        
        public EffectType EffectType => effectType;
        public GameObject EffectPrefab => effectPrefab;
    }

    /// <summary>
    /// エフェクトデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "EffectDataSO", menuName = "SGC2025/Effect/EffectData")]
    public class EffectDataSO : ScriptableObject
    {
        [Header("エフェクトリスト")]
        [SerializeField] private EffectData[] effectDataList;
        
        public EffectData[] EffectDataList => effectDataList;
    }
}
