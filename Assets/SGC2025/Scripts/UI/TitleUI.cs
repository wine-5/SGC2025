using UnityEngine;
using Tyotyo.Audio;

namespace Tyotyo.UI
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
