
using UnityEngine;


public sealed partial class GameManager : MonoBehaviour {

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
	


	//////////////////////////////////////////////////////////////////////////
	public	static void	SetInGameAs( bool value )
	{
		m_InGame = value;
	}



	//////////////////////////////////////////////////////////////////////////
	private			void		Awake ()
	{
		print("GameManager::Awake");
		// SINGLETON
		if ( m_Instance != null )
		{
			Destroy(this.gameObject );
			return;
		}
		DontDestroyOnLoad( this );

		// Instance
		m_Instance		= this as GameManager;
		m_StreamEvents	= this as IStreamEvents;
		m_PauseEvents	= this as IPauseEvents;
		m_UpdateEvents	= this as IUpdateEvents;
		m_FieldsOfViewManager = this as IFieldsOfViewManager;
		m_InGame		= false;
	}



	//////////////////////////////////////////////////////////////////////////
	private	void	ReseteDelegates()
	{
		// StreamEvents
		m_OnSave			= delegate ( StreamData streamData ) { return null; };
		m_OnSaveComplete	= delegate ( StreamData streamData ) { return null; };
		this.m_OnLoad.Clear();//	= delegate ( StreamData streamData ) { return null; };
		this.m_OnLoadComplete.Clear();//	= delegate ( StreamData streamData ) { return null; };
		this.m_SaveLoadState		= EStreamingState.NONE;

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

		this.ReseteDelegates();

		GlobalManager.InputMgr.BindCall
		(
			command:		EInputCommands.INVENTORY,
			inputEventID:	"Inventory",
			action:			() =>
				{
					if ( UIManager.Instance.IsCurrentActive( UIManager.Inventory ) )
					{
						UIManager.Instance.GoToMenu( UIManager.InGame );
					}
					else
					{
						UIManager.Instance.GoToMenu( UIManager.Inventory );
					}
				}
			,
			predicate:		() => WeaponManager.Instance.IsZoomed == false
		);


		GlobalManager.InputMgr.BindCall
		(
			command:		EInputCommands.WPN_CUSTOMIZATION,
			inputEventID:	"WeaponCustomization",
			action:			() =>
				{
					if ( UIManager.Instance.IsCurrentActive( UIManager.WeaponCustomization ) )
					{
						UIManager.Instance.GoToMenu( UIManager.InGame );
					}
					else
					{
						UIManager.Instance.GoToMenu( UIManager.WeaponCustomization );
					}
				}
			,
			predicate:		() => WeaponManager.Instance.IsZoomed == false
		);

		// Because we ha bypassed the menu and this flag is set true only after async load is completed,
		// this is the only way to check if user used the main menu
		if ( UI_MainMenu.IsComingFromMenu == false )
		{
			m_InGame = true;
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private			void		OnDisable()
	{
		if ( m_Instance != this )
			return;

		GlobalManager.InputMgr.UnbindCall( EInputCommands.WPN_CUSTOMIZATION,	"WeaponCustomization"	);
		GlobalManager.InputMgr.UnbindCall( EInputCommands.INVENTORY,			"Inventory"				);

		this.ReseteDelegates();

		m_InGame = false;
	}
	


	//////////////////////////////////////////////////////////////////////////
	public	void		QuitRequest()
	{
		m_QuitRequest = true;
		GlobalManager.InputMgr.DisableCategory( EInputCategory.ALL );
		Debug.Log("GameManager: Requesting exit");
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	RequireFrameSkip()
	{
		this.m_SkipOneFrame = true;
	}



	//////////////////////////////////////////////////////////////////////////
	private			void		FixedUpdate()
	{
		m_OnPhysicFrame( Time.fixedDeltaTime );
	}



	int counter=0;
//	private const float	m_InputUpdateDelay = 0.020f;
//	private	float	m_CurrentInputDelay = 0.0f;
	//////////////////////////////////////////////////////////////////////////
	private			void		Update()
	{
		//		if ( Time.frameCount % 90 == 0 )
		//		{
		//			System.GC.Collect( 1, System.GCCollectionMode.Optimized );
		//		}

		this.EDITOR_InGame = m_InGame;
		if ( m_InGame == false )
		{
			return;
		}
		
		// This prevent the ui interaction can trigger actions in-game
		if (this.m_SkipOneFrame == true )
		{
			this.m_SkipOneFrame = false;
			return;
		}

		/// TODO Manage problems on update
//		m_CurrentInputDelay -= Time.deltaTime;
//		if ( m_CurrentInputDelay <= 0.0f )
		{
//			m_CurrentInputDelay = m_InputUpdateDelay;
			// Update inputs
			GlobalManager.InputMgr.Update();
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
			GlobalManager.Configs.SaveContextSections( "Camera" );
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

		if ( Input.GetKeyDown( KeyCode.N ) )
		{
			UIManager.ComInterface.SendNotification( "Ciao Mamma"+ this.counter++, Color.red );
		}


		// Thinking Update
		this.m_ThinkTimer += Time.deltaTime;
		if (this.m_ThinkTimer > Entity.THINK_TIMER )
		{
			m_OnThink();
			this.UpdateCurrentFieldOfView();
			this.m_ThinkTimer = 0f;
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
			m_PauseEvents.SetPauseState( true );
		}

		if ( Input.GetKeyDown( KeyCode.PageDown ) )
		{
			CustomSceneManager.LoadSceneData data = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.NEXT
			};
			CustomSceneManager.LoadSceneAsync( data );
		}

		// Exit request
		if ( m_QuitRequest == true )
		{
			Debug.Log("GameManager: Processing exit request");
			if (this.m_SaveLoadState != EStreamingState.SAVING )
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
		Debug.Log("GameManager::QuitInstanly: Exiting");
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;		
#else
		Application.Quit();
#endif
	}



	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		if ( (Object)m_Instance != this )
			return;

		this.ReseteDelegates();

		m_Instance		= null;
		m_StreamEvents	= null;
		m_PauseEvents	= null;
		m_UpdateEvents	= null;
		m_FieldsOfViewManager = null;

		this.EDITOR_InGame = false;

		m_InGame = false;
		m_QuitRequest = false;
		this.m_SkipOneFrame = false;

		this.m_ThinkTimer = 0f;

		m_IsPaused = false;
	}

}
