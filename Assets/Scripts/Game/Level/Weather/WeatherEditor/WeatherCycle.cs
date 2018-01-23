using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class WeatherCycle : ScriptableObject {

		[SerializeField]
		public	string				WeatherName	= string.Empty;

		[SerializeField]
		public	string				AssetPath	= string.Empty;

		[SerializeField]
		public	string				FolderPath	= string.Empty;

		[SerializeField]
		public	List<EnvDescriptor> Descriptors = new List<EnvDescriptor>();

	}

}