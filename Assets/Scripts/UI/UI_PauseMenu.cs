
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_PauseMenu : UI_Base, IStateDefiner {


	private		Button	m_ResumeButton			= null;
/*	private		Button	m_SaveButton			= null;
	private		Button	m_SettingsButton		= null;
	private		Button	m_MainMenuButton		= null;
	private		Button	m_QuitButton			= null;
*/



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

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_IsInitialized = true;
		{
			// RESUME BUTTON
			if (m_IsInitialized &= transform.TrySearchComponentByChildName( "Button_Resume", out m_ResumeButton ) )
			{
				m_ResumeButton.onClick.AddListener( delegate()
				{
					//UI.Instance.SetPauseMenuState( false );
					Resume();
				} );
			}
		/*
			// SAVE BUTTON
			if ( m_IsInitialized &= transform.TrySearchComponentInChild( "Button_Save", ref m_SaveButton ) )
			{
				m_SaveButton.onClick.AddListener( Save );
			}

			// SETTINGS BUTTON
			if ( m_IsInitialized &= transform.TrySearchComponentInChild( "Button_Settings", ref m_SettingsButton ) )
			{
				m_SettingsButton.onClick.AddListener( delegate()
				{
					UI.Instance.GoToSubMenu( UI.Instance.Settings.transform );
				});
			}

			// SETTINGS BUTTON
			if ( m_IsInitialized &= transform.TrySearchComponentInChild( "Button_MainMenu", ref m_MainMenuButton ) )
			{
				m_MainMenuButton.onClick.AddListener( delegate()
				{
					ReturnToMenu();
				});
			}

			// SETTINGS BUTTON
			if ( m_IsInitialized &= transform.TrySearchComponentInChild( "Button_Quit", ref m_QuitButton ) )
			{
				m_QuitButton.onClick.AddListener( delegate()
				{
					GameManager.QuitInstanly();
				});
			}
			*/
		}

		if (m_IsInitialized )
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
		return m_IsInitialized;
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
			Destroy( FPSEntityCamera.Instance.transform.gameObject );
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

		// update current active transform
		UIManager.Instance.GoToMenu( UIManager.MainMenu );

		// Stop all current running coroutines
		CoroutinesManager.StopAll();

		// Restore Time Scale
		Time.timeScale = 1.0f;

		// Load menu
		CustomSceneManager.LoadSceneData loadSceneData = new CustomSceneManager.LoadSceneData()
		{
			eScene = ESceneEnumeration.MAIN_MENU
		};
		CustomSceneManager.LoadSceneAsync( loadSceneData );
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
