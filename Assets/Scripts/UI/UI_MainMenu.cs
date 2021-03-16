
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_MainMenu : UI_Base, IStateDefiner
{
	private				Button				m_NewGameButton					= null;
	private				Button				m_ResumeButton					= null;
	private				Button				m_SettingsButton				= null;
	private				Button				m_QuitButton					= null;
	private				bool				m_IsInitialized					= false;
						bool				IStateDefiner.IsInitialized		=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			// New game button
			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_NewGame", out m_NewGameButton)))
			{
				m_NewGameButton.onClick.AddListener(OnNewGame);
			}

			// Resume button
			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Resume", out m_ResumeButton)))
			{
				bool bHasSavedSceneIndex	= PlayerPrefs.HasKey("SaveSceneIdx");
				bool bHasSaveFilePath		= PlayerPrefs.HasKey("SaveFilePath");
				bool bSaveFileExists		= bHasSaveFilePath && System.IO.File.Exists(PlayerPrefs.GetString("SaveFilePath"));

				m_ResumeButton.interactable = bHasSavedSceneIndex && bHasSaveFilePath && bSaveFileExists;
				m_ResumeButton.onClick.AddListener(OnResume);
			}

			// Settings button
			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Settings", out m_SettingsButton)))
			{
				m_SettingsButton.onClick.AddListener(OnSettings);
			}

			// Quit button
			if(CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Quit", out m_QuitButton)))
			{
				m_QuitButton.onClick.AddListener(OnQuit);
			}

			// disable navigation for everything
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
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
		CustomAssertions.IsTrue(m_IsInitialized);

		UIManager.EffectFrame.color = Color.clear;

		GlobalManager.SetCursorVisibility(true);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnNewGame()
	{
		void onNewGame()
		{
			// New game confirmed, removing previous game reference
			PlayerPrefs.DeleteKey("SaveSceneIdx");

			CustomSceneManager.LoadSceneData LoadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.OPENWORLD1,
				sSaveToLoad = "",
				bMustLoadSave = false
			};
			CustomSceneManager.LoadSceneAsync(LoadSceneData);
		}

		if (PlayerPrefs.HasKey("SaveSceneIdx"))
		{
			UIManager.Confirmation.Show("Do you want to start another game?", onNewGame);
		}
		else
		{
			onNewGame();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnResume()
	{
		bool bHasSavedSceneIndex	= PlayerPrefs.HasKey( "SaveSceneIdx" );
		bool bHasSaveFilePath		= PlayerPrefs.HasKey( "SaveFilePath" );
		bool bSaveFileExists		= bHasSaveFilePath && System.IO.File.Exists( PlayerPrefs.GetString( "SaveFilePath" ) );

		int saveSceneIdx			= PlayerPrefs.GetInt( "SaveSceneIdx" );
		string saveFilePath			= PlayerPrefs.GetString( "SaveFilePath" );

		if ( bHasSavedSceneIndex && bHasSaveFilePath && bSaveFileExists )
		{
			CustomSceneManager.LoadSceneData LoadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene				= (ESceneEnumeration)saveSceneIdx,
				sSaveToLoad			= saveFilePath,
				bMustLoadSave		= true
			};
			CustomSceneManager.LoadSceneAsync( LoadSceneData );
		}
		else
		{
			Debug.LogError( "Cannot load last save" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnSettings()
	{
		UIManager.Instance.GoToSubMenu(UIManager.Settings);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnQuit()
	{
		GlobalManager.QuitInstanly();
	}
}
