using UnityEngine;
using SGC2025.Manager;

namespace SGC2025.Enemy
{
    /// <summary>
    /// 通過した地面の緑化を茶色（非緑化）へ戻すボス能力。
    /// EnemyControllerに収集され、生存中にTickされる。Excavatorボス等のプレハブに付与する。
    /// </summary>
    public class GroundRevertAbility : MonoBehaviour, IEnemyAbility
    {
        [SerializeField, Tooltip("戻す範囲（0=1マス, 1=3x3, 2=5x5）")]
        private int revertRadius = 1;

        // 同一セルでの無駄な再生成を避けるための直近セル
        private static readonly Vector2Int InvalidCell = new(int.MinValue, int.MinValue);
        private Vector2Int lastCell = InvalidCell;

        public void OnSpawn(EnemyController owner)
        {
            // 再利用時に前回の位置が残らないようリセット
            lastCell = InvalidCell;
        }

        public void Tick(float deltaTime)
        {
            if (GroundManager.I == null) return;

            // セルをまたいだ時だけ処理（毎フレームのTile再生成を避ける）
            Vector2Int currentCell = GroundManager.I.WorldToCell(transform.position);
            if (currentCell == lastCell) return;

            lastCell = currentCell;
            GroundManager.I.RevertGroundArea(transform.position, revertRadius);
        }

        public void OnDespawn()
        {
            lastCell = InvalidCell;
        }
    }
}
