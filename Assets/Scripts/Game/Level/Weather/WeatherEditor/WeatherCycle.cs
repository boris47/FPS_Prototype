using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class WeatherCycle : ScriptableObject {

		[SerializeField][ReadOnly][HideInInspector]
		public	string				AssetPath	= string.Empty;

		[SerializeField]
		public	EnvDescriptor[]		Descriptors	= new EnvDescriptor[ 24 ];

	}

}