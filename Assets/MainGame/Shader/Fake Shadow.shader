Shader "Custom/Unlit/Fake Shadow"
{
    Properties
    {
        _MainTex   ("Albedo (RGB)",     2D)   = "white" {}
        _AlphaMap  ("Alpha Map (B&W)",  2D)   = "white" {}
        _TintColor ("Tint Color",       Color) = (1,1,1,1)
        _Opacity   ("Opacity",          Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Cull Back       // cull back‐faces
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Cull Back   // ensure pass‐level back‐face culling

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma cull back
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _AlphaMap;
            float4   _TintColor;
            float    _Opacity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Base color tinted
                fixed4 col = tex2D(_MainTex, i.uv) * _TintColor;

                // Alpha from map (invert if desired)
                fixed alphaMap = tex2D(_AlphaMap, i.uv).r;
                col.a = (1 - alphaMap) * _Opacity;

                return col;
            }
            ENDCG
        }
    }
}
