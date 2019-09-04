
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PauseMenu : MonoBehaviour, IStateDefiner {


	private		Button	m_ResumeButton			= null;
/*	private		Button	m_SaveButton			= null;
	private		Button	m_SettingsButton		= null;
	private		Button	m_MainMenuButton		= null;
	private		Button	m_QuitButton			= null;
*/



	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_bIsInitialized = true;
		{
			// RESUME BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_Resume", ref m_ResumeButton ) )
			{
				m_ResumeButton.onClick.AddListener( delegate()
				{
					//UI.Instance.SetPauseMenuState( false );
					Resume();
				} );
			}
		/*
			// SAVE BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_Save", ref m_SaveButton ) )
			{
				m_SaveButton.onClick.AddListener( Save );
			}

			// SETTINGS BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_Settings", ref m_SettingsButton ) )
			{
				m_SettingsButton.onClick.AddListener( delegate()
				{
					UI.Instance.GoToSubMenu( UI.Instance.Settings.transform );
				});
			}

			// SETTINGS BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_MainMenu", ref m_MainMenuButton ) )
			{
				m_MainMenuButton.onClick.AddListener( delegate()
				{
					ReturnToMenu();
				});
			}

			// SETTINGS BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "Button_Quit", ref m_QuitButton ) )
			{
				m_QuitButton.onClick.AddListener( delegate()
				{
					GameManager.QuitInstanly();
				});
			}
			*/
		}

		if ( m_bIsInitialized )
		{
			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
		}
		else
		{
			Debug.LogError( "UI_PauseMenu: Bad initialization!!!" );
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
	// Resume
	public	void	Resume()
	{
		GameManager.Instance.ResumeFromPause();
	}


	//////////////////////////////////////////////////////////////////////////
	// ReturnToMenu
	public	void	ReturnToMenu()
	{
		// Destroy singletons
		{
			Destroy( CameraControl.Instance.Transform.gameObject );
			Destroy( Player.Instance.gameObject );
			Destroy( WeaponManager.Instance.GameObject );
			Destroy( GameManager.Instance.gameObject );
		}

		// Restore user input
		InputManager.IsEnabled = true;

		// Effect frame reset
		UIManager.EffectFrame.color = Color.black;

		// Hide Pause menu UI
		gameObject.SetActive( false );

		// Hide In-Game UI
		UIManager.InGame.gameObject.SetActive( false );

		// Show MainMenu object
		UIManager.MainMenu.gameObject.SetActive( true );

		// update current active transform
		UIManager.Instance.GoToMenu( UIManager.MainMenu );

		// Stop all current running coroutines
		CoroutinesManager.StopAll();

		// Restore Time Scale
		Time.timeScale = 1.0f;

		// Load menu
		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			iSceneIdx = SceneEnumeration.MAIN_MENU
		};
		CustomSceneManager.LoadSceneSync( loadSceneData );
	}


	//////////////////////////////////////////////////////////////////////////
	// Save
	public	void	Save()
	{
		GameManager.StreamEvents.Save();
	}


	private void Update()
	{
		// Pause Event
		if ( Input.GetKeyDown( KeyCode.Escape ) && GameManager.IsPaused == true )
		{
			Resume();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Quit
	public	void	Quit()
	{
		GameManager.QuitInstanly();
	}

}
