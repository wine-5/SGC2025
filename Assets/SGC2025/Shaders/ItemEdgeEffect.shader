Shader "SGC2025/ItemEdgeEffect"
{
    // 画面全面のUI(Image/RawImage)に貼り、_Progress(0→1) に応じて
    // 上端中央から左右の縁をコメット状の光が下端まで走る演出。
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _Progress ("Progress (0-1)", Range(0, 1)) = 0
        _Thickness ("Border Thickness", Range(0.0, 0.2)) = 0.03
        _Trail ("Trail Length", Range(0.01, 1.0)) = 0.25
        _Glow ("Head Glow", Range(0, 3)) = 1.5
        _Intensity ("Intensity", Range(0, 3)) = 1.0
        _EdgeFeather ("Edge Feather", Range(0.0, 0.05)) = 0.01

        // UGUI 標準（マスク等のため）
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One        // 加算合成で光らせる（通常合成にしたい場合は One → OneMinusSrcAlpha）
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float4 color     : COLOR;
                float2 uv        : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            fixed4 _Color;
            float _Progress;
            float _Thickness;
            float _Trail;
            float _Glow;
            float _Intensity;
            float _EdgeFeather;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            // エッジへの近さ(距離d)を 1=縁ちょうど 〜 0=帯の内縁 のマスクに変換
            float EdgeMask(float d, float th, float feather)
            {
                return 1.0 - smoothstep(th - feather, th, d);
            }

            // 候補(パスパラメータ・マスク)を、より縁に近い方で採用
            void Consider(float mask, float param, inout float bestMask, inout float bestParam)
            {
                if (mask > bestMask)
                {
                    bestMask = mask;
                    bestParam = param;
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // スプライトのUVに依存せず、画面座標(0-1)を使う
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float th = _Thickness;
                float ft = _EdgeFeather;

                // 各端までの距離
                float dTop    = 1.0 - uv.y;
                float dBottom = uv.y;
                float dRight  = 1.0 - uv.x;
                float dLeft   = uv.x;

                // 片側のパス長 = 上半分(0.5) + 縦(1.0) + 下半分(0.5) = 2.0
                const float HALF  = 0.5;
                const float SIDE  = 1.0;
                const float TOTAL = 2.0;

                float bestMask = 0.0;
                float bestParam = -1.0;

                // ---- 右パス（上端中央→右→下端中央） ----
                if (uv.x >= 0.5)
                {
                    // 上端(右半分)
                    Consider(EdgeMask(dTop, th, ft), (uv.x - 0.5) / TOTAL, bestMask, bestParam);
                    // 下端(右半分)
                    Consider(EdgeMask(dBottom, th, ft), (HALF + SIDE + (1.0 - uv.x)) / TOTAL, bestMask, bestParam);
                }
                // 右端（縦）
                Consider(EdgeMask(dRight, th, ft), (HALF + (1.0 - uv.y) * SIDE) / TOTAL, bestMask, bestParam);

                // ---- 左パス（上端中央→左→下端中央） ----
                if (uv.x <= 0.5)
                {
                    // 上端(左半分)
                    Consider(EdgeMask(dTop, th, ft), (0.5 - uv.x) / TOTAL, bestMask, bestParam);
                    // 下端(左半分)
                    Consider(EdgeMask(dBottom, th, ft), (HALF + SIDE + uv.x) / TOTAL, bestMask, bestParam);
                }
                // 左端（縦）
                Consider(EdgeMask(dLeft, th, ft), (HALF + (1.0 - uv.y) * SIDE) / TOTAL, bestMask, bestParam);

                if (bestParam < 0.0)
                    return fixed4(0, 0, 0, 0);

                // ヘッドが _Progress=1 で末尾まで抜けきるよう、Trail分だけ余分に進める
                float head = _Progress * (1.0 + _Trail);

                // ヘッドより後方(Trail内)を光らせる＝コメットの尾
                float d = head - bestParam;                 // 後方で正
                float behind = step(0.0, d);
                float trailI = saturate(1.0 - d / max(_Trail, 1e-4)) * behind;
                // ヘッド付近を強く
                float headGlow = smoothstep(_Trail, 0.0, d) * behind * _Glow;

                float intensity = (trailI + headGlow) * bestMask * _Intensity;

                // 不透明度は光の強さから決める（_Color.a には依存しない＝色のアルファが0でも出る）
                fixed4 col;
                col.rgb = _Color.rgb * i.color.rgb;
                col.a = saturate(intensity);
                return col;
            }
            ENDCG
        }
    }
}
