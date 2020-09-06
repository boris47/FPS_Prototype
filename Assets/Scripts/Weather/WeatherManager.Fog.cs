
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Fog {

	}


	public sealed partial class WeatherManager {

		private	static	IWeatherManager_Fog		m_FogInstance	= null;
		public	static	IWeatherManager_Fog		Fog
		{
			get { return m_FogInstance; }
		}

	}

}