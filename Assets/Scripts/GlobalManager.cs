using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
		Application.logMessageReceived += HandleException;
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

		m_Settings	= new SectionMap();
		m_Configs	= new SectionMap();

		m_Settings.LoadFile( settingspath );
		m_Configs.LoadFile( configsPath );

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
