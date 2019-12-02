

Shader "Custom/SkyMixerShader" {
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
}