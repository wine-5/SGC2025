using UnityEngine;

namespace SGC2025.Player.Bullet
{
    /// <summary>
    /// 弾のパラメーターを定義するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Bullet Data", menuName = "SGC2025/Bullet Data")]
    public class BulletDataSO : ScriptableObject
    {
        [Header("弾の基本設定")]
        [SerializeField] private string bulletName = "基本弾";
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float lifeTime = 5f; // 弾の生存時間（秒）
        
        [Header("見た目設定")]
        [SerializeField] private Sprite bulletSprite; // 弾のスプライト
        [SerializeField] private float bulletSize = 0.1f;
        
        // プロパティ
        public string BulletName => bulletName;
        public float MoveSpeed => moveSpeed;
        public float Damage => damage;
        public float LifeTime => lifeTime;
        public Sprite BulletSprite => bulletSprite;
        public float BulletSize => bulletSize;
        
        private void OnValidate()
        {
            // バリデーション
            moveSpeed = Mathf.Max(0.1f, moveSpeed);
            damage = Mathf.Max(0.1f, damage);
            lifeTime = Mathf.Max(0.1f, lifeTime);
            bulletSize = Mathf.Max(0.01f, bulletSize);
        }
    }
}