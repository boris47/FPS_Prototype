
using UnityEngine;


public sealed partial class GameManager : MonoBehaviourSingleton<GameManager>
{
	public	const			float			THINK_TIMER				= 0.2f; // 200 ms
	private	static			bool			m_QuitRequest			= false;

	private					float			m_ThinkTimer			= 0f;

	//////////////////////////////////////////////////////////////////////////
	protected override void OnInitialize()
	{
		base.OnInitialize();

		CustomSceneManager.RegisterOnLoad(ESceneLoadStep.SCENE_LOADED, OnInGameScene);
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnInGameScene(ESceneEnumeration prevScene, ESceneEnumeration currScene)
	{
		void TogglePauseMenu()
		{
			bool newState = UIManager.Instance.IsCurrentActive(UIManager.InGame);
			PauseEvents.SetPauseState(newState);
		}

		void ToggleMenu(UI_Base A, UI_Base B)
		{
			if (UIManager.Instance.IsCurrentActive(B))
			{
				UIManager.Instance.GoToMenu(A);
			}
			else
			{
				UIManager.Instance.GoToMenu(B);
			}
		}
#if UNITY_EDITOR
		if (currScene == prevScene && CustomSceneManager.IsGameScene(currScene)) // Play in editor
		{
			GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_MENU, "InGame_Pause", TogglePauseMenu);
			GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_SAVE, "InGame_Save", Save, () => GlobalManager.bCanSave);
			GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_LOAD, "InGame_Load", Load);
			GlobalManager.InputMgr.BindCall(EInputCommands.INVENTORY,   "Inventory", () => ToggleMenu(UIManager.Inventory, UIManager.InGame), () => !WeaponManager.Instance.IsZoomed);
			GlobalManager.InputMgr.BindCall(EInputCommands.INVENTORY,   "WeaponCustomization", () => ToggleMenu(UIManager.WeaponCustomization, UIManager.InGame), () => !WeaponManager.Instance.IsZoomed);
		} else
#endif
		// Entering or exiting game
		if (CustomSceneManager.IsGameScene(prevScene) != CustomSceneManager.IsGameScene(currScene))
		{
			if (prevScene <= ESceneEnumeration.MAIN_MENU) // Entering InGame
			{
				GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_MENU, "InGame_Pause", TogglePauseMenu);
				GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_SAVE, "InGame_Save", Save, () => GlobalManager.bCanSave);
				GlobalManager.InputMgr.BindCall(EInputCommands.INGAME_LOAD, "InGame_Load", Load);
				GlobalManager.InputMgr.BindCall(EInputCommands.INVENTORY,   "Inventory", () => ToggleMenu(UIManager.Inventory, UIManager.InGame), () => !WeaponManager.Instance.IsZoomed);
				GlobalManager.InputMgr.BindCall(EInputCommands.INVENTORY,   "WeaponCustomization", () => ToggleMenu(UIManager.WeaponCustomization, UIManager.InGame), () => !WeaponManager.Instance.IsZoomed);
			}
			else // Exiting InGame
			{
				GlobalManager.InputMgr.UnbindCall(EInputCommands.INGAME_MENU, "InGame_Pause");
				GlobalManager.InputMgr.UnbindCall(EInputCommands.INGAME_SAVE, "InGame_Save");
				GlobalManager.InputMgr.UnbindCall(EInputCommands.INGAME_LOAD, "InGame_Load");
				GlobalManager.InputMgr.UnbindCall(EInputCommands.INVENTORY,   "Inventory");
				GlobalManager.InputMgr.UnbindCall(EInputCommands.INVENTORY,   "WeaponCustomization");
				ResetEvents();
			}
		}
		else // scene changed but is still a game scene
		{

		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void FixedUpdate()
	{
		m_OnPhysicFrame(Time.fixedDeltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	private void Save()
	{
		StreamEvents.Save();
	}


	//////////////////////////////////////////////////////////////////////////
	private void Load()
	{
		bool bHasSavedSceneIndex	= PlayerPrefs.HasKey("SaveSceneIdx");
		bool bHasSaveFilePath		= PlayerPrefs.HasKey("SaveFilePath");
		if (bHasSavedSceneIndex && bHasSaveFilePath)
		{
			int saveSceneIdx		= PlayerPrefs.GetInt("SaveSceneIdx");
			string saveFilePath		= PlayerPrefs.GetString("SaveFilePath");
			StreamEvents.Load(saveFilePath);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		// Thinking Update
		m_ThinkTimer += Time.deltaTime;
		if (m_ThinkTimer > THINK_TIMER)
		{
			m_OnThink();
			m_ThinkTimer = 0f;
		}

		// Frame Update
		if (!m_IsPaused)
		{
			m_OnFrame(Time.deltaTime);
		}

		// Exit request
		if (m_QuitRequest)
		{
			Debug.Log("GameManager: Processing exit request");
			if (m_SaveLoadState != EStreamingState.SAVING)
			{
				GlobalManager.QuitInstanly();
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		m_OnLateFrame(Time.deltaTime);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	ResetEvents()
	{
		// StreamEvents
		m_OnSave							= delegate { return true; };
		m_OnSaveComplete					= delegate { return true; };
		m_OnLoad.Clear();					//= new List<GameEvents.StreamingEvent>();
		m_OnLoadComplete.Clear();			//= new List<GameEvents.StreamingEvent>();
		m_SaveLoadState						= EStreamingState.NONE;

		// PauseEvents
		m_OnPauseSet						= delegate { };
		m_IsPaused							= false;
		m_PrevTimeScale						= 1f;
		m_PrevCanParseInput					= false;
		m_PrevInputEnabled					= false;

		// UpdateEvents
		m_OnThink							= delegate { };
		m_OnPhysicFrame						= delegate { };
		m_OnFrame							= delegate { };
		m_OnLateFrame						= delegate { };

	//	m_FieldsOfViewList.Clear();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		ResetEvents();

		m_QuitRequest			= false;
		m_ThinkTimer			= 0f;
		m_IsPaused				= false;
	}
}
