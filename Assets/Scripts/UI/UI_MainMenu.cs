
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour, IStateDefiner {

	private	const string shaderPath = "VR/SpatialMapping/Wireframe";
	
//	private		Button	m_NewGameButton			= null;
	private		Button	m_ResumeButton			= null;
//	private		Button	m_SettingsButton		= null;
	


	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{
			/*
			// NEW GAME BUTTON
			if ( transform.SearchComponentInChild( "Button_NewGame", ref m_NewGameButton ) )
			{
				m_NewGameButton.onClick.AddListener( OnNewGame );
			}
			*/
			// RESUME BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_Resume", ref m_ResumeButton ) )
			{
//				m_ResumeButton.onClick.AddListener( OnResume );
			}
			/*
			// SETTINGS BUTTON
			if ( transform.SearchComponentInChild( "Button_Settings", ref m_SettingsButton ) )
			{
				m_SettingsButton.onClick.AddListener( delegate()
				{
					UI.Instance.GoToSubMenu( UI.Instance.Settings.transform );
				});
			}
			*/
		}

		if ( m_bIsInitialized )
		{
			RenderSettings.skybox = new Material( Shader.Find( shaderPath ) );
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
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		if ( CameraControl.Instance != null )
			Destroy( CameraControl.Instance.Transform.gameObject );

		if ( Player.Instance != null )
			Destroy( Player.Instance.gameObject );

		if ( WeaponManager.Instance != null )
			Destroy( WeaponManager.Instance.GameObject );

		UIManager.Instance.EffectFrame.color = Color.clear;
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private IEnumerator Start()
	{
		if ( Player.Instance != null )
			Destroy( Player.Instance.gameObject );

		// Cursor
		GlobalManager.SetCursorVisibility( false );

		yield return null;

		// UI interaction
		UIManager.Instance.DisableInteraction( this.transform );
		{
			yield return CoroutinesManager.WaitPendingCoroutines();
		}
		UIManager.Instance.EnableInteraction( this.transform );

		// Cursor
		GlobalManager.SetCursorVisibility( true );

		// Destroying singleton
		if ( CameraControl.Instance != null )
			Destroy( CameraControl.Instance.Transform.gameObject );

		if ( WeaponManager.Instance != null )
			Destroy( WeaponManager.Instance.GameObject );

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
		System.Action onNewGame = delegate()
		{
			PlayerPrefs.DeleteKey( "SaveSceneIdx" );

			CustomSceneManager.LoadSceneData loadData = new CustomSceneManager.LoadSceneData()
			{
				iSceneIdx			= 2,
				sSaveToLoad			= "",
				bMustLoadSave		= false,
				pOnLoadCompleted	= delegate { UIManager.Instance.GoToMenu( UIManager.Instance.InGame.transform ); }
			};
			CustomSceneManager.LoadSceneAsync( loadData );
		};

		if ( PlayerPrefs.HasKey( "SaveSceneIdx" ) )
		{
			UIManager.Instance.Confirmation.Show( "Do you want to start another game?", onNewGame );
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
			CustomSceneManager.LoadSceneData loadData = new CustomSceneManager.LoadSceneData()
			{
				iSceneIdx			= saveSceneIdx,
				sSaveToLoad			= saveFilePath,
				bMustLoadSave		= true,
				pOnLoadCompleted	= delegate { UIManager.Instance.GoToMenu( UIManager.Instance.InGame.transform ); }
			};
			CustomSceneManager.LoadSceneAsync( loadData );
		}
		else
		{
			Debug.LogError( "Cannot load last save" );
		}
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// OnLevelWasLoaded
	private void OnLevelWasLoaded( int level )
	{
		UI.Instance.EffectFrame.color = Color.clear;
	}
	*/
}
