using UnityEngine;
using SGC2025.Audio;

namespace SGC2025.UI
{
    /// <summary>
    /// タイトル画面UI
    /// </summary>
    public class TitleUI : UIBase
    {
        private void Start()
        {
            if (AudioManager.I != null)
                AudioManager.I.PlayBGM(BGMType.Title);
        }
    }
}
