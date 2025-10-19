using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SGC2025
{
    /// <summary>
    /// SEの音声データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New SE Data", menuName = "SGC2025/Audio/SE Data")]
    public class SEData : ScriptableObject
    {
        [System.Serializable]
        public class SEClip
        {
            [Tooltip("SE名（参照用）")]
            public string name;
            [Tooltip("音声クリップ")]
            public AudioClip clip;
            [Tooltip("音量（0.0 - 1.0）")]
            [Range(0f, 1f)]
            public float volume = 1f;
        }
        
        [Header("SE設定")]
        [SerializeField] private List<SEClip> seClips = new List<SEClip>();
        
        [Header("全体設定")]
        [Tooltip("SE全体の音量")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        
        /// <summary>
        /// SE全体の音量
        /// </summary>
        public float Volume => masterVolume;
        
        /// <summary>
        /// 指定した名前のSEクリップを取得
        /// </summary>
        /// <param name="seName">SE名</param>
        /// <returns>AudioClip（見つからない場合はnull）</returns>
        public AudioClip GetClip(string seName)
        {
            var seClip = seClips.FirstOrDefault(se => se.name == seName);
            return seClip?.clip;
        }
        
        /// <summary>
        /// 指定した名前のSEの音量を取得
        /// </summary>
        /// <param name="seName">SE名</param>
        /// <returns>音量（見つからない場合は1.0）</returns>
        public float GetVolume(string seName)
        {
            var seClip = seClips.FirstOrDefault(se => se.name == seName);
            return seClip?.volume ?? 1f;
        }
        
        /// <summary>
        /// 指定した名前のSEが存在するかチェック
        /// </summary>
        /// <param name="seName">SE名</param>
        /// <returns>存在するかどうか</returns>
        public bool HasSE(string seName)
        {
            return seClips.Any(se => se.name == seName);
        }
        
        /// <summary>
        /// 全体音量を設定
        /// </summary>
        /// <param name="volume">音量（0.0 - 1.0）</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// 登録されているSE名の一覧を取得
        /// </summary>
        /// <returns>SE名のリスト</returns>
        public List<string> GetSENames()
        {
            return seClips.Select(se => se.name).ToList();
        }
        
        /// <summary>
        /// SEクリップ数を取得
        /// </summary>
        public int ClipCount => seClips.Count;
        
        /// <summary>
        /// バリデーション
        /// </summary>
        private void OnValidate()
        {
            // SE名の重複チェック
            var duplicateNames = seClips
                .GroupBy(se => se.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            foreach (var duplicateName in duplicateNames)
            {
                Debug.LogWarning($"[SEData] SE名が重複しています: {duplicateName}");
            }
            
            // 空の名前をチェック
            for (int i = 0; i < seClips.Count; i++)
            {
                if (string.IsNullOrEmpty(seClips[i].name))
                {
                    Debug.LogWarning($"[SEData] SE {i} の名前が空です");
                }
                
                if (seClips[i].clip == null)
                {
                    Debug.LogWarning($"[SEData] SE '{seClips[i].name}' のクリップが設定されていません");
                }
            }
        }
    }
}