Shader "Custom/RopeShaderDoubleSided" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0,2)) = 1.0 // Added Normal Scale property
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _OcclusionStrength("Occlusion Strength", Range(0,1)) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // --- Disable back-face culling ---
        Cull off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap; // Usually shares UVs with MainTex, but can be separate
            float2 uv_OcclusionMap; // Usually shares UVs with MainTex
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _BumpScale; // Added Normal Scale variable
        half _OcclusionStrength;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // #pragma multi_compile_instancing // Uncomment if you need GPU Instancing

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // Metallic and Smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // Normal map with scaling
            fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)); // Use uv_BumpMap assuming same UVs
            // Apply scale: Blend between flat normal (0,0,1) and the sampled normal
            // A common way to scale normals in tangent space:
            normal.xy *= _BumpScale;
            normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy))); // Recalculate Z component
            o.Normal = normal;

            // Occlusion map
            half occ = tex2D(_OcclusionMap, IN.uv_OcclusionMap).g; // Often occlusion is stored in the Green channel
            o.Occlusion = lerp(1, occ, _OcclusionStrength); // Apply strength

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}