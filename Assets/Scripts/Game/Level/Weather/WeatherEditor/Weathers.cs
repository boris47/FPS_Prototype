using UnityEngine;
using System.Collections.Generic;



namespace WeatherSystem {

	[System.Serializable]
	public class Weathers : ScriptableObject {

		[SerializeField][HideInInspector]
		public List<WeatherCycle> Cycles = new List<WeatherCycle>();

	}

}
