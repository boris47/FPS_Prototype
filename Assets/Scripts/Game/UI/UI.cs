using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public interface IUI {

	UI_MainMenu		MainMenu			{ get; }
	UI_InGame		InGame				{ get; }
	UI_Bindings		Bindings			{ get; }
	UI_Confirmation	Confirmation		{ get; }

	Image			EffectFrame			{ get; }

	void			SetPauseMenuState	( bool IsVisible );

	void			LoadSceneByIdx		( int sceneIdx, bool loadSave = false );
	void			ReturnToMainMenu	();

}


public class UI : MonoBehaviour, IUI {
	
	private const	float			TRANSITION_SPEED				= 15f;

	public	static	IUI				Instance						= null;

	private			UI_MainMenu		m_MainMenu						= null;
	private			UI_InGame		m_InGame						= null;
	private			UI_PauseMenu	m_PauseMenu						= null;
	private			UI_Bindings		m_Bindings						= null;
	private			UI_Confirmation	m_Confirmation					= null;

	private			Image			m_EffectFrame					= null;

	private			Transform		m_Settings						= null;
	private			Transform		m_Settings_Graphics				= null;
	private			Transform		m_Settings_GraphicsInGame		= null;
	private			Transform		m_Settings_Audio				= null;
	private			Transform		m_Settings_AudioInGame			= null;

	private			Slider			m_MusicSlider					= null;
	private			Slider			m_SoundSlider					= null;


	private			Image[]			m_MainMenuImages				= null;
//	private			Image[]			m_SettingsImages				= null;
//	private			Image[]			m_SettingsGraphicsImages		= null;
//	private			Image[]			m_SettingsAudioImages			= null;

	// INTERFACE START
					UI_MainMenu		IUI.MainMenu					{ get { return m_MainMenu; } }
					UI_InGame		IUI.InGame						{ get { return m_InGame; } }
					UI_Bindings		IUI.Bindings					{ get { return m_Bindings; } }
					Image			IUI.EffectFrame					{ get { return m_EffectFrame; } }
					UI_Confirmation	IUI.Confirmation				{ get { return m_Confirmation; } }

	// INTERFACE END


	private			AsyncOperation	m_AsyncOperation				= null;
	[SerializeField, ReadOnly]
	private			Transform		m_CurrentActiveTrasform			= null;
	private			Transform		m_PrevActiveTransform			= null;
	private			Transform		m_RayCastInterceptor			= null;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		// SINGLETON
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		// Get Menus
		m_MainMenu		= GetComponentInChildren<UI_MainMenu>( includeInactive : true );
		m_InGame		= GetComponentInChildren<UI_InGame>( includeInactive : true  );
		m_PauseMenu		= GetComponentInChildren<UI_PauseMenu>( includeInactive : true  );
		m_Bindings		= GetComponentInChildren<UI_Bindings>( includeInactive: true );
		m_Confirmation	= GetComponentInChildren<UI_Confirmation>( includeInactive: true );
		m_Confirmation.Initialize();

		// Find Transforms
		m_Settings						= transform.Find( "Settings" );
		m_Settings_Graphics				= transform.Find( "Graphics" );
		m_Settings_GraphicsInGame		= transform.Find( "Settings_GraphicsInGame" );
		m_Settings_Audio				= transform.Find( "Audio" );
		m_Settings_AudioInGame			= transform.Find( "Audio_InGame" );
		m_EffectFrame					= transform.Find( "EffectFrame" ).GetComponent<Image>();
		m_RayCastInterceptor			= transform.Find( "RayCastInterceptor" );
		m_RayCastInterceptor.gameObject.SetActive( false );

		// Get Audio Sliders and Set Value
		m_MusicSlider					= m_Settings_Audio.Find( "Slider_MusicVolume" ).GetComponent<Slider>();
		m_SoundSlider					= m_Settings_Audio.Find( "Slider_SoundVolume" ).GetComponent<Slider>();
		m_MusicSlider.value				= SoundManager.Instance.MusicVolume;
		m_SoundSlider.value				= SoundManager.Instance.SoundVolume;;

		m_MainMenuImages			= m_MainMenu.GetComponentsInChildren<Image>();

		m_CurrentActiveTrasform = m_InGame.gameObject.activeSelf ? m_InGame.transform : m_MainMenu.transform;
	}


	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	public	void	SwitchTo( Transform trasformToShow )
	{
		m_CurrentActiveTrasform.gameObject.SetActive( false );
		trasformToShow.gameObject.SetActive( true );
		m_CurrentActiveTrasform	= trasformToShow;
	}


	//////////////////////////////////////////////////////////////////////////
	// ShowPauseMenu ( Interface )
	void	IUI.SetPauseMenuState	( bool IsVisible )
	{
		// Pausing
		if ( IsVisible == true )
		{
			m_PrevActiveTransform = m_CurrentActiveTrasform;
			m_CurrentActiveTrasform.gameObject.SetActive( false );
			m_PauseMenu.gameObject.SetActive( true );
			m_CurrentActiveTrasform = m_PauseMenu.transform;
		}
		else
		{
			m_CurrentActiveTrasform = m_PrevActiveTransform;
			m_CurrentActiveTrasform.gameObject.SetActive( true );
			m_PauseMenu.gameObject.SetActive( false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReturnToMainMenu ( Interface )
	/// <summary> used to return to menu from in-game pause menu </summary>
	void	IUI.ReturnToMainMenu()
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
		m_EffectFrame.color = Color.black;

		// Sho MainMenu object
		m_MainMenu.gameObject.SetActive( true );

		// Hide In-Game UI
		m_InGame.gameObject.SetActive( false );

		// Hide Pause menu UI
		m_PauseMenu.gameObject.SetActive( false );

		// update current active transform
		m_CurrentActiveTrasform = m_MainMenu.transform;

		// Load menu
		SceneManager.LoadScene( 0 );
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx ( Interface )
	void	IUI.LoadSceneByIdx( int sceneIdx, bool loadSave )
	{
		// Main Menu not allowed
		if ( sceneIdx == 0 )
			return;

		if ( sceneIdx == SceneManager.GetActiveScene().buildIndex )
			return;

		m_MainMenu.gameObject.SetActive( false );

		m_CurrentActiveTrasform = m_InGame.transform;

		StartCoroutine( LoadSceneByIdxCO( sceneIdx, loadSave ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx ( Coroutine )
	private	IEnumerator	LoadSceneByIdxCO( int sceneIdx, bool bMustLoadSave, string SaveToLoad = "SaveFile.txt" )
	{
		// Set global state as ChangingScene state
		GameManager.IsChangingScene = true;

		// Start async load of scene
		m_AsyncOperation = SceneManager.LoadSceneAsync( sceneIdx );
		m_AsyncOperation.allowSceneActivation = false;

		// Wait for load completion
		while ( m_AsyncOperation.progress < 0.9f )
			yield return null;

		// Enable start MonoBehaviours
		m_AsyncOperation.allowSceneActivation = true;

		// Wait for start completion
		while ( m_AsyncOperation.isDone == false )
			yield return null;

		// Remove global state as ChangingScene state
		GameManager.IsChangingScene = false;

		// Wait until main calsses are loaded
		{
			System.Func<bool> mainClassesLoaded = delegate()
			{
				return GameManager.Instance != null
					&& CameraControl.Instance != null
					&& WeaponManager.Instance != null;;
			};
			yield return new WaitUntil( mainClassesLoaded );
		}
		
		// if is loading process, complete load
		if ( bMustLoadSave == true )
		{
			GameManager.StreamEvents.Load();
		}

		// Enable in-game UI
		m_InGame.gameObject.SetActive( true );
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableInteraction
	private	void	DisableInteraction( Transform menu )
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>( includeInactive: true );
		System.Array.ForEach( selectables, ( s ) => s.interactable = false );
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableInteraction
	private	void	EnableInteraction( Transform menu )
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>( includeInactive: true );
		System.Array.ForEach( selectables, ( s ) => s.interactable = true );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnQuit
	public	void	OnQuit()
	{
		// This mean that a game is currently active
		if ( GameManager.Instance != null )
		{
			DisableInteraction( m_CurrentActiveTrasform );
			GameManager.QuitRequest();
			return;
		}

		GameManager.QuitInstanly();
	}

}
