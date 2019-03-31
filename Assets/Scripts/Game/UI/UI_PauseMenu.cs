
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


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
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
				
		}
		else
		{
			Debug.LogError( "UI_PauseMenu: Bad initialization!!!" );
		}
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
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
		// Only if paused can return to main menu
		if ( GameManager.IsPaused == false )
			return;

		// Exit pause state
		GameManager.PauseEvents.SetPauseState( false );

		// Force curso to be visible
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Effect frame reset
		UI.Instance.EffectFrame.color = Color.black;

		// Hide Pause menu UI
		gameObject.SetActive( false );

		// Hide In-Game UI
		UI.Instance.InGame.gameObject.SetActive( false );

		// Show MainMenu object
		UI.Instance.MainMenu.gameObject.SetActive( true );

		// update current active transform
		UI.Instance.GoToMenu( UI.Instance.MainMenu.transform );

		// Load menu
		UnityEngine.SceneManagement.SceneManager.LoadScene( 0 );
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
