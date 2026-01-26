using UnityEngine;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 武器の強化レベルごとのパラメーターを定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon Upgrade Data", menuName = "SGC2025/Weapon Upgrade Data")]
    public class WeaponUpgradeDataSO : ScriptableObject
    {
        [Header("強化設定")]
        [SerializeField] private int enemiesPerUpgrade = 5; // 何体倒すごとに強化するか
        
        [Header("レベル別設定")]
        [SerializeField] private WeaponLevelData[] levelData;
        
        // プロパティ
        public int EnemiesPerUpgrade => enemiesPerUpgrade;
        
        /// <summary>
        /// 指定レベルの武器データを取得
        /// </summary>
        public WeaponLevelData GetLevelData(int level)
        {
            if (levelData == null || levelData.Length == 0)
            {
                Debug.LogError("WeaponUpgradeDataSO: レベルデータが設定されていません");
                return new WeaponLevelData(); // デフォルト値
            }
            
            // レベルが範囲外の場合は最大レベルのデータを返す
            int index = Mathf.Min(level - 1, levelData.Length - 1);
            index = Mathf.Max(0, index);
            
            return levelData[index];
        }
        
        /// <summary>
        /// 撃破数からレベルを計算
        /// </summary>
        public int CalculateLevel(int enemiesKilled)
        {
            return (enemiesKilled / enemiesPerUpgrade) + 1;
        }
        
        /// <summary>
        /// 最大レベルを取得
        /// </summary>
        public int MaxLevel => levelData?.Length ?? 1;
        
        private void OnValidate()
        {
            enemiesPerUpgrade = Mathf.Max(1, enemiesPerUpgrade);
        }
    }
    
    /// <summary>
    /// 武器のレベル別データ
    /// </summary>
    [System.Serializable]
    public struct WeaponLevelData
    {
        [Header("レベル情報")]
        public int level;
        
        [Header("発射設定")]
        public int bulletDirections; // 弾の発射方向数（4, 8, 12, 16...）
        
        /// <summary>
        /// デフォルトコンストラクタ（レベル1の設定）
        /// </summary>
        public WeaponLevelData(int defaultLevel = 1)
        {
            level = defaultLevel;
            bulletDirections = 4;
        }
    }
}