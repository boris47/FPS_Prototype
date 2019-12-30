// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Custom/SkyMixerShader" {

	Properties {
		_Tint( "Tint Color", Color ) = ( 0.0, 0.0, 0.0, 0.0 )
		_Interpolant( "Interpolant", Range( 0.0, 1.0 ) ) = 0.0
		[Gamma] _Exposure ("Exposure", Range(0.01, 2)) = 1.0
		_Skybox1( "Skybox one", Cube ) = ""
		_Skybox2( "Skybox two", Cube ) = ""
	}

	SubShader {

		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Fog { Mode Off }
		Lighting Off
		Color [_Tint]
      
		Pass {
          
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			half4 _Tint;
			float _Interpolant;
			half _Exposure;
			samplerCUBE _Skybox1;
			samplerCUBE _Skybox2;

			half4 _Tex_HDR;

			struct appdata_t {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
             };

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.texcoord = v.vertex;
                 return o;
             }

			fixed4 frag (v2f i) : SV_Target
			{
				float4 env1 = texCUBE (_Skybox1, i.texcoord);
				float4 env2 = texCUBE (_Skybox2, i.texcoord);
				float4 env = lerp( env1, env2, _Interpolant );
				const half3 c = env.rgb * _Tint.rgb * unity_ColorSpaceDouble * _Exposure;
				return half4(c, _Tint.a);
			//	return env * _Tint.rgb * unity_ColorSpaceDouble * _Tint.a;
//				return env;
			}
			ENDCG
		}
	}
	Fallback "Skybox/Cubemap", 1
 

	/*
	Properties
	{
		_Tint( "Tint Color", Color ) = ( 0.0, 0.0, 0.0, 0.0 )
		_Blend( "Interpolant", Range( 0.0, 1.0 ) ) = 0.0
		_Skybox1( "Skybox one", Cube ) = ""
		_Skybox2( "Skybox two", Cube ) = ""
	}

	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off
		ZWrite Off
		Fog { Mode Off }
		Lighting Off
		Color [_Tint]

		Pass
		{
			SetTexture [_Skybox1] { combine texture }
			SetTexture [_Skybox2] { constantColor( 0, 0, 0, [_Blend] ) combine texture lerp( constant ) previous }
			SetTexture [_Skybox2] { combine previous + -primary, previous * primary }
		}
	}

	Fallback "Skybox/Cubemap", 1
	*/
}


// https://answers.unity.com/questions/616078/shader-blending-2-cubemaps.html
/*
 Shader "Skybox/CubemapSkyboxBlend" {
     Properties {
         _Tint ("Tint Color", Color) = (.5, .5, .5, 1)
         _Tint2 ("Tint Color 2", Color) = (.5, .5, .5, 1)
         [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
         _Rotation ("Rotation", Range(0, 360)) = 0
         _BlendCubemaps ("Blend Cubemaps", Range(0, 1)) = 0.5
         [NoScaleOffset] _Tex ("Cubemap (HDR)", Cube) = "grey" {}
         [NoScaleOffset] _Tex2 ("Cubemap (HDR) 2", Cube) = "grey" {}
     }
     SubShader {
         Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
         Cull Off ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha
      
         Pass {
          
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
      
             #include "UnityCG.cginc"
      
             samplerCUBE _Tex;
             samplerCUBE _Tex2;
             float _BlendCubemaps;
             half4 _Tex_HDR;
             half4 _Tint;
             half4 _Tint2;
             half _Exposure;
             float _Rotation;
      
             float4 RotateAroundYInDegrees (float4 vertex, float degrees)
             {
                 float alpha = degrees * UNITY_PI / 180.0;
                 float sina, cosa;
                 sincos(alpha, sina, cosa);
                 float2x2 m = float2x2(cosa, -sina, sina, cosa);
                 return float4(mul(m, vertex.xz), vertex.yw).xzyw;
             }
          
             struct appdata_t {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
             };
      
             struct v2f {
                 float4 vertex : SV_POSITION;
                 float3 texcoord : TEXCOORD0;
             };
      
             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = mul(UNITY_MATRIX_MVP, RotateAroundYInDegrees(v.vertex, _Rotation));
                 o.texcoord = v.vertex;
                 return o;
             }
      
             fixed4 frag (v2f i) : SV_Target
             {
                 float4 env1 = texCUBE (_Tex, i.texcoord);
                 float4 env2 = texCUBE (_Tex2, i.texcoord);
                 float4 env = lerp( env2, env1, _BlendCubemaps );
                 float4 tint = lerp( _Tint, _Tint2, _BlendCubemaps );
                 half3 c = DecodeHDR (env, _Tex_HDR);
                 c = c * tint.rgb * unity_ColorSpaceDouble;
                 c *= _Exposure;
                 return half4(c, tint.a);
             }
             ENDCG
         }
     }
     Fallback Off
 }
*/