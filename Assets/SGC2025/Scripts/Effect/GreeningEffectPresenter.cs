using UnityEngine;
using Tyotyo.Core;
using Tyotyo.Audio;

namespace Tyotyo.Effect
{
    /// <summary>
    /// 範囲緑化（<see cref="GroundAreaGreenifiedEvent"/>）を購読し、緑化エフェクトの生成とSE再生を担当するPresenter。
    /// GroundManagerから演出責務を切り離し、地面の状態管理を単一責務に保つために存在する。
    /// </summary>
    public class GreeningEffectPresenter : MonoBehaviour
    {
        private const float GRASS_EFFECT_DURATION = 2f;
        private const float GRASS_EFFECT_Y_OFFSET = 0.1f;

        [SerializeField, Tooltip("緑化範囲(size)に応じたエフェクト拡大の強さ。size=1は等倍、size>1は (1 + (size-1)×この値) 倍")]
        private float areaEffectScaleFactor = 0.5f;

        private void OnEnable()
        {
            EventBus.Subscribe<GroundAreaGreenifiedEvent>(OnGroundAreaGreenified);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundAreaGreenifiedEvent>(OnGroundAreaGreenified);
        }

        private void OnGroundAreaGreenified(GroundAreaGreenifiedEvent e)
        {
            // 中心位置にエフェクトと音を1回だけ生成（エフェクトはsizeに比例して拡大）
            if (EffectFactory.I != null)
            {
                Vector3 effectPos = e.CenterPosition + Vector3.up * GRASS_EFFECT_Y_OFFSET;
                GameObject effect = EffectFactory.I.CreateEffect(EffectType.GrassRestorationEffect, effectPos, GRASS_EFFECT_DURATION);

                // 全軸を一律に拡大（エフェクトが回転していても見た目が崩れない）
                // size に等倍だと大きすぎるため、係数で緩やかに拡大する
                if (effect != null && e.Size > 1)
                    effect.transform.localScale *= 1f + (e.Size - 1) * areaEffectScaleFactor;
            }

            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.Grass);
        }
    }
}
