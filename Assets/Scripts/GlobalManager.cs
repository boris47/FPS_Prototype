using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyFileLogHandler : ILogHandler
{
    private System.IO.FileStream m_FileStream = null;
    private System.IO.StreamWriter m_StreamWriter = null;

	private ILogHandler m_DefaultLogHandler = Debug.unityLogger.logHandler;

	//////////////////////////////////////////////////////////////////////////
    public MyFileLogHandler()
    {
		// Application.persistentDataPath: C:/Users/Drako/AppData/LocalLow/BeWide&Co/Project Orion
		
        string filePath = Application.dataPath + "/MyLogs.txt";

        m_FileStream = new System.IO.FileStream( filePath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite );
        m_StreamWriter = new System.IO.StreamWriter( m_FileStream );

		// Replace the default debug log handler
		Debug.unityLogger.logHandler = this;
    }


	//////////////////////////////////////////////////////////////////////////
    public void LogFormat( LogType logType, UnityEngine.Object context, string format, params object[] args )
    {
        m_StreamWriter.WriteLine( System.String.Format( format, args ) );
        m_StreamWriter.Flush();
        m_DefaultLogHandler.LogFormat( logType, context, format, args );
    }


	//////////////////////////////////////////////////////////////////////////
    public void LogException( System.Exception exception, UnityEngine.Object context )
    {
        m_DefaultLogHandler.LogException( exception, context );
    }
}



public class GlobalManager : MonoBehaviour {

	private	static			GlobalManager	m_Instance				= null;
	public	static			GlobalManager	Instance
	{
		get { return m_Instance; }
	}

	// Load Settings and Configs
	private	const string settingspath		= "Settings";
	private	const string configsPath		= "Configs\\All";

	public	static			bool			bIsChangingScene			= false;
	public	static			bool			bIsLoadingScene				= false;
	public	static			bool			bCanSave					= true;


	private	static			SectionMap		m_Settings				= null;
	public	static			SectionMap		Settings
	{
		get { return m_Settings; }
	}


	private	static			SectionMap		m_Configs				= null;
	public	static			SectionMap		Configs
	{
		get { return m_Configs; }
	}

	private					InputManager	m_InputMgr				= null;
	public					InputManager	InputMgr
	{
		get { return m_InputMgr; }
	}


	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
		if ( Application.isEditor == false )
		{
			Application.logMessageReceived += HandleException;
			new MyFileLogHandler();
		}

		m_Settings	= new SectionMap();
		m_Configs	= new SectionMap();

		m_Settings.LoadFile( settingspath );
		m_Configs.LoadFile( configsPath );
	}

	static void HandleException( string condition, string stackTrace, LogType type )
    {
		if ( type == LogType.Exception || type == LogType.Assert || type == LogType.Error )
		{
			QuitInstanly();
		}
    }

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// SINGLETON
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance		= this;

		m_InputMgr	= new InputManager();
		m_InputMgr.Setup();
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
			CustomSceneManager.LoadSceneSync( new CustomSceneManager.LoadSceneData() { iSceneIdx = SceneEnumeration.MAIN_MENU } );
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
