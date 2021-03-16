using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	[System.Serializable]
	public class EnvDescriptor : ScriptableObject {
		
		[SerializeField][ReadOnly]//[HideInInspector]
		public	string					AssetPath			= string.Empty;

		[SerializeField][ReadOnly]//[HideInInspector]
		public	string					Identifier			= "none";
		[SerializeField][ReadOnly]
		public	float					ExecTime			= 0.0f;

		// Ambient Color
		[SerializeField]
		public	Color					AmbientColor		= Color.clear;
		[SerializeField]
		public	AudioCollection			AmbientEffects		= null;
		[SerializeField]
		public	float					FogFactor			= 0.0f;
		[SerializeField]
		public	float					RainIntensity		= 0.0f;

		// Sky
		[SerializeField]
		public	Cubemap					SkyCubemap			= null;
		[SerializeField]
		public	Color					SkyColor			= Color.clear;
		/*
		// Gradient
		[SerializeField]
		public	float					GradientRadius		= 0.5f;
		[SerializeField]
		public	Material				GradientMaterial	= null;
		*/
		// Sun
		[SerializeField]
		public	Color					SunColor			= Color.clear;
		[SerializeField]
		public	Vector3					SunRotation			= Vector3.zero;

		[SerializeField]
		public bool						IsSet					= false;


		//////////////////////////////////////////////////////////////////////////
		public	static	EnvDescriptor	Copy ( ref EnvDescriptor A, EnvDescriptor B, bool DeepCopy = false )
		{
			A.AmbientColor	= B.AmbientColor;
			A.FogFactor		= B.FogFactor;
			A.SkyColor		= B.SkyColor;
			A.SunColor		= B.SunColor;

			if ( DeepCopy )
			{
				A.Identifier		= B.Identifier;
				A.ExecTime			= B.ExecTime;
				A.AmbientEffects	= B.AmbientEffects;
				A.RainIntensity		= B.RainIntensity;
				A.SkyCubemap		= B.SkyCubemap;
				A.SunRotation		= B.SunRotation;
			}
			return A;
		}
	


	}

	[System.Serializable]
	public struct EnvDescriptorMixer {

		// Ambient Color
		public	Color					AmbientColor;
		public	float					FogFactor;
		public	float					RainIntensity;

		// Sky
		public	Color					SkyColor;
		
		public	Color					SunColor;
		public	Vector3					SunRotation;
	};
}