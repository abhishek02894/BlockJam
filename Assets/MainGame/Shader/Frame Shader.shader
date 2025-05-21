Shader "Custom/Frame Shader" {
    Properties {
        _Color ("Base Color", Color) = (1,1,1,1)
        _PatternMap ("Pattern Texture", 2D) = "white" {}
        _PatternOpacity ("Pattern Opacity", Range(0,1)) = 1
        _PatternRotation ("Pattern Rotation", Range(0,360)) = 0
        [Toggle]_UseScreenSpace ("Pattern Screen Space", Float) = 0
        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _AOOpacity ("AO Opacity", Range(0,1)) = 1
        _LightDirection1 ("Light Direction 1", Vector) = (0,1,0,0)
        _LightIntensity1 ("Light Intensity 1", Range(0,10)) = 1
        _LightDirection2 ("Light Direction 2", Vector) = (0,1,0,0)
        _LightIntensity2 ("Light Intensity 2", Range(0,10)) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            sampler2D _PatternMap;
            float _PatternOpacity;
            float _PatternRotation;
            float _UseScreenSpace;
            sampler2D _OcclusionMap;
            float _AOOpacity;
            float4 _LightDirection1;
            float _LightIntensity1;
            float4 _LightDirection2;
            float _LightIntensity2;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 pos : SV_POSITION;
                float4 screenPos : TEXCOORD2;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // Determine pattern UV (object-uv or screen-space uv)
                float2 uvPat;
                if (_UseScreenSpace > 0.5) {
                    uvPat = i.screenPos.xy / i.screenPos.w;
                } else {
                    uvPat = i.uv;
                }
                // Rotate UV around center
                float rad = radians(_PatternRotation);
                float2 center = float2(0.5, 0.5);
                float2 dir = uvPat - center;
                float cs = cos(rad);
                float sn = sin(rad);
                float2 rotated = float2(dir.x * cs - dir.y * sn, dir.x * sn + dir.y * cs) + center;

                // Sample pattern and occlusion
                fixed4 patt = tex2D(_PatternMap, rotated);
                fixed occ = tex2D(_OcclusionMap, i.uv).r;

                // Base color
                fixed3 baseCol = _Color.rgb;

                // Blend pattern and AO by their opacities
                fixed3 patternBlend = lerp(fixed3(1,1,1), patt.rgb, _PatternOpacity);
                fixed aoBlend = lerp(1.0, occ, _AOOpacity);

                // Normalize normal
                float3 N = normalize(i.worldNormal);

                // First custom light
                float3 L1 = normalize(_LightDirection1.xyz);
                float NdotL1 = saturate(dot(N, L1));
                fixed3 lightCol1 = NdotL1 * _LightIntensity1;

                // Second custom light
                float3 L2 = normalize(_LightDirection2.xyz);
                float NdotL2 = saturate(dot(N, L2));
                fixed3 lightCol2 = NdotL2 * _LightIntensity2;

                // Combine lighting
                fixed3 totalLight = lightCol1 + lightCol2;

                // Final color composition
                fixed3 final = baseCol * patternBlend * aoBlend * totalLight;
                return fixed4(final, _Color.a);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
