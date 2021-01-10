﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
class EditorInitializer
{
	static EditorInitializer ()
	{
		// Assets/Resources/Scriptables/WeatherCollection.asset
		string assetPath = System.IO.Path.Combine( WeatherSystem.WindowWeatherEditor.ASSETS_SCRIPTABLES_PATH, $"{WeatherSystem.WeatherManager.RESOURCES_WEATHERSCOLLECTION}.asset");
		if (System.IO.File.Exists(assetPath))
		{
			WeatherSystem.Weathers weathers = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherSystem.Weathers>( assetPath );
			UnityEngine.Assertions.Assert.IsNotNull
			(
				weathers,
				"Cannot preload weather cycles"
			);
			Debug.Log( "Weathers cycles preloaded!" );
		}
	}
	/*
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void LoadLoadingScene()
	{
		if (!SceneManager.GetSceneByBuildIndex( (int)ESceneEnumeration.LOADING).isLoaded)
		{
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Loading.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
		}
	}
	*/
}
#endif

public class CustomLogHandler : ILogHandler
{
    private System.IO.FileStream m_FileStream = null;
    private System.IO.StreamWriter m_StreamWriter = null;

	public static ILogHandler m_DefaultLogHandler { get; private set; }

	private readonly System.Globalization.CultureInfo cultureInfo = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();

	//////////////////////////////////////////////////////////////////////////
	public CustomLogHandler()
    {
        m_DefaultLogHandler = Debug.unityLogger.logHandler;
        Debug.unityLogger.logHandler = this;

		{
			string filePath = $"{Application.dataPath}/SessionLog.log";
			m_FileStream = new System.IO.FileStream( filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write );
			m_StreamWriter = new System.IO.StreamWriter(m_FileStream );
		}

		cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
    }

	//////////////////////////////////////////////////////////////////////////
    public void LogFormat( LogType logType, Object context, string format, params object[] args )
    {
		m_StreamWriter.WriteLine( $"[{Time.time.ToString("0.000", cultureInfo)}] {System.String.Format( format, args )}" );
        m_DefaultLogHandler.LogFormat( logType, context, format, args );
    }


	//////////////////////////////////////////////////////////////////////////
    public void LogException( System.Exception exception, Object context )
    {
		m_StreamWriter.WriteLine( $"[{Time.time.ToString("0.000", cultureInfo)}] {exception.Message}" );
		m_StreamWriter.WriteLine( exception.StackTrace );
		m_StreamWriter.Flush();
        m_DefaultLogHandler.LogException( exception, context );
    }
	

	//////////////////////////////////////////////////////////////////////////
	public  void UnSetup()
	{
		m_StreamWriter.Flush();
		m_StreamWriter.Close();
		m_FileStream.Close();

		Debug.unityLogger.logHandler = m_DefaultLogHandler;
	}
	
}



public class GlobalManager : SingletonMonoBehaviour<GlobalManager>
{
#if UNITY_EDITOR
	private short updateCount = 0;
    private short fixedUpdateCount = 0;
    private short updateUpdateCountPerSecond;
    private short updateFixedUpdateCountPerSecond;
	private GUIStyle fontSize = null;

	private void Awake()
	{
		IEnumerator Loop()
		{
			while (true)
			{
				yield return new WaitForSeconds(1);
				updateUpdateCountPerSecond = updateCount;
				updateFixedUpdateCountPerSecond = fixedUpdateCount;

				updateCount = 0;
				fixedUpdateCount = 0;
			}
		}
		StartCoroutine(Loop());
	}

	private void FixedUpdate()
	{
		fixedUpdateCount += 1;
	}

	private void OnGUI()
	{
		fontSize = fontSize ?? new GUIStyle( GUI.skin.GetStyle( "label" ) )
		{
			fontSize = 10
		};
		GUI.Label(new Rect(20,  5, 200, 50), $"Update: {updateUpdateCountPerSecond.ToString()}", fontSize );
        GUI.Label(new Rect(20, 15, 200, 50), $"FixedUpdate: {updateFixedUpdateCountPerSecond.ToString()}", fontSize );
	}
#endif

	private	const			string				m_SettingsFilePath	= "Settings";
	private	const			string				m_ConfigsFilePath	= "Configs/All";


	private static			CustomLogHandler	m_LoggerInstance	= null;
	public	static			bool				bIsChangingScene	= false;
	public	static			bool				bIsLoadingScene		= false;
	public	static			bool				bCanSave			= true;


	private	static			SectionMap			m_Settings			= null;
	public	static			SectionMap			Settings
	{
		get {
			if ( m_Settings == null )
			{
				m_Settings = new SectionMap(m_SettingsFilePath);
			}
			return m_Settings;
		}
	}


	private	static			SectionMap			m_Configs			= null;
	public	static			SectionMap			Configs
	{
		get {
			if ( m_Configs == null )
			{
				m_Configs = new SectionMap(m_ConfigsFilePath);
			}
			return m_Configs;
		}
	}

	public static InputManager InputMgr = null;


	//////////////////////////////////////////////////////////////////////////
	private static void HandleException( string condition, string stackTrace, LogType type )
    {
		switch ( type )
		{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
			{
				QuitInstanly();
				break;
			}
		}
    }


	//////////////////////////////////////////////////////////////////////////
	protected override void OnInitialize()
	{
		InputMgr = new InputManager();
//		if ( Application.isEditor == false )
		{
			Application.logMessageReceived += HandleException;
		}

		if ( Application.isEditor == false )
		{
			m_LoggerInstance = new CustomLogHandler();
		}

		// Whether physics queries should hit back-face triangles.
		Physics.queriesHitBackfaces = false;

		// Priority of background loading thread.
		Application.backgroundLoadingPriority = ThreadPriority.Low;

		// Async texture upload provides timesliced async texture upload on the render thread
		// with tight control over memory and timeslicing. There are no allocations except
		// for the ones which driver has to do. To read data and upload texture data a ringbuffer
		// whose size can be controlled is re-used. Use asyncUploadBufferSize to set the
		// buffer size for asynchronous texture uploads. The size is in megabytes. Minimum
		// value is 2 and maximum is 512. Although the buffer will resize automatically
		// to fit the largest texture currently loading, it is recommended to set the value
		// approximately to the size of biggest texture used in the Scene to avoid re-sizing
		// of the buffer which can incur performance cost.
		QualitySettings.asyncUploadBufferSize = 24; // MB

		// If not already loaded from previous scenes, ensure scene loaded for next sequence
		Scene loadingScene = SceneManager.GetSceneByBuildIndex( (int) ESceneEnumeration.LOADING );
		if (!loadingScene.isLoaded)
		{
			CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.LOADING,
				eMode = LoadSceneMode.Additive
			};
			CustomSceneManager.LoadSceneSync( loadSceneData );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		if ( m_LoggerInstance != null )
			m_LoggerInstance.UnSetup();
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetCursorVisibility( bool newState )
	{
		Cursor.visible = newState;
		Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetTimeScale( float value )
	{
		SoundManager.Pitch = value;

		Time.timeScale = value;
	}


	//	float maximum = 1;
	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
#if UNITY_EDITOR
		updateCount++;
#endif
		/*
		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			System.Diagnostics.Stopwatch m_StopWatch = new System.Diagnostics.Stopwatch();
			m_StopWatch.Start();
			for ( int i = 0; i < maximum; i++ )
			{
				SectionMap sectionMap = new SectionMap();
				sectionMap.LoadFile( configsPath ); 
			}
			m_StopWatch.Stop();

			print( "Performance test: operations done in " + m_StopWatch.Elapsed.Milliseconds + "ms, maximum iterations " + maximum );
			maximum *= 2f;
		}
		*/

		if ( Input.GetKeyDown( KeyCode.K ) )
		{
			SoundManager.Pitch = Time.timeScale = 0.02f;
		}

		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			UIManager.PauseMenu.ReturnToMenu();
//			Destroy( UIManager.InGame?.transform.parent.gameObject );
//			CustomSceneManager.LoadSceneSync( new CustomSceneManager.LoadSceneData() { eScene = ESceneEnumeration.MAIN_MENU } );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		QuitInstanly()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}


	/*//////////////////////////////////////////////////////////////////////////
	public	static void		Assert( bool condition, string message )
	{
		if ( condition == false )
		{
			throw new System.Exception( message );
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	public	static	void	ForcedQuit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

}
