
using System.Collections;
using UnityEngine;


namespace WeatherSystem {

	public partial class WeatherManager : MonoBehaviour {

		// Environment
	//	cRainEffect * pRainEffect							= NULL;
	//	cFogEffect * pFogEffect								= NULL;
	//	cSunEffect * pSunEffect								= NULL;

		private		float					m_DayTimeNow			= -1.0f;
		public		bool					IsDynamic
		{
			get; set;
		}

		private		EnvDescriptor[]			m_Descriptors			= null;

		// Descriptors
		private		EnvDescriptor			m_EnvDescriptorCurrent	= null;
		private		EnvDescriptor			m_EnvDescriptorNext		= null;
		private		EnvDescriptor			m_EnvDescriptorMixer	= null;

		// Global light
		private		Light					m_GlobalLight			= null;

		// Sky
		private		float					m_FadingFactor			= 0.0f;
		private		Material				m_SkyMaterial			= null;


		private		bool					m_IsOK					= false;



		/// //////////////////////////////////////////////////////////////////////////
		/// AWAKE
		private void	Awake()
		{
			string sStartTime = "09:00:00";	


			Section pSection = GLOBALS.Configs.GetSection( "Time" );

			if ( pSection != null )
				sStartTime = pSection.AsString( "StartTime", sStartTime );

			this.TansformTime( sStartTime, ref m_DayTimeNow );
		}

		/// //////////////////////////////////////////////////////////////////////////
		/// START
		private void	Start()
		{
		
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// Initialize
		private	bool	Initalize()
		{
			return false;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// ResetWeather
		private	void	ResetWeather()
		{

		}


		/// //////////////////////////////////////////////////////////////////////////
		/// LoadDescriptors
		private	bool	LoadDescriptors( string LevelWheater )
		{
			return false;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// Load
		private	void	Load( string LevelWheater )
		{

		}


	


		/// //////////////////////////////////////////////////////////////////////////
		/// UpdateSkies
		private	void	UpdateSkies()
		{

		}


		/// //////////////////////////////////////////////////////////////////////////
		/// TimeDiff
		private	float	TimeDiff( float prev, float cur)
		{
			return 0;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// TimeInterpolant
		private	float	TimeInterpolant( float val, float min_t, float max_t )
		{
			return 0;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SelectDescriptors
		private	void	SelectDescriptors( EnvDescriptor Current, EnvDescriptor next, float DayTime )
		{

		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SelectDescriptors
		private	void	SelectDescriptors( float DayTime )
		{

		}


		/// //////////////////////////////////////////////////////////////////////////
		/// TimeDiff
		private	void	EnvironmentLerp()
		{

		}


		/// //////////////////////////////////////////////////////////////////////////
		/// UNITY
		private void	Update()
		{
		
		}





















		/// //////////////////////////////////////////////////////////////////////////
		/// Utility
		bool TansformTime( string sTime, ref float Time )
		{
			int iH = 0, iM = 0, iS = 0;

			var parts = sTime.Split( ':' );
			iH = int.Parse( parts[0] );
			iM = int.Parse( parts[1] );
			iS = int.Parse( parts[2] );

			if ( IsValidTime( iH, iM, iS ) == false )
			{
				Utils.Msg.MSGCRT( "cWeatherManager::TansformTime:Incorrect weather time, %s", sTime );
				return false;
			}

			Time = ( float )( ( iH * 3600f ) + ( iM * 60f ) + iS );
			return true;
		}
		bool IsValidTime( float h, float m, float s )
		{
			return ( ( h >= 0 ) && ( h < 24 ) && ( m >= 0 ) && ( m < 60 ) && ( s >= 0 ) && ( s < 60 ) );
		}



	}

}