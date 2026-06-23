using UnityEngine;
using TMPro;

namespace SGC2025.UI
{
    /// <summary>
    /// リザルトの1行（見出しラベル＋数値）をまとめたコンポーネント。
    /// 同じ行プレハブを複製して並べることで、数値の列を簡単にそろえられる。
    /// 見出しはプレハブ側で設定し、数値だけをResultUIから流し込む使い方を想定。
    /// </summary>
    public class ResultStatRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText; // 見出し（緑化度 など）。省略可
        [SerializeField] private TextMeshProUGUI valueText; // 数値（35.3% など）

        /// <summary>数値テキストを設定する。</summary>
        public void SetValue(string value)
        {
            if (valueText != null)
                valueText.SetText(value);
        }

        /// <summary>見出しテキストを設定する（コードから変えたい場合のみ。通常はプレハブ側で設定）。</summary>
        public void SetLabel(string label)
        {
            if (labelText != null)
                labelText.SetText(label);
        }
    }
}
