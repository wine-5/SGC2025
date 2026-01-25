using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SGC2025.Audio
{
    /// <summary>
    /// オーディオ再生を管理するマネージャー
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("音量設定")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float bgmVolume = 0.7f;
        [Range(0f, 1f)] public float seVolume = 1f;

        [Header("フェード設定")]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("AudioData")]
        [SerializeField] private AudioDataSO audioData;

        [Header("AudioMixer")]
        [SerializeField] private AudioMixerGroup masterMixerGroup;
        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private AudioMixerGroup seMixerGroup;

        // BGM用のAudioSource
        private AudioSource bgmAudioSource1;
        private AudioSource bgmAudioSource2;
        private AudioSource currentBgmAudioSource;
        private AudioSource previousBgmAudioSource;

        // SE用のAudioSourceプール
        private List<AudioSource> seAudioSourcePool = new List<AudioSource>();
        private int initialSePoolSize = 10;

        // 音声データのディクショナリ
        private Dictionary<SEType, SEAudioData> seDataDict = new Dictionary<SEType, SEAudioData>();
        private Dictionary<BGMType, BGMAudioData> bgmDataDict = new Dictionary<BGMType, BGMAudioData>();

        // フェード用のコルーチン
        private Coroutine fadeCoroutine;

        // 現在再生中のBGM
        private BGMType currentBgmType = BGMType.None;

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
            BuildAudioDataDictionaries();
        }

        private void InitializeAudioSources()
        {
            bgmAudioSource1 = CreateAudioSource("BGM_AudioSource1", bgmMixerGroup);
            bgmAudioSource2 = CreateAudioSource("BGM_AudioSource2", bgmMixerGroup);
            currentBgmAudioSource = bgmAudioSource1;
            previousBgmAudioSource = bgmAudioSource2;

            for (int i = 0; i < initialSePoolSize; i++)
            {
                AudioSource seSource = CreateAudioSource($"SE_AudioSource_{i}", seMixerGroup);
                seAudioSourcePool.Add(seSource);
            }
        }

        private AudioSource CreateAudioSource(string name, AudioMixerGroup mixerGroup)
        {
            GameObject audioObject = new GameObject(name);
            audioObject.transform.SetParent(transform);
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.playOnAwake = false;
            return audioSource;
        }

        private void BuildAudioDataDictionaries()
        {
            if (audioData == null)
                throw new System.NullReferenceException("[AudioManager] AudioDataSO is not assigned!");

            foreach (var seData in audioData.SEAudioDataList)
            {
                if (!seDataDict.ContainsKey(seData.SEType))
                    seDataDict.Add(seData.SEType, seData);
            }

            foreach (var bgmData in audioData.BGMAudioDataList)
            {
                if (!bgmDataDict.ContainsKey(bgmData.BGMType))
                    bgmDataDict.Add(bgmData.BGMType, bgmData);
            }
        }

        /// <summary>
        /// SEを再生
        /// </summary>
        public void PlaySE(SEType seType, float volumeMultiplier = 1f)
        {
            if (seType == SEType.None || !seDataDict.ContainsKey(seType)) return;

            SEAudioData seData = seDataDict[seType];
            if (seData.AudioClip == null) return;

            AudioSource availableSource = GetAvailableSeAudioSource();
            if (availableSource == null) return;
            
            availableSource.clip = seData.AudioClip;
            availableSource.volume = seVolume * masterVolume * seData.VolumeMultiplier * volumeMultiplier;
            availableSource.Play();
        }

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(BGMType bgmType, bool useFade = false)
        {
            if (bgmType == BGMType.None)
            {
                StopBGM(useFade);
                return;
            }

            if (currentBgmType == bgmType && currentBgmAudioSource.isPlaying)
                return;

            if (!bgmDataDict.ContainsKey(bgmType)) return;

            BGMAudioData bgmData = bgmDataDict[bgmType];
            if (bgmData.AudioClip == null) return;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            if (useFade && bgmData.UseFadeIn)
            {
                fadeCoroutine = StartCoroutine(CrossFadeBGM(bgmData));
            }
            else
            {
                SwapBgmAudioSources();
                currentBgmAudioSource.clip = bgmData.AudioClip;
                currentBgmAudioSource.loop = bgmData.Loop;
                currentBgmAudioSource.volume = bgmVolume * masterVolume * bgmData.VolumeMultiplier;
                currentBgmAudioSource.Play();

                previousBgmAudioSource.Stop();
            }

            currentBgmType = bgmType;
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM(bool useFade = false)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            if (useFade && currentBgmAudioSource.isPlaying)
            {
                BGMType currentType = currentBgmType;
                if (bgmDataDict.ContainsKey(currentType) && bgmDataDict[currentType].UseFadeOut)
                    fadeCoroutine = StartCoroutine(FadeOutBGM(bgmDataDict[currentType].FadeOutDuration));
                else
                    currentBgmAudioSource.Stop();
            }
            else
            {
                currentBgmAudioSource.Stop();
                previousBgmAudioSource.Stop();
            }

            currentBgmType = BGMType.None;
        }

        /// <summary>
        /// BGMの音量を設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            UpdateBgmVolume();
        }

        /// <summary>
        /// SEの音量を設定
        /// </summary>
        public void SetSEVolume(float volume) => seVolume = Mathf.Clamp01(volume);

        /// <summary>
        /// マスター音量を設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateBgmVolume();
        }

        private AudioSource GetAvailableSeAudioSource()
        {
            foreach (var source in seAudioSourcePool)
            {
                if (!source.isPlaying)
                    return source;
            }

            AudioSource newSource = CreateAudioSource($"SE_AudioSource_{seAudioSourcePool.Count}", seMixerGroup);
            seAudioSourcePool.Add(newSource);
            return newSource;
        }

        private void SwapBgmAudioSources()
        {
            AudioSource temp = currentBgmAudioSource;
            currentBgmAudioSource = previousBgmAudioSource;
            previousBgmAudioSource = temp;
        }

        private void UpdateBgmVolume()
        {
            if (currentBgmType != BGMType.None && bgmDataDict.ContainsKey(currentBgmType))
            {
                BGMAudioData bgmData = bgmDataDict[currentBgmType];
                currentBgmAudioSource.volume = bgmVolume * masterVolume * bgmData.VolumeMultiplier;
            }
        }

        private IEnumerator CrossFadeBGM(BGMAudioData newBgmData)
        {
            float fadeInDuration = newBgmData.FadeInDuration;
            float fadeOutDuration = 0f;

            if (currentBgmType != BGMType.None && bgmDataDict.ContainsKey(currentBgmType))
                fadeOutDuration = bgmDataDict[currentBgmType].FadeOutDuration;

            SwapBgmAudioSources();

            currentBgmAudioSource.clip = newBgmData.AudioClip;
            currentBgmAudioSource.loop = newBgmData.Loop;
            currentBgmAudioSource.volume = 0f;
            currentBgmAudioSource.Play();

            float maxDuration = Mathf.Max(fadeInDuration, fadeOutDuration);
            float elapsedTime = 0f;

            while (elapsedTime < maxDuration)
            {
                elapsedTime += Time.deltaTime;

                if (fadeInDuration > 0f)
                {
                    float fadeInProgress = Mathf.Clamp01(elapsedTime / fadeInDuration);
                    float fadeInVolume = fadeCurve.Evaluate(fadeInProgress);
                    currentBgmAudioSource.volume = bgmVolume * masterVolume * newBgmData.VolumeMultiplier * fadeInVolume;
                }

                if (fadeOutDuration > 0f && elapsedTime < fadeOutDuration)
                {
                    float fadeOutProgress = Mathf.Clamp01(elapsedTime / fadeOutDuration);
                    float fadeOutVolume = fadeCurve.Evaluate(1f - fadeOutProgress);
                    if (currentBgmType != BGMType.None && bgmDataDict.ContainsKey(currentBgmType))
                    {
                        previousBgmAudioSource.volume = bgmVolume * masterVolume * bgmDataDict[currentBgmType].VolumeMultiplier * fadeOutVolume;
                    }
                }

                yield return null;
            }

            previousBgmAudioSource.Stop();
            currentBgmAudioSource.volume = bgmVolume * masterVolume * newBgmData.VolumeMultiplier;
            fadeCoroutine = null;
        }

        private IEnumerator FadeOutBGM(float fadeOutDuration)
        {
            float startVolume = currentBgmAudioSource.volume;
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float fadeProgress = elapsedTime / fadeOutDuration;
                float volume = startVolume * fadeCurve.Evaluate(1f - fadeProgress);
                currentBgmAudioSource.volume = volume;
                yield return null;
            }

            currentBgmAudioSource.Stop();
            fadeCoroutine = null;
        }
    }
}