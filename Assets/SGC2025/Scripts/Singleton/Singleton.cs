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
            Debug.Log($"[Singleton<{typeof(T).Name}>] Awake - this: {(this as T)?.GetInstanceID()}, instance: {instance?.GetInstanceID()}, instance.gameObject: {(instance?.gameObject != null ? "alive" : "null")}");
            
            // 既存のインスタンスがnullまたは破棄されている場合
            if (instance == null || instance.gameObject == null)
            {
                Debug.Log($"[Singleton<{typeof(T).Name}>] Setting new instance: {(this as T)?.GetInstanceID()}");
                instance = this as T;
                Init();
                if (UseDontDestroyOnLoad) 
                {
                    // DontDestroyOnLoadはルートオブジェクトにのみ適用可能
                    if (transform.parent == null)
                    {
                        DontDestroyOnLoad(this.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning($"[{typeof(T).Name}] DontDestroyOnLoad requires root GameObject. Current object has parent: {transform.parent.name}");
                    }
                }
            }
            else if (instance != this)
            {
                // 既存のインスタンスがUseDontDestroyOnLoadでない場合、新しいインスタンスに置き換える
                if (!UseDontDestroyOnLoad)
                {
                    T oldInstance = instance;
                    Debug.Log($"[Singleton<{typeof(T).Name}>] Replacing old instance {oldInstance?.GetInstanceID()} with new instance {(this as T)?.GetInstanceID()}");
                    instance = this as T;
                    if (oldInstance != null && oldInstance.gameObject != null)
                    {
                        Destroy(oldInstance.gameObject);
                    }
                    Init();
                }
                else
                {
                    Debug.LogWarning($"[Singleton<{typeof(T).Name}>] Destroying duplicate - this: {(this as T)?.GetInstanceID()}, keeping instance: {instance?.GetInstanceID()}");
                    if (DestroyTargetGameObject)
                        Destroy(gameObject);
                    else Destroy(this);
                }
            }
        }

        /// <summary>
        ///インスタンス破棄時の処理
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        /// <summary>
        /// 派生クラスで初期化処理を追加したい場合にオーバーライドして使用可能
        /// </summary>
        protected virtual void Init() { }
    }
}