using UnityEngine;
using TechC;
using System.Collections.Generic;
using SGC2025.Manager;

namespace SGC2025.Effect
{
    /// <summary>
    /// エフェクト生成を一元管理するFactory
    /// </summary>
    public class EffectFactory : Singleton<EffectFactory>
    {
        [Header("エフェクトデータ")]
        [SerializeField] private EffectDataSO effectDataSO;
        
        [Header("プール設定")]
        [SerializeField] private ObjectPool objectPool;
        
        private Dictionary<EffectType, EffectData> effectDataDictionary;
        
        protected override bool UseDontDestroyOnLoad => false;
        
        protected override void Init()
        {
            base.Init();
            
            // ObjectPoolが未設定の場合、自動検索
            if (objectPool == null)
            {
                objectPool = FindAnyObjectByType<ObjectPool>();
                
                if (objectPool == null)
                {
                    Debug.LogError("[EffectFactory] ObjectPool が見つかりません！");
                }
            }
            
            InitializeEffectDataDictionary();
        }
        
        private void InitializeEffectDataDictionary()
        {
            effectDataDictionary = new Dictionary<EffectType, EffectData>();
            
            if (effectDataSO == null)
            {
                Debug.LogError("[EffectFactory] EffectDataSOが設定されていません!");
                return;
            }
            
            if (effectDataSO.EffectDataList == null)
            {
                Debug.LogError("[EffectFactory] EffectDataListがnullです!");
                return;
            }
            
            foreach (var effectData in effectDataSO.EffectDataList)
            {
                if (effectData != null && effectData.EffectPrefab != null)
                {
                    effectDataDictionary[effectData.EffectType] = effectData;
                }
                else if (effectData != null)
                {
                    Debug.LogWarning($"[EffectFactory] {effectData.EffectType} のプレハブが設定されていません");
                }
            }
        }
        
        /// <summary>
        /// エフェクトを生成
        /// </summary>
        /// <param name="effectType">生成するエフェクトの種類</param>
        /// <param name="position">生成位置</param>
        /// <param name="followTarget">追従対象（nullの場合は追従しない）</param>
        /// <returns>生成されたエフェクトオブジェクト</returns>
        public GameObject CreateEffect(EffectType effectType, Vector3 position, Transform followTarget = null)
        {
            if (effectDataDictionary == null || effectDataDictionary.Count == 0)
            {
                Debug.LogError("[EffectFactory] エフェクトデータ辞書が初期化されていません");
                InitializeEffectDataDictionary();
                if (effectDataDictionary == null || effectDataDictionary.Count == 0)
                {
                    return null;
                }
            }
            
            if (!effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                Debug.LogError($"[EffectFactory] EffectType '{effectType}' のデータが見つかりません");
                return null;
            }
            
            if (objectPool == null)
            {
                Debug.LogError("[EffectFactory] ObjectPool が利用できません");
                return null;
            }
            
            if (data.EffectPrefab == null)
            {
                Debug.LogError($"[EffectFactory] EffectType '{effectType}' のプレハブがnullです");
                return null;
            }
            
            // プールからエフェクトオブジェクトを取得
            var result = objectPool.GetObject(data.EffectPrefab, position, Quaternion.identity);
            
            if (result != null)
            {
                // スケールをプレハブの元の値にリセット
                result.transform.localScale = data.EffectPrefab.transform.localScale;
                
                // 追従設定
                var controller = result.GetComponent<EffectController>();
                if (controller != null)
                {
                    controller.Initialize(followTarget, data.Duration);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// エフェクトをプールに返却
        /// </summary>
        /// <param name="effectObject">返却するエフェクトオブジェクト</param>
        public void ReturnEffect(GameObject effectObject)
        {
            if (effectObject == null) return;
            
            if (objectPool == null)
            {
                Debug.LogError("[EffectFactory] ObjectPool が利用できません！エフェクトを直接破棄します");
                Destroy(effectObject);
                return;
            }
            
            objectPool.ReturnObject(effectObject);
        }
        
        /// <summary>
        /// 一定時間後にエフェクトをプールに返却
        /// </summary>
        /// <param name="effectObject">返却するエフェクトオブジェクト</param>
        /// <param name="delay">返却までの時間</param>
        public void ReturnEffectDelayed(GameObject effectObject, float delay)
        {
            if (effectObject != null)
                StartCoroutine(ReturnEffectAfterDelay(effectObject, delay));
        }
        
        private System.Collections.IEnumerator ReturnEffectAfterDelay(GameObject effectObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effectObject != null)
                ReturnEffect(effectObject);
        }

        /// <summary>
        /// 利用可能なエフェクトタイプの一覧を取得
        /// </summary>
        /// <returns>利用可能なエフェクトタイプの配列</returns>
        public EffectType[] GetAvailableEffectTypes()
        {
            if (effectDataDictionary == null) return new EffectType[0];
            
            EffectType[] types = new EffectType[effectDataDictionary.Count];
            effectDataDictionary.Keys.CopyTo(types, 0);
            return types;
        }
        
        /// <summary>
        /// 指定したエフェクトタイプが利用可能かチェック
        /// </summary>
        /// <param name="effectType">チェックするエフェクトタイプ</param>
        /// <returns>利用可能な場合はtrue</returns>
        public bool IsEffectAvailable(EffectType effectType)
        {
            return effectDataDictionary != null && effectDataDictionary.ContainsKey(effectType);
        }
        
        /// <summary>
        /// 指定したエフェクトタイプの継続時間を取得
        /// </summary>
        /// <param name="effectType">取得するエフェクトタイプ</param>
        /// <returns>継続時間、エフェクトが見つからない場合は0f</returns>
        public float GetEffectDuration(EffectType effectType)
        {
            if (effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                return data.Duration;
            }
            
            Debug.LogWarning($"[EffectFactory] EffectType '{effectType}' のデータが見つかりません");
            return 0f;
        }
    }
}
