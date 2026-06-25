using UnityEngine;
using System.Collections.Generic;
using Tyotyo.Systems;
using Tyotyo.Core;
using Tyotyo.Core.Log;

namespace Tyotyo.Effect
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

            if (objectPool == null)
            {
                CusLog.Error("EffectFactory", "ObjectPool が見つかりません！");
                return;
            }

            InitializeEffectDataDictionary();
        }

        private void InitializeEffectDataDictionary()
        {
            effectDataDictionary = new Dictionary<EffectType, EffectData>();

            if (effectDataSO == null)
            {
                CusLog.Error("EffectFactory", "EffectDataSOが設定されていません!");
                return;
            }

            if (effectDataSO.EffectDataList == null)
            {
                CusLog.Error("EffectFactory", "EffectDataListがnullです!");
                return;
            }

            foreach (var effectData in effectDataSO.EffectDataList)
            {
                if (effectData != null && effectData.EffectPrefab != null)
                    effectDataDictionary[effectData.EffectType] = effectData;
            }
        }

        /// <summary>
        /// エフェクトを生成
        /// </summary>
        /// <param name="effectType">生成するエフェクトの種類</param>
        /// <param name="position">生成位置</param>
        /// <param name="duration">エフェクトの持続時間</param>
        /// <param name="followTarget">追従対象（nullの場合は追従しない）</param>
        /// <returns>生成されたエフェクトオブジェクト</returns>
        public GameObject CreateEffect(EffectType effectType, Vector3 position, float duration, Transform followTarget = null)
        {
            if (effectDataDictionary == null || effectDataDictionary.Count == 0)
            {
                CusLog.Error("EffectFactory", "エフェクトデータ辞書が初期化されていません");
                InitializeEffectDataDictionary();
                if (effectDataDictionary == null || effectDataDictionary.Count == 0) return null;
            }

            if (!effectDataDictionary.TryGetValue(effectType, out EffectData data))
            {
                CusLog.Error("EffectFactory", $"EffectType '{effectType}' のデータが見つかりません");
                return null;
            }

            if (objectPool == null)
            {
                CusLog.Error("EffectFactory", "ObjectPool が利用できません");
                return null;
            }

            if (data.EffectPrefab == null)
            {
                CusLog.Error("EffectFactory", $"EffectType '{effectType}' のプレハブがnullです");
                return null;
            }

            var result = objectPool.GetObject(data.EffectPrefab, position, data.EffectPrefab.transform.rotation);

            if (result != null)
            {
                result.transform.localScale = data.EffectPrefab.transform.localScale;

                var controller = result.GetComponent<EffectController>();
                if (controller != null)
                    controller.Initialize(followTarget, duration);
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
                CusLog.Error("EffectFactory", "ObjectPool is not available! Cannot return effect to pool.");
                effectObject.SetActive(false);
                return;
            }

            objectPool.ReturnObject(effectObject);
        }
    }
}
