﻿
using UnityEngine;


public partial class GameManager : MonoBehaviour {

	private	static			GameManager		m_Instance				= null;
	public	static			GameManager		Instance
	{
		get { return m_Instance; }
	}

	private	static			bool			m_InGame				= true;
	public static			bool			InGame
	{
		get { return m_InGame; }
	}

	private	static			bool			m_QuitRequest			= false;


	private					bool			m_SkipOneFrame			= true;
	private					float			m_ThinkTimer			= 0f;

	private bool EDITOR_InGame = false;
	

	private	void OnLevelLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
	{
		if ( scene.buildIndex == 0 )
			{
				Destroy(gameObject);
			}
	}

	//////////////////////////////////////////////////////////////////////////
	private			void		Awake ()
	{
		// SINGLETON
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );

		// Instance
		m_Instance		= this as GameManager;
		m_StreamEvents	= this as IStreamEvents;
		m_PauseEvents	= this as IPauseEvents;
		m_UpdateEvents	= this as IUpdateEvents;
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	RestoreDelegates()
	{
		// StreamEvents
		m_OnSave			= delegate ( StreamData streamData ) { return null; };
		m_OnSaveComplete	= delegate ( StreamData streamData ) { return null; };
		m_OnLoad			= delegate ( StreamData streamData ) { return null; };
		m_OnLoadComplete	= delegate ( StreamData streamData ) { return null; };
		m_SaveLoadState		= StreamingState.NONE;

		// PauseEvents
		m_OnPauseSet		= delegate { };

		// UpdateEvents
		m_OnThink			= delegate { };
		m_OnPhysicFrame		= delegate { };
		m_OnFrame			= delegate { };
		m_OnLateFrame		= delegate { };
	}

	//////////////////////////////////////////////////////////////////////////
	private			void		OnEnable()
	{
		if ( m_Instance != this )
			return;

		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelLoaded;

		m_InGame = true;

		RestoreDelegates();

//		System.Action toggleInventory = delegate()
//		{
//			if ( UIManager.Instance.cu )
//
//		}

		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.INVENTORY,	"Inventory", () => UIManager.Instance.GoToMenu ( UIManager.Inventory ) );
		GlobalManager.Instance.InputMgr.BindCall( eInputCommands.WPN_CUSTOMIZATION, "WeaponCustomization", () => UIManager.Instance.GoToMenu( UIManager.WeaponCustomization ) );

//		Debug.Log( "GameManager::OnEnable: m_InGame = true" );
	}


	//////////////////////////////////////////////////////////////////////////
	private			void		OnDisable()
	{
		if ( m_Instance != this )
			return;

		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.WPN_CUSTOMIZATION, "WeaponCustomization" );
		GlobalManager.Instance.InputMgr.UnbindCall( eInputCommands.INVENTORY, "Inventory" );

		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnLevelLoaded;

		RestoreDelegates();

		m_InGame = false;

//		Debug.Log( "GameManager::OnDisable: m_InGame = false" );
	}
	

	//////////////////////////////////////////////////////////////////////////
	public	void		QuitRequest()
	{
		m_QuitRequest = true;
		GlobalManager.Instance.InputMgr.DisableCategory( InputCategory.ALL );
		Debug.Log("GameManager: Requesting exit");
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	RequireFrameSkip()
	{
		m_SkipOneFrame = true;
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
//		if ( Time.frameCount % 90 == 0 )
//		{
//			System.GC.Collect( 1, System.GCCollectionMode.Optimized );
//		}
	
		EDITOR_InGame = m_InGame;
		if ( m_InGame == false )
			return;

		if ( Input.GetKeyDown( KeyCode.Return ) && WeaponManager.Instance.IsZoomed == false )
		{
			UIManager.Instance.GoToMenu( UIManager.Inventory );
		}

		if ( Input.GetKeyDown (KeyCode.O) )
		{
			UIManager.Instance.GoToMenu( UIManager.InGame );
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
			GlobalManager.Instance.InputMgr.Update();
		}
		/*
		if ( Input.GetKeyDown( KeyCode.L) )
		{
			InputMgr.ToggleCategory( InputCategory.MOVE );
			print("removing movement");
		}
		*/
		/*
		if ( Input.GetKeyDown( KeyCode.L ) )
		{
			Configs.SaveContextSections( "Camera" );
			print("Saving Camera Section");
		}
		*/
		/*
		if ( Input.GetKeyDown( KeyCode.K ) )
		{
			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, "WPN_Module_Prop_BiggerMag" );
		}
		
		if ( Input.GetKeyDown( KeyCode.L ) )
		{
			WeaponManager.Instance.RemoveModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, "WPN_Module_Prop_BiggerMag" );
		}
		*/
		/*
		if ( Input.GetKeyDown( KeyCode.L ) )
		{
			WeaponModuleSlot slot = null;
			WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( WeaponSlots.PRIMARY, ref slot );
			slot.WeaponModule.ResetBaseConfiguration();
		}
		*/
		

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
		if ( Input.GetKeyDown( KeyCode.F5 ) && GlobalManager.bCanSave == true )
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
		if ( Input.GetKeyDown( KeyCode.Escape ) && m_IsPaused == false )
		{
			m_PauseEvents.SetPauseState( !m_IsPaused );
		}

		if ( Input.GetKeyDown( KeyCode.PageDown ) )
		{
			CustomSceneManager.LoadSceneData data = new CustomSceneManager.LoadSceneData()
			{
				iSceneIdx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex +1,
				eLoadMode = UnityEngine.SceneManagement.LoadSceneMode.Single
			};
			CustomSceneManager.LoadSceneAsync( data );
		}

		// Exit request
		if ( m_QuitRequest == true )
		{
			Debug.Log("GameManager: Processing exit request");
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

	
	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_OnLateFrame( Time.deltaTime );
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void		QuitInstanly()
	{
		Debug.Log("GameManager: Exiting");
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}

}
