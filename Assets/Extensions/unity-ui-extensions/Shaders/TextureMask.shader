// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/FastMask"
{
	Properties
	{
		[PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
		_waveTex ("Sprite1 Texture", 2D) = "white" {}
		_XOffSetAnimation ("XOffSetAnimation",Float) = 0
		_YOffSetAnimation ("XOffSetAnimation",Float) = 0
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
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _waveTex;
			float4 _waveTex_ST;

			float _XOffSetAnimation;
			float _YOffSetAnimation;
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord2 : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				half2 texcoord2  : TEXCOORD2;
				float4 worldPosition : TEXCOORD1;
				
			};
			
			fixed4 _TextureSampleAdd;
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1);
				#endif
				OUT.texcoord2 = TRANSFORM_TEX(IN.texcoord2,_waveTex);
				OUT.texcoord2.x+=(_Time.x * _XOffSetAnimation );
				OUT.texcoord2.y+=(_Time.x * _YOffSetAnimation );
				OUT.color = IN.color;
				return OUT;
			}

			
			fixed4 frag(v2f IN) : SV_Target
			{
				return ((tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color) * (tex2D(_waveTex, IN.texcoord2));
			}
		ENDCG
		}
	}
}