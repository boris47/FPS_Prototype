
using System.Collections;
using UnityEngine;

namespace WeatherSystem {

	public interface IWeatherManagerInternal {

		float	DayTimeNow { set; }

		void StartSelectDescriptors( float DayTime );
//		void InternaUpdate();
	}

	[ExecuteInEditMode]
	public partial class WeatherManager : MonoBehaviour, IWeatherManagerInternal {

		public static WeatherManager		Instance				= null;

		public	const	float				DAY_LENGTH				= 86400f;

		public	const	string				WEATHERS_COLLECTION		= "Weather/Descriptors/WeatherCollection";
		public	const	string				SKYMIXER_MATERIAL		= "Weather/SkyMaterials/SkyMixer";

		public	static	bool				EditorLinked			= false;
		[SerializeField]
		private	bool						EnableInEditor			= false;

//		[Range(0,1)]
//		public	float	interpolante;
//		public	string CurrentDayTime = "";

		// Environment
	//	cRainEffect * pRainEffect							= NULL;
	//	cFogEffect * pFogEffect								= NULL;
	//	cSunEffect * pSunEffect								= NULL;

		public		bool					IsDynamic
		{
			get; set;
		}

		[SerializeField]
		private		float					m_DayTimeNow			= -1.0f;
		public		float					DayTime
		{
			get { return m_DayTimeNow; }
		}
		float IWeatherManagerInternal.DayTimeNow
		{
			set{ m_DayTimeNow = value; }
		}
//		[SerializeField]
		private		EnvDescriptor[]			m_Descriptors			= null;

		// Descriptors
//		[SerializeField]
		private		EnvDescriptor			m_EnvDescriptorCurrent	= null;
//		[SerializeField]
		private		EnvDescriptor			m_EnvDescriptorNext		= null;
//		[SerializeField]
		private		EnvDescriptor			m_EnvDescriptorMixer	= null;

		// Global light
//		[SerializeField]
		private		Light					m_GlobalLight			= null;

		// Sky
		public	Material					SkyMaterial
		{
			get;set; 	
		}

//		private		bool					m_IsOK					= false;



		/////////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void	Awake()
		{
			Instance = this;

			string sStartTime = "09:30:00";	

			if ( GLOBALS.Configs != null )
			{
				Section pSection = GLOBALS.Configs.GetSection( "Time" );

				if ( pSection != null )
					sStartTime = pSection.AsString( "StartTime", sStartTime );
			}

			m_GlobalLight = GameObject.Find( "Sun" ).GetComponent<Light>();

			TansformTime( sStartTime, ref m_DayTimeNow );

			m_EnvDescriptorMixer = new EnvDescriptor();
		}
		
		/////////////////////////////////////////////////////////////////////////////
		// START
		private void	Start()
		{
			SkyMaterial = Resources.Load<Material>( SKYMIXER_MATERIAL );
			if ( SkyMaterial == null )
			{
				print( "WeatherManager::Start: SkyMateria is null !!" );
				return;
			}

			Weathers allCycles = Resources.Load<Weathers>( WEATHERS_COLLECTION );
			if ( allCycles == null )
			{
				print( "WeatherManager::Start: allCycles is null !!" );
				return;
			}

			m_Descriptors = allCycles.Cycles[0].Descriptors;

			StartSelectDescriptors( m_DayTimeNow );
			EnvironmentLerp();

			IsDynamic = true;
//			m_IsOK = true;
		}

		/*
		/////////////////////////////////////////////////////////////////////////////
		// Initialize
		private	bool	Initalize()
		{
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		// ResetWeather
		private	void	ResetWeather()
		{

		}

		
		/// //////////////////////////////////////////////////////////////////////////
		/// LoadDescriptors
		private	bool	LoadDescriptors( string LevelWheater )
		{
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////
		// Load
		private	void	Load( string LevelWheater )
		{

		}
		*/

		/*
		/////////////////////////////////////////////////////////////////////////////
		// UpdateSkies
		private	void	UpdateSkies()
		{

		}
		*/

		/////////////////////////////////////////////////////////////////////////////
		// TimeDiff
		private	float	TimeDiff( float Current, float Next )
		{
			if ( Current > Next )
				return  ( (DAY_LENGTH - Current ) + Next );
			else
			{
				return ( Next - Current );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeInterpolant
		private	float	TimeInterpolant( float DayTime, float Current, float Next )
		{
			float fInterpolant = 0.0f;
			float fLength = TimeDiff( Current, Next );
			if ( Utils.Math.SimilarZero( fLength, Utils.Math.EPS ) == false ) {
				if ( Current > Next )
				{
					if ( ( DayTime >= Current ) || ( DayTime <= Next ) )
						fInterpolant = TimeDiff( Current, DayTime ) / fLength;
				}
				else
				{
					if ( ( DayTime >= Current ) && ( DayTime <= Next ) )
						fInterpolant = TimeDiff( Current, DayTime ) / fLength;
				}
				fInterpolant = Mathf.Clamp01( fInterpolant + 0.0001f );
			}
			return fInterpolant;
		}


		////////////////////////////////////////////////////////////////////////////
		// SetCubemaps
		private	void	SetCubemaps()
		{   
			SkyMaterial.SetTexture( "_Skybox1", m_EnvDescriptorCurrent.SkyCubemap );
			SkyMaterial.SetTexture( "_Skybox2", m_EnvDescriptorNext.SkyCubemap );
			SkyMaterial.SetFloat( "_Blend", 0.0f );
		}


		////////////////////////////////////////////////////////////////////////////
		// GetNextDescriptor
		private	EnvDescriptor	GetNextDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx + 1 ) == m_Descriptors.Length ? 0 : ( idx + 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// GetPreviousDescriptor
		private	EnvDescriptor	GetPreviousDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx ) == 0 ? m_Descriptors.Length - 1 : ( idx - 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// StartSelectDescriptors
		void IWeatherManagerInternal.StartSelectDescriptors( float DayTime )
		{
			StartSelectDescriptors( DayTime );
		}
		private	void	StartSelectDescriptors( float DayTime )
		{
			EnvDescriptor descriptor = System.Array.FindLast( m_Descriptors, ( EnvDescriptor d ) => d.ExecTime < DayTime );

			EnvDescriptor first = m_Descriptors[ 0 ];
			EnvDescriptor last  = m_Descriptors[ m_Descriptors.Length - 1 ];
			if ( descriptor == null )
			{
				m_EnvDescriptorCurrent	= last;
				m_EnvDescriptorNext		= first;
			}
			else
			{
				m_EnvDescriptorCurrent = ( descriptor == m_Descriptors[ 0 ] ) ? last : GetPreviousDescriptor( descriptor );
				m_EnvDescriptorNext = descriptor;
			}

			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// SelectDescriptors
		public void	SelectDescriptors( float DayTime )
		{
			bool bSelect = false;
			
			if ( m_EnvDescriptorCurrent.ExecTime > m_EnvDescriptorNext.ExecTime )
			{
				bSelect = ( DayTime > m_EnvDescriptorNext.ExecTime ) && ( DayTime < m_EnvDescriptorCurrent.ExecTime );
			}
			else
			{
				bSelect = ( DayTime > m_EnvDescriptorNext.ExecTime );
			}

			if ( bSelect )
			{
				m_EnvDescriptorCurrent = m_EnvDescriptorNext;
				EnvDescriptor next = GetNextDescriptor( m_EnvDescriptorNext );
				m_EnvDescriptorNext = ( next != null ) ? next : m_Descriptors[ 0 ];
			}
			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeDiff
		private	void	EnvironmentLerp()
		{
			float interpolant = TimeInterpolant( m_DayTimeNow, m_EnvDescriptorCurrent.ExecTime, m_EnvDescriptorNext.ExecTime );
			InterpolanteOthers( m_EnvDescriptorCurrent, m_EnvDescriptorNext, interpolant );
			SkyMaterial.SetFloat( "_Blend", interpolant );
		}


		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void	InterpolanteOthers( EnvDescriptor current, EnvDescriptor next, float interpolant )
		{
			m_EnvDescriptorMixer.AmbientColor		= Color.Lerp( current.AmbientColor, next.AmbientColor, interpolant );
			m_EnvDescriptorMixer.FogFactor			= Mathf.Lerp( current.FogFactor, next.FogFactor, interpolant );
			m_EnvDescriptorMixer.SkyColor			= Color.Lerp( current.SkyColor, next.SkyColor, interpolant );
			m_EnvDescriptorMixer.SunColor			= Color.Lerp( current.SunColor, next.SunColor, interpolant );
			m_EnvDescriptorMixer.SunRotation		= Vector3.Lerp( current.SunRotation, next.SunRotation, interpolant );
		}

		public string CurrentDayTime;
		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void	Update()
		{
			if ( Instance == null )
				Instance = this;

			if ( EnableInEditor == false )
				return;

			if ( IsDynamic == true )
			{
				if ( EditorLinked == false )
				{
					m_DayTimeNow += Time.deltaTime * 300f; // + Level()->GetTimeFactor();
				}

				if ( m_DayTimeNow > DAY_LENGTH )
					m_DayTimeNow = 0.0f;

					SelectDescriptors( m_DayTimeNow );
				EnvironmentLerp();
			}

			// Sun rotation by data
			m_GlobalLight.transform.rotation = Quaternion.LookRotation( m_EnvDescriptorMixer.SunRotation );


			TransformTime( m_DayTimeNow, ref CurrentDayTime );
		}
























		/// //////////////////////////////////////////////////////////////////////////
		/// Utility
		public	static	bool	TansformTime( string sTime, ref float Time )
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
		public	static	void	TransformTime( float fTime, ref string Time, bool considerSeconds = true )
		{
			int iH = ( int ) (   fTime / ( 3600f ) );
			int iM = ( int ) ( ( fTime / 60f ) % 60f );
			int iS = ( int ) ( fTime % 60f );
			Time = ( iH.ToString( "00" ) + ":" + iM.ToString( "00" ) );

			if ( considerSeconds )
				Time +=  ( ":" + iS.ToString( "00" ) );
//			print( Time );
		}
		public static bool IsValidTime( float h, float m, float s )
		{
			return ( ( h >= 0 ) && ( h < 24 ) && ( m >= 0 ) && ( m < 60 ) && ( s >= 0 ) && ( s < 60 ) );
		}



	}

}