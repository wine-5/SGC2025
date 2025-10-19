using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SGC2025
{
    /// <summary>
    /// BGMの音声データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New BGM Data", menuName = "SGC2025/Audio/BGM Data")]
    public class BGMData : ScriptableObject
    {
        [System.Serializable]
        public class BGMClip
        {
            [Tooltip("BGM名（参照用）")]
            public string name;
            [Tooltip("音声クリップ")]
            public AudioClip clip;
            [Tooltip("音量（0.0 - 1.0）")]
            [Range(0f, 1f)]
            public float volume = 1f;
            [Tooltip("ループするかどうか")]
            public bool loop = true;
            [Tooltip("フェードイン時間（秒）")]
            public float fadeInTime = 0f;
            [Tooltip("フェードアウト時間（秒）")]
            public float fadeOutTime = 0f;
        }
        
        [Header("BGM設定")]
        [SerializeField] private List<BGMClip> bgmClips = new List<BGMClip>();
        
        [Header("全体設定")]
        [Tooltip("BGM全体の音量")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        
        /// <summary>
        /// BGM全体の音量
        /// </summary>
        public float Volume => masterVolume;
        
        /// <summary>
        /// 指定した名前のBGMクリップを取得
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>AudioClip（見つからない場合はnull）</returns>
        public AudioClip GetClip(string bgmName)
        {
            var bgmClip = bgmClips.FirstOrDefault(bgm => bgm.name == bgmName);
            return bgmClip?.clip;
        }
        
        /// <summary>
        /// 指定した名前のBGMの音量を取得
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>音量（見つからない場合は1.0）</returns>
        public float GetVolume(string bgmName)
        {
            var bgmClip = bgmClips.FirstOrDefault(bgm => bgm.name == bgmName);
            return bgmClip?.volume ?? 1f;
        }
        
        /// <summary>
        /// 指定した名前のBGMがループするかを取得
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>ループするかどうか（見つからない場合はtrue）</returns>
        public bool GetLoop(string bgmName)
        {
            var bgmClip = bgmClips.FirstOrDefault(bgm => bgm.name == bgmName);
            return bgmClip?.loop ?? true;
        }
        
        /// <summary>
        /// 指定した名前のBGMのフェードイン時間を取得
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>フェードイン時間（見つからない場合は0）</returns>
        public float GetFadeInTime(string bgmName)
        {
            var bgmClip = bgmClips.FirstOrDefault(bgm => bgm.name == bgmName);
            return bgmClip?.fadeInTime ?? 0f;
        }
        
        /// <summary>
        /// 指定した名前のBGMのフェードアウト時間を取得
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>フェードアウト時間（見つからない場合は0）</returns>
        public float GetFadeOutTime(string bgmName)
        {
            var bgmClip = bgmClips.FirstOrDefault(bgm => bgm.name == bgmName);
            return bgmClip?.fadeOutTime ?? 0f;
        }
        
        /// <summary>
        /// 指定した名前のBGMが存在するかチェック
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <returns>存在するかどうか</returns>
        public bool HasBGM(string bgmName)
        {
            return bgmClips.Any(bgm => bgm.name == bgmName);
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
        /// 登録されているBGM名の一覧を取得
        /// </summary>
        /// <returns>BGM名のリスト</returns>
        public List<string> GetBGMNames()
        {
            return bgmClips.Select(bgm => bgm.name).ToList();
        }
        
        /// <summary>
        /// BGMクリップ数を取得
        /// </summary>
        public int ClipCount => bgmClips.Count;
        
        /// <summary>
        /// バリデーション
        /// </summary>
        private void OnValidate()
        {
            // BGM名の重複チェック
            var duplicateNames = bgmClips
                .GroupBy(bgm => bgm.name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            foreach (var duplicateName in duplicateNames)
            {
                Debug.LogWarning($"[BGMData] BGM名が重複しています: {duplicateName}");
            }
            
            // 空の名前をチェック
            for (int i = 0; i < bgmClips.Count; i++)
            {
                if (string.IsNullOrEmpty(bgmClips[i].name))
                {
                    Debug.LogWarning($"[BGMData] BGM {i} の名前が空です");
                }
                
                if (bgmClips[i].clip == null)
                {
                    Debug.LogWarning($"[BGMData] BGM '{bgmClips[i].name}' のクリップが設定されていません");
                }
            }
        }
    }
}