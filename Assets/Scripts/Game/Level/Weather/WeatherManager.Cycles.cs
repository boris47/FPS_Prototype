
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Cycles {

		void						SetWeatherByName( string weatherName );
		void						SetTime( float DayTime );
		void						SetTime( float H, float M, float S );
		void						SetTime( string sTime );
		string						GetTimeAsString();

		Material					SkyMixerMaterial					{ get; }
		string						CurrentCycleName					{ get; }
		bool						AreResLoaded						{ get; }
	}


	public partial class WeatherManager : IWeatherManager_Cycles {

		private	static	IWeatherManager_Cycles	m_CyclesInstance	= null;
		public	static	IWeatherManager_Cycles	Cycles
		{
			get { return m_CyclesInstance; }
		}

		Material			IWeatherManager_Cycles.SkyMixerMaterial					{ get { return m_SkyMaterial; } }
		bool				IWeatherManager_Cycles.AreResLoaded						{ get { return m_AreResLoaded_Cylces; } }

		[Header( "Cycles" )]

		[SerializeField, ReadOnly]
		private		Weathers					m_Cycles					= null;
		[SerializeField, ReadOnly ]
		private		WeatherCycle				m_CurrentCycle				= null;
		[ SerializeField, ReadOnly ]
		private		string						m_CurrentCycleName			= string.Empty;
		[SerializeField, ReadOnly]
		private		List<EnvDescriptor>			m_Descriptors				= null;
		private		EnvDescriptor				m_EnvDescriptorCurrent		= null;
		private		EnvDescriptor				m_EnvDescriptorNext			= null;
		private		EnvDescriptorMixer			m_EnvDescriptorMixer		= new EnvDescriptorMixer();
		private		Material					m_SkyMaterial				= null;
		private		float						m_EnvEffectTimer			= 0f;

		private 	Quaternion					m_RotationOffset			= Quaternion.AngleAxis( 180f, Vector3.up );
		[SerializeField, ReadOnly ]
		private		bool						m_AreResLoaded_Cylces		= false;


#region INTERFACE

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
			TansformTime( sTime, ref m_DayTimeNow );
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
		// SetWeatherByName
		void	IWeatherManager_Cycles.SetWeatherByName( string weatherName )
		{
//			if ( m_CurrentCycle != null && m_CurrentCycle.name == weatherName )
//				return;

			WeatherCycle newCycle = m_Cycles.LoadedCycles.Find( c => c.name == weatherName );
			if ( newCycle == null )
				return;

			ChangeWeather( newCycle );
		}

#endregion

		/////////////////////////////////////////////////////////////////////////////
		private	void			Awake_Cycles()
		{
			m_CyclesInstance	= this as IWeatherManager_Cycles;
		}

		/////////////////////////////////////////////////////////////////////////////
		private	void			OnEnable_Cycles()
		{
			m_CyclesInstance	= this as IWeatherManager_Cycles;
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void			OnDisable_Cycles()
		{
			m_AreResLoaded_Cylces	= false;

			Reset_Cycles();
		}


		/////////////////////////////////////////////////////////////////////////////
		private IEnumerator		Start_Cycles()
		{
			yield return null;
#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
				yield break;
#endif
			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Loading Cycles" );

			if ( Editor.INTERNAL_EditorLinked == false )
			{
				m_AreResLoaded_Cylces	= false;

				ResourceManager.LoadData<Weathers> cycles = new ResourceManager.LoadData<Weathers>();
				System.Action<Weathers> onLoaded = delegate( Weathers weathers )
				{
					m_Cycles = weathers;
					m_AreResLoaded_Cylces = true;
				};
				yield return ResourceManager.LoadResourceAsyncCoroutine
				(
					ResourcePath: WEATHERS_COLLECTION,
					loadData: cycles,
				 	OnResourceLoaded: onLoaded
				);
			}

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Loading cycles " + ( m_AreResLoaded_Cylces ? "done" : "failed") );

			if ( m_AreResLoaded_Cylces )
				Setup_Cycles();
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void			Update_Cycles()
		{
			if ( m_IsOK == false )
				return;

			if ( m_AreResLoaded_Cylces == false )
				return;

			if ( m_EnvDescriptorCurrent.set == false || m_EnvDescriptorNext.set == false )
				return;

#if UNITY_EDITOR
			// Only every 10 frames
			if ( UnityEditor.EditorApplication.isPlaying == true && Time.frameCount % 10 == 0 )
				return;
			
//			if ( EnableInEditor == false && UnityEditor.EditorApplication.isPlaying == false )
//				return;
#else
			// Only every 10 frames
			if ( Time.frameCount % 10 == 0 )
				return;
#endif

			if ( Editor.INTERNAL_EditorCycleLinked == false )
			{
				m_DayTimeNow += Time.deltaTime * m_TimeFactor; // + Level()->GetTimeFactor();
			}

			if ( m_DayTimeNow > DAY_LENGTH )
				m_DayTimeNow = 0.0f;

			SelectDescriptors( m_DayTimeNow );
			EnvironmentLerp();

			AmbientEffectUpdate();

			// Sun rotation by data
			if ( Editor.INTERNAL_EditorDescriptorLinked == false )
			{
				m_Sun.transform.rotation = m_RotationOffset * Quaternion.LookRotation( m_EnvDescriptorMixer.SunRotation );
			}


			TransformTime( m_DayTimeNow, ref CurrentDayTime );
		}

		/////////////////////////////////////////////////////////////////////////////
		private	void			Reset_Cycles()
		{
			m_AreResLoaded_Cylces	= false;
			m_Cycles				= null;
			m_CurrentCycle			= null; 
			m_CurrentCycleName		= string.Empty;
			m_Descriptors			= null;
			m_EnvDescriptorCurrent	= null;
			m_EnvDescriptorNext		= null;
		}

		
		/////////////////////////////////////////////////////////////////////////////
		private void			Setup_Cycles()
		{
			if ( m_Sun == null )
			{
				// Create Sun
				{
					Transform child = transform.Find( "Sun" );
					if ( child != null )
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
				TansformTime( startTime, ref m_DayTimeNow );

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
				m_Descriptors = cycle.LoadedDescriptors;
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
				m_Descriptors = cycle.LoadedDescriptors;
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
			EnvDescriptor correspondingDescriptor = newCycle.LoadedDescriptors.Find( d => d.ExecTime == m_EnvDescriptorNext.ExecTime );
			if ( correspondingDescriptor == null )
				return;

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager: Changing weather, requested: " + newCycle.name );

			// set as current
			m_CurrentCycle = newCycle;

			// update current descriptors
			m_Descriptors = m_CurrentCycle.LoadedDescriptors;
					
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
			int newIdx = Random.Range( 0, m_Cycles.CyclesPaths.Count );
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
	}

}