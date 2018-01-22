using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	public class EnvDescriptor : ScriptableObject {

		[HideInInspector]
		public	float					ExecTime			= 0.0f;
		public	string					Identifier			= "none";
		public	EnvDescriptor 			Next				= null;

		// Ambient Color
		public	Color					AmbientColor		= Color.clear;
		public	float					FogFactor			= 0.0f;

		// Sky
		public	Material				SkyMaterial			= null;
		public	Color					SkyColor			= Color.clear;

		// Gradient
		public	float					GradientRadius		= 0.5f;
		public	Material				GradientMaterial	= null;

		// Sun
		public	Color					SunColor			= Color.clear;
		public	Vector3					SunRotation			= Vector3.zero;
		public	Material				SunMaterial			= null;


		public	string					AssetPath			= string.Empty;

	}

}