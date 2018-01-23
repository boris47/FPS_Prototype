﻿Shader "Custom/CG Reflective pixel lit texture" 
{ 
	Properties
	{
		_MainTex("Texture", 2D) = "white" { }
		_ReflectionMap("Reflection Map", Cube) = "" {}
		_LightDir("Light direction", Vector) = (1.0, 0.0, 0.0, 0.0)
		_LightColor("Main color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess("Shininess", Range(1, 128)) = 10
		_Reflectivity("Reflectivity", Range(0, 1)) = 0.3
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
			float4 _MainTex_ST; //Used to intract with Unity Editor

			samplerCUBE _ReflectionMap;
			float _Reflectivity;

			float3 _LightDir;
			float4 _LightColor;
			float _Shininess;

			struct AppData 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct VertexToFragment 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			};

			// vertex shader 
			VertexToFragment vert(AppData input)
			{
				VertexToFragment output;

				output.pos = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);

				output.normal = input.normal;
				output.viewDir = normalize(ObjSpaceViewDir(input.vertex));

				return output;
			}

			// fragment shader
			float4 frag(VertexToFragment input) : COLOR
			{
				float4 textureColor = tex2D(_MainTex, input.uv);

				float3 normal = normalize(input.normal);
				float3 viewDir = normalize(input.viewDir);
				float3 lightDir = normalize(_LightDir);
				float3 reflectedLightDir = reflect(-lightDir, input.normal);

				float diffuseComponent = max(0.0, dot(lightDir, normal));
				float specularComponent = max(0.0, dot(reflectedLightDir, viewDir));

				float3 lightColor = _LightColor * diffuseComponent;
				float3 specularColor = _LightColor.rgb * pow(specularComponent, _Shininess) * _LightColor.a;

				float3 reflectionVector = reflect(-viewDir, normal);
				float3 reflectionColor = texCUBE(_ReflectionMap, reflectionVector) * _Reflectivity;

				float4 finalColor;

				finalColor.rgb = (textureColor.rgb * lightColor) + specularColor + reflectionColor;
				finalColor.a = 1.0;

				return finalColor;
			}

			ENDCG // here ends the part in Cg 
		}
	}
}
