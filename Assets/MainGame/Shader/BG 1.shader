Shader "Custom/BackgroundVignette"
{
    Properties
    {
        _InnerColor  ("Center Color", Color) = (0.76,0.51,0.34,1)  // light wood tone
        _OuterColor  ("Edge Color",   Color) = (0.40,0.24,0.13,1)  // darker rim tone
        _Radius      ("Radius",       Range(0,1)) = 0.5          // where darkening starts
        _Softness    ("Softness",     Range(0,1)) = 0.4          // transition smoothness
        _Center      ("Center Offset", Vector) = (0.5,0.5,0,0)   // vignette center in UV
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float2 uv      : TEXCOORD0;
                float4 pos     : SV_POSITION;
            };

            fixed4 _InnerColor;
            fixed4 _OuterColor;
            float   _Radius;
            float   _Softness;
            float4  _Center;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // compute distance from center in UV space
                float2 diff = i.uv - _Center.xy;
                float  d    = length(diff);

                // smooth transition from inner to outer
                float  t    = smoothstep(_Radius, _Radius + _Softness, d);

                // lerp between center and edge colors
                return lerp(_InnerColor, _OuterColor, t);
            }
            ENDCG
        }
    }
}
