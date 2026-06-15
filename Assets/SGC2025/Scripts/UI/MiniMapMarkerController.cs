using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SGC2025.Core;
using SGC2025.Enemy;
using SGC2025.Manager;
using SGC2025.Player;

namespace SGC2025.UI
{
    public class MiniMapMarkerController : MonoBehaviour
    {
        private const float BOSS_MARKER_SIZE = 20f;

        [SerializeField] private MiniMapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform playerMarkerRect;
        [SerializeField] private RectTransform bossMarkerContainer;
        [SerializeField, Tooltip("ボスマーカーの色")] private Color bossMarkerColor = Color.red;
        [SerializeField, Tooltip("ボスマーカーのスプライト（未指定なら塗りつぶしの四角）")] private Sprite bossMarkerSprite;
        [SerializeField, Tooltip("拡大マップ上のボスマーカーのサイズ")] private float expandBossMarkerSize = 40f;
        [SerializeField, Tooltip("Shift押下中に表示する拡大マップのオブジェクト")]
        private GameObject expandMapObject;
        [SerializeField] private RectTransform expandPlayerMarkerRect;
        private readonly List<(EnemyController enemy, RectTransform miniMarker, RectTransform expandMarker)> bossMarkers = new();

        private void Start()
        {
            textureRenderer?.Initialize();

            // bossMarkerContainer自身に付いている見た目（テンプレの赤マーカー）は隠す。
            // 入れ物はアクティブのまま残し、実行時に生成する子マーカーのみ表示する。
            if (bossMarkerContainer != null && bossMarkerContainer.TryGetComponent<Graphic>(out var containerGraphic))
                containerGraphic.enabled = false;
        }

        private void InitializePlayerMarker()
        {
            if (playerMarkerRect == null || PlayerDataProvider.I == null || !PlayerDataProvider.I.IsPlayerRegistered)
                return;

            Vector3 playerWorldPos = PlayerDataProvider.I.PlayerTransform.position;
            Vector2 miniMapPos = WorldToMiniMapPos(playerWorldPos);
            playerMarkerRect.anchoredPosition = miniMapPos;

            if (expandPlayerMarkerRect != null)
            {
                Vector2 expandMapPos = WorldToExpandMapPos(playerWorldPos);
                expandPlayerMarkerRect.anchoredPosition = expandMapPos;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Subscribe<GroundUngreenifiedEvent>(OnGroundUngreenified);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<MiniMapExpandStartedEvent>(OnMiniMapExpandStarted);
            EventBus.Subscribe<MiniMapExpandCanceledEvent>(OnMiniMapExpandCanceled);

            InitializePlayerMarker();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Unsubscribe<GroundUngreenifiedEvent>(OnGroundUngreenified);
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Unsubscribe<MiniMapExpandStartedEvent>(OnMiniMapExpandStarted);
            EventBus.Unsubscribe<MiniMapExpandCanceledEvent>(OnMiniMapExpandCanceled);
        }

        private bool playerInitialized = false;

        private void Update()
        {
            bool hasMarker = playerMarkerRect != null;
            bool hasProvider = PlayerDataProvider.I != null;
            bool isRegistered = hasProvider && PlayerDataProvider.I.IsPlayerRegistered;

            if (hasMarker && hasProvider && isRegistered)
            {
                if (!playerInitialized)
                {
                    InitializePlayerMarker();
                    playerInitialized = true;
                }

                UpdatePlayerMarker();
            }

            UpdateBossMarkers();
        }

        private void OnGroundGreenified(GroundGreenifiedEvent e)
        {
            textureRenderer?.UpdateGreenifiedCell(e.Position);
        }

        private void OnGroundUngreenified(GroundUngreenifiedEvent e)
        {
            textureRenderer?.UpdateUngreenifiedCell(e.Position);
        }

        private void OnMiniMapExpandStarted(MiniMapExpandStartedEvent e)
        {
            expandMapObject.SetActive(true);
        }

        private void OnMiniMapExpandCanceled(MiniMapExpandCanceledEvent e)
        {
            expandMapObject.SetActive(false);
        }

        private void UpdatePlayerMarker()
        {
            if (playerMarkerRect == null || PlayerDataProvider.I?.IsPlayerRegistered != true)
                return;

            Vector3 playerWorldPos = PlayerDataProvider.I.PlayerTransform.position;
            Vector2 miniMapPos = WorldToMiniMapPos(playerWorldPos);
            playerMarkerRect.anchoredPosition = miniMapPos;
            playerMarkerRect.rotation = PlayerDataProvider.I.PlayerTransform.rotation;

            if (expandPlayerMarkerRect != null)
            {
                Vector2 expandMapPos = WorldToExpandMapPos(playerWorldPos);
                expandPlayerMarkerRect.anchoredPosition = expandMapPos;
                expandPlayerMarkerRect.rotation = PlayerDataProvider.I.PlayerTransform.rotation;
            }
        }

        private void OnEnemySpawned(EnemySpawnedEvent e)
        {
            if (e.Enemy?.EnemyData?.IsBoss != true) return;

            CreateBossMarker(e.Enemy);
        }

        private void CreateBossMarker(EnemyController enemy)
        {
            // 小マップ用と拡大マップ用、両方のマーカーを生成する
            RectTransform miniMarker = CreateMarkerImage(bossMarkerContainer, BOSS_MARKER_SIZE);

            // 拡大マップ側は、拡大マップのプレイヤーマーカーと同じ親へ配置する（サイズは別指定で大きめ）
            RectTransform expandParent = expandPlayerMarkerRect != null ? expandPlayerMarkerRect.parent as RectTransform : null;
            RectTransform expandMarker = CreateMarkerImage(expandParent, expandBossMarkerSize);

            if (miniMarker == null && expandMarker == null) return;

            bossMarkers.Add((enemy, miniMarker, expandMarker));
            enemy.OnDeath += () => RemoveBossMarker(enemy);
        }

        /// <summary>指定の親の下にボスマーでｋカー（Image）を1つ生成する</summary>
        private RectTransform CreateMarkerImage(RectTransform container, float size)
        {
            if (container == null) return null;

            GameObject markerGO = new GameObject("BossMarker", typeof(RectTransform), typeof(Image));

            RectTransform markerRect = markerGO.GetComponent<RectTransform>();
            // worldPositionStays=false: Canvasスケールの影響を受けずローカル基準で配置する
            markerRect.SetParent(container, false);
            markerRect.anchorMin = markerRect.anchorMax = markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.localScale = Vector3.one;
            markerRect.localPosition = Vector3.zero;
            markerRect.sizeDelta = new Vector2(size, size);

            var image = markerGO.GetComponent<Image>();
            image.color = bossMarkerColor;
            image.raycastTarget = false;
            if (bossMarkerSprite != null)
                image.sprite = bossMarkerSprite;

            return markerRect;
        }

        private void UpdateBossMarkers()
        {
            for (int i = bossMarkers.Count - 1; i >= 0; i--)
            {
                var (enemy, miniMarker, expandMarker) = bossMarkers[i];

                if (enemy == null || !enemy.IsAlive)
                {
                    if (miniMarker != null) Destroy(miniMarker.gameObject);
                    if (expandMarker != null) Destroy(expandMarker.gameObject);
                    bossMarkers.RemoveAt(i);
                    continue;
                }

                Vector3 enemyPos = enemy.transform.position;
                if (miniMarker != null)
                    miniMarker.anchoredPosition = WorldToMiniMapPos(enemyPos);
                if (expandMarker != null)
                    expandMarker.anchoredPosition = WorldToExpandMapPos(enemyPos);
            }
        }

        private void RemoveBossMarker(EnemyController enemy)
        {
            for (int i = 0; i < bossMarkers.Count; i++)
            {
                if (bossMarkers[i].enemy == enemy)
                {
                    if (bossMarkers[i].miniMarker != null) Destroy(bossMarkers[i].miniMarker.gameObject);
                    if (bossMarkers[i].expandMarker != null) Destroy(bossMarkers[i].expandMarker.gameObject);
                    bossMarkers.RemoveAt(i);
                    return;
                }
            }
        }

        private Vector2 WorldToMiniMapPos(Vector3 worldPos)
        {
            if (GroundManager.I?.MapData == null) return Vector2.zero;

            var mapData = GroundManager.I.MapData;
            Vector2 mapMaxPos = mapData.MapMaxWorldPosition;

            float nx = Mathf.Clamp01(worldPos.x / mapMaxPos.x);
            float ny = Mathf.Clamp01(worldPos.y / mapMaxPos.y);

            RectTransform parentRect = playerMarkerRect.parent as RectTransform;
            if (parentRect == null) return Vector2.zero;

            Vector2 rect = parentRect.sizeDelta;
            return new Vector2(
                (nx - 0.5f) * rect.x,
                (ny - 0.5f) * rect.y
            );
        }

        private Vector2 WorldToExpandMapPos(Vector3 worldPos)
        {
            if (GroundManager.I?.MapData == null || expandPlayerMarkerRect == null) return Vector2.zero;

            var mapData = GroundManager.I.MapData;
            Vector2 mapMaxPos = mapData.MapMaxWorldPosition;

            float nx = Mathf.Clamp01(worldPos.x / mapMaxPos.x);
            float ny = Mathf.Clamp01(worldPos.y / mapMaxPos.y);

            RectTransform parentRect = expandPlayerMarkerRect.parent as RectTransform;
            if (parentRect == null) return Vector2.zero;

            Vector2 rect = parentRect.sizeDelta;
            return new Vector2(
                (nx - 0.5f) * rect.x,
                (ny - 0.5f) * rect.y
            );
        }
    }
}
