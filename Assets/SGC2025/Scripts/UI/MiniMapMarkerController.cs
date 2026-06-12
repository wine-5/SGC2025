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
        private const float PLAYER_MARKER_LERP_SPEED = 5f;

        [SerializeField] private MiniMapTextureRenderer textureRenderer;
        [SerializeField] private RectTransform playerMarkerRect;
        [SerializeField] private Sprite playerMarkerSprite;
        [SerializeField] private RectTransform bossMarkerContainer;
        [SerializeField] private Sprite bossMarkerSprite;

        [SerializeField] private RectTransform expandRect;
        private readonly List<(EnemyController enemy, RectTransform marker)> bossMarkers = new();
        private Color bossMarkerColor = new(1f, 0.2f, 0.2f);

        private void Start()
        {
            textureRenderer?.Initialize();

            if (playerMarkerRect != null)
            {
                Image playerImage = playerMarkerRect.GetComponent<Image>();
                if (playerImage == null)
                    playerImage = playerMarkerRect.gameObject.AddComponent<Image>();

                if (playerImage != null && playerMarkerSprite != null)
                    playerImage.sprite = playerMarkerSprite;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<MiniMapExpandStartedEvent>(OnMiniMapExpandStarted);
            EventBus.Subscribe<MiniMapExpandCanceledEvent>(OnMiniMapExpandCanceled);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GroundGreenifiedEvent>(OnGroundGreenified);
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Unsubscribe<MiniMapExpandStartedEvent>(OnMiniMapExpandStarted);
            EventBus.Unsubscribe<MiniMapExpandCanceledEvent>(OnMiniMapExpandCanceled);
        }

        private void Update()
        {
            UpdatePlayerMarker();
            UpdateBossMarkers();
        }

        private void OnGroundGreenified(GroundGreenifiedEvent e)
        {
            textureRenderer?.UpdateGreenifiedCell(e.Position);
        }

        private void OnMiniMapExpandStarted(MiniMapExpandStartedEvent e)
        {
            expandRect.gameObject.SetActive(true);
        }

        private void OnMiniMapExpandCanceled(MiniMapExpandCanceledEvent e)
        {
            expandRect.gameObject.SetActive(false);
        }

        private void UpdatePlayerMarker()
        {
            if (playerMarkerRect == null || PlayerDataProvider.I?.IsPlayerRegistered != true)
                return;

            Vector3 playerWorldPos = PlayerDataProvider.I.PlayerTransform.position;
            Vector2 miniMapPos = WorldToMiniMapPos(playerWorldPos);

            playerMarkerRect.anchoredPosition = Vector2.Lerp(
                playerMarkerRect.anchoredPosition,
                miniMapPos,
                Time.deltaTime * PLAYER_MARKER_LERP_SPEED
            );

            playerMarkerRect.rotation = PlayerDataProvider.I.PlayerTransform.rotation;
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

            Image markerImage = markerGO.AddComponent<Image>();
            markerImage.sprite = bossMarkerSprite;
            markerImage.color = bossMarkerColor;

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

            RectTransform targetRect = expandRect.gameObject.activeSelf ? expandRect : playerMarkerRect.parent as RectTransform;
            if (targetRect == null) return Vector2.zero;

            Vector2 rect = targetRect.sizeDelta;
            return new Vector2(
                (nx - 0.5f) * rect.x,
                (ny - 0.5f) * rect.y
            );
        }
    }
}
