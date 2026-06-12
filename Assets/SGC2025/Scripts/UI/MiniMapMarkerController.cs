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
        [SerializeField, Tooltip("Shift押下中に表示する拡大マップのオブジェクト")]
        private GameObject expandMapObject;
        [SerializeField] private RectTransform expandPlayerMarkerRect;
        private readonly List<(EnemyController enemy, RectTransform marker)> bossMarkers = new();

        private void Start()
        {
            textureRenderer?.Initialize();
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
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<MiniMapExpandStartedEvent>(OnMiniMapExpandStarted);
            EventBus.Subscribe<MiniMapExpandCanceledEvent>(OnMiniMapExpandCanceled);

            InitializePlayerMarker();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);
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
            if (bossMarkerContainer == null) return;

            GameObject markerGO = new GameObject("BossMarker");
            markerGO.transform.SetParent(bossMarkerContainer);

            RectTransform markerRect = markerGO.AddComponent<RectTransform>();
            markerRect.sizeDelta = new Vector2(BOSS_MARKER_SIZE, BOSS_MARKER_SIZE);

            markerGO.AddComponent<Image>();

            bossMarkers.Add((enemy, markerRect));
            enemy.OnDeath += () => RemoveBossMarker(enemy);
        }

        private void UpdateBossMarkers()
        {
            for (int i = bossMarkers.Count - 1; i >= 0; i--)
            {
                var (enemy, marker) = bossMarkers[i];

                if (enemy == null || !enemy.IsAlive)
                {
                    if (marker != null)
                        Destroy(marker.gameObject);
                    bossMarkers.RemoveAt(i);
                    continue;
                }

                if (marker != null)
                {
                    Vector2 miniMapPos = WorldToMiniMapPos(enemy.transform.position);
                    marker.anchoredPosition = miniMapPos;
                }
            }
        }

        private void RemoveBossMarker(EnemyController enemy)
        {
            for (int i = 0; i < bossMarkers.Count; i++)
            {
                if (bossMarkers[i].enemy == enemy)
                {
                    if (bossMarkers[i].marker != null)
                        Destroy(bossMarkers[i].marker.gameObject);
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
