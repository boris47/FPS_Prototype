namespace WeatherSystem
{

	// Public interface
	public partial interface IWeatherManager_Rain {

	}


	public sealed partial class WeatherManager {

		private	static	IWeatherManager_Rain	m_RainInstance	= null;
		public	static	IWeatherManager_Rain	Rain
		{
			get { return m_RainInstance; }
		}

	}

}