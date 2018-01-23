Shader "Custom/CG unlit masked texture" 
{ 
	Properties
	{
		_MainTex("Texture", 2D) = "white" { }
		_SecondaryTex("Secondary texture", 2D) = "white" { }
		_Mask("Blend mask", 2D) = "black" { }
	}

	SubShader // Unity chooses the subshader that fits the GPU best
	{
		Pass // some shaders require multiple passes
		{
			CGPROGRAM // here begins the part in Unity's Cg

			#pragma vertex vert 
			#pragma fragment frag

			#include "UnityCG.cginc" //Defines Unity standard shader functions

			sampler2D _MainTex;
			sampler2D _SecondaryTex;
			sampler2D _Mask;

			float4 _MainTex_ST;
			float4 _SecondaryTex_ST;
			float4 _Mask_ST;

			struct AppData 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct VertexToFragment 
			{
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
			};

			// vertex shader 
			VertexToFragment vert(AppData input)
			{
				VertexToFragment output;

				output.pos = UnityObjectToClipPos(input.vertex);

				output.uv0 = TRANSFORM_TEX(input.uv, _MainTex);
				output.uv1 = TRANSFORM_TEX(input.uv, _SecondaryTex);
				output.uv2 = TRANSFORM_TEX(input.uv, _Mask);

				return output;
			}

			// fragment shader
			float4 frag(VertexToFragment input) : COLOR
			{
				float4 mainTexCol = tex2D(_MainTex, input.uv0);
				float4 secondaryTexCol = tex2D(_SecondaryTex, input.uv1);
				
				float blendAmount = tex2D(_Mask, input.uv2).r;

				float4 color = lerp(mainTexCol, secondaryTexCol, blendAmount);

				return color;
			}

			ENDCG // here ends the part in Cg 
		}
	}
}
