﻿Shader "Custom/ShaderRGB"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Albedo (RGB)", 2D) = "white" {}
		_ColorOne("ColorOne", Color) = (1,1,1,1)
		_ColorTwo("ColorTwo", Color) = (1,1,1,1)
		_ColorThree("ColorThree", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags 
		{ 
			"IgnoreProjector" = "true" 
			"Queue" = "Transparent-100" 
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}
		ZWrite Off
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;

			float4 _MainTexColor;
			float4 _MaskTexColor;

			float4 finalColor;

			float4 _ColorOne : _ColorOne;
			float4 _ColorTwo : _ColorTwo;
			float4 _ColorThree : _ColorThree;

			fixed4 frag(v2f i) : SV_Target
			{
				_MainTexColor = tex2D(_MainTex, i.uv);
				_MaskTexColor = tex2D(_MaskTex, i.uv);
				finalColor = _MainTexColor;

				float u = _MaskTexColor.r;
				float v = _MaskTexColor.g;
				float w = _MaskTexColor.b;
				float x = 1 - u - v - w;

				finalColor *= _ColorOne * u + _ColorTwo * v + _ColorThree * w + float4(1,1,1,1) * x;

				clip(finalColor.a - 0.5f);
				return finalColor;
			}
			ENDCG
		}
	}
	Fallback "Custom/CutoutComplex"
}