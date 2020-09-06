
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Wind {

	}


	public sealed partial class WeatherManager {

		private	static	IWeatherManager_Wind	m_WindInstance	= null;
		public	static	IWeatherManager_Wind	Wind
		{
			get { return m_WindInstance; }
		}

	}

}