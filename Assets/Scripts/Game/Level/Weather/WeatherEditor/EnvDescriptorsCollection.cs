using UnityEngine;
using System.Collections.Generic;

namespace WeatherSystem {

	[System.Serializable]
	public class EnvDescriptorsCollection : ScriptableObject {


		public	string				WeatherName	= string.Empty;
		public	List<EnvDescriptor> Descriptors = null;

		public string				AssetPath	= string.Empty;

		private void OnEnable()
		{
			if ( Descriptors == null )
				Descriptors = new List<EnvDescriptor>();
		}

	}

}