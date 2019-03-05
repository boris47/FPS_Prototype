
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Rain {

	}


	public partial class WeatherManager {

		private	static	IWeatherManager_Rain	m_RainInstance	= null;
		public	static	IWeatherManager_Rain	Rain
		{
			get { return m_RainInstance; }
		}

	}

}