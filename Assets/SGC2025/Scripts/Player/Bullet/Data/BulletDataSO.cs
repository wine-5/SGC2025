using UnityEngine;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾のパラメーターを定義するScriptableObject
    /// インスペクターでの設定とコードでの参照の両方をサポート
    /// バリデーション機能により不正値を自動修正
    /// </summary>
    [CreateAssetMenu(fileName = "New Bullet Data", menuName = "SGC2025/Bullet Data")]
    public class BulletDataSO : ScriptableObject
    {
        #region フィールド

        [Header("基本設定")]
        [Tooltip("弾の識別名")]
        [SerializeField] private string bulletName = "基本弾";
        
        [Tooltip("移動速度（Unity単位/秒）")]
        [SerializeField] private float moveSpeed = 10f;
        
        [Tooltip("ダメージ値")]
        [SerializeField] private float damage = 1f;
        
        [Tooltip("生存時間（秒）")]
        [SerializeField] private float lifeTime = 5f;
         
        [Tooltip("弾のサイズ倍率")]
        [SerializeField] private float bulletSize = 0.1f;

        #endregion

        #region プロパティ

        /// <summary>弾の識別名</summary>
        public string BulletName => bulletName;
        
        /// <summary>移動速度（Unity単位/秒）</summary>
        public float MoveSpeed => moveSpeed;
        
        /// <summary>ダメージ値</summary>
        public float Damage => damage;
        
        /// <summary>生存時間（秒）</summary>
        public float LifeTime => lifeTime;
                
        /// <summary>弾のサイズ倍率</summary>
        public float BulletSize => bulletSize;

        #endregion

        #region バリデーション

        /// <summary>
        /// インスペクター値変更時の自動バリデーション
        /// </summary>
        private void OnValidate()
        {
            ValidateNumericValues();
        }

        /// <summary>
        /// 数値パラメーターの妥当性を検証・修正
        /// </summary>
        private void ValidateNumericValues()
        {
            // 最小値制限による自動修正
            moveSpeed = Mathf.Max(Constants.MIN_MOVE_SPEED, moveSpeed);
            damage = Mathf.Max(Constants.MIN_DAMAGE, damage);
            lifeTime = Mathf.Max(Constants.MIN_LIFE_TIME, lifeTime);
            bulletSize = Mathf.Max(Constants.MIN_BULLET_SIZE, bulletSize);
        }

        #endregion

        #region 定数

        /// <summary>
        /// BulletDataSOで使用する定数
        /// </summary>
        private static class Constants
        {
            public const float MIN_MOVE_SPEED = 0.1f;
            public const float MIN_DAMAGE = 0.1f;
            public const float MIN_LIFE_TIME = 0.1f;
            public const float MIN_BULLET_SIZE = 0.01f;
        }

        #endregion
    }
}