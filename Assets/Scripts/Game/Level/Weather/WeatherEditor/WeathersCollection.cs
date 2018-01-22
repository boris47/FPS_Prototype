using UnityEngine;
using System.Collections.Generic;



namespace WeatherSystem {

	[System.Serializable]
	public class WeathersCollection : ScriptableObject {

		public List<EnvDescriptorsCollection> Weathers = null;

		private void OnEnable()
		{
			if ( Weathers == null )
				Weathers = new List<EnvDescriptorsCollection>();
		}

	}

}
