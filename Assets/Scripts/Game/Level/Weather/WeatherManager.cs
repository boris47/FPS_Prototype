
using System.Collections;
using UnityEngine;

namespace WeatherSystem {

	public interface IWeatherManagerInternal {

		float	DayTimeNow { set; }
		EnvDescriptor	CurrentDescriptor { get; }

		void StartSelectDescriptors( float DayTime, WeatherCycle cycle );
		void Start( WeatherCycle cycle, float choiseFactor );
	}

	[ExecuteInEditMode]
	public partial class WeatherManager : MonoBehaviour, IWeatherManagerInternal {


		// STATIC
		public static IWeatherManager		Instance				= null;

		// Editor Stuf
		public	static	bool				EditorLinked			= false;
		public	static	bool				EditorDescriptorLinked	= false;


		// CONST
		public	const	float				DAY_LENGTH				= 86400f;
		private	const	string				WEATHERS_COLLECTION		= "Weather/Descriptors/WeatherCollection";
		private	const	string				SKYMIXER_MATERIAL		= "Weather/SkyMaterials/SkyMixer";


		// SERIALIZED
		[SerializeField]
		private	bool						EnableInEditor			= false;

		[SerializeField][Range( 1f, 500f )]
		private	float						m_TimeFactor			= 1.0f;
		public	float						TimeFactor
		{
			get { return m_TimeFactor; }
			set { m_TimeFactor = value; }
		}

		[Header("Weather Info")]
		[ReadOnly]
		public	string						CurrentDayTime			= string.Empty;

		[ SerializeField ][ReadOnly]
		private		string					m_CurrentCycleName		= string.Empty;

		[SerializeField][ReadOnly]
		private		float					m_WeatherChoiceFactor	= 1.0f;


		// GET/SET

		private		float					m_DayTimeNow			= -1.0f;
		public		float					DayTimeNow
		{
			get { return m_DayTimeNow; }
		}
		float IWeatherManagerInternal.DayTimeNow
		{
			set{ m_DayTimeNow = value; }
		}

		private		EnvDescriptor			m_EnvDescriptorCurrent	= null;
		EnvDescriptor IWeatherManagerInternal.CurrentDescriptor
		{
			get { return m_EnvDescriptorCurrent; }
		}

		private		Light					m_GlobalLight			= null;
		public		Light					Sun
		{
			get { return m_GlobalLight; }
		}

		public		bool					IsDynamic
		{
			get; set;
		}

		private	Material					m_SkyMaterial
		{
			get; set; 	
		}


		// PRIVATE PROPERTIES
		private		Weathers				m_Cycles				= null;
		private		WeatherCycle			m_CurrentCycle			= null;
		private		EnvDescriptor[]			m_Descriptors			= null;
		private		EnvDescriptor			m_EnvDescriptorNext		= null;
		private		EnvDescriptor			m_EnvDescriptorMixer	= null;

		private		bool					m_IsOK					= false;



		/////////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void	Awake()
		{
			Instance = this;

			// Create Env Mixer
			m_EnvDescriptorMixer = new EnvDescriptor();

			LoadAndSetup();
		}


		/////////////////////////////////////////////////////////////////////////////
		// START
		void IWeatherManagerInternal.Start( WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle; 
			m_Descriptors			= m_CurrentCycle.Descriptors;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_WeatherChoiceFactor	= choiceFactor;
		}
		private void	Start()
		{
			LoadAndSetup();

			if ( m_IsOK == false )
				return;

			m_WeatherChoiceFactor = Random.value;

			// Select descriptors
			StartSelectDescriptors( m_DayTimeNow );

			// Make first env lerp
			EnvironmentLerp();

			IsDynamic = true;
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void OnEnable()
		{
			Instance = this;
			this.Start();
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void OnDisable()
		{
			m_CurrentCycle			= null; 
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;
			m_CurrentCycleName		= "";
			m_WeatherChoiceFactor	= 1.0f;
			Instance				= null;
		}


		/////////////////////////////////////////////////////////////////////////////
		// LoadAndSetup
		private	void	LoadAndSetup()
		{
			m_IsOK = false;

			// Load Sky Material
			if ( m_SkyMaterial == null )
				m_SkyMaterial = Resources.Load<Material>( SKYMIXER_MATERIAL );
			if ( m_SkyMaterial == null )
			{
				print( "WeatherManager::Start: SkyMaterial is null !!" );
				return;
			}

			// Load Cylces
			if ( m_Cycles == null )
				m_Cycles = Resources.Load<Weathers>( WEATHERS_COLLECTION );
			if ( m_Cycles == null )
			{
				print( "WeatherManager::Start: allCycles is null !!" );
				return;
			}

			// Find sun
			m_GlobalLight = GameObject.Find( "Sun" ).GetComponent<Light>();

			// Defaults
			string startTime = "09:30:00";	
			string startWeather = "Rainy";

			// Get info from settings file
			if ( GLOBALS.Configs != null )
			{
				Section pSection = GLOBALS.Configs.GetSection( "Time" );
				if ( pSection != null )
				{
					pSection.bAsString( "StartTime", ref startTime );
					pSection.bAsString( "StartWeather", ref startWeather );
				}
			}

			// Set current time
			TansformTime( startTime, ref m_DayTimeNow );

			startWeather = startWeather.Replace( "\"", "" );

			// Set current cycle
			WeatherCycle cycle = m_Cycles.Cycles.Find( c => c.name == startWeather );
			if ( cycle != null )
			{
				// set as current
				m_CurrentCycle = cycle;
				// update current descriptors
				m_Descriptors = m_CurrentCycle.Descriptors;
				// current updated
				m_EnvDescriptorCurrent = m_EnvDescriptorNext;
			}
			m_CurrentCycleName = m_CurrentCycle.name;

			m_IsOK = true;
		}


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
			m_SkyMaterial.SetTexture( "_Skybox1", m_EnvDescriptorCurrent.SkyCubemap );
			m_SkyMaterial.SetTexture( "_Skybox2", m_EnvDescriptorNext.SkyCubemap );
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
		void IWeatherManagerInternal.StartSelectDescriptors( float DayTime, WeatherCycle cycle )
		{
			m_CurrentCycle					= cycle;
			m_CurrentCycleName				= m_CurrentCycle.name;
			m_CurrentCycleName				= cycle.name;
			m_WeatherChoiceFactor			= 2f;
			m_EnvDescriptorCurrent			= null;
			m_EnvDescriptorNext				= null;
			StartSelectDescriptors( DayTime, cycle );
		}
		private	void	StartSelectDescriptors( float DayTime, WeatherCycle cycle = null )
		{
			if ( cycle != null )
				m_Descriptors = cycle.Descriptors;

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
		// ChangeWeather
		public	void	ChangeWeather( WeatherCycle newCycle )
		{
			// find the corresponding of the current descriptor in the nex cycle
			EnvDescriptor correspondingDescriptor = System.Array.Find( newCycle.Descriptors, d => d.ExecTime == m_EnvDescriptorNext.ExecTime );
			if ( correspondingDescriptor == null )
				return;

			// set as current
			m_CurrentCycle = newCycle;

			// update current descriptors
			m_Descriptors = m_CurrentCycle.Descriptors;
					
			// current updated
			m_EnvDescriptorCurrent = m_EnvDescriptorNext;
			
			m_CurrentCycleName = m_CurrentCycle.name;

			// get descriptor next current from new cycle
			m_EnvDescriptorNext = GetNextDescriptor( correspondingDescriptor );
			print( "New cycle: " + newCycle.name );
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetWeather
		public	void	SetWeather( string weatherName )
		{
			if ( m_CurrentCycle != null && m_CurrentCycle.name == weatherName )
				return;

			WeatherCycle newCycle = m_Cycles.Cycles.Find( c => c.name == weatherName );
			if ( newCycle == null )
				return;

			ChangeWeather( newCycle );
		}


		/////////////////////////////////////////////////////////////////////////////
		// RandomWeather
		public	void	RandomWeather()
		{
			// Choose a new cycle
			int newIdx = Random.Range( 0, m_Cycles.Cycles.Count );
			ChangeWeather( m_Cycles.Cycles[ newIdx ] );
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
				// Choice for a new cycle
				float randomValue = Random.value;
				if ( randomValue > ( m_WeatherChoiceFactor ) )
				{
					RandomWeather();
					m_WeatherChoiceFactor += randomValue;
				}
				else
				{
					// Editor stuff
					if ( m_WeatherChoiceFactor <= 2.0f )
						m_WeatherChoiceFactor = Mathf.Clamp01( m_WeatherChoiceFactor  - 0.2f );
					m_EnvDescriptorCurrent = m_EnvDescriptorNext;
					m_EnvDescriptorNext = GetNextDescriptor( m_EnvDescriptorNext );
				}
				SetCubemaps();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeDiff
		private	void	EnvironmentLerp()
		{
			float interpolant = TimeInterpolant( m_DayTimeNow, m_EnvDescriptorCurrent.ExecTime, m_EnvDescriptorNext.ExecTime );
			InterpolateOthers( m_EnvDescriptorCurrent, m_EnvDescriptorNext, interpolant * 2f );
			m_SkyMaterial.SetFloat( "_Blend", interpolant );
		}


		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void	InterpolateOthers( EnvDescriptor current, EnvDescriptor next, float interpolant )
		{
			m_EnvDescriptorMixer.AmbientColor		= Color.Lerp( current.AmbientColor, next.AmbientColor, interpolant );
			m_EnvDescriptorMixer.FogFactor			= Mathf.Lerp( current.FogFactor, next.FogFactor, interpolant );
			m_EnvDescriptorMixer.RainIntensity		= Mathf.Lerp( current.RainIntensity, next.RainIntensity, interpolant );
			m_EnvDescriptorMixer.SkyColor			= Color.Lerp( current.SkyColor, next.SkyColor, interpolant );
			m_EnvDescriptorMixer.SunColor			= Color.Lerp( current.SunColor, next.SunColor, interpolant );
			m_EnvDescriptorMixer.SunRotation		= Vector3.Lerp( current.SunRotation, next.SunRotation, interpolant );

			RenderSettings.ambientSkyColor	= m_EnvDescriptorMixer.SkyColor;
			RenderSettings.ambientLight		= m_EnvDescriptorMixer.AmbientColor;

			RenderSettings.fog				= m_EnvDescriptorMixer.FogFactor > 0.0f;
			RenderSettings.fogDensity		= m_EnvDescriptorMixer.FogFactor;

			if ( RainManager.Instance != null )
				RainManager.Instance.RainIntensity = m_EnvDescriptorMixer.RainIntensity;

			m_GlobalLight.color				= m_EnvDescriptorMixer.SunColor;
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void	Update()
		{
			if ( m_IsOK == false )
				return;

			if ( EnableInEditor == false && UnityEditor.EditorApplication.isPlaying == false )
				return;

			if ( IsDynamic == true )
			{
				if ( EditorLinked == false )
				{
					m_DayTimeNow += Time.deltaTime * m_TimeFactor; // + Level()->GetTimeFactor();
				}

				if ( m_DayTimeNow > DAY_LENGTH )
					m_DayTimeNow = 0.0f;

				SelectDescriptors( m_DayTimeNow );
				EnvironmentLerp();
			}

			// Sun rotation by data
			if ( EditorDescriptorLinked == false )
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