
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager {

		float						TimeFactor				{ get; set; }
		float						DayTime					{ get; }
	}

	// Public interface
	public partial interface IWeatherManager_Cycles {

		void						SetWeatherByName( string weatherName );
		void						SetTime( float DayTime );
		void						SetTime( float H, float M, float S );
		void						SetTime( string sTime );
		string						GetTimeAsString();
		void						OverrideSkyColor( Color color );

		string						CurrentCycleName					{ get; }
	}

	// CLASS
	public partial class WeatherManager : MonoBehaviour, IWeatherManager, IWeatherManager_Cycles {

	#region VARS

		// STATIC
		private	static	IWeatherManager			m_Instance						= null;
		public	static	IWeatherManager			Instance						=> m_Instance;

		private	static	IWeatherManager_Cycles	m_Instance_Cycles				= null;
		public	static	IWeatherManager_Cycles	Cycles							=> m_Instance_Cycles;


		// CONST
		public	const	float					DAY_LENGTH						= 86400f;
		public	const	string					RESOURCES_WEATHERSCOLLECTION	= "WeatherCollection";
		private	const	string					RESOURCES_SKYMIXER_MAT			= "Materials/SkyMixer";

		// SERIALIZED
		[Header("Main")]
		[ SerializeField, ReadOnly ]
		private		string						m_CurrentDayTime				= string.Empty;

		[ Header("Weather Info") ]

		[ SerializeField, Range( 1f, 500f ) ]
		private		float						m_TimeFactor					= 1.0f;

		[ SerializeField, ReadOnly ]
		private		float						m_WeatherChoiceFactor			= 1.0f;

		[Header( "Cycles" )]

		[SerializeField, ReadOnly]
		private		Material					m_SkyMaterial					= null;

		[SerializeField]
		private		Weathers					m_Cycles						= null;

		[SerializeField, ReadOnly ]
		private		WeatherCycle				m_CurrentCycle					= null;

		[ SerializeField, ReadOnly ]
		private		string						m_CurrentCycleName				= string.Empty;

		[SerializeField, ReadOnly]
		private		List<EnvDescriptor>			m_Descriptors					= null;

		private		EnvDescriptor				m_EnvDescriptorCurrent			= null;
		private		EnvDescriptor				m_EnvDescriptorNext				= null;
		private		EnvDescriptorMixer			m_EnvDescriptorMixer			= new EnvDescriptorMixer();

		private		float						m_EnvEffectTimer				= 0.0f;
		private 	Quaternion					m_RotationOffset				= Quaternion.AngleAxis( 180f, Vector3.up );
		private		Light						m_Sun							= null;
		private		float						m_DayTimeNow					= -1.0f;
		private		bool						m_ShowDebugInfo					= false;

	#endregion

	#region INTERFACE BASE

		float IWeatherManager.TimeFactor
		{
			get { return m_TimeFactor; }
			set { m_TimeFactor =  Mathf.Max( value, 0f ); }
		}

		float			IWeatherManager.DayTime
		{
			get { return m_DayTimeNow; }
		}

	#endregion

	#region INTERFACE CYCLES

		/////////////////////////////////////////////////////////////////////////////
		// CurrentCycleName
		string	IWeatherManager_Cycles.CurrentCycleName
		{
			get { return m_CurrentCycleName; }
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( float DayTime )
		{
			m_DayTimeNow = Mathf.Clamp( DayTime, 0f, DAY_LENGTH );
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( float H, float M, float S )
		{
			m_DayTimeNow = Mathf.Clamp( ( ( H * 3600f ) + ( M * 60f ) + S ), 0f, DAY_LENGTH );
			
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( string sTime )
		{
			TransformTime( sTime, ref m_DayTimeNow );
		}


		/////////////////////////////////////////////////////////////////////////////
		// GetTimeAsString
		string	IWeatherManager_Cycles.GetTimeAsString()
		{
			float f = m_DayTimeNow;

			string iH = ( ( f / ( 60 * 60 ) ) ).ToString( "00" );
			string iM = ( ( f / 60 ) % 60 ).ToString( "00" );
			string iS = ( f % 60 ).ToString( "00" );

			return string.Format( "{0}:{1}:{2}", iH, iM, iS );
		}


		/////////////////////////////////////////////////////////////////////////////
		// OverrideSkyColor
		void IWeatherManager_Cycles.OverrideSkyColor( Color color )
		{
			m_SkyMaterial.SetColor( "_Tint", color );
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetWeatherByName
		void	IWeatherManager_Cycles.SetWeatherByName( string weatherName )
		{
			WeatherCycle newCycle = m_Cycles.LoadedCycles.Find( c => c.name == weatherName );
			if ( newCycle )
			{
				ChangeWeather( newCycle );
			}
		}
		
	#endregion

	#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		// Utility
		/// <summary> Set the corrisponding float value for given time ( HH:MM:SS ), return boolean as result </summary>
		public static	bool	TransformTime( string sTime, ref float Time )
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

		/// <summary> Convert the given float value into a formatted string ( HH:MM:SS ), optionally can append seconds </summary>
		public	static	void	TransformTime( float fTime, ref string Time, bool considerSeconds = true )
		{
			int iH = ( int ) ( fTime / ( 3600f ) );
			int iM = ( int ) ( ( fTime / 60f ) % 60f );
			int iS = ( int ) ( fTime % 60f );
			Time = ( iH.ToString( "00" ) + ":" + iM.ToString( "00" ) );

			if ( considerSeconds )
				Time +=  ( ":" + iS.ToString( "00" ) );
		}

		/// <summary> Return true if the given value of HH, MM and SS are valid </summary>
		public static bool		IsValidTime( float h, float m, float s )
		{
			return ( ( h >= 0 ) && ( h < 24 ) && ( m >= 0 ) && ( m < 60 ) && ( s >= 0 ) && ( s < 60 ) );
		}

		#endregion

	#region INITIALIZATION

		/////////////////////////////////////////////////////////////////////////////
		// Awake
		private void			Awake()
		{
			// Singleton
			if ( m_Instance != null )
			{
#if UNITY_EDITOR
				// In EDITOR: If is editor play mode
				if ( UnityEditor.EditorApplication.isPlaying == true )
					Destroy/*DestroyImmediate*/( gameObject );	// Immediate is allowed
				else
					Destroy( gameObject );	// If not in play we destroy normally
#else
				// In BUILD: Destroy normally ( because of singleton this should never happen )
				Debug.Log( "WeatherMnager::Awake: Destroy called in build, this should never happen" );
				Destroy( gameObject );	
#endif
				// in any case return
				return;
			}

			Database.Section debugInfosSection = null;
			if ( m_ShowDebugInfo == false && GlobalManager.Configs.bGetSection( "DebugInfos", ref debugInfosSection ) )
			{
				m_ShowDebugInfo = debugInfosSection.AsBool( "WeatherManager", false);
				if ( m_ShowDebugInfo )
					Debug.Log( "WeatherManager::Awake: : Log Enabled" );
			}
			
#if UNITY_EDITOR
			// In EDITOR: If is editor play mode
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );

			System.Action<UnityEditor.PlayModeStateChange> OnPlayModeStateChanged = delegate( UnityEditor.PlayModeStateChange newState )
			{
			//	Debug.Log( newState.ToString() );
				IWeatherManager_Editor a = WindowWeatherEditor.GetWMGR( true );
			};

			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
			// In BUILD: DontDestroyOnLoad called normally
			DontDestroyOnLoad( this );
#endif

			m_Instance			= this as IWeatherManager;
			m_Instance_Cycles	= this as IWeatherManager_Cycles;
			m_Instance_Editor	= this as IWeatherManager_Editor;

			LoadSkyMixerMaterial();
		}



		/////////////////////////////////////////////////////////////////////////////
		// LoadSkyMixerMaterial
		private	bool	LoadSkyMixerMaterial()
		{
			if ( m_SkyMaterial )
				return true;

			Material loadedMaterial = Resources.Load<Material>( RESOURCES_SKYMIXER_MAT );

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnEnabled: Loaded Material " + RESOURCES_SKYMIXER_MAT + ": " + ( ( loadedMaterial ) ? "done" : "failed" ) );

			if ( loadedMaterial )
			{
				// Load Sky Material
				m_SkyMaterial = Object.Instantiate(loadedMaterial);
				return true;
			}
			return false;
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void			OnEnable()
		{
//			GameManager.StreamEvents.OnSave += StreamEvents_OnSave;
//			GameManager.StreamEvents.OnLoad += StreamEvents_OnLoad;

			LoadSkyMixerMaterial();

			if ( m_Cycles.IsNotNull() && m_Cycles.LoadedCycles.Count > 0 )
			{
				Setup_Cycles();

				// Select descriptors
				StartSelectDescriptors( m_DayTimeNow = Random.value * WeatherManager.DAY_LENGTH );

				// Make first env lerp
				EnvironmentLerp();
			}
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
//			GameManager.StreamEvents.OnSave -= StreamEvents_OnSave;
//			GameManager.StreamEvents.OnLoad -= StreamEvents_OnLoad;

			m_WeatherChoiceFactor	= 1.0f;
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private StreamUnit StreamEvents_OnSave( StreamData streamData )
		{
			StreamUnit streamUnit	= streamData.NewUnit( gameObject );
			{
				streamUnit.SetInternal( "DayTimeNow", m_DayTimeNow );
				streamUnit.SetInternal( "CycleName", m_CurrentCycleName );
			}
			return streamUnit;
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private StreamUnit StreamEvents_OnLoad( StreamData streamData )
		{
			StreamUnit streamUnit = null;
			if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
			{
				m_DayTimeNow = streamUnit.GetAsFloat( "DayTimeNow" );
				string cycleName = streamUnit.GetInternal( "CycleName" );

				int index = m_Cycles.LoadedCycles.FindIndex( c => c.name == cycleName );
				if ( index > -1 )
				{
					WeatherCycle cycle = m_Cycles.LoadedCycles[ index ];
					ChangeWeather( cycle );
				}
			}
			return streamUnit;
		}


		/////////////////////////////////////////////////////////////////////////////
		private void			Setup_Cycles()
		{
			if ( m_Sun == null )
			{
				// Create Sun
				Transform child = transform.Find( "Sun" );
				if ( child )
				{
					m_Sun = child.GetOrAddIfNotFound<Light>();
				}
				//	if ( child == null )
				else
				{
					child = new GameObject( "Sun" ).transform;
					child.SetParent( this.transform );
					m_Sun = child.gameObject.AddComponent<Light>();
				}
			}
			m_Sun.type				= LightType.Directional;
			m_Sun.shadows			= LightShadows.Soft;
			
			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Sun configured" );

			// Setup for Environment
			RenderSettings.sun		= m_Sun;
			RenderSettings.skybox	= m_SkyMaterial;

			// Defaults
			string startTime = "09:30:00";
			string startWeather = m_Cycles.LoadedCycles[0].name;

			// Get info from settings file
			Database.Section pSection = null;
			if ( GlobalManager.Configs.bGetSection( "Time", ref pSection) == true )
			{
				pSection.bAsString( "StartTime",	ref startTime );
				pSection.bAsString( "StartWeather", ref startWeather );
				pSection.bAsFloat(	"TimeFactor",	ref m_TimeFactor );
			}

			// Set current time
			if ( m_DayTimeNow == -1f )
				TransformTime( startTime, ref m_DayTimeNow );

			startWeather = startWeather.Replace( "\"", "" );
			m_CurrentCycleName = "Invalid";

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Applying time " + startWeather + ", " + startTime );

			// Set current cycle
			int index = m_Cycles.LoadedCycles.FindIndex( c => c.name == startWeather );
			if ( index > -1 )
			{
				WeatherCycle cycle = m_Cycles.LoadedCycles[ index ];
				// set as current
				m_CurrentCycle = cycle;
				// update current descriptors
				m_Descriptors =  new List<EnvDescriptor>( cycle.LoadedDescriptors );
				// update cycle name
				m_CurrentCycleName = cycle.name;
				// current updated
				m_EnvDescriptorCurrent = m_EnvDescriptorNext;
			}

			m_WeatherChoiceFactor = 1.1f;

			m_EnvEffectTimer = Random.Range( 2f, 5f );

			// Select descriptors
			if ( m_Cycles.CyclesPaths.Count > 0 )
				StartSelectDescriptors( m_DayTimeNow );
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
		private	float			TimeInterpolant( float DayTime )
		{
			if ( m_EnvDescriptorCurrent == null || m_EnvDescriptorNext == null )
				return 0.0f;

			float Current = m_EnvDescriptorCurrent.ExecTime;
			float Next = m_EnvDescriptorNext.ExecTime;

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
		private	EnvDescriptor	GetPreviousDescriptor( EnvDescriptor current )
		{
			int idx = m_Descriptors.IndexOf( current );
			return m_Descriptors[ ( idx ) == 0 ? m_Descriptors.Count - 1 : ( idx - 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// GetNextDescriptor
		private	EnvDescriptor	GetNextDescriptor( EnvDescriptor current )
		{
			int idx = m_Descriptors.IndexOf( current );
			return m_Descriptors[ ( idx + 1 ) == m_Descriptors.Count ? 0 : ( idx + 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// StartSelectDescriptors
		private	void			StartSelectDescriptors( float DayTime, WeatherCycle cycle = null )
		{
			if ( cycle != null )
			{
				m_Descriptors = new List<EnvDescriptor>( cycle.LoadedDescriptors );
			}

			// get the last valid descriptor where its execTime is less than dayTime
			EnvDescriptor descriptor = m_Descriptors.FindLast( ( EnvDescriptor d ) => d.ExecTime < DayTime );

			EnvDescriptor first = m_Descriptors[ 0 ];
			EnvDescriptor last  = m_Descriptors[ m_Descriptors.Count - 1 ];
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

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Descriptors selected: " + m_EnvDescriptorCurrent.Identifier + "," + m_EnvDescriptorNext.Identifier );


			SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// ChangeWeather
		private	void			ChangeWeather( WeatherCycle newCycle )
		{
			// find the corresponding of the current descriptor in the nex cycle
			int correspondingDescriptorIndex = System.Array.FindIndex (newCycle.LoadedDescriptors, ( d => d.Identifier == m_EnvDescriptorNext.Identifier ) );
			if ( correspondingDescriptorIndex == -1 )
				return;

			EnvDescriptor correspondingDescriptor = newCycle.LoadedDescriptors[correspondingDescriptorIndex];

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Changing weather, requested: " + newCycle.name );

			// set as current
			m_CurrentCycle = newCycle;

			// update current descriptors
			m_Descriptors = new List<EnvDescriptor>( m_CurrentCycle.LoadedDescriptors );
					
			// current updated
			m_EnvDescriptorCurrent = m_EnvDescriptorNext;
			
			m_CurrentCycleName = m_CurrentCycle.name;

			// get descriptor next current from new cycle
			m_EnvDescriptorNext = GetNextDescriptor( correspondingDescriptor );
//			print( "New cycle: " + newCycle.name );
		}


		/////////////////////////////////////////////////////////////////////////////
		// RandomWeather
		private	void			RandomWeather()
		{
			// Choose a new cycle
			int newIdx = Random.Range( 0, m_Cycles.CyclesPaths.Count-1 );
			WeatherCycle cycle = m_Cycles.LoadedCycles[ newIdx ];

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Setting random Weather: " + cycle.name );

			ChangeWeather( cycle );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		// SelectDescriptors
		private	void			SelectDescriptors( float DayTime )
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
		private	void			EnvironmentLerp()
		{
			float interpolant = TimeInterpolant( m_DayTimeNow );
			InterpolateOthers( interpolant );
			m_SkyMaterial.SetFloat( "_Blend", interpolant );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void			InterpolateOthers( float interpolant )
		{
			EnvDescriptor current = m_EnvDescriptorCurrent;
			EnvDescriptor next = m_EnvDescriptorNext;

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
			 
			m_Sun.color								= m_EnvDescriptorMixer.SunColor;
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

#endregion

		/////////////////////////////////////////////////////////////////////////////
		// Update
		private void			Update()
		{
			if ( this.runInEditMode ) // 
				return;

			if ( m_EnvDescriptorCurrent.set == false || m_EnvDescriptorNext.set == false )
				return;

			// Only every 10 frames
			if ( Time.frameCount % 10 == 0 )
				return;

			m_DayTimeNow += Time.deltaTime * m_TimeFactor;
			if ( m_DayTimeNow > DAY_LENGTH )
				m_DayTimeNow = 0.0f;

			TransformTime( m_DayTimeNow, ref m_CurrentDayTime );

			SelectDescriptors( m_DayTimeNow );

			EnvironmentLerp();

			AmbientEffectUpdate();

			// Sun rotation by data
			m_Sun.transform.rotation = m_RotationOffset * Quaternion.LookRotation( m_EnvDescriptorMixer.SunRotation );
		}


		/////////////////////////////////////////////////////////////////////////////
		// Reset
		private void			Reset()
		{
			m_Cycles				= null;
			m_CurrentCycle			= null; 
			m_CurrentCycleName		= string.Empty;
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;

			m_Instance			= this as IWeatherManager;
			m_Instance_Cycles	= this as IWeatherManager_Cycles;
			m_Instance_Editor	= this as IWeatherManager_Editor;

			LoadSkyMixerMaterial();
#if UNITY_EDITOR
			this.runInEditMode = false;
			UnityEditor.EditorApplication.update -= EditorUpdate;
#endif
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void			OnDestroy()
		{
		//	if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnEnabled: OnDestroy" );
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnApplicationQuit
		private void			OnApplicationQuit()
		{
			m_DayTimeNow = 0f;
		}

	}

}