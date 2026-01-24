using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// SE用のオーディオデータ
    /// </summary>
    [System.Serializable]
    public class SEAudioData
    {
        [SerializeField] private SEType seType;
        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1.0f;

        public SEType SEType => seType;
        public AudioClip AudioClip => audioClip;
        public float VolumeMultiplier => volumeMultiplier;
    }

    /// <summary>
    /// BGM用のオーディオデータ
    /// </summary>
    [System.Serializable]
    public class BGMAudioData
    {
        [SerializeField] private BGMType bgmType;
        [SerializeField] private AudioClip audioClip;
        [SerializeField, Range(0f, 2f)] private float volumeMultiplier = 1.0f;
        [SerializeField] private bool loop = true;
        
        [Header("フェード設定")]
        [SerializeField] private bool useFadeIn = false;
        [SerializeField] private float fadeInDuration = 2f;
        [SerializeField] private bool useFadeOut = false;
        [SerializeField] private float fadeOutDuration = 2f;

        public BGMType BGMType => bgmType;
        public AudioClip AudioClip => audioClip;
        public float VolumeMultiplier => volumeMultiplier;
        public bool Loop => loop;
        public bool UseFadeIn => useFadeIn;
        public float FadeInDuration => fadeInDuration;
        public bool UseFadeOut => useFadeOut;
        public float FadeOutDuration => fadeOutDuration;
    }

    /// <summary>
    /// オーディオデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioData", menuName = "SGC2025/AudioData")]
    public class AudioDataSO : ScriptableObject
    {
        [Header("SE List")]
        [SerializeField] private SEAudioData[] seAudioDataList;
        
        [Header("BGM List")]
        [SerializeField] private BGMAudioData[] bgmAudioDataList;

        public SEAudioData[] SEAudioDataList => seAudioDataList;
        public BGMAudioData[] BGMAudioDataList => bgmAudioDataList;
    }
}
