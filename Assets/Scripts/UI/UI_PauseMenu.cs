using UnityEngine;
using UnityEngine.UI;

public sealed class UI_PauseMenu : UI_Base, IStateDefiner
{
	private				Button								m_ResumeButton						= null;
	private				Button								m_SaveButton						= null;
	private				Button								m_SettingsButton					= null;
	private				Button								m_MainMenuButton					= null;
	private				Button								m_QuitButton						= null;

	private				bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Resume", out m_ResumeButton)))
			{
				m_ResumeButton.onClick.AddListener(() => GameManager.PauseEvents.SetPauseState(false));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Save", out m_SaveButton)))
			{
				m_SaveButton.onClick.AddListener(OnSave);
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Settings", out m_SettingsButton)))
			{
				m_SettingsButton.onClick.AddListener(() => UIManager.Instance.GoToSubMenu(UIManager.Settings));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_MainMenu", out m_MainMenuButton)))
			{
				m_MainMenuButton.onClick.AddListener(ReturnToMenu);
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Quit", out m_QuitButton)))
			{
				m_QuitButton.onClick.AddListener(GlobalManager.QuitInstanly);
			}

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		GlobalManager.SetCursorVisibility(true);
	}


	//////////////////////////////////////////////////////////////////////////
	private void	ReturnToMenu()
	{
		// Destroy singletons
		{
			Destroy(Player.Instance.gameObject);
			Destroy(FPSEntityCamera.Instance.gameObject);
			Destroy(WeaponManager.Instance.GameObject);
		}
		

		// Restore user input
		InputManager.IsEnabled = true;

		// Effect frame reset
		UIManager.EffectFrame.color = Color.black;

		// Stop all current running coroutines
		CoroutinesManager.StopAll();

		// Restore Time Scale
		GlobalManager.SetTimeScale(1f);

		// Load menu
		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			eScene = ESceneEnumeration.MAIN_MENU
		};
		CustomSceneManager.LoadSceneAsync(loadSceneData);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnSave()
	{
		GameManager.StreamEvents.Save();
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			OnResume();
		}
	}
	*/
}
