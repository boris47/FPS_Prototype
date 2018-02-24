
using System.Collections;
using UnityEngine;

namespace WeatherSystem {

	public interface IWeatherManager {

		float				TimeFactor { get; set; }

		bool				IsDynamic { get; set; }

		float				DayTime { get; }

		Light				Sun { get; }

		void				SetWeatherByName( string weatherName );

		void				SetTime( float DayTime );

		void				SetTime( float H, float M, float S );

		void				SetTime( string sTime );

		string				GetTimeAsString( float t );

	}

	internal interface IWeatherManagerInternal {

		bool			EditorLinked				{ get; set; }
		bool			EditorDescriptorLinked		{ get; set; }
		float			DayTimeNow					{ set; }
		EnvDescriptor	CurrentDescriptor			{ get; }
		EnvDescriptor	NextDescriptor				{ get; }

		void			StartSelectDescriptors( float DayTime, WeatherCycle cycle );
		void			Start( WeatherCycle cycle, float choiseFactor );
	}

	// CLASS

	[ExecuteInEditMode]
	public class WeatherManager : MonoBehaviour, IWeatherManager, IWeatherManagerInternal {

#region VARS

		// STATIC
		public static IWeatherManager		Instance					= null;

		internal static	IWeatherManagerInternal	Internal				= null;


		// CONST
		public	const	float				DAY_LENGTH					= 86400f;
		private	const	string				WEATHERS_COLLECTION			= "Scriptables/WeatherCollection";
		private	const	string				SKYMIXER_MATERIAL			= "Materials/SkyMixer";


		// Editor Stuf
		[ReadOnly]
		public	string							CurrentDayTime			= string.Empty;
		bool			IWeatherManagerInternal.EditorLinked			{ get; set; }
		bool			IWeatherManagerInternal.EditorDescriptorLinked	{ get; set; }
		float			IWeatherManagerInternal.DayTimeNow				{ set{ m_DayTimeNow = value; } }
		EnvDescriptor	IWeatherManagerInternal.CurrentDescriptor		{ get { return m_EnvDescriptorCurrent; } }
		EnvDescriptor	IWeatherManagerInternal.NextDescriptor			{ get { return m_EnvDescriptorNext; } }


		// SERIALIZED
		[SerializeField]
		private	bool						EnableInEditor			= false;

		[SerializeField][Range( 1f, 500f )]
		private	float						m_TimeFactor			= 1.0f;
		public	float						TimeFactor
		{
			get { return m_TimeFactor; }
			set { m_TimeFactor =  Mathf.Max( value, 0f ); }
		}

		[Header("Weather Info")]

		[ SerializeField ][ReadOnly]
		private		string					m_CurrentCycleName		= string.Empty;

		[SerializeField][ReadOnly]
		private		float					m_WeatherChoiceFactor	= 1.0f;


		// GET/SET
		public		bool					IsDynamic				{ get; set; }

		private		float					m_DayTimeNow			= -1.0f;
		float IWeatherManager.DayTime
		{
			get { return m_DayTimeNow; }
		}

		public		Light					Sun
		{
			get; private set;
		}


		private	Material					m_SkyMaterial			{ get; set; }


		// PRIVATE PROPERTIES
		private		Weathers				m_Cycles				= null;
		private		WeatherCycle			m_CurrentCycle			= null;
		private		EnvDescriptor[]			m_Descriptors			= null;
		private		EnvDescriptor			m_EnvDescriptorCurrent	= null;
		private		EnvDescriptor			m_EnvDescriptorNext		= null;
		private		EnvDescriptor			m_EnvDescriptorMixer	= null;

		private		bool					m_IsOK					= false;

#endregion

#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		/// Utility
		public static	bool	TansformTime( string sTime, ref float Time )
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

#endregion

#region INITIALIZATION

		/////////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void	Awake()
		{
			Instance = this as IWeatherManager;
			Internal = this as IWeatherManagerInternal;

			// Create Env Mixer
			m_EnvDescriptorMixer = new EnvDescriptor();

			LoadAndSetup();
		}


		/////////////////////////////////////////////////////////////////////////////
		// START
		private void	Start()
		{
			LoadAndSetup();

			if ( m_IsOK == false )
				return;

			m_WeatherChoiceFactor = 1.1f;

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
			Instance = this as IWeatherManager;
			Internal = this as IWeatherManagerInternal;

//			UnityEditor.EditorApplication.update += Update;

			this.Start();
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void OnDisable()
		{
//			UnityEditor.EditorApplication.update -= Update;
			m_CurrentCycle			= null; 
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;
			m_CurrentCycleName		= "";
			m_WeatherChoiceFactor	= 1.0f;
			Instance				= null;
			Internal				= null;
		}

#endregion

#region PUBLIC INTERFACE

		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		void IWeatherManager.SetTime( float DayTime )
		{
			m_DayTimeNow = DayTime;
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		void IWeatherManager.SetTime( float H, float M, float S )
		{
			m_DayTimeNow = ( ( H * 3600f ) + ( M * 60f ) + S );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		void IWeatherManager.SetTime( string sTime )
		{
			TansformTime( sTime, ref m_DayTimeNow );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// GetTimeAsString
		string	IWeatherManager.GetTimeAsString( float f )
		{
			int iH = ( int ) ( f / ( 60 * 60 ) );
			int iM = ( int ) ( f / 60 ) % 60;
			int iS = ( int ) f % 60;

			return string.Format( "%02d:%02d:%02d", iH, iM, iS );
		}

		/////////////////////////////////////////////////////////////////////////////
		// SetWeatherByName
		void	IWeatherManager.SetWeatherByName( string weatherName )
		{
//			if ( m_CurrentCycle != null && m_CurrentCycle.name == weatherName )
//				return;

			WeatherCycle newCycle = m_Cycles.Cycles.Find( c => c.name == weatherName );
			if ( newCycle == null )
				return;

			ChangeWeather( newCycle );
		}

#endregion

#region INTERNAL INTERFACE

		void IWeatherManagerInternal.Start( WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle; 
			m_Descriptors			= m_CurrentCycle.Descriptors;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_WeatherChoiceFactor	= choiceFactor;
		}

		void IWeatherManagerInternal.StartSelectDescriptors( float DayTime, WeatherCycle cycle )
		{
			m_CurrentCycle					= cycle;
			m_CurrentCycleName				= cycle.name;
			m_WeatherChoiceFactor			= 2f;
			m_EnvDescriptorCurrent			= null;
			m_EnvDescriptorNext				= null;
			StartSelectDescriptors( DayTime, cycle );
		}

		#endregion

#region INTERNAL METHODS

		/////////////////////////////////////////////////////////////////////////////
		// LoadAndSetup
		private void	LoadAndSetup()
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

			// Create Sun
			{
				Sun = transform.GetChild( 0 ).GetComponent<Light>();
			}

			// Defaults
			string startTime = "09:30:00";	
			string startWeather = "Rainy";

			// Get info from settings file
			if ( GLOBALS.Configs != null )
			{
				var pSection = GLOBALS.Configs.GetSection( "Time" );
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
				return  ( ( DAY_LENGTH - Current ) + Next );
			else
			{
				return ( Next - Current );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeInterpolant
		private	float	TimeInterpolant( float DayTime, float Current, float Next )
		{
			float interpolant = 0.0f;
			float fLength = TimeDiff( Current, Next );
			if ( Utils.Math.SimilarZero( fLength, Utils.Math.EPS ) == false ) {
				if ( Current > Next )
				{
					if ( ( DayTime >= Current ) || ( DayTime <= Next ) )
						interpolant = TimeDiff( Current, DayTime ) / fLength;
				}
				else
				{
					if ( ( DayTime >= Current ) && ( DayTime <= Next ) )
						interpolant = TimeDiff( Current, DayTime ) / fLength;
				}
				interpolant = Mathf.Clamp01( interpolant + 0.0001f );
			}
			return interpolant;
			
		}


		////////////////////////////////////////////////////////////////////////////
		// SetCubemaps
		private	void	SetCubemaps()
		{   
			m_SkyMaterial.SetTexture( "_Skybox1", m_EnvDescriptorCurrent.SkyCubemap );
			m_SkyMaterial.SetTexture( "_Skybox2", m_EnvDescriptorNext.SkyCubemap );
		}


		////////////////////////////////////////////////////////////////////////////
		// GetPreviousDescriptor
		private	EnvDescriptor	GetPreviousDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx ) == 0 ? m_Descriptors.Length - 1 : ( idx - 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// GetNextDescriptor
		private	EnvDescriptor	GetNextDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx + 1 ) == m_Descriptors.Length ? 0 : ( idx + 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// StartSelectDescriptors
		private	void	StartSelectDescriptors( float DayTime, WeatherCycle cycle = null )
		{
			if ( cycle != null )
				m_Descriptors = cycle.Descriptors;

			// get the last valid descriptor where its execTime is less than dayTime
			EnvDescriptor descriptor = System.Array.FindLast( m_Descriptors, ( EnvDescriptor d ) => d.ExecTime < DayTime );

			EnvDescriptor first = m_Descriptors[ 0 ];
			EnvDescriptor last  = m_Descriptors[ m_Descriptors.Length - 1 ];
			if ( descriptor == last )
			{
				m_EnvDescriptorCurrent	= last;
				m_EnvDescriptorNext		= first;
			}
			else
			{
				m_EnvDescriptorCurrent = descriptor;
				m_EnvDescriptorNext = GetNextDescriptor( descriptor );
			}

			/*
			EnvDescriptor descriptor = System.Array.FindLast( m_Descriptors, ( EnvDescriptor d ) => d.ExecTime < DayTime );

			m_EnvDescriptorCurrent = descriptor;
			m_EnvDescriptorNext = GetNextDescriptor( descriptor );
			*/
			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// ChangeWeather
		private	void	ChangeWeather( WeatherCycle newCycle )
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
		// RandomWeather
		private	void	RandomWeather()
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
			if ( DayTime > m_EnvDescriptorCurrent.ExecTime && DayTime > m_EnvDescriptorNext.ExecTime )

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
			InterpolateOthers( m_EnvDescriptorCurrent, m_EnvDescriptorNext, interpolant );
			m_SkyMaterial.SetFloat( "_Blend", interpolant );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void	InterpolateOthers( EnvDescriptor current, EnvDescriptor next, float interpolant )
		{
			m_EnvDescriptorMixer.AmbientColor		= Color.Lerp( current.AmbientColor,		next.AmbientColor,	interpolant );
			m_EnvDescriptorMixer.FogFactor			= Mathf.Lerp( current.FogFactor,		next.FogFactor,		interpolant );
			m_EnvDescriptorMixer.RainIntensity		= Mathf.Lerp( current.RainIntensity,	next.RainIntensity, interpolant );
			m_EnvDescriptorMixer.SkyColor			= Color.Lerp( current.SkyColor,			next.SkyColor,		interpolant );
			m_EnvDescriptorMixer.SunColor			= Color.Lerp( current.SunColor,			next.SunColor,		interpolant );
			m_EnvDescriptorMixer.SunRotation		= Vector3.Lerp( current.SunRotation,	next.SunRotation,	interpolant );

			RenderSettings.ambientSkyColor			= m_EnvDescriptorMixer.SkyColor;
			RenderSettings.ambientLight				= m_EnvDescriptorMixer.AmbientColor;

			RenderSettings.fog						= m_EnvDescriptorMixer.FogFactor > 0.0f;
			RenderSettings.fogDensity				= m_EnvDescriptorMixer.FogFactor;

			if ( RainManager.Instance != null )
				RainManager.Instance.RainIntensity	= m_EnvDescriptorMixer.RainIntensity;

			Sun.color								= m_EnvDescriptorMixer.SunColor;
		}


		/////////////////////////////////////////////////////////////////////////////
		// UNITY
		private void	Update()
		{
			if ( m_IsOK == false )
				return;

#if UNITY_EDITOR
			if ( EnableInEditor == false && UnityEditor.EditorApplication.isPlaying == false )
				return;
#endif

			if ( IsDynamic == true )
			{
				if ( Internal.EditorLinked == false )
				{
					m_DayTimeNow += Time.deltaTime * m_TimeFactor; // + Level()->GetTimeFactor();
				}

				if ( m_DayTimeNow > DAY_LENGTH )
					m_DayTimeNow = 0.0f;

				SelectDescriptors( m_DayTimeNow );
				EnvironmentLerp();
			}

			// Sun rotation by data
			if ( Internal.EditorDescriptorLinked == false )
				Sun.transform.rotation = Quaternion.LookRotation( m_EnvDescriptorMixer.SunRotation );


			TransformTime( m_DayTimeNow, ref CurrentDayTime );
		}

#endregion

	}

}