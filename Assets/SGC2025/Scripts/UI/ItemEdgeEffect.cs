using System;
using UnityEngine;
using UnityEngine.UI;
using SGC2025.Core;
using SGC2025.Item;

namespace SGC2025.UI
{
    /// <summary>
    /// アイテム取得時に画面の縁を走らせる演出の「駆動部」。
    /// ItemCollectedEvent を購読し、アイテム種類ごとの色（_Color）と
    /// 上端中央→下端への進行度（_Progress 0→1）をマテリアルへ流し込む。
    /// 実際の縁の描画はアサインしたマテリアル（シェーダ）が担当する。
    /// </summary>
    public class ItemEdgeEffect : MonoBehaviour
    {
        [Serializable]
        private struct ItemColorEntry
        {
            public ItemType itemType;
            public Color color;
        }

        [Header("演出設定")]
        [SerializeField, Tooltip("上端中央→下端まで走りきる時間（秒）")]
        private float duration = 0.4f;

        [SerializeField, Tooltip("アイテム種類ごとの色（例: SpeedBoost=青, AreaGreenify=緑, PlayerClone=黄）")]
        private ItemColorEntry[] itemColors;

        [SerializeField, Tooltip("マップに無い種類のときの既定色")]
        private Color defaultColor = Color.white;

        [Header("シェーダプロパティ名")]
        [SerializeField] private string progressProperty = "_Progress";
        [SerializeField] private string colorProperty = "_Color";

        private Graphic graphic;
        private Material runtimeMaterial;
        private float timer;
        private bool playing;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();

            // 共有マテリアル（アセット）を書き換えないよう、ランタイム用に複製して使う
            if (graphic != null && graphic.material != null)
            {
                runtimeMaterial = Instantiate(graphic.material);
                graphic.material = runtimeMaterial;
            }

            SetIdle();
            EventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
        }

        private void OnItemCollected(ItemCollectedEvent e)
        {
            ApplyColor(ResolveColor(e.ItemType));
            timer = 0f;
            playing = true;
            if (graphic != null)
                graphic.enabled = true;
        }

        private void Update()
        {
            if (!playing) return;

            timer += Time.deltaTime;
            float progress = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
            SetProgress(progress);

            if (timer >= duration)
            {
                playing = false;
                SetIdle();
            }
        }

        private void SetIdle()
        {
            SetProgress(0f);
            if (graphic != null)
                graphic.enabled = false; // 待機中は描画オフ（演出中のみ表示）
        }

        private Color ResolveColor(ItemType type)
        {
            if (itemColors != null)
            {
                foreach (var entry in itemColors)
                {
                    if (entry.itemType == type)
                        return entry.color;
                }
            }
            return defaultColor;
        }

        private void ApplyColor(Color color)
        {
            if (runtimeMaterial != null && runtimeMaterial.HasProperty(colorProperty))
                runtimeMaterial.SetColor(colorProperty, color);
        }

        private void SetProgress(float value)
        {
            if (runtimeMaterial != null && runtimeMaterial.HasProperty(progressProperty))
                runtimeMaterial.SetFloat(progressProperty, value);
        }
    }
}
