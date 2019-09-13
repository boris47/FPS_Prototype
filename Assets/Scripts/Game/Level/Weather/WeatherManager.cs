
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager {

		Transform					Transform				{ get; }
		float						TimeFactor				{ get; set; }
		float						DayTime					{ get; }
		Light						Sun						{ get; }
	}

	// CLASS
	public partial class WeatherManager : MonoBehaviour, IWeatherManager {

#region VARS

		// STATIC
		private	static	IWeatherManager			m_Instance					= null;
		public	static	IWeatherManager			Instance
		{
			get { return m_Instance; }
		}

		// CONST
		public	const	float					DAY_LENGTH					= 86400f;
		public	const	string					WEATHERS_COLLECTION			= "Scriptables/WeatherCollection";
		private	const	string					SKYMIXER_MATERIAL			= "Materials/SkyMixer";

		[Header("Main")]
		[ SerializeField, ReadOnly ]
		private	string							CurrentDayTime				= string.Empty;

		// SERIALIZED
		[ Header("Weather Info") ]

		[ SerializeField, Range( 1f, 500f ) ]
		private	float							m_TimeFactor				= 1.0f;

		[ SerializeField, ReadOnly ]
		private		float						m_WeatherChoiceFactor		= 1.0f;

		[SerializeField, ReadOnly]
		private		bool						m_IsOK						= true;
		
		// PRIVATE VARS
		private	Light							m_Sun						= null;
		private	static float					m_DayTimeNow				= -1.0f;
		private	static bool						m_ShowDebugInfo				= false;

	#region INTERFACE

		float IWeatherManager.TimeFactor
		{
			get { return m_TimeFactor; }
			set { m_TimeFactor =  Mathf.Max( value, 0f ); }
		}
		Light			IWeatherManager.Sun
		{
			get { return m_Sun; }
		}

		Transform		IWeatherManager.Transform							{ get { return transform; } }
		float			IWeatherManager.DayTime
		{
			get { return m_DayTimeNow; }
		}

	#endregion


#endregion

#region STATIC MEMBERS

		/////////////////////////////////////////////////////////////////////////////
		// Utility
		/// <summary> Set the corrisponding float value for given time ( HH:MM:SS ), return boolean as result </summary>
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
		/// <summary> Convert the given float value into a formatted string ( HH:MM:SS ), optionally can append seconds </summary>
		public	static	void	TransformTime( float fTime, ref string Time, bool considerSeconds = true )
		{
			int iH = ( int ) (   fTime / ( 3600f ) );
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
				if ( UnityEditor.EditorApplication.isPlaying == true )
					DestroyImmediate( gameObject );
				else
					Destroy( gameObject );
#else
				Destroy( gameObject );
#endif
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
			if ( UnityEditor.EditorApplication.isPlaying == true )
				DontDestroyOnLoad( this );
#else
			DontDestroyOnLoad( this );
#endif

			m_Instance			= this as IWeatherManager;
			Awake_Cycles();
			Awake_Editor();
		}

		/*
		/////////////////////////////////////////////////////////////////////////////
		// OnLevelLoaded
		private void OnLevelLoaded( UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadMode )
		{
			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnLevelLoaded: : On Level " + (SceneEnumeration)scene.buildIndex + " loaded" );

			// Load Sky Material
			m_SkyMaterial = Resources.Load<Material>( SKYMIXER_MATERIAL );

			// Setup for Environment
			RenderSettings.sun		= m_Sun;
			RenderSettings.skybox	= m_SkyMaterial;
		}
		*/

		/////////////////////////////////////////////////////////////////////////////
		// OnEnable
		private void			OnEnable()
		{
//			CustomSceneManager.RegisterOnLoad( OnLevelLoaded );

#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false )
			{
				UnityEditor.EditorApplication.update = Update;
			}
#endif

//			GameManager.StreamEvents.OnSave += StreamEvents_OnSave;
//			GameManager.StreamEvents.OnLoad += StreamEvents_OnLoad;
			
			// Load Sky Material
			m_SkyMaterial = Resources.Load<Material>( SKYMIXER_MATERIAL );

			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnEnabled: Loaded Material " + SKYMIXER_MATERIAL + ": " + ( ( m_SkyMaterial != null ) ? "done":"failed" ) );

			OnEnable_Cycles();
			OnEnable_Editor();
		}



		/////////////////////////////////////////////////////////////////////////////
		// OnDisable
		private void			OnDisable()
		{
//			CustomSceneManager.UnregisterOnLoad( OnLevelLoaded );

#if UNITY_EDITOR
			if ( UnityEditor.EditorApplication.isPlaying == false && Editor.INTERNAL_EditorLinked == false )
			{
				UnityEditor.EditorApplication.update = null;
			}
#endif

//			GameManager.StreamEvents.OnSave -= StreamEvents_OnSave;
//			GameManager.StreamEvents.OnLoad -= StreamEvents_OnLoad;

			OnDisable_Editor();
			OnDisable_Cycles();

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
		// START
		private IEnumerator		Start()
		{
			// Start Modules
			{
				const int processCount = 2;

				enabled = false; // prevent update method to be called before resources are loaded
				CoroutinesManager.AddCoroutineToPendingCount(processCount);
				{
					yield return CoroutinesManager.Start( Start_Cycles(), "WeatherManager::Start: Start of cycles" );
					yield return CoroutinesManager.Start( Start_Editor(), "Wheathermanger::Start: Start of editor" );
				}
				enabled = true;
				CoroutinesManager.RemoveCoroutineFromPendingCount(processCount);
			}

			if ( m_IsOK == false )
			{
				Debug.Log( "WeatherManager: Something goes wrong" );
				yield break;
			}

			if ( m_AreResLoaded_Cylces )
			{
				m_WeatherChoiceFactor = 1.1f;

				// Select descriptors
				StartSelectDescriptors( m_DayTimeNow );

				// Make first env lerp
				EnvironmentLerp();
			}
		}

#endregion

		/////////////////////////////////////////////////////////////////////////////
		// Update
		private void			Update()
		{
			if ( m_AreResLoaded_Cylces == false )
			{
				return;
			}
			Update_Cycles();
			Update_Editor();
		}


		/////////////////////////////////////////////////////////////////////////////
		// Reset
		private void			Reset()
		{
			m_IsOK = true;
			Reset_Cycles();
			Reset_Editor();
#if UNITY_EDITOR
			this.runInEditMode = false;
#endif
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnDestroy
		private void			OnDestroy()
		{
			if ( m_ShowDebugInfo )
				Debug.Log( "WeatherManager::OnEnabled: OnDestroy" );
		}


		/////////////////////////////////////////////////////////////////////////////
		// OnApplicationQuit
		private void			OnApplicationQuit()
		{
			m_DayTimeNow = -1f;

			if ( m_INTERNAL_EditorLinked == false )
			{
				m_Cycles.LoadedCycles.ForEach( c => WeatherCycle.OnEndPlay(c) );
			}
		}

	}

}