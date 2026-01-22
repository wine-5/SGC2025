using UnityEngine;

namespace SGC2025
{
    /// <summary>
    /// タイトル画面UI
    /// </summary>
    public class TitleUI : UIBase
    {
        override public void Start()
        {
            base.Start();

            CreateMenu("UI/RankingCanvas");
        }
    }
}
