
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_MainMenu : UI_Base, IStateDefiner
{
//	private	const string shaderPath = "VR/SpatialMapping/Wireframe";
	
	private		Button	m_ResumeButton			= null;
	
	private	static		bool	m_IsComingFromMenu				= false;
	public	static		bool	IsComingFromMenu
	{
		get { return m_IsComingFromMenu; }
	}


	private	bool			m_IsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}



	//////////////////////////////////////////////////////////////////////////
	public void PreInit() { }

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true )
			yield break;

		yield return null;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_IsInitialized = true;
		{
			/*
			// NEW GAME BUTTON
			if ( transform.TrySearchComponentInChild( "Button_NewGame", ref m_NewGameButton ) )
			{
				m_NewGameButton.onClick.AddListener( OnNewGame );
			}
			*/
			// RESUME BUTTON
			if (m_IsInitialized &= transform.TrySearchComponentByChildName( "Button_Resume", out m_ResumeButton ) )
			{
//				m_ResumeButton.onClick.AddListener( OnResume );
			}
			/*
			// SETTINGS BUTTON
			if ( transform.TrySearchComponentInChild( "Button_Settings", ref m_SettingsButton ) )
			{
				m_SettingsButton.onClick.AddListener( delegate()
				{
					UI.Instance.GoToSubMenu( UI.Instance.Settings.transform );
				});
			}
			*/
		}

		if (m_IsInitialized )
		{
		//	RenderSettings.skybox = new Material( Shader.Find( shaderPath ) );
			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
		}
		else
		{
			Debug.LogError( "UI_MainMenu: Bad initialization!!!" );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}



	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	private void Awake()
	{
		if ( UIManager.MainMenu != this )
			return;

		CoroutinesManager.Start(OnStart() );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		print( "MainMenu::OnEnable" );

		UIManager.EffectFrame.color = Color.clear;

		m_IsComingFromMenu = true;

		GlobalManager.SetCursorVisibility( true );
	}



	//////////////////////////////////////////////////////////////////////////
	// Start
	private IEnumerator OnStart()
	{
		print( "MainMenu::OnStart" );

		// Cursor
		GlobalManager.SetCursorVisibility( false );

		yield return null;

		// UI interaction
		UIManager.Instance.DisableInteraction(this);
		{
			yield return CoroutinesManager.WaitPendingCoroutines();
		}

		print( "MainMenu::OnStart await completed" );

		UIManager.Instance.EnableInteraction(this);

		// Cursor
		GlobalManager.SetCursorVisibility( true );

		bool bHasSavedSceneIndex	= PlayerPrefs.HasKey( "SaveSceneIdx" );
		bool bHasSaveFilePath		= PlayerPrefs.HasKey( "SaveFilePath" );
		bool bSaveFileExists		= bHasSaveFilePath && System.IO.File.Exists( PlayerPrefs.GetString( "SaveFilePath" ) );

		// Resume button
		m_ResumeButton.interactable = bHasSavedSceneIndex && bHasSaveFilePath && bSaveFileExists;
	}



	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnNewGame()
	{
		void onNewGame()
		{
			PlayerPrefs.DeleteKey("SaveSceneIdx");

			CustomSceneManager.LoadSceneData LoadSceneData = new CustomSceneManager.LoadSceneData()
			{
				eScene = ESceneEnumeration.OPENWORLD1,
				sSaveToLoad = "",
				bMustLoadSave = false
			};
			CustomSceneManager.LoadSceneAsync(LoadSceneData);
		}

		if ( PlayerPrefs.HasKey( "SaveSceneIdx" ) )
		{
			UIManager.Confirmation.Show( "Do you want to start another game?", onNewGame );
		}
		else
		{
			onNewGame();
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnResume()
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
				eScene			= (ESceneEnumeration)saveSceneIdx,
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
}
