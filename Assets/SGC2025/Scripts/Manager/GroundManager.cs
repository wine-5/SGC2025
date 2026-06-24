using UnityEngine;
using UnityEngine.Tilemaps;
using Tyotyo.Core;
using Tyotyo.Audio;
using Tyotyo.Effect;
using Tyotyo.InGame.Item;
using Tyotyo.InGame.Ground;

namespace Tyotyo.Manager
{
    /// <summary>
    /// 地面システムの管理クラス
    /// Tilemapによるマップ描画、タイルの緑化処理
    /// </summary>
    public class GroundManager : Singleton<GroundManager>
    {
        protected override bool UseDontDestroyOnLoad => false;

        private const float GRASS_EFFECT_DURATION = 2f;
        private const float GRASS_EFFECT_Y_OFFSET = 0.1f;

        [Header("地面データ設定")]
        [SerializeField]
        [Tooltip("使用するGroundDataSO（マップ設定の単一の真実の源泉）")]
        private GroundDataSO groundData;

        [Header("Tilemap設定")]
        [SerializeField, Tooltip("地面を描画するTilemap（Grid配下）。セルサイズ・位置はコードから自動設定される")]
        private Tilemap tilemap;

        [SerializeField, Tooltip("通常（茶）タイルのスプライト")]
        private Sprite groundSprite;

        [SerializeField, Tooltip("緑化（草）タイルのスプライト")]
        private Sprite grassSprite;

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

        // ランタイム生成するTile（草・土）。スプライトをセルサイズへ合わせて拡縮した状態で保持する
        private TileBase groundTile;
        private TileBase grassTile;

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

            if (tilemap == null)
            {
                Debug.LogError("[GroundManager] Tilemap is not assigned!");
                return;
            }

            BuildTiles();
            SetStageObject();
            EventBus.Subscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);

            // ItemManagerのインスタンスを生成しておく（敵撃破イベントを受信させるため）
            _ = ItemManager.I;
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<EnemyDestroyedEvent>(OnEnemyDestroyed);

            // ランタイム生成したTileを破棄（ScriptableObjectのリーク防止）
            if (groundTile != null) Destroy(groundTile);
            if (grassTile != null) Destroy(grassTile);

            base.OnDestroy();
        }

        private void OnEnemyDestroyed(EnemyDestroyedEvent e)
        {
            // 緑化サイズは敵データ由来。緑化範囲上昇アイテム中はブースト値を使う
            int size = e.GreeningSize;
            if (ItemManager.I != null && ItemManager.I.IsAreaGreenifyActive)
                size = e.GreeningSizeBoosted;

            DrawGroundArea(e.Position, size);
        }

        /// <summary>指定位置の地面を緑化（1マス）</summary>
        public bool DrawGround(Vector3 enemyPosition) => DrawGroundArea(enemyPosition, 1);

        /// <summary>
        /// 指定位置を中心に size×size マスを緑化する。
        /// エフェクトは1種類のプレハブを size に応じて拡縮して1回だけ生成する。
        /// 偶数サイズは中心から下・左側に1マス広く取る。
        /// </summary>
        public bool DrawGroundArea(Vector3 enemyPosition, int size)
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

                if (AudioManager.I != null)
                    AudioManager.I.PlaySE(SEType.Grass);
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

        /// <summary>単一タイルを緑化（Tilemapのセルを草タイルへ差し替えるだけ）</summary>
        private bool DrawSingleTile(int x, int y)
        {
            if (grassTile == null) return false;

            tilemap.SetTile(new Vector3Int(x, y, 0), grassTile);
            currentGroundArray[x, y].isDrawn = true;

            EventBus.Publish(new GroundGreenifiedEvent(currentGroundArray[x, y].worldPos));

            return true;
        }

        /// <summary>単一タイルを非緑化（通常タイルへ戻す）。DrawSingleTileの逆処理</summary>
        private bool RevertSingleTile(int x, int y)
        {
            if (groundTile == null) return false;

            tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
            currentGroundArray[x, y].isDrawn = false;

            EventBus.Publish(new GroundUngreenifiedEvent(currentGroundArray[x, y].worldPos));

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

        /// <summary>
        /// 草・土のTileをランタイム生成し、Gridのセルサイズ・原点を地面データへ合わせる。
        /// スプライトのワールドサイズがセルサイズと異なる場合はTileの変換行列で拡縮して隙間/はみ出しを防ぐ
        /// （旧 AdjustTileScale と同等の処理をTile単位で行う）。
        /// </summary>
        private void BuildTiles()
        {
            float cellW = groundData.ActualCellWidth;
            float cellH = groundData.ActualCellHeight;

            groundTile = CreateScaledTile(groundSprite, cellW, cellH);
            grassTile = CreateScaledTile(grassSprite, cellW, cellH);

            // セル(x,y)の中心がワールド(x*cellW, y*cellH)に来るよう、Gridの原点を半セルずらす。
            // これにより既存の座標系（プレイヤー移動・弾・敵・ミニマップ）と完全に一致する。
            Grid grid = tilemap.layoutGrid;
            if (grid != null)
            {
                grid.cellSize = new Vector3(cellW, cellH, 0f);
                grid.transform.position = new Vector3(-cellW * 0.5f, -cellH * 0.5f, 0f);
            }
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
        }

        /// <summary>スプライトを targetWidth×targetHeight に収まるよう一律拡縮したTileを生成する</summary>
        private TileBase CreateScaledTile(Sprite sprite, float targetWidth, float targetHeight)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;

            if (sprite != null)
            {
                float pixelsPerUnit = sprite.pixelsPerUnit;
                float spriteW = pixelsPerUnit > 0f ? sprite.rect.width / pixelsPerUnit : targetWidth;
                float spriteH = pixelsPerUnit > 0f ? sprite.rect.height / pixelsPerUnit : targetHeight;

                float scaleX = spriteW > 0f ? targetWidth / spriteW : 1f;
                float scaleY = spriteH > 0f ? targetHeight / spriteH : 1f;
                float uniformScale = Mathf.Min(scaleX, scaleY);

                tile.transform = Matrix4x4.Scale(new Vector3(uniformScale, uniformScale, 1f));
                tile.flags = TileFlags.LockTransform;
            }

            return tile;
        }

        /// <summary>Tilemapを全マス通常タイルで敷き詰め、状態配列を初期化する</summary>
        private void SetStageObject()
        {
            currentGroundArray = new GroundData[groundData.columns, groundData.rows];

            float cellW = groundData.ActualCellWidth;
            float cellH = groundData.ActualCellHeight;

            tilemap.ClearAllTiles();

            for (int y = 0; y < groundData.rows; y++)
            {
                for (int x = 0; x < groundData.columns; x++)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), groundTile);

                    currentGroundArray[x, y].isDrawn = false;
                    currentGroundArray[x, y].worldPos = new Vector2(x * cellW, y * cellH);
                }
            }

            currentOriginPosition = transform.position;
        }
    }
}
