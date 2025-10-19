using UnityEngine;

namespace SGC2025
{
    public enum AudioType
    {
        SE,
        BGM
    }

    [CreateAssetMenu(menuName = "SGC2025/AudioData")]
    public class AudioData : ScriptableObject
    {
        [System.Serializable]
        public struct AudioEntry
        {
            public AudioType Type;
            public string Name;
            public AudioClip Clip;
        }

        [SerializeField] private AudioEntry[] audioList;
        [SerializeField, Range(0f, 1f)] private float seVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;

        public float SEVolume
        {
            get => seVolume;
            set => seVolume = Mathf.Clamp01(value);
        }
        public float BGMVolume
        {
            get => bgmVolume;
            set => bgmVolume = Mathf.Clamp01(value);
        }

        public AudioClip GetClip(AudioType type, string name)
        {
            foreach (var entry in audioList)
            {
                if (entry.Type == type && entry.Name == name)
                    return entry.Clip;
            }
            return null;
        }
    }
}