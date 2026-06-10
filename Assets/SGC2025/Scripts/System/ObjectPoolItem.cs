using System;
using UnityEngine;

namespace TechC
{
    /// <summary>
    /// オブジェクトプールの各アイテムを定義するクラス
    /// </summary>
    [Serializable]
    public class ObjectPoolItem
    {
        [Tooltip("プール識別用の名前")]
        public string name;
        
        [Tooltip("プールするプレハブ")]
        public GameObject prefab;
        
        [Tooltip("生成されたオブジェクトを格納する親オブジェクト")]
        public GameObject parent;
        
        [Tooltip("初期プールサイズ")]
        [Range(0, 1000)]
        public int initialSize = 5;
    }
}