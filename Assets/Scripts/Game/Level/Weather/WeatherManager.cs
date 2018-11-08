
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public interface IWeatherManager {

		Transform			Transform				{ get; }
		float				TimeFactor				{ get; set; }
		bool				IsDynamic				{ get; set; }
		float				DayTime					{ get; }
		Light				Sun						{ get; }
		string				CurrentCycleName		{ get; }

		void				SetWeatherByName( string weatherName );
		void				SetTime( float DayTime );
		void				SetTime( float H, float M, float S );
		void				SetTime( string sTime );
		string				GetTimeAsString( float t );

	}

	// Editing interface
	internal interface IWeatherManagerInternal {

		bool			EditorLinked				{ get; set; }
		bool			EditorDescriptorLinked		{ get; set; }
		float			DayTimeNow					{ set; }
		EnvDescriptor	CurrentDescriptor			{ get; }
		EnvDescriptor	NextDescriptor				{ get; }
		Material		SkyMixerMaterial			{ get; }

		void			StartSelectDescriptors( float DayTime, ref WeatherCycle cycle );
		void			Start( ref WeatherCycle cycle, float choiseFactor );
	}

	// CLASS
	[ExecuteInEditMode]
	public class WeatherManager : MonoBehaviour, IWeatherManager, IWeatherManagerInternal {

#region VARS

		// STATIC
		public	 static	IWeatherManager			Instance					= null;
		internal static	IWeatherManagerInternal	Internal					= null;


		// CONST
		public	const	float					DAY_LENGTH					= 86400f;
		private	const	string					WEATHERS_COLLECTION			= "Scriptables/WeatherCollection";
		private	const	string					SKYMIXER_MATERIAL			= "Materials/SkyMixer";


		// Editor Stuf
		[ SerializeField, ReadOnly ]
		private	string							CurrentDayTime				= string.Empty;

		bool			IWeatherManagerInternal.EditorLinked				{ get; set; }
		bool			IWeatherManagerInternal.EditorDescriptorLinked		{ get; set; }
		float			IWeatherManagerInternal.DayTimeNow					{ set{ m_DayTimeNow = value; } }
		EnvDescriptor	IWeatherManagerInternal.CurrentDescriptor			{ get { return m_EnvDescriptorCurrent; } }
		EnvDescriptor	IWeatherManagerInternal.NextDescriptor				{ get { return m_EnvDescriptorNext; } }
		Material		IWeatherManagerInternal.SkyMixerMaterial			{ get { return m_SkyMaterial; } }

		Transform		IWeatherManager.Transform							{ get { return transform; } }


		// SERIALIZED
		[ SerializeField ]
		private	bool							EnableInEditor				= false;

		[ SerializeField, Range( 1f, 500f ) ]
		private	float							m_TimeFactor				= 1.0f;
		public	float							TimeFactor
		{
			get { return m_TimeFactor; }
			set { m_TimeFactor =  Mathf.Max( value, 0f ); }
		}

		[ Header("Weather Info") ]

		[ SerializeField, ReadOnly ]
		private		string						m_CurrentCycleName			= string.Empty;

		[ SerializeField, ReadOnly ]
		private		float						m_WeatherChoiceFactor		= 1.0f;


		// GET/SET
		public		bool						IsDynamic					{ get; set; }
		public		Light						Sun							{ get; private set;	}

		private	static float					m_DayTimeNow				= -1.0f;
		float		IWeatherManager.DayTime
		{
			get { return m_DayTimeNow; }
		}
		string		IWeatherManager.CurrentCycleName
		{
			get { return m_CurrentCycleName; }
		}


		// PRIVATE PROPERTIES
		private		Weathers					m_Cycles					= null;
		private		WeatherCycle				m_CurrentCycle				= null;
		private		EnvDescriptor[]				m_Descriptors				= null;
		private		EnvDescriptor				m_EnvDescriptorCurrent		= null;
		private		EnvDescriptor				m_EnvDescriptorNext			= null;
		private		EnvDescriptor				m_EnvDescriptorMixer		= null;
		private		Material					m_SkyMaterial				= null;
		private		float						m_EnvEffectTimer			= 0f;

		private		bool						m_IsOK						= false;
		private		bool						m_IsThisTheClone			= false;
		private		Quaternion					m_RotationOffset			= Quaternion.AngleAxis( 180f, Vector3.up );

#endregion

#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		/// Utility
		public static	bool	TansformTime( string sTime, ref float Time )
		{
			int iH = 0, iM = 0, iS = 0;

			string[] parts = sTime.Split( ':' );
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
		}
		public static bool		IsValidTime( float h, float m, float s )
		{
			return ( ( h >= 0 ) && ( h < 24 ) && ( m >= 0 ) && ( m < 60 ) && ( s >= 0 ) && ( s < 60 ) );
		}

#endregion

#region INITIALIZATION

		/////////////////////////////////////////////////////////////////////////////
		// AWAKE
		private void			Awake()
		{
			if ( Instance != null )
			{
				m_IsThisTheClone = true;	// useful, to prevent that onDisable and onDestroy remove reference to the instance
				Destroy( gameObject );
				return;
			}
			
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );
#else
			DontDestroyOnLoad( this );
#endif

			Instance = this as IWeatherManager;
			Internal = this as IWeatherManagerInternal;
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void			OnEnable()
		{
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				UnityEditor.EditorApplication.update += Update;
#endif
			Instance = this as IWeatherManager;
			Internal = this as IWeatherManagerInternal;

			if ( m_EnvDescriptorMixer == null )
				m_EnvDescriptorMixer = new EnvDescriptor();

			LoadAndSetup();

			if ( m_IsOK == false )
				return;

			m_WeatherChoiceFactor = 1.1f;

			m_EnvEffectTimer = Random.Range( 2f, 5f );

			// Select descriptors
			StartSelectDescriptors( m_DayTimeNow );
		}

		/////////////////////////////////////////////////////////////////////////////
		// OnLevelWasLoaded
		private void OnLevelWasLoaded( int level )
		{
//			Awake();
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				UnityEditor.EditorApplication.update -= Update;
#endif
			m_CurrentCycle			= null; 
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;
			m_CurrentCycleName		= "";
			m_WeatherChoiceFactor	= 1.0f;
			if ( m_IsThisTheClone == false )
			{
				Instance				= null;
				Internal				= null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// START
		private void			Start()
		{
			Instance = this as IWeatherManager;
			Internal = this as IWeatherManagerInternal;

//			LoadAndSetup();

			if ( m_IsOK == false )
				return;

			m_WeatherChoiceFactor = 1.1f;

			// Select descriptors
			StartSelectDescriptors( m_DayTimeNow );

			// Make first env lerp
			EnvironmentLerp();

			IsDynamic = true;
		}

#endregion

#region PUBLIC INTERFACE

		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		void IWeatherManager.SetTime( float DayTime )
		{
			m_DayTimeNow = Mathf.Clamp( DayTime, 0f, DAY_LENGTH );
		}


		/// //////////////////////////////////////////////////////////////////////////
		/// SetTime
		void IWeatherManager.SetTime( float H, float M, float S )
		{
			m_DayTimeNow = Mathf.Clamp( ( ( H * 3600f ) + ( M * 60f ) + S ), 0f, DAY_LENGTH );
			
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
			if ( f == -1f )
				f = m_DayTimeNow;

			string iH = ( ( f / ( 60 * 60 ) ) ).ToString( "00" );
			string iM = ( ( f / 60 ) % 60 ).ToString( "00" );
			string iS = ( f % 60 ).ToString( "00" );

			return string.Format( "{0}:{1}:{2}", iH, iM, iS );
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

			ChangeWeather( ref newCycle );
		}

#endregion

#region INTERNAL INTERFACE

		void IWeatherManagerInternal.Start( ref WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle; 
			m_Descriptors			= m_CurrentCycle.Descriptors;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_WeatherChoiceFactor	= choiceFactor;
		}

		void IWeatherManagerInternal.StartSelectDescriptors( float DayTime, ref WeatherCycle cycle )
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
		private void			LoadAndSetup()
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
				Transform child = ( transform.childCount > 0 ) ? transform.GetChild( 0 ) : null;
				if ( child == null )
				{
					child = new GameObject( "Sun" ).transform;
					child.SetParent( this.transform );
					child.gameObject.AddComponent<Light>();
				}
				Sun = child.GetComponent<Light>();

				if ( Sun == null )
				{
					Sun = child.gameObject.AddComponent<Light>();
				}

				Sun.type				= LightType.Directional;
				Sun.shadows				= LightShadows.Soft;
			}

			// Setup for Environment
			RenderSettings.sun = Sun;
			RenderSettings.skybox = m_SkyMaterial;

			// Defaults
			string startTime = "09:30:00";	
			string startWeather = "Rainy";

			// Get info from settings file
			if ( GameManager.Configs != null )
			{
				Database.Section pSection = null;
				GameManager.Configs.GetSection( "Time", ref pSection );
				if ( pSection != null )
				{
					pSection.bAsString( "StartTime",	ref startTime );
					pSection.bAsString( "StartWeather", ref startWeather );
					pSection.bAsFloat(	"TimeFactor",	ref m_TimeFactor );
				}
			}

			// Set current time
			if ( m_DayTimeNow == -1f )
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
		private	float			TimeDiff( float Current, float Next )
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
		private	float			TimeInterpolant( float DayTime, float Current, float Next )
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
		private	void			SetCubemaps()
		{   
			m_SkyMaterial.SetTexture( "_Skybox1", m_EnvDescriptorCurrent.SkyCubemap );
			m_SkyMaterial.SetTexture( "_Skybox2", m_EnvDescriptorNext.SkyCubemap );
		}


		////////////////////////////////////////////////////////////////////////////
		// GetPreviousDescriptor
		private	EnvDescriptor	GetPreviousDescriptor( ref EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx ) == 0 ? m_Descriptors.Length - 1 : ( idx - 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// GetNextDescriptor
		private	EnvDescriptor	GetNextDescriptor( ref EnvDescriptor current )
		{
			int idx = System.Array.IndexOf( m_Descriptors, current );
			return m_Descriptors[ ( idx + 1 ) == m_Descriptors.Length ? 0 : ( idx + 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// StartSelectDescriptors
		private	void			StartSelectDescriptors( float DayTime, WeatherCycle cycle = null )
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
				m_EnvDescriptorNext = GetNextDescriptor( ref descriptor );
			}
			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// ChangeWeather
		private	void			ChangeWeather( ref WeatherCycle newCycle )
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
			m_EnvDescriptorNext = GetNextDescriptor( ref correspondingDescriptor );
//			print( "New cycle: " + newCycle.name );
		}


		/////////////////////////////////////////////////////////////////////////////
		// RandomWeather
		private	void			RandomWeather()
		{
			// Choose a new cycle
			int newIdx = Random.Range( 0, m_Cycles.Cycles.Count );
			WeatherCycle cycle = m_Cycles.Cycles[ newIdx ];
			ChangeWeather( ref cycle );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		// SelectDescriptors
		public void				SelectDescriptors( float DayTime )
		{
			bool bSelect = false;
		//	if ( DayTime > m_EnvDescriptorCurrent.ExecTime && DayTime > m_EnvDescriptorNext.ExecTime )

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
					m_EnvDescriptorNext = GetNextDescriptor( ref m_EnvDescriptorNext );
				}
				SetCubemaps();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeDiff
		private	void			EnvironmentLerp()
		{
			float interpolant = TimeInterpolant( m_DayTimeNow, m_EnvDescriptorCurrent.ExecTime, m_EnvDescriptorNext.ExecTime );
			InterpolateOthers( ref m_EnvDescriptorCurrent, ref m_EnvDescriptorNext, interpolant );
			m_SkyMaterial.SetFloat( "_Blend", interpolant );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void			InterpolateOthers( ref EnvDescriptor current, ref EnvDescriptor next, float interpolant )
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

			Sun.color						= m_EnvDescriptorMixer.SunColor;
		}


		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void			AmbientEffectUpdate()
		{
			m_EnvEffectTimer -= Time.deltaTime;
			if ( m_EnvEffectTimer < 0f )
			{
				AudioCollection effectCollection = m_EnvDescriptorCurrent.AmbientEffects;
				if ( effectCollection != null )
				{
					AudioClip clip = effectCollection.AudioClips[ Random.Range( 0, effectCollection.AudioClips.Length ) ];
					AudioSource.PlayClipAtPoint( clip, Player.Instance.transform.position );
				}

				m_EnvEffectTimer = Random.Range( 3f, 7f );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// Update
		private void			Update()
		{
			if ( m_IsOK == false )
				return;

#if UNITY_EDITOR
			// Only every 10 frames
			if ( UnityEditor.EditorApplication.isPlaying == true && Time.frameCount % 10 == 0 )
				return;
			
			if ( EnableInEditor == false && UnityEditor.EditorApplication.isPlaying == false )
				return;
#else
			// Only every 10 frames
			if ( Time.frameCount % 10 == 0 )
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
			AmbientEffectUpdate();

			// Sun rotation by data
			if ( Internal.EditorDescriptorLinked == false )
			{
				Sun.transform.rotation = m_RotationOffset * Quaternion.LookRotation( m_EnvDescriptorMixer.SunRotation );
			}


			TransformTime( m_DayTimeNow, ref CurrentDayTime );
		}
#endregion


		/////////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void OnDestroy()
		{
			Instance = null;
			Internal = null;
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnApplicationQuit
		private void OnApplicationQuit()
		{
			m_DayTimeNow = -1f;
		}

	}

}