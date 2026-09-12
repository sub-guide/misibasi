Shader "UI/OiiaChromaKey"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _ChromaColor ("Chroma Key Color", Color) = (0, 1, 0, 1)
        _ChromaThreshold ("Threshold", Range(0, 1)) = 0.25
        _ChromaSoftness ("Softness", Range(0.001, 0.5)) = 0.12
        _DarkLumaCutoff ("Dark Luma Cutoff", Range(0, 0.5)) = 0.1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ChromaColor;
            float _ChromaThreshold;
            float _ChromaSoftness;
            float _DarkLumaCutoff;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1) 원본 영상 색으로 크로마키 (틴트 적용 전 — 초록 이펙트가 키잉에 걸리지 않음)
                half4 src = tex2D(_MainTex, i.texcoord);

                half luma = dot(src.rgb, half3(0.299h, 0.587h, 0.114h));
                half greenExcess = src.g - max(src.r, src.b);
                half3 chromaDiff = src.rgb - _ChromaColor.rgb;
                half chromaDist = length(chromaDiff);

                half greenBg = saturate(greenExcess / max(_ChromaSoftness, 0.001h));
                half distBg = 1.0h - saturate((chromaDist - _ChromaThreshold) / max(_ChromaSoftness, 0.001h));
                half darkBg = (1.0h - saturate(luma / max(_DarkLumaCutoff, 0.001h)))
                    * (1.0h - saturate((min(src.r, min(src.g, src.b)) - 0.35h) * 3.0h));

                half background = max(greenBg, max(distBg, darkBg));
                half visibility = 1.0h - background;
                visibility = smoothstep(0.04h, 0.2h, visibility);

                // 2) 남은 전경에만 Vertex Color 틴트
                half4 col;
                col.rgb = src.rgb * i.color.rgb;
                col.a = src.a * visibility * i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.02);
                #endif

                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
