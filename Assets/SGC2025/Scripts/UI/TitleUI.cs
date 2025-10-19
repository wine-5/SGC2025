using UnityEngine;

namespace SGC2025
{
    public class TitleUI : UIBase
    {
        override public void Start()
        {
            base.Start();

            CreateMenu("UI/RankingCanvas");
        }
    }
}
