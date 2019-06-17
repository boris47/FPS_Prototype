﻿
using UnityEngine;


public partial class GameManager : MonoBehaviour {

	private	static			GameManager		m_Instance				= null;
	public	static			GameManager		Instance
	{
		get { return m_Instance; }
	}

	private					InputManager	m_InputMgr				= null;
	public					InputManager	InputMgr
	{
		get { return m_InputMgr; }
	}

	private	static			bool			m_InGame				= true;
	public static			bool			InGame
	{
		get { return m_InGame; }
		set { m_InGame = value; }
	}

	private	static			bool			m_QuitRequest			= false;


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
		m_InputMgr	= new InputManager();
		InputMgr.Setup();

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
			GlobalManager.SetCursorVisibility( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	static	void		QuitRequest()
	{
		m_QuitRequest = true;
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

		if ( m_InGame == false )
			return;

		if ( Input.GetKeyDown( KeyCode.Return ) && WeaponManager.Instance.IsZoomed == false )
		{
			UI.Instance.GoToMenu( UI.Instance.Inventory.transform );
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
		Debug.Log("GameManager: Exiting");
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}

}