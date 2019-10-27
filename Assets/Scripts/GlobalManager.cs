using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
class EditorInitializer
{
    static EditorInitializer ()
    {
		const string assetPath = WeatherSystem.WindowWeatherEditor.RESOURCE_PATH + "/" + WeatherSystem.WindowWeatherEditor.COLLECTION_FILENAME + ".asset";
		var a = UnityEditor.AssetDatabase.LoadAssetAtPath<WeatherSystem.Weathers>( assetPath );
		UnityEngine.Assertions.Assert.IsNotNull
		(
			a,
			"Cannot preload weather cycles"
		);
    }
}
#endif

public class CustomFileLogHandler : ILogHandler
{
    private System.IO.FileStream m_FileStream = null;
    private System.IO.StreamWriter m_StreamWriter = null;

	public static ILogHandler m_DefaultLogHandler { get; private set; }

	private System.Globalization.CultureInfo cultureInfo = (System.Globalization.CultureInfo)System.Globalization.CultureInfo.InvariantCulture.Clone();

	//////////////////////////////////////////////////////////////////////////
	public CustomFileLogHandler()
    {
        m_DefaultLogHandler = Debug.unityLogger.logHandler;
        Debug.unityLogger.logHandler = this;

		string filePath = Application.dataPath + "/SessionLog.log";
		{
			m_FileStream = new System.IO.FileStream( filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write );
			m_StreamWriter = new System.IO.StreamWriter( m_FileStream );
		}

		cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
    }

	//////////////////////////////////////////////////////////////////////////
    public void LogFormat( LogType logType, Object context, string format, params object[] args )
    {
        m_StreamWriter.WriteLine( "[" + Time.time.ToString("0.000", cultureInfo) + "]" +  System.String.Format( format, args ) );
        m_DefaultLogHandler.LogFormat( logType, context, format, args );
    }


	//////////////////////////////////////////////////////////////////////////
    public void LogException( System.Exception exception, Object context )
    {
		m_StreamWriter.WriteLine( "[" + Time.time.ToString("0.000", cultureInfo) + "]" + exception.Message );
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



public class GlobalManager : MonoBehaviour {

	private static			CustomFileLogHandler m_LoggerInstance	= null;

	private	static			GlobalManager	m_Instance				= null;
	public	static			GlobalManager	Instance
	{
		get { return m_Instance; }
	}

	private	static			bool			m_IsInitialized			= false;

	// Load Settings and Configs
	private	const string settingspath		= "Settings";
	private	const string configsPath		= "Configs/All";

	public	static			bool			bIsChangingScene		= false;
	public	static			bool			bIsLoadingScene			= false;
	public	static			bool			bCanSave				= true;


	private	static			SectionMap		m_Settings				= null;
	public	static			SectionMap		Settings
	{
		get {
			if ( m_Settings == null )
			{
				m_Settings	= new SectionMap();
				m_Settings.LoadFile( settingspath );
			}
			return m_Settings;
		}
	}


	private	static			SectionMap		m_Configs				= null;
	public	static			SectionMap		Configs
	{
		get {
			if ( m_Configs == null )
			{
				m_Configs	= new SectionMap();
				m_Configs.LoadFile( configsPath );
			}
			return m_Configs;
		}
	}

	private	static				InputManager	m_InputMgr				= null;
	public	static				InputManager	InputMgr
	{
		get { return m_InputMgr; }
	}

	
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
		if ( Application.isEditor == false )
		{
			Application.logMessageReceived += HandleException;
		}

		Debug.developerConsoleVisible = true;
		Physics.queriesHitBackfaces = false;
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		QualitySettings.asyncUploadBufferSize = 24; // MB

		m_LoggerInstance = new CustomFileLogHandler();
	}


	public void OnBeforeSceneActivation()
	{
		Debug.Log("GlobalManager::OnBeforeSceneActivation");
	}

	public void OnAfterSceneActivation()
	{
		Debug.Log("GlobalManager::OnAfterSceneActivation");
	}

	public void OnAfterLoadedData()
	{
		Debug.Log("GlobalManager::OnAfterLoadedData");
	}




	static void HandleException( string condition, string stackTrace, LogType type )
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
	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private	static	void	Initialize()
	{
		if ( m_IsInitialized == false )
		{
			m_Instance = FindObjectOfType<GlobalManager>();
			if ( m_Instance == null )
			{
				m_Instance = new GameObject("GlobalManager").AddComponent<GlobalManager>();
			}
			m_Instance.hideFlags = HideFlags.DontSave;
			m_IsInitialized = true;

			DontDestroyOnLoad( m_Instance );

			m_InputMgr	= new InputManager();
			m_InputMgr.Setup();
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( m_Instance != this )
			return;

		m_IsInitialized = false;
		m_Instance = null;

		m_LoggerInstance.UnSetup();
	}



	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetCursorVisibility( bool newState )
	{
		Cursor.visible = newState;
		Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
//		Debug.Log( "SetCursorVisibility: " + newState );
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		SetTimeScale( float value )
	{
		SoundManager.Instance.Pitch = Time.timeScale = value;
	}

//	float maximum = 1;
	private void Update()
	{
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

			print( "Performance test: operaztions done in " + m_StopWatch.Elapsed.Milliseconds + "ms, maximium iterations " + maximum );
			maximum *= 2f;
		}
		*/

		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			Destroy( UIManager.InGame.transform.parent.gameObject );
			CustomSceneManager.LoadSceneSync( new CustomSceneManager.LoadSceneData() { eScene = SceneEnumeration.MAIN_MENU } );
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
