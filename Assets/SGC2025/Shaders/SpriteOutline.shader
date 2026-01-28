Shader "Sprites/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 1
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            fixed4 _EmissionColor;
            float _EmissionIntensity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord);
                c.rgb *= c.a;
                
                // アウトライン検出（8方向サンプリング）
                float outline = 0;
                float2 size = _OutlineSize * _MainTex_TexelSize.xy;
                
                outline += tex2D(_MainTex, i.texcoord + float2(size.x, 0)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(-size.x, 0)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(0, size.y)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(0, -size.y)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(size.x, size.y)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(-size.x, -size.y)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(size.x, -size.y)).a;
                outline += tex2D(_MainTex, i.texcoord + float2(-size.x, size.y)).a;
                
                outline = min(outline, 1.0);
                
                fixed4 outlineC = _OutlineColor;
                outlineC.a *= outline;
                
                // Add emission effect
                fixed3 emission = _EmissionColor.rgb * _EmissionIntensity * outline;
                outlineC.rgb += emission;
                
                outlineC.rgb *= outlineC.a;
                
                // アウトラインとスプライトを合成
                return lerp(outlineC, c * i.color, c.a);
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}
