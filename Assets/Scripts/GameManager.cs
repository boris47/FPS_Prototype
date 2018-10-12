
using CFG_Reader;
using UnityEngine;


public partial class GameManager : MonoBehaviour {

	/// <summary> Use this to identify if is executing in editor or in build </summary>
#if UNITY_EDITOR
	public	const	bool InEditor = true;
#else
	public	const	bool InEditor = false;
#endif
	public	static			GameManager		m_Instance				= null;
	public	static			GameManager		Instance
	{
		get { return m_Instance; }
	}

	public	static			Reader			Settings				= null;
	public	static			Reader			Configs					= null;
	public	static			InputManager	InputMgr				= null;

	public	static			bool			IsChangingScene			= false;
	public	static			bool			IsLoadingScene			= false;
	public	static			bool			CanSave					= true;

	private	static			bool			m_InGame				= true;
	public static			bool			InGame
	{
		get { return m_InGame; }
		set { m_InGame = value; }
	}

	private	static			bool			m_QuitRequest			= false;


	[SerializeField]
	private					bool			m_HideCursor			= true;


	private					bool			m_SkipOneFrame			= false;
	private					float			m_ThinkTimer			= 0f;


	//////////////////////////////////////////////////////////////////////////
	private			void		Awake ()
	{
		// SINGLETON
		if ( Instance != null )
		{
			print( "GameManager: Object set inactive" );
			gameObject.SetActive( false );
			return;
		}
		DontDestroyOnLoad( this );
		
		// Instances
		m_Instance		= this;
		m_StreamEvents	= this;
		m_PauseEvents	= this;
		m_UpdateEvents	= this;

		// Internal classes
		InputMgr	= new InputManager();
		Settings	= new Reader();
		Configs		= new Reader();

		new Blackboard.BlackboardSingleton();


		// Load Settings and Configs
		string settingspath		= InEditor ? "Assets/Resources/Settings.txt" : "Settings";
		string configsPath		= InEditor ? "Assets/Resources/Configs/All.txt" : "Configs\\All";

		Settings.LoadFile( settingspath );
		Configs.LoadFile( configsPath );
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		OnEnable()
	{
//		Cursor.visible = false;
//		Cursor.lockState = CursorLockMode.Locked;
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		OnLevelWasLoaded( int level )
	{
		m_InGame = level != 0;
		if ( m_InGame )
		{
			if ( m_HideCursor )
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void		SetTimeScale( float value )
	{
		SoundManager.Instance.Pitch = Time.timeScale = value;
	}



	//////////////////////////////////////////////////////////////////////////
	public	static	void		QuitRequest()
	{
		m_QuitRequest = true;
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		FixedUpdate()
	{
		m_OnPhysicFrame( Time.fixedDeltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		Update()
	{
		if ( m_InGame == false )
			return;

		// This prevent the ui interaction can trigger actions in-game
		if ( m_SkipOneFrame == true )
		{
			m_SkipOneFrame = false;
			return;
		}

		// Update inputs
		InputMgr.Update();

		// Thinking Update
		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > Brain.THINK_TIMER )
		{
			m_OnThink();
			m_ThinkTimer = 0f;
		}

		// Frame Update
		if ( m_IsPaused == false )
		{
			m_OnFrame( Time.deltaTime );
		}


		// Save Event
		if ( Input.GetKeyDown( KeyCode.F5 ) && CanSave == true )
		{
			m_StreamEvents.Save();
		}

		// Load Event
		if ( Input.GetKeyDown( KeyCode.F9 ) )
		{
			m_StreamEvents.Load();
		}

		// Pause Event
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			m_PauseEvents.SetPauseState( !m_IsPaused );
		}

		// Exit request
		if ( m_QuitRequest == true )
		{
			if ( m_SaveLoadCO == null )
			{
				QuitInstanly();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		OnDestroy()
	{
		// Clear instances refs
		m_Instance				= null;
		m_StreamEvents			= null;
		m_PauseEvents			= null;
		m_UpdateEvents			= null;

		// Clear Internal classes
		InputMgr				= null;
		Settings				= null;
		Configs					= null;

		// Internal Vars
		IsChangingScene			= false;
		IsLoadingScene			= false;
		CanSave					= true;

		// Reset Events
		m_OnSave				= null;
		m_OnLoad				= null;
		m_OnPauseSet			= null;
		m_OnThink				= null;
		m_OnFrame				= null;
		m_OnPhysicFrame			= null;

		m_IsPaused				= false;
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void		QuitInstanly()
	{
		if ( InEditor == true )
		{
			UnityEditor.EditorApplication.isPlaying = false;
		} 
		else
		{
#pragma warning disable CS0162 // È stato rilevato codice non raggiungibile
			Application.Quit();
#pragma warning restore CS0162 // È stato rilevato codice non raggiungibile
		}
	}

}
