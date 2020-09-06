
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
		void						SetSkyColor( Color color );
		Color						GetSkyColor();
		void						SetSkyExposure(float newValue);
		float						GetSkyExposure();

		string						CurrentCycleName					{ get; }
	}

	// CLASS
	public sealed partial class WeatherManager : MonoBehaviour, IWeatherManager, IWeatherManager_Cycles {

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
		private		EnvDescriptor[]				m_Descriptors					= null;

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
			get { return this.m_TimeFactor; }
			set { this.m_TimeFactor =  Mathf.Max( value, 0f ); }
		}

		float			IWeatherManager.DayTime
		{
			get { return this.m_DayTimeNow; }
		}

	#endregion

	#region INTERFACE CYCLES

		/////////////////////////////////////////////////////////////////////////////
		// CurrentCycleName
		string	IWeatherManager_Cycles.CurrentCycleName
		{
			get { return this.m_CurrentCycleName; }
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( float DayTime )
		{
			this.m_DayTimeNow = Mathf.Clamp( DayTime, 0f, DAY_LENGTH );
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( float H, float M, float S )
		{
			this.m_DayTimeNow = Mathf.Clamp( ( ( H * 3600f ) + ( M * 60f ) + S ), 0f, DAY_LENGTH );
			
		}


		/////////////////////////////////////////////////////////////////////////////
		// SetTime
		void	IWeatherManager_Cycles.SetTime( string sTime )
		{
			TransformTime( sTime, ref this.m_DayTimeNow );
		}


		/////////////////////////////////////////////////////////////////////////////
		// GetTimeAsString
		string	IWeatherManager_Cycles.GetTimeAsString()
		{
			float f = this.m_DayTimeNow;

			string iH = ( ( f / ( 60 * 60 ) ) ).ToString( "00" );
			string iM = ( ( f / 60 ) % 60 ).ToString( "00" );
			string iS = ( f % 60 ).ToString( "00" );

			return string.Format( "{0}:{1}:{2}", iH, iM, iS );
		}


		/////////////////////////////////////////////////////////////////////////////
		void IWeatherManager_Cycles.SetSkyColor( Color color )
		{
			this.m_SkyMaterial.SetColor( "_Tint", color );
		}

		Color IWeatherManager_Cycles.GetSkyColor()
		{
			return this.m_SkyMaterial.GetColor("_Tint");
		}

		void IWeatherManager_Cycles.SetSkyExposure(float newValue)
		{
			this.m_SkyMaterial.SetFloat("_Exposure", newValue);
		}

		float IWeatherManager_Cycles.GetSkyExposure()
		{
			return this.m_SkyMaterial.GetFloat("_Exposure");

		}


		/////////////////////////////////////////////////////////////////////////////
		// SetWeatherByName
		void	IWeatherManager_Cycles.SetWeatherByName( string weatherName )
		{
			WeatherCycle newCycle = this.m_Cycles.LoadedCycles.Find( c => c.name == weatherName );
			if ( newCycle )
			{
				this.ChangeWeather( newCycle );
			}
		}
		
	#endregion

	#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		// Utility
		/// <summary> Set the corrisponding float value for given time ( HH:MM[:SS] ), return boolean as result </summary>
		public static	bool	TransformTime( string sTime, ref float Time )
		{
			int iH = 0, iM = 0, iS = 0;

			string[] parts = sTime.Split( ':' );
			iH = int.Parse( parts[0] );
			iM = int.Parse( parts[1] );
			iS = parts.Length == 3 ? int.Parse( parts[2] ) : 0;

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
				if (UnityEditor.EditorApplication.isPlaying == true)
				{
					Destroy/*DestroyImmediate*/(this.gameObject);   // Immediate is allowed
				}
				else
				{
					if (!UnityEditor.EditorApplication.isPlaying)
					{
						DestroyImmediate(this.gameObject);  // If not in play we destroy normally
					}
					else
					{
						Destroy(this.gameObject);
					}
				}
#else
				// In BUILD: Destroy normally ( because of singleton this should never happen )
				Debug.Log( "WeatherMnager::Awake: Destroy called in build, this should never happen" );
				Destroy( gameObject );	
#endif
				// in any case return
				return;
			}

			Database.Section debugInfosSection = null;
			if (this.m_ShowDebugInfo == false && GlobalManager.Configs.GetSection( "DebugInfos", ref debugInfosSection ) )
			{
				this.m_ShowDebugInfo = debugInfosSection.AsBool( "WeatherManager", false);
				if (this.m_ShowDebugInfo )
					Debug.Log( "WeatherManager::Awake: : Log Enabled" );
			}
			
#if UNITY_EDITOR
			// In EDITOR: If is editor play mode
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );

			// This callback for the WindowWeatherEditor to update the reference to the current available instance of WeatherManager in scene
			void OnPlayModeStateChanged( UnityEditor.PlayModeStateChange newState )
			{
				//	Debug.Log( newState.ToString() );
				IWeatherManager_Editor a = WindowWeatherEditor.GetWMGR( true );
			}

			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			m_Instance_Editor	= this as IWeatherManager_Editor;
#else
			// In BUILD: DontDestroyOnLoad called normally
			DontDestroyOnLoad( this );
#endif

			m_Instance			= this as IWeatherManager;
			m_Instance_Cycles	= this as IWeatherManager_Cycles;

			this.LoadSkyMixerMaterial();
		}



		/////////////////////////////////////////////////////////////////////////////
		// LoadSkyMixerMaterial
		private	bool	LoadSkyMixerMaterial()
		{
			if (this.m_SkyMaterial )
				return true;

			Material loadedMaterial = Resources.Load<Material>( RESOURCES_SKYMIXER_MAT );

			if (this.m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnEnabled: Loaded Material " + RESOURCES_SKYMIXER_MAT + ": " + ( ( loadedMaterial ) ? "done" : "failed" ) );

			if ( loadedMaterial )
			{
				// Load Sky Material
				this.m_SkyMaterial = new Material(loadedMaterial);
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

			this.LoadSkyMixerMaterial();

			if (this.m_Cycles.IsNotNull() && this.m_Cycles.LoadedCycles.Count > 0 )
			{
				this.Setup_Cycles();

				// Select descriptors
				this.StartSelectDescriptors(this.m_DayTimeNow = Random.value * WeatherManager.DAY_LENGTH );

				// Make first env lerp
				this.EnvironmentLerp();
			}
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
			//			GameManager.StreamEvents.OnSave -= StreamEvents_OnSave;
			//			GameManager.StreamEvents.OnLoad -= StreamEvents_OnLoad;

			this.m_WeatherChoiceFactor	= 1.0f;
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private StreamUnit StreamEvents_OnSave( StreamData streamData )
		{
			StreamUnit streamUnit	= streamData.NewUnit(this.gameObject );
			{
				streamUnit.SetInternal( "DayTimeNow", this.m_DayTimeNow );
				streamUnit.SetInternal( "CycleName", this.m_CurrentCycleName );
			}
			return streamUnit;
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private StreamUnit StreamEvents_OnLoad( StreamData streamData )
		{
			StreamUnit streamUnit = null;
			if ( streamData.GetUnit(this.gameObject, ref streamUnit ) == false )
			{
				this.m_DayTimeNow = streamUnit.GetAsFloat( "DayTimeNow" );
				string cycleName = streamUnit.GetInternal( "CycleName" );

				int index = this.m_Cycles.LoadedCycles.FindIndex( c => c.name == cycleName );
				if ( index > -1 )
				{
					WeatherCycle cycle = this.m_Cycles.LoadedCycles[ index ];
					this.ChangeWeather( cycle );
				}
			}
			return streamUnit;
		}


		private void			SetupSun()
		{
			if (this.m_Sun == null )
			{
				// Create Sun
				Transform child = this.transform.Find( "Sun" );
				if ( child )
				{
					this.m_Sun = child.GetOrAddIfNotFound<Light>();
				}
				//	if ( child == null )
				else
				{
					child = new GameObject( "Sun" ).transform;
					child.SetParent( this.transform );
					this.m_Sun = child.gameObject.AddComponent<Light>();
				}
			}
			this.m_Sun.type				= LightType.Directional;
			this.m_Sun.shadows			= LightShadows.Soft;
			
			if (this.m_ShowDebugInfo )
			{
				Debug.Log( "WeatherManager: Sun configured" );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private void			Setup_Cycles()
		{
			this.SetupSun();

			// Setup for Environment
			RenderSettings.sun		= this.m_Sun;
			RenderSettings.skybox	= this.m_SkyMaterial;

			// Defaults
			string startTime = "09:30:00";
			string startWeather = this.m_Cycles.LoadedCycles[0].name;

			// Get info from settings file
			Database.Section pSection = null;
			if ( GlobalManager.Configs.GetSection( "Time", ref pSection) == true )
			{
				pSection.bAsString( "StartTime",	ref startTime );
				pSection.bAsString( "StartWeather", ref startWeather );
				pSection.bAsFloat(	"TimeFactor",	ref this.m_TimeFactor );
			}

			// Set current time
			if (this.m_DayTimeNow == -1f )
				TransformTime( startTime, ref this.m_DayTimeNow );

			startWeather = startWeather.Replace( "\"", "" );
			this.m_CurrentCycleName = "Invalid";

			if (this.m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Applying time " + startWeather + ", " + startTime );

			// Set current cycle
			int index = this.m_Cycles.LoadedCycles.FindIndex( c => c.name == startWeather );
			if ( index > -1 )
			{
				WeatherCycle cycle = this.m_Cycles.LoadedCycles[ index ];
				// set as current
				this.m_CurrentCycle = cycle;
				// update current descriptors
				this.m_Descriptors =  cycle.LoadedDescriptors;
				// update cycle name
				this.m_CurrentCycleName = cycle.name;
				// current updated
				this.m_EnvDescriptorCurrent = this.m_EnvDescriptorNext;
			}

			this.m_WeatherChoiceFactor = 1.1f;

			this.m_EnvEffectTimer = Random.Range( 2f, 5f );

			// Select descriptors
			if (this.m_Cycles.CyclesPaths.Count > 0 )
				this.StartSelectDescriptors(this.m_DayTimeNow );
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
			if (this.m_EnvDescriptorCurrent == null || this.m_EnvDescriptorNext == null )
				return 0.0f;

			float Current = this.m_EnvDescriptorCurrent.ExecTime;
			float Next = this.m_EnvDescriptorNext.ExecTime;

			float interpolant = 0.0f;
			float fLength = this.TimeDiff( Current, Next );
			if ( Utils.Math.SimilarZero( fLength, Utils.Math.EPS ) == false ) {
				if ( Current > Next )
				{
					if ( ( DayTime >= Current ) || ( DayTime <= Next ) )
						interpolant = this.TimeDiff( Current, DayTime ) / fLength;
				}
				else
				{
					if ( ( DayTime >= Current ) && ( DayTime <= Next ) )
						interpolant = this.TimeDiff( Current, DayTime ) / fLength;
				}
				interpolant = Mathf.Clamp01( interpolant + 0.0001f );
			}
			return interpolant;
			
		}


		////////////////////////////////////////////////////////////////////////////
		// SetCubemaps
		private	void			SetCubemaps()
		{
			this.m_SkyMaterial.SetTexture( "_Skybox1", this.m_EnvDescriptorCurrent.SkyCubemap );
			this.m_SkyMaterial.SetTexture( "_Skybox2", this.m_EnvDescriptorNext.SkyCubemap );
		}


		////////////////////////////////////////////////////////////////////////////
		// GetPreviousDescriptor
		private	EnvDescriptor	GetPreviousDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf(this.m_Descriptors, current );
			return this.m_Descriptors[ ( idx ) == 0 ? this.m_Descriptors.Length - 1 : ( idx - 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// GetNextDescriptor
		private	EnvDescriptor	GetNextDescriptor( EnvDescriptor current )
		{
			int idx = System.Array.IndexOf(this.m_Descriptors, current );
			return this.m_Descriptors[ ( idx + 1 ) == this.m_Descriptors.Length ? 0 : ( idx + 1 ) ];
		}


		////////////////////////////////////////////////////////////////////////////
		// StartSelectDescriptors
		private	void			StartSelectDescriptors( float DayTime, WeatherCycle cycle = null )
		{
			if ( cycle != null )
			{
				this.m_Descriptors = cycle.LoadedDescriptors;
			}

			// get the last valid descriptor where its execTime is less than dayTime
			EnvDescriptor descriptor = System.Array.FindLast(this.m_Descriptors, ( EnvDescriptor d ) => d.ExecTime < DayTime );

			EnvDescriptor first = this.m_Descriptors[ 0 ];
			EnvDescriptor last  = this.m_Descriptors[this.m_Descriptors.Length - 1 ];
			if ( descriptor == last )
			{
				this.m_EnvDescriptorCurrent	= last;
				this.m_EnvDescriptorNext		= first;
			}
			else
			{
				this.m_EnvDescriptorCurrent = descriptor;
				this.m_EnvDescriptorNext = this.GetNextDescriptor( descriptor );
			}

			if (this.m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Descriptors selected: " + this.m_EnvDescriptorCurrent.Identifier + "," + this.m_EnvDescriptorNext.Identifier );

			this.SetCubemaps();
		}


		/////////////////////////////////////////////////////////////////////////////
		// ChangeWeather
		private	void			ChangeWeather( WeatherCycle newCycle )
		{
			// find the corresponding of the current descriptor in the nex cycle
			int correspondingDescriptorIndex = System.Array.FindIndex (newCycle.LoadedDescriptors, ( d => d.Identifier == this.m_EnvDescriptorNext.Identifier ) );
			if ( correspondingDescriptorIndex == -1 )
				return;

			EnvDescriptor correspondingDescriptor = newCycle.LoadedDescriptors[correspondingDescriptorIndex];

			if (this.m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Changing weather, requested: " + newCycle.name );

			// set as current
			this.m_CurrentCycle = newCycle;

			// update current descriptors
			this.m_Descriptors = this.m_CurrentCycle.LoadedDescriptors;

			// current updated
			this.m_EnvDescriptorCurrent = this.m_EnvDescriptorNext;

			this.m_CurrentCycleName = this.m_CurrentCycle.name;

			// get descriptor next current from new cycle
			this.m_EnvDescriptorNext = this.GetNextDescriptor( correspondingDescriptor );
			print( "New cycle: " + newCycle.name );
		}


		/////////////////////////////////////////////////////////////////////////////
		// RandomWeather
		private	void			RandomWeather()
		{
			// Choose a new cycle
			int newIdx = Random.Range( 0, this.m_Cycles.CyclesPaths.Count-1 );
			WeatherCycle cycle = this.m_Cycles.LoadedCycles[ newIdx ];

			if (this.m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Setting random Weather: " + cycle.name );

			this.ChangeWeather( cycle );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		// SelectDescriptors
		private	void			SelectDescriptors( float DayTime )
		{
			bool bSelect = false;

			if (this.m_EnvDescriptorCurrent.ExecTime > this.m_EnvDescriptorNext.ExecTime )
			{
				bSelect = ( DayTime > this.m_EnvDescriptorNext.ExecTime ) && ( DayTime < this.m_EnvDescriptorCurrent.ExecTime );
			}
			else
			{
				bSelect = ( DayTime > this.m_EnvDescriptorNext.ExecTime );
			}
			if ( bSelect )
			{
				// Choice for a new cycle
				float randomValue = Random.value;
				if ( randomValue > (this.m_WeatherChoiceFactor ) )
				{
					this.RandomWeather();
					this.m_WeatherChoiceFactor += randomValue;
				}
				else
				{
					// Editor stuff
					if (this.m_WeatherChoiceFactor <= 1.0f )
						this.m_WeatherChoiceFactor = Mathf.Clamp01(this.m_WeatherChoiceFactor  - 0.2f );
					this.m_EnvDescriptorCurrent = this.m_EnvDescriptorNext;
					this.m_EnvDescriptorNext = this.GetNextDescriptor(this.m_EnvDescriptorNext );
				}
				this.SetCubemaps();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// TimeDiff
		private	void			EnvironmentLerp()
		{
			float interpolant = this.TimeInterpolant(this.m_DayTimeNow );
			this.InterpolateOthers( interpolant );
			this.m_SkyMaterial.SetFloat( "_Interpolant", interpolant );
		}

		
		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void			InterpolateOthers( float interpolant )
		{
			EnvDescriptor current = this.m_EnvDescriptorCurrent;
			EnvDescriptor next = this.m_EnvDescriptorNext;

			this.m_EnvDescriptorMixer.AmbientColor		= Color.Lerp( current.AmbientColor,		next.AmbientColor,	interpolant );
			this.m_EnvDescriptorMixer.FogFactor			= Mathf.Lerp( current.FogFactor,		next.FogFactor,		interpolant );
			this.m_EnvDescriptorMixer.RainIntensity		= Mathf.Lerp( current.RainIntensity,	next.RainIntensity, interpolant );
			this.m_EnvDescriptorMixer.SkyColor			= Color.Lerp( current.SkyColor,			next.SkyColor,		interpolant );
			this.m_EnvDescriptorMixer.SunColor			= Color.Lerp( current.SunColor,			next.SunColor,		interpolant );
			this.m_EnvDescriptorMixer.SunRotation		= Vector3.Lerp( current.SunRotation,	next.SunRotation,	interpolant );

			RenderSettings.ambientSkyColor			= this.m_EnvDescriptorMixer.SkyColor;
			RenderSettings.ambientLight				= this.m_EnvDescriptorMixer.AmbientColor;

			RenderSettings.fog						= this.m_EnvDescriptorMixer.FogFactor > 0.0f;
			RenderSettings.fogDensity				= this.m_EnvDescriptorMixer.FogFactor;

			if ( RainManager.Instance != null )
				RainManager.Instance.RainIntensity	= this.m_EnvDescriptorMixer.RainIntensity;

			this.m_Sun.color								= this.m_EnvDescriptorMixer.SunColor;
		}


		/////////////////////////////////////////////////////////////////////////////
		//TimeDiff
		private	void			AmbientEffectUpdate()
		{
			this.m_EnvEffectTimer -= Time.deltaTime;
			if (this.m_EnvEffectTimer < 0f )
			{
				AudioCollection effectCollection = this.m_EnvDescriptorCurrent.AmbientEffects;
				if ( effectCollection != null )
				{
					AudioClip clip = effectCollection.AudioClips[ Random.Range( 0, effectCollection.AudioClips.Length ) ];
					AudioSource.PlayClipAtPoint( clip, Player.Instance.transform.position );
				}

				this.m_EnvEffectTimer = Random.Range( 3f, 7f );
			}
		}

#endregion

		/////////////////////////////////////////////////////////////////////////////
		// Update
		private void			Update()
		{
#if UNITY_EDITOR
			if ( this.runInEditMode ) // 
				return;
#endif

			if (this.m_EnvDescriptorCurrent.set == false || this.m_EnvDescriptorNext.set == false )
				return;

			this.m_DayTimeNow += Time.deltaTime * this.m_TimeFactor;
			if (this.m_DayTimeNow > DAY_LENGTH )
				this.m_DayTimeNow = 0.0f;

			// Only every 10 frames
			if ( Time.frameCount % 10 == 0 )
				return;

			TransformTime(this.m_DayTimeNow, ref this.m_CurrentDayTime );

			this.SelectDescriptors(this.m_DayTimeNow );

			this.EnvironmentLerp();

			this.AmbientEffectUpdate();

			// Sun rotation by data
			this.m_Sun.transform.rotation = this.m_RotationOffset * Quaternion.LookRotation(this.m_EnvDescriptorMixer.SunRotation );
		}


		/////////////////////////////////////////////////////////////////////////////
		// Reset
		private void			Reset()
		{
			this.m_Cycles				= null;
			this.m_CurrentCycle			= null;
			this.m_CurrentCycleName		= string.Empty;
			this.m_Descriptors			= null;
			this.m_EnvDescriptorCurrent	= null;
			this.m_EnvDescriptorNext		= null;

			m_Instance			= this as IWeatherManager;
			m_Instance_Cycles	= this as IWeatherManager_Cycles;

			this.LoadSkyMixerMaterial();
#if UNITY_EDITOR
			m_Instance_Editor	= this as IWeatherManager_Editor;
			this.runInEditMode = false;
			UnityEditor.EditorApplication.update -= this.EditorUpdate;
#endif
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void			OnDestroy()
		{
		//	if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnDestroy" );
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnApplicationQuit
		private void			OnApplicationQuit()
		{
			this.m_DayTimeNow = 0f;
		}

	}

}