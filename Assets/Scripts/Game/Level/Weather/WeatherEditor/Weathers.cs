using UnityEngine;
using System.Collections.Generic;



namespace WeatherSystem {

	[System.Serializable]
	public class Weathers : ScriptableObject {

		[SerializeField]
		public List<WeatherCycle> Cycles = new List<WeatherCycle>();

	}

}
