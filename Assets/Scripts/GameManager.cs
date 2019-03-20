
using UnityEngine;


public partial class GameManager : MonoBehaviour {

	/// <summary> Use this to identify if is executing in editor or in build </summary>
#if UNITY_EDITOR
	public	const	bool InEditor = true;
#else
	public	const	bool InEditor = false;
#endif
	private	static			GameManager		m_Instance				= null;
	public	static			GameManager		Instance
	{
		get { return m_Instance; }
	}

	public	static			SectionMap		Settings				= null;
	public	static			SectionMap		Configs					= null;
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


	private					bool			m_SkipOneFrame			= true;
	private					float			m_ThinkTimer			= 0f;

	//////////////////////////////////////////////////////////////////////////
	private			void		Awake ()
	{
		// SINGLETON
		if ( m_Instance != null )
		{
			Destroy( gameObject );
//			gameObject.SetActive( false );
			return;
		}
		DontDestroyOnLoad( this );

		// Instance
		m_Instance		= this as GameManager;
		m_StreamEvents	= this as StreamEvents;
		m_PauseEvents	= this as PauseEvents;
		m_UpdateEvents	= this as UpdateEvents;

		// Internal classes
		InputMgr	= new InputManager();
		Settings	= new SectionMap();
		Configs		= new SectionMap();

		// Load Settings and Configs
		string settingspath		= InEditor ? "Assets/Resources/Settings.txt" : "Settings";
		string configsPath		= InEditor ? "Assets/Resources/Configs/All.txt" : "Configs\\All";

		Settings.LoadFile( settingspath );
		Configs.LoadFile( configsPath );

		Physics.queriesHitBackfaces = false;
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

//	private const float	m_InputUpdateDelay = 0.020f;
//	private	float	m_CurrentInputDelay = 0.0f;
	//////////////////////////////////////////////////////////////////////////
	private			void		Update()
	{
		if ( Time.frameCount % 30 == 0 )
		{
		   System.GC.Collect();
		}

		if ( m_InGame == false )
			return;

		if ( Input.GetKeyDown( KeyCode.Return ) && WeaponManager.Instance.IsZoomed == false )
		{
			UI.Instance.GoToMenu( UI.Instance.WeaponCustomization.transform );
		}

		if ( Input.GetKeyDown (KeyCode.O) )
		{
			UI.Instance.GoToMenu( UI.Instance.InGame.transform );
		}

		// This prevent the ui interaction can trigger actions in-game
		if ( m_SkipOneFrame == true )
		{
			m_SkipOneFrame = false;
			return;
		}

		/// TODO Manage problems on update
//		m_CurrentInputDelay -= Time.deltaTime;
//		if ( m_CurrentInputDelay <= 0.0f )
		{
//			m_CurrentInputDelay = m_InputUpdateDelay;
			// Update inputs
			InputMgr.Update();
		}


		if ( Input.GetKeyDown( KeyCode.T ) )
		{
			Configs.SaveContextSections( "Camera" );
			print("Saving Camera Section");
		}

		
		if ( Input.GetKeyDown( KeyCode.K ) )
		{
			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, "WPN_Module_Prop_BiggerMag" );
		}
		if ( Input.GetKeyDown( KeyCode.L ) )
		{
			WeaponModuleSlot slot = null;
			WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( WeaponSlots.PRIMARY, ref slot );
			slot.WeaponModule.ResetBaseConfiguration();
		}
		

		// Thinking Update
		m_ThinkTimer += Time.deltaTime;
		if ( m_ThinkTimer > Entity.THINK_TIMER )
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
			bool bHasSavedSceneIndex	= PlayerPrefs.HasKey( "SaveSceneIdx" );
			bool bHasSaveFilePath		= PlayerPrefs.HasKey( "SaveFilePath" );
			if ( bHasSavedSceneIndex && bHasSaveFilePath )
			{
				int saveSceneIdx	= PlayerPrefs.GetInt( "SaveSceneIdx" );
				string saveFilePath	= PlayerPrefs.GetString( "SaveFilePath" );
				m_StreamEvents.Load( saveFilePath );
			}
		}

		// Pause Event
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			m_PauseEvents.SetPauseState( !m_IsPaused );
		}

		// Exit request
		if ( m_QuitRequest == true )
		{			
			if ( m_SaveLoadState != StreamingState.SAVING )
			{
				QuitInstanly();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	ResumeFromPause()
	{
		m_PauseEvents.SetPauseState( false );
	}

	/*
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
	*/

	
	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_OnLateFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void		QuitInstanly()
	{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}

}
