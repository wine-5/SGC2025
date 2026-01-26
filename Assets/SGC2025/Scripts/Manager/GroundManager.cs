using UnityEngine;
using SGC2025.Events;
using SGC2025.Audio;
using SGC2025.Effect;
using System.Collections;

namespace SGC2025.Manager
{
    /// <summary>
    /// 地面システムの管理クラス
    /// マップ生成、タイルの緑化処理
    /// </summary>
    public class GroundManager : Singleton<GroundManager>
    {
        private const float TILE_Z_POSITION = 0f;
        private const float GRASS_EFFECT_DURATION = 2f;
        private const float GRASS_EFFECT_Y_OFFSET = 0.1f;
        
        [Header("地面データ設定")]
        [SerializeField]
        [Tooltip("使用するGroundDataSO（マップ設定の単一の真実の源泉）")]
        private GroundDataSO groundData;

        private struct GroundData
        {
            public Vector2Int gridPos;
            public Vector2 worldPos;
            public bool isDrawn;
            public int point;
            public Renderer renderer;
        }

        private GroundData[,] currentGroundArray;
        private Vector3 currentOriginPosition;
        private GameObject[,] tileObjects;
        
        public GroundDataSO MapData => groundData;
        public int MapColumns => groundData?.columns ?? 0;
        public int MapRows => groundData?.rows ?? 0;
        public Vector2Int MapMaxIndex => new Vector2Int(MapColumns - 1, MapRows - 1);
        
        /// <summary>Playerのスポーン位置を取得（マップの中心）</summary>
        public Vector3 GetPlayerSpawnPosition()
        {
            if (groundData == null) return Vector3.zero;
            return groundData.MapCenterPosition;
        }

        public void Start()
        {
            if (groundData == null)
            {
                Debug.LogError("[GroundManager] GroundDataSO is not assigned!");
                return;
            }
            
            SetStageObject();
            InitHighObject();
            EnemyEvents.OnEnemyDestroyedAtPosition += OnEnemyDestroyed;
        }
        
        protected override void OnDestroy()
        {
            EnemyEvents.OnEnemyDestroyedAtPosition -= OnEnemyDestroyed;
            base.OnDestroy();
        }
        
        private void OnEnemyDestroyed(Vector3 enemyPosition) => DrawGround(enemyPosition);

        /// <summary>指定位置の地面を緑化</summary>
        public bool DrawGround(Vector3 enemyPosition)
        {
            if (currentGroundArray == null) return false;
            
            Vector2Int cellPosition = SearchCellIndex(enemyPosition);
            
            if (cellPosition.x < 0 || cellPosition.x >= groundData.columns ||
                cellPosition.y < 0 || cellPosition.y >= groundData.rows) return false;
            
            if (currentGroundArray[cellPosition.x, cellPosition.y].isDrawn) return false;
            
            if (groundData.grassTilePrefab == null) return false;
            
            if (tileObjects != null && tileObjects[cellPosition.x, cellPosition.y] != null)
                Destroy(tileObjects[cellPosition.x, cellPosition.y]);
            
            Vector3 pos = currentGroundArray[cellPosition.x, cellPosition.y].worldPos;
            GameObject grassTile = Instantiate(groundData.grassTilePrefab, pos, Quaternion.identity, transform);
            grassTile.name = $"GrassTile_{cellPosition.x}_{cellPosition.y}";
            
            AdjustTileScale(grassTile, groundData.ActualCellWidth, groundData.ActualCellHeight);
            
            if (tileObjects != null)
                tileObjects[cellPosition.x, cellPosition.y] = grassTile;
            
            Renderer newRenderer = grassTile.GetComponent<Renderer>();
            currentGroundArray[cellPosition.x, cellPosition.y].renderer = newRenderer;
            currentGroundArray[cellPosition.x, cellPosition.y].isDrawn = true;

            int points = currentGroundArray[cellPosition.x, cellPosition.y].point;
            GroundEvents.TriggerGroundGreenified(pos, points);
            
            // 緑化エフェクトを生成（地面タイルの中心、少し上に表示）
            if (EffectFactory.I != null)
            {
                Vector3 effectPos = pos + Vector3.up * GRASS_EFFECT_Y_OFFSET; // Y軸を少し上に調整
                GameObject grassEffect = EffectFactory.I.CreateEffect(EffectType.GrassRestorationEffect, effectPos, GRASS_EFFECT_DURATION);
                
                // エフェクト持続時間後にPool返却処理
                if (grassEffect != null)
                    StartCoroutine(ReturnEffectToPoolAfterDelay(grassEffect, GRASS_EFFECT_DURATION));
            }
            
            if (AudioManager.I != null)
                AudioManager.I.PlaySE(SEType.Grass);
            
            return true;
        }

        private Vector2Int SearchCellIndex(Vector3 position)
        {
            int x = Mathf.RoundToInt((position.x - currentOriginPosition.x) / groundData.ActualCellWidth);
            int y = Mathf.RoundToInt((position.y - currentOriginPosition.y) / groundData.ActualCellHeight);

            x = Mathf.Clamp(x, 0, groundData.columns - 1);
            y = Mathf.Clamp(y, 0, groundData.rows - 1);
            
            return new Vector2Int(x, y);
        }

        /// <summary>緑化率を取得（0.0～1.0）</summary>
        public float GetGreenificationRate()
        {
            if (currentGroundArray == null || groundData == null) return 0f;
            
            int totalTiles = groundData.columns * groundData.rows;
            if (totalTiles == 0) return 0f;
            
            int greenifiedCount = CountGreenifiedTiles();
            return (float)greenifiedCount / totalTiles;
        }

        /// <summary>緑化済みタイル数を取得</summary>
        public int CountGreenifiedTiles()
        {
            if (currentGroundArray == null) return 0;
            
            int count = 0;
            for (int x = 0; x < groundData.columns; x++)
            {
                for (int y = 0; y < groundData.rows; y++)
                {
                    if (currentGroundArray[x, y].isDrawn)
                        count++;
                }
            }
            return count;
        }

        /// <summary>総タイル数を取得</summary>
        public int GetTotalTileCount()
        {
            if (groundData == null) return 0;
            return groundData.columns * groundData.rows;
        }

        private void SetStageObject()
        {
            currentGroundArray = new GroundData[groundData.columns, groundData.rows];
            tileObjects = new GameObject[groundData.columns, groundData.rows];
            
            for (int y = 0; y < groundData.rows; y++)
            {
                for (int x = 0; x < groundData.columns; x++)
                {
                    Vector3 pos = new Vector3(x * groundData.ActualCellWidth, y * groundData.ActualCellHeight, TILE_Z_POSITION);
                    
                    if (groundData.tilePrefab == null) return;
                    
                    GameObject tile = Instantiate(groundData.tilePrefab, pos, Quaternion.identity, transform);
                    tile.name = $"Tile_{x}_{y}";
                    tileObjects[x, y] = tile;

                    if (ScoreManager.I == null) return;
                    
                    currentGroundArray[x, y].point = ScoreManager.I.NormalTilePoint;
                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = pos;
                    currentGroundArray[x, y].gridPos = new Vector2Int(x, y);
                    currentGroundArray[x, y].renderer = tile.GetComponent<Renderer>();
                }
            }
            
            currentOriginPosition = transform.position;
        }

        private void InitHighObject()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("HighScoreObject");
            
            if (ScoreManager.I == null) return;
            
            foreach(GameObject highScore in objects)
            {
                Vector2Int cellPosition = SearchCellIndex(highScore.transform.position);
                int multiplier = ScoreManager.I.HighScoreTileMultiplier;
                currentGroundArray[cellPosition.x, cellPosition.y].point *= multiplier;
            }
        }
        
        /// <summary>タイルのスケールをセルサイズに合わせて調整</summary>
        private void AdjustTileScale(GameObject tile, float targetWidth, float targetHeight)
        {
            if (tile == null) return;
            
            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                float pixelsPerUnit = sprite.pixelsPerUnit;
                
                Vector2 spriteSize = new Vector2(
                    sprite.rect.width / pixelsPerUnit,
                    sprite.rect.height / pixelsPerUnit
                );
                
                float scaleX = targetWidth / spriteSize.x;
                float scaleY = targetHeight / spriteSize.y;
                float uniformScale = Mathf.Min(scaleX, scaleY);
                tile.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                
                return;
            }
            
            var meshRenderer = tile.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Bounds bounds = meshRenderer.bounds;
                float scaleX = targetWidth / bounds.size.x;
                float scaleY = targetHeight / bounds.size.y;
                float uniformScale = Mathf.Min(scaleX, scaleY);
                tile.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
                return;
            }

        }
        
        /// <summary>
        /// 指定時間後にエフェクトをPoolに返却するCoroutine
        /// </summary>
        private System.Collections.IEnumerator ReturnEffectToPoolAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (effect != null && EffectFactory.I != null)
            {
                EffectFactory.I.ReturnEffect(effect);
            }
        }
    }
}
