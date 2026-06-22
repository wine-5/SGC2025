using UnityEngine;
using SGC2025.Core;
using SGC2025.Audio;
using SGC2025.Effect;
using SGC2025.Item;

namespace SGC2025.Manager
{
    /// <summary>
    /// 地面システムの管理クラス
    /// マップ生成、タイルの緑化処理
    /// </summary>
    public class GroundManager : Singleton<GroundManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        private const float TILE_Z_POSITION = 0f;
        private const float GRASS_EFFECT_DURATION = 2f;
        private const float GRASS_EFFECT_Y_OFFSET = 0.1f;
        
        [Header("地面データ設定")]
        [SerializeField]
        [Tooltip("使用するGroundDataSO（マップ設定の単一の真実の源泉）")]
        private GroundDataSO groundData;

        [Header("緑化エフェクト設定")]
        [SerializeField, Tooltip("緑化範囲(size)に応じたエフェクト拡大の強さ。size=1は等倍、size>1は (1 + (size-1)×この値) 倍")]
        private float areaEffectScaleFactor = 0.5f;

        private struct GroundData
        {
            public Vector2 worldPos;
            public bool isDrawn;
        }

        private GroundData[,] currentGroundArray;
        private Vector3 currentOriginPosition;
        private GameObject[,] tileObjects;
        
        public GroundDataSO MapData => groundData;

        /// <summary>Playerのスポーン位置を取得（マップの中心）</summary>
        public Vector3 GetPlayerSpawnPosition()
        {
            if (groundData == null) return Vector3.zero;
            return groundData.MapCenterPosition;
        }

        private void Start()
        {
            if (groundData == null)
            {
                Debug.LogError("[GroundManager] GroundDataSO is not assigned!");
                return;
            }

            SetStageObject();
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);

            // ItemManagerのインスタンスを生成しておく（敵撃破イベントを受信させるため）
            _ = ItemManager.I;
        }
        
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);
            base.OnDestroy();
        }
        
        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            // 緑化サイズは敵データ由来。緑化範囲上昇アイテム中はブースト値を使う
            int size = e.GreeningSize;
            if (ItemManager.I != null && ItemManager.I.IsAreaGreenifyActive)
                size = e.GreeningSizeBoosted;

            DrawGroundArea(e.Position, size, e.IsBoss);
        }

        /// <summary>指定位置の地面を緑化（1マス）</summary>
        public bool DrawGround(Vector3 enemyPosition) => DrawGroundArea(enemyPosition, 1);

        /// <summary>
        /// 指定位置を中心に size×size マスを緑化する。
        /// エフェクトは1種類のプレハブを size に応じて拡縮して1回だけ生成する。
        /// 偶数サイズは中心から下・左側に1マス広く取る。
        /// </summary>
        public bool DrawGroundArea(Vector3 enemyPosition, int size, bool isBoss = false)
        {
            if (currentGroundArray == null || size <= 0) return false;

            Vector2Int centerCell = SearchCellIndex(enemyPosition);

            if (centerCell.x < 0 || centerCell.x >= groundData.columns ||
                centerCell.y < 0 || centerCell.y >= groundData.rows) return false;

            // size×size を中心に展開（偶数サイズは下/左寄りに1マス広い）
            int lo = (size - 1) / 2;
            int hi = size / 2;

            bool anyDrawn = false;

            for (int dx = -lo; dx <= hi; dx++)
            {
                for (int dy = -lo; dy <= hi; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || x >= groundData.columns || y < 0 || y >= groundData.rows)
                        continue;

                    if (currentGroundArray[x, y].isDrawn)
                        continue;

                    if (DrawSingleTile(x, y))
                        anyDrawn = true;
                }
            }

            // 中心位置にエフェクトと音を1回だけ生成（エフェクトはsizeに比例して拡大）
            if (anyDrawn)
            {
                Vector3 centerPos = currentGroundArray[centerCell.x, centerCell.y].worldPos;

                if (EffectFactory.I != null)
                {
                    Vector3 effectPos = centerPos + Vector3.up * GRASS_EFFECT_Y_OFFSET;
                    GameObject effect = EffectFactory.I.CreateEffect(EffectType.GrassRestorationEffect, effectPos, GRASS_EFFECT_DURATION);

                    // 全軸を一律に拡大（エフェクトが回転していても見た目が崩れない）
                    // size に等倍だと大きすぎるため、係数で緩やかに拡大する
                    if (effect != null && size > 1)
                        effect.transform.localScale *= 1f + (size - 1) * areaEffectScaleFactor;
                }

                // ボス撃破による緑化はボス専用SE、それ以外は通常の緑化SE
                if (AudioManager.I != null)
                    AudioManager.I.PlaySE(isBoss ? SEType.BossDefeated : SEType.Grass);
            }

            return anyDrawn;
        }

        /// <summary>指定位置を中心に範囲内の緑化済みタイルを茶色（非緑化）へ戻す（ボスの通過跡など）</summary>
        public bool RevertGroundArea(Vector3 worldPosition, int radius)
        {
            if (currentGroundArray == null) return false;

            Vector2Int centerCell = SearchCellIndex(worldPosition);
            bool anyReverted = false;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerCell.x + dx;
                    int y = centerCell.y + dy;

                    if (x < 0 || x >= groundData.columns || y < 0 || y >= groundData.rows)
                        continue;

                    // 緑化済みのセルのみ戻す
                    if (!currentGroundArray[x, y].isDrawn)
                        continue;

                    if (RevertSingleTile(x, y))
                        anyReverted = true;
                }
            }

            return anyReverted;
        }

        /// <summary>ワールド座標を対応するセルインデックスへ変換</summary>
        public Vector2Int WorldToCell(Vector3 worldPosition) => SearchCellIndex(worldPosition);

        /// <summary>単一タイルを緑化</summary>
        private bool DrawSingleTile(int x, int y)
        {
            if (groundData.grassTilePrefab == null) return false;

            if (tileObjects != null && tileObjects[x, y] != null)
                Destroy(tileObjects[x, y]);

            Vector3 pos = currentGroundArray[x, y].worldPos;
            GameObject grassTile = Instantiate(groundData.grassTilePrefab, pos, Quaternion.identity, transform);
            grassTile.name = $"GrassTile_{x}_{y}";

            AdjustTileScale(grassTile, groundData.ActualCellWidth, groundData.ActualCellHeight);

            if (tileObjects != null)
                tileObjects[x, y] = grassTile;

            currentGroundArray[x, y].isDrawn = true;

            EventBus.Publish(new GroundGreenifiedEvent(pos));

            return true;
        }

        /// <summary>単一タイルを非緑化（通常タイルへ戻す）。DrawSingleTileの逆処理</summary>
        private bool RevertSingleTile(int x, int y)
        {
            if (groundData.tilePrefab == null) return false;

            if (tileObjects != null && tileObjects[x, y] != null)
                Destroy(tileObjects[x, y]);

            Vector3 pos = currentGroundArray[x, y].worldPos;
            GameObject tile = Instantiate(groundData.tilePrefab, pos, Quaternion.identity, transform);
            tile.name = $"Tile_{x}_{y}";

            AdjustTileScale(tile, groundData.ActualCellWidth, groundData.ActualCellHeight);

            if (tileObjects != null)
                tileObjects[x, y] = tile;

            currentGroundArray[x, y].isDrawn = false;

            EventBus.Publish(new GroundUngreenifiedEvent(pos));

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
        private int CountGreenifiedTiles()
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

                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = pos;
                }
            }
            
            currentOriginPosition = transform.position;
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
    }
}
