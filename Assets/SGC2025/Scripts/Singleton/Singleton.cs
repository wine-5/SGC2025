using UnityEngine;

namespace SGC2025
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T I => Instance;

        private static T instance;

        protected virtual bool UseDontDestroyOnLoad => true;
        protected virtual bool DestroyTargetGameObject => false;

        /// <summary>
        /// シングルトンのインスタンスを取得
        /// </summary>
        private static T Instance
        {
            get
            {
                if (instance == null)
                {
                    var obj = new GameObject(typeof(T).Name + " (Singleton)");
                    instance = obj.AddComponent<T>();
                }
                return instance;
            }
        }

        
        /// <summary>
        /// シングルトンの初期化処理
        /// なければ自身を追加
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                Init();
                if (UseDontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
            }
            else if (instance != this)
            {
                if (DestroyTargetGameObject) Destroy(gameObject);
                else Destroy(this);
            }
        }

        /// <summary>
        ///インスタンス破棄時の処理
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 派生クラスで初期化処理を追加したい場合にオーバーライドして使用可能
        /// </summary>
        protected virtual void Init() { }
    }
}