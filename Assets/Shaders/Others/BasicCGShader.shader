Shader "Custom/Cg basic shader" 
{ 
	Properties
	{
		_MainColor("Main color", Color) = (1.0, 1.0, 1.0, 1.0) //RGBA
	}

	SubShader // Unity chooses the subshader that fits the GPU best
	{
		Pass // some shaders require multiple passes
		{
			CGPROGRAM // here begins the part in Unity's Cg

			#pragma vertex vert 
			#pragma fragment frag

			float4 _MainColor;
		
			struct AppData 
			{
				float4 vertex : POSITION;
			};

			struct VertexToFragment 
			{
				float4 pos : SV_POSITION;
			};

			// vertex shader 
			VertexToFragment vert(AppData input)
			{
				VertexToFragment output;

				//output.pos = mul(UNITY_MATRIX_MV, input.vertex); //Model View transformation
				//output.pos = mul(UNITY_MATRIX_P, output.pos); //Projection transformation

				//output.pos = mul(UNITY_MATRIX_MVP, input.vertex); //Model * View * Projection
				output.pos = UnityObjectToClipPos(input.vertex);

				return output;
			}

			// fragment shader
			float4 frag(VertexToFragment input) : COLOR
			{
				return _MainColor;
			}

			ENDCG // here ends the part in Cg 
		}
	}
}
