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


	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoad ()
	{
		UnityEngine.Assertions.Assert.raiseExceptions = true;
		Debug.developerConsoleVisible = true;
		print( "GlobalManager::OnBeforeSceneLoad" );
	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad ()
	{
		print( "GlobalManager::AfterSceneLoad" );
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

		Settings.LoadFile( settingspath );
		Configs.LoadFile( configsPath );

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

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			new GameObjectsPool<Transform>( new GameObjectsPoolConstructorData<Transform>() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		static		void		QuitInstanly()
	{
		Debug.Log( "GlobalManager: Exiting" );
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}


	//////////////////////////////////////////////////////////////////////////
	public	static void		Assert( bool condition, string message )
	{
		if ( condition == false )
		{
			Debug.LogError( message );
			ForcedQuit();
		}
	}


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
