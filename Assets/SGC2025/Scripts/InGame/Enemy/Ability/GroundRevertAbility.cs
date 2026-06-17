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
        [Header("戻す範囲：一辺のマス数を直接入力。奇数のみ有効（偶数は禁止）。例: 3 → 3x3, 5 → 5x5")]
        [SerializeField]
        private int revertSize = 3;

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
            // 一辺のマス数 → 中心からの半径（3→1, 5→2）。偶数が入っても切り捨てで動作
            int radius = Mathf.Max(0, (revertSize - 1) / 2);
            GroundManager.I.RevertGroundArea(transform.position, radius);
        }

        public void OnDespawn()
        {
            lastCell = InvalidCell;
        }
    }
}
