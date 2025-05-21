Shader "Custom/Block Shader"
{
    Properties
    {
        _BaseColor           ("Base Color",          Color) = (1,1,1,1)
        _Scale               ("Texture Scale",       Float)      = 1.0
        _Rotation            ("UV Rotation (°)",     Range(0,360)) = 0.0
        _NormalMap           ("Normal Map",          2D)    = "bump" {}
        _NormalStrength      ("Normal Strength",     Range(0, 2)) = 1.0
        _OcclusionMap        ("Occlusion (R)",       2D)    = "white" {}
        _AOColor             ("AO Color",            Color)      = (0,0,0,1)
        _AO_Strength         ("AO Strength",         Range(0,1)) = 1.0
        _AOPower             ("AO Power",            Range(0.01, 5.0)) = 1.0
        _AOPowerStrength     ("AO Power Strength",   Range(0,1)) = 1.0
        _RoughnessMap        ("Roughness (R)",       2D)    = "white" {}
        _Smoothness          ("Base Smoothness",     Range(0,1)) = 0.5
        _RoughnessStrength   ("Roughness Strength",  Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _NormalMap;
        sampler2D _OcclusionMap;
        sampler2D _RoughnessMap;
        fixed4    _BaseColor;
        fixed4    _AOColor;
        half      _Smoothness;
        half      _AO_Strength;
        half      _RoughnessStrength;
        half      _Scale;
        half      _AOPower;
        half      _AOPowerStrength;
        half      _NormalStrength;
        half      _Rotation;        // new

        struct Input
        {
            float2 uv_NormalMap;
            float2 uv_OcclusionMap;
            float2 uv_RoughnessMap;
        };

        // helper to rotate UV around center (0.5,0.5)
        float2 RotateUV(float2 uv, float radians)
        {
            float s = sin(radians);
            float c = cos(radians);
            uv -= 0.5;
            float2 rotated;
            rotated.x = uv.x * c - uv.y * s;
            rotated.y = uv.x * s + uv.y * c;
            return rotated + 0.5;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Tiled UVs
            float2 uvNorm  = IN.uv_NormalMap    * _Scale;
            float2 uvAO    = IN.uv_OcclusionMap * _Scale;
            float2 uvRough = IN.uv_RoughnessMap * _Scale;

            // Rotate UVs by _Rotation degrees
            float ang = radians(_Rotation);
            uvNorm  = RotateUV(uvNorm,  ang);
            uvAO    = RotateUV(uvAO,    ang);
            uvRough = RotateUV(uvRough, ang);

            // Base Color
            o.Albedo = _BaseColor.rgb;

            // Smoothness ← from roughness map
            float mapRough = tex2D(_RoughnessMap, uvRough).r;
            float smoothMap = 1.0 - mapRough;
            o.Smoothness = lerp(_Smoothness, smoothMap, _RoughnessStrength);

            // Normal mapping + strength
            float3 normalTex = UnpackNormal(tex2D(_NormalMap, uvNorm));
            normalTex = normalize(lerp(float3(0,0,1), normalTex, _NormalStrength));
            o.Normal = normalTex;

            // Ambient Occlusion
            float aoTex = tex2D(_OcclusionMap, uvAO).r;
            float appliedPower = lerp(1.0, _AOPower, _AOPowerStrength);
            float aoPowered = pow(aoTex, appliedPower);
            float aoFinal = saturate(aoPowered * _AO_Strength);
            o.Occlusion = aoFinal;

            // Invert the AO mask so deep AO areas light up with _AOColor
            float aoMask = 1.0 - aoFinal;
            o.Emission = _AOColor.rgb * aoMask;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
