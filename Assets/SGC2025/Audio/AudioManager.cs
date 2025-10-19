using UnityEngine;
using System.Collections;
using TechC;

namespace SGC2025
{
    /// <summary>
    /// ゲーム全体のオーディオを管理するマネージャー
    /// SE・BGMの再生、音量制御、フェード機能を提供
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("オーディオデータ")]
        [SerializeField] private SEData seData;
        [SerializeField] private BGMData bgmData;
        
        [Header("オーディオソース")]
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioSource bgmSource;
        
        [Header("設定")]
        [SerializeField] private bool enableDebugLog = false;
        
        // BGMフェード用の変数
        private Coroutine bgmFadeCoroutine;
        private string currentBGMName = "";
        
        // 音量設定
        private float seVolume = 1f;
        private float bgmVolume = 1f;
        
        // プロパティ
        public float SEVolume 
        { 
            get => seVolume; 
            set 
            { 
                seVolume = Mathf.Clamp01(value);
                UpdateSEVolume();
            }
        }
        
        public float BGMVolume 
        { 
            get => bgmVolume; 
            set 
            { 
                bgmVolume = Mathf.Clamp01(value);
                UpdateBGMVolume();
            }  
        }
        
        public bool IsPlayingBGM => bgmSource != null && bgmSource.isPlaying;
        public string CurrentBGMName => currentBGMName;

        protected override void Init()
        {
            base.Init();
            
            // AudioSourceのセットアップ
            SetupAudioSources();
            
            // 初期音量設定
            UpdateVolumes();
            
            if (enableDebugLog)
            {
                Debug.Log("[AudioManager] 初期化完了");
            }
        }
        
        /// <summary>
        /// AudioSourceを設定
        /// </summary>
        private void SetupAudioSources()
        {
            if (seSource == null)
            {
                var seObj = new GameObject("SESource");
                seObj.transform.SetParent(this.transform);
                seSource = seObj.AddComponent<AudioSource>();
                seSource.playOnAwake = false;
            }
            
            if (bgmSource == null)
            {
                var bgmObj = new GameObject("BGMSource");
                bgmObj.transform.SetParent(this.transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        /// <param name="seName">SE名</param>
        public void PlaySE(string seName)
        {
            if (seData == null)
            {
                Debug.LogWarning("[AudioManager] SEDataが設定されていません");
                return;
            }
            
            var clip = seData.GetClip(seName);
            if (clip != null)
            {
                float volume = seData.GetVolume(seName) * seData.Volume * seVolume;
                seSource.PlayOneShot(clip, volume);
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AudioManager] SE再生: {seName} (音量: {volume:F2})");
                }
            }
            else if (enableDebugLog)
            {
                Debug.LogWarning($"[AudioManager] SE '{seName}' が見つかりません");
            }
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="fadeInTime">フェードイン時間（-1で設定値を使用）</param>
        public void PlayBGM(string bgmName, float fadeInTime = -1f)
        {
            if (bgmData == null)
            {
                Debug.LogWarning("[AudioManager] BGMDataが設定されていません");
                return;
            }
            
            var clip = bgmData.GetClip(bgmName);
            if (clip == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"[AudioManager] BGM '{bgmName}' が見つかりません");
                }
                return;
            }
            
            // 既に同じBGMが再生中の場合は何もしない
            if (currentBGMName == bgmName && IsPlayingBGM)
            {
                return;
            }
            
            // フェード時間の決定
            float actualFadeTime = fadeInTime >= 0f ? fadeInTime : bgmData.GetFadeInTime(bgmName);
            
            // BGMの設定
            bgmSource.clip = clip;
            bgmSource.loop = bgmData.GetLoop(bgmName);
            currentBGMName = bgmName;
            
            if (actualFadeTime > 0f)
            {
                StartBGMFadeIn(actualFadeTime);
            }
            else
            {
                UpdateBGMVolume();
                bgmSource.Play();
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"[AudioManager] BGM再生: {bgmName} (フェードイン: {actualFadeTime:F1}秒)");
            }
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        /// <param name="fadeOutTime">フェードアウト時間（-1で設定値を使用）</param>
        public void StopBGM(float fadeOutTime = -1f)
        {
            if (!IsPlayingBGM) return;
            
            // フェード時間の決定
            float actualFadeTime = fadeOutTime >= 0f ? fadeOutTime : 
                                   (bgmData != null ? bgmData.GetFadeOutTime(currentBGMName) : 0f);
            
            if (actualFadeTime > 0f)
            {
                StartBGMFadeOut(actualFadeTime);
            }
            else
            {
                bgmSource.Stop();
                currentBGMName = "";
            }
            
            if (enableDebugLog)
            {
                Debug.Log($"[AudioManager] BGM停止: {currentBGMName} (フェードアウト: {actualFadeTime:F1}秒)");
            }
        }
        
        /// <summary>
        /// BGMを一時停止
        /// </summary>
        public void PauseBGM()
        {
            if (IsPlayingBGM)
            {
                bgmSource.Pause();
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AudioManager] BGM一時停止: {currentBGMName}");
                }
            }
        }
        
        /// <summary>
        /// BGMを再開
        /// </summary>
        public void ResumeBGM()
        {
            if (bgmSource.clip != null && !IsPlayingBGM)
            {
                bgmSource.UnPause();
                
                if (enableDebugLog)
                {
                    Debug.Log($"[AudioManager] BGM再開: {currentBGMName}");
                }
            }
        }

        /// <summary>
        /// 全音量を更新
        /// </summary>
        public void UpdateVolumes()
        {
            UpdateSEVolume();
            UpdateBGMVolume();
        }
        
        /// <summary>
        /// SE音量を更新
        /// </summary>
        private void UpdateSEVolume()
        {
            if (seSource != null && seData != null)
            {
                seSource.volume = seData.Volume * seVolume;
            }
        }
        
        /// <summary>
        /// BGM音量を更新
        /// </summary>
        private void UpdateBGMVolume()
        {
            if (bgmSource != null && bgmData != null && !string.IsNullOrEmpty(currentBGMName))
            {
                float targetVolume = bgmData.GetVolume(currentBGMName) * bgmData.Volume * bgmVolume;
                bgmSource.volume = targetVolume;
            }
        }
        
        /// <summary>
        /// BGMフェードイン開始
        /// </summary>
        private void StartBGMFadeIn(float fadeTime)
        {
            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }
            
            bgmFadeCoroutine = StartCoroutine(FadeBGM(0f, 1f, fadeTime, true));
        }
        
        /// <summary>
        /// BGMフェードアウト開始
        /// </summary>
        private void StartBGMFadeOut(float fadeTime)
        {
            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
            }
            
            bgmFadeCoroutine = StartCoroutine(FadeBGM(1f, 0f, fadeTime, false));
        }
        
        /// <summary>
        /// BGMフェード処理
        /// </summary>
        private IEnumerator FadeBGM(float startVolume, float endVolume, float fadeTime, bool playAtStart)
        {
            if (playAtStart)
            {
                bgmSource.volume = 0f;
                bgmSource.Play();
            }
            
            float targetVolume = bgmData != null && !string.IsNullOrEmpty(currentBGMName) ? 
                                bgmData.GetVolume(currentBGMName) * bgmData.Volume * bgmVolume : 1f;
            
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeTime;
                float currentVolume = Mathf.Lerp(startVolume, endVolume, t) * targetVolume;
                bgmSource.volume = currentVolume;
                yield return null;
            }
            
            bgmSource.volume = endVolume * targetVolume;
            
            if (!playAtStart && endVolume <= 0f)
            {
                bgmSource.Stop();
                currentBGMName = "";
            }
            
            bgmFadeCoroutine = null;
        }
    }
}