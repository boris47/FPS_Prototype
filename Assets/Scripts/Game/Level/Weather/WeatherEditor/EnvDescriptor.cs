using UnityEngine;
using System.Collections;

namespace WeatherSystem {

	[System.Serializable]
	public class EnvDescriptor : ScriptableObject {

		[HideInInspector][SerializeField]
		public	float					ExecTime			= 0.0f;
		[SerializeField]
		public	string					Identifier			= "none";
		[SerializeField]
		public	EnvDescriptor 			Next				= null;

		// Ambient Color
		[SerializeField]
		public	Color					AmbientColor		= Color.clear;
		[SerializeField]
		public	float					FogFactor			= 0.0f;

		// Sky
		[SerializeField]
		public	Material				SkyMaterial			= null;
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
//		[SerializeField]
//		public	Material				SunMaterial			= null;

		[SerializeField]
		public	string					AssetPath			= string.Empty;

	}

}