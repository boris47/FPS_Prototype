
using CFG_Reader;
using UnityEngine;

[System.Serializable]
public class GameEvent      : UnityEngine.Events.UnityEvent { }

public	delegate	void	OnTriggerCall( Collider collider );
public	delegate	void	OnPauseSet( bool isPaused );
public	delegate	void	OnThink();


public partial class GameManager : MonoBehaviour {

    /// <summary> Use this to identity is executing in editor or in build </summary>
#if UNITY_EDITOR
    public	const	bool InEditor = true;
#else
	public	const	bool InEditor = false;
#endif

	public	static			Reader			Settings				= null;
	public	static			Reader			Configs					= null;
	public	static			InputManager	InputMgr				= null;
	public	static			GameManager		Instance				= null;
	public	static			bool			IsChangingScene			= false;
	public	static			bool			IsLoadingScene			= false;
	public	static			bool			CanSave					= true;


	private static event	OnPauseSet		m_OnPauseSet			= null;
	public	static			OnPauseSet		OnPauseSet
	{
		get { return m_OnPauseSet; }
		set { if ( value != null ) m_OnPauseSet = value; }
	}

	private static event	OnThink			m_OnThink				= null;
	public	static			OnThink			OnThink
	{
		get { return m_OnThink; }
		set { if ( value != null ) m_OnThink = value; }
	}

	private	static			bool			m_InGame				= true;
	public static			bool			InGame
	{
		get { return m_InGame; }
		set { m_InGame = value; }
	}

	private static			bool			m_IsPaused				= false;
	public static			bool			IsPaused
	{
		get { return m_IsPaused; }
		set { OnPauseChange( value ); }
	}

	private	static			bool			m_QuitRequest			= false;

	[SerializeField]
	private					bool			m_HideCursor			= true;


	private					bool			m_SkipOneFrame			= false;
	private					float			m_ThinkTimer            = 0f;

	// Pause vars
	private					float			m_PrevTimeScale			= 1f;
	private					bool			m_PrevCanParseInput		= false;
	private					bool			m_PrevInputEnabled		= false;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake ()
	{
		// SINGLETON
		if ( Instance != null )
		{
			print( "GameManager: Object set inactive" );
//			Destroy( gameObject );
			gameObject.SetActive( false );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		// Internal classes
		InputMgr = new InputManager();
		Settings = new Reader();
		Configs = new Reader();

		// Load Settings and Configs
		string settingspath = InEditor ? "Assets/Resources/Settings.txt" : "Settings";
		string configsPath = InEditor ? "Assets/Resources/Configs/All.txt" : "Configs\\All";
		Settings.LoadFile( settingspath );
		Configs.LoadFile( configsPath );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
//		Cursor.visible = false;
//		Cursor.lockState = CursorLockMode.Locked;
	}


	public	static	void	SetTimeScale( float value )
	{
		SoundManager.Instance.Pitch = Time.timeScale = value;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnLevelWasLoaded
	private void OnLevelWasLoaded( int level )
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
	// OnPauseChange
	private static	void	OnPauseChange( bool isPaused )
	{
		m_IsPaused = isPaused;
		OnPauseSet( IsPaused );
	}


	//////////////////////////////////////////////////////////////////////////
	// TooglePauseState
	public	void	TooglePauseState()
	{
		IsPaused = !IsPaused;
		UI.Instance.TooglePauseMenu();

		Cursor.visible = IsPaused;
		Cursor.lockState = IsPaused == true ? CursorLockMode.None : CursorLockMode.Locked;
		
		// Pausing
		if ( m_IsPaused == true )
		{
			m_PrevTimeScale							= Time.timeScale;
			m_PrevCanParseInput						= CameraControl.Instance.CanParseInput;
			m_PrevInputEnabled						= InputManager.IsEnabled;
			Time.timeScale							= 0f;
			CameraControl.Instance.CanParseInput	= false;
			InputManager.IsEnabled					= false;
		}
		else
		{
			Time.timeScale							= m_PrevTimeScale;
			CameraControl.Instance.CanParseInput	= m_PrevCanParseInput;
			InputManager.IsEnabled					= m_PrevInputEnabled;
		}
		m_SkipOneFrame = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	public	static	void	QuitRequest()
	{
		m_QuitRequest = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		if ( m_InGame == false )
			return;

		// Update inputs
		InputMgr.Update();

		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > AI.Brain.THINK_TIMER )
		{
			m_OnThink();
			m_ThinkTimer = 0f;
		}

		// This prevent the ui interaction can trigger actions in-game
		if ( m_SkipOneFrame == true )
		{
			m_SkipOneFrame = false;
			return;
		}

		// Save
		if ( Input.GetKeyDown( KeyCode.F5 ) && CanSave == true )
		{
			Save();
		}

		// Load
		if ( Input.GetKeyDown( KeyCode.F9 ) )
		{
			Load();
		}

		// APPLICATION QUIT REQUEST
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			TooglePauseState();
		}

		if ( m_QuitRequest == true )
		{
			if ( m_SaveLoadCO == null )
			{
				QuitInstanly();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		Settings				= null;
		Configs					= null;
		InputMgr				= null;
		Instance				= null;
		IsChangingScene			= false;
		IsLoadingScene			= false;
		CanSave					= true;
		m_OnPauseSet			= null;
		m_IsPaused				= false;
	}


	//////////////////////////////////////////////////////////////////////////
	// QuitApplication
	public	static	void	QuitInstanly()
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
