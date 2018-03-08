using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	[System.Serializable]
	public class EnvDescriptor/*: ScriptableObject*/ {

		[SerializeField][HideInInspector]
		public	string					Identifier			= "none";
		[SerializeField]
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
		public bool						set					= false;


		public	void	Copy ( EnvDescriptor other )
		{
			AmbientColor	= other.AmbientColor;
			FogFactor		= other.FogFactor;
			SkyColor		= other.SkyColor;
			SunColor		= other.SunColor;
		}
	}

}