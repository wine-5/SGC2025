using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SGC2025.Manager
{
    /// <summary>
    /// シーン遷移時の「緑化タイルめくり」演出を担当するシングルトン。
    /// 画面をグリッド状のタイルに分割し、中央から外へ向かって順にめくれて画面を覆う（Cover）／
    /// 覆った状態から中央→外へ順に消える（Uncover）。
    /// 緑のタイルで覆うことで、ゲームのコアである「緑化」を遷移演出に重ねている。
    ///
    /// UI はすべてコードで生成するためプレハブ不要。
    /// SceneController から LoadScene のたびに呼び出され、DontDestroyOnLoad で常駐する。
    /// Time.timeScale = 0（ポーズ中）でも動くよう unscaledDeltaTime を使う。
    /// </summary>
    public class SceneTransition : Singleton<SceneTransition>
    {
        protected override bool UseDontDestroyOnLoad => true;

        // --- 演出パラメータ（Manager シーンに配置すればインスペクターから調整可能。未配置時はここの初期値で動作）---
        [Header("グリッド")]
        [Tooltip("横方向のタイル分割数（縦はアスペクト比から自動算出）")]
        [SerializeField] private int columns = 16;
        [Tooltip("他のCanvasより手前に出すための描画順")]
        [SerializeField] private int sortingOrder = 32000;

        [Header("覆う（Cover）速度")]
        [Tooltip("1タイルの開アニメ時間（秒）")]
        [SerializeField] private float coverTileDuration = 0.22f;
        [Tooltip("中央→最外周までの広がり時間（秒）")]
        [SerializeField] private float coverSpreadTime = 0.45f;

        [Header("晴れる（Uncover）速度 ＝ 徐々に見えてくる演出")]
        [Tooltip("1タイルの閉アニメ時間（秒）。大きいほどゆっくり")]
        [SerializeField] private float uncoverTileDuration = 0.32f;
        [Tooltip("中央→最外周までの広がり時間（秒）。大きいほどゆっくり")]
        [SerializeField] private float uncoverSpreadTime = 0.85f;

        [Header("タイルの色（蝶の自然回復をイメージした2トーン）")]
        [SerializeField] private Color greenBase = new Color(0.30f, 0.69f, 0.31f); // #4CAF50系
        [SerializeField] private Color greenDeep = new Color(0.22f, 0.56f, 0.24f); // 一段濃い緑

        private RectTransform[] tiles;
        private float[] tileNorms;    // 各タイルの中央からの正規化距離（中央=0、最外周=1）。広がり順に使う
        private Image blocker;        // 遷移中の入力を遮断するフルスクリーンの透明Image
        private int builtWidth, builtHeight;

        private bool isTransitioning;

        /// <summary>遷移中かどうか（多重遷移の抑止に使う）。</summary>
        public bool IsTransitioning => isTransitioning;

        /// <summary>
        /// 「覆う → シーン読み込み → 晴れる」の一連を、この常駐オブジェクト自身のUniTaskで実行する。
        /// SceneController から呼び出される（fire-and-forget）。
        /// </summary>
        public void TransitionTo(string sceneName)
        {
            if (isTransitioning) return;
            // CancellationToken は破棄されない側（DontDestroyOnLoad な自分自身）に紐づける。
            // これによりシーンロードを跨いでも晴れる演出まで確実に走り切る。
            TransitionAsync(sceneName, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid TransitionAsync(string sceneName, CancellationToken ct)
        {
            isTransitioning = true;

            // 1. 中央→外へタイルで画面を覆う
            EnsureBuilt();
            blocker.enabled = true;
            await Animate(reveal: true, ct);

            // 2. 覆っている間にシーンを切り替える
            //    （ポーズ起因の timeScale=0 をここで確実に戻す）
            Time.timeScale = 1f;
            await SceneManager.LoadSceneAsync(sceneName).ToUniTask(cancellationToken: ct);

            // 3. 中央→外へ覆いを晴らし、次シーンを見せる（解像度変化にも追従）
            EnsureBuilt();
            await Animate(reveal: false, ct);
            blocker.enabled = false;

            isTransitioning = false;
        }

        /// <summary>
        /// reveal=true: scale 0→1（覆う） / reveal=false: scale 1→0（晴れる）。
        /// 各タイルは中央からの距離に応じた遅延つきで scale をアニメさせる。
        /// </summary>
        private async UniTask Animate(bool reveal, CancellationToken ct)
        {
            // Cover / Uncover で速度を分ける（Uncover ＝ 徐々に見えてくる演出を遅めに）
            float spread = reveal ? coverSpreadTime : uncoverSpreadTime;
            float tileDuration = reveal ? coverTileDuration : uncoverTileDuration;
            float total = spread + tileDuration;
            float elapsed = 0f;

            // 開始フレームの状態を確定
            // Cover: 全タイル scale0（クリア）から / Uncover: 全タイル scale1（完全に覆った状態）から始める。
            // どちらも elapsed=0 を渡せば、中央（遅延0）→外周の順でアニメする。
            ApplyState(0f, reveal, spread, tileDuration);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);

            while (elapsed < total)
            {
                elapsed += Time.unscaledDeltaTime;
                ApplyState(elapsed, reveal, spread, tileDuration);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            ApplyState(total, reveal, spread, tileDuration);
        }

        /// <summary>経過時間 elapsed に対する全タイルのスケールを反映する。</summary>
        private void ApplyState(float elapsed, bool reveal, float spread, float tileDuration)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                float delay = tileNorms[i] * spread;
                float local = Mathf.Clamp01((elapsed - delay) / tileDuration);
                float eased = EaseOutCubic(local);
                float scale = reveal ? eased : 1f - eased;
                tiles[i].localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static float EaseOutCubic(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }

        /// <summary>Canvasとタイルグリッドを（未生成または解像度変化時に）生成する。</summary>
        private void EnsureBuilt()
        {
            if (tiles != null && builtWidth == Screen.width && builtHeight == Screen.height)
                return;

            // 既存の子（前回ビルド分）を破棄してから作り直す
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            Build();
            builtWidth = Screen.width;
            builtHeight = Screen.height;
        }

        private void Build()
        {
            // --- Canvas ---
            var canvasGo = new GameObject("TransitionCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // --- 入力ブロッカー（透明・遷移中のみ有効）---
            var blockerGo = new GameObject("InputBlocker", typeof(Image));
            blockerGo.transform.SetParent(canvasGo.transform, false);
            var blockerRect = (RectTransform)blockerGo.transform;
            Stretch(blockerRect);
            blocker = blockerGo.GetComponent<Image>();
            blocker.color = new Color(0f, 0f, 0f, 0f);
            blocker.raycastTarget = true;
            blocker.enabled = false;

            // --- タイルグリッド ---
            int rows = Mathf.Max(1, Mathf.RoundToInt(columns * (float)Screen.height / Mathf.Max(1, Screen.width)));
            tiles = new RectTransform[columns * rows];
            tileNorms = new float[columns * rows];

            float centerC = (columns - 1) * 0.5f;
            float centerR = (rows - 1) * 0.5f;
            float maxDist = Mathf.Max(centerC, centerR); // チェビシェフ距離の最大値

            int index = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var tileGo = new GameObject($"Tile_{c}_{r}", typeof(Image));
                    tileGo.transform.SetParent(canvasGo.transform, false);

                    var rect = (RectTransform)tileGo.transform;
                    rect.anchorMin = new Vector2((float)c / columns, (float)r / rows);
                    rect.anchorMax = new Vector2((float)(c + 1) / columns, (float)(r + 1) / rows);
                    // セル境界の継ぎ目を防ぐためわずかに広げる
                    rect.offsetMin = new Vector2(-1f, -1f);
                    rect.offsetMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 0.5f); // 中央から拡縮
                    rect.localScale = Vector3.zero;

                    var img = tileGo.GetComponent<Image>();
                    img.color = ((c + r) & 1) == 0 ? greenBase : greenDeep; // 市松で2トーン
                    img.raycastTarget = false;

                    // 中央からの正規化距離（チェビシェフ, 0..1）で広がり順を決める。
                    // 実際の遅延時間は Animate 側で spread を掛けて算出する（Cover/Uncoverで速度可変）。
                    float dist = Mathf.Max(Mathf.Abs(c - centerC), Mathf.Abs(r - centerR));
                    float norm = maxDist > 0f ? dist / maxDist : 0f;

                    tiles[index] = rect;
                    tileNorms[index] = norm;
                    index++;
                }
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
