using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public interface IUI {

	UI_MainMenu		MainMenu			{ get; }
	UI_InGame		InGame				{ get; }

	void			TooglePauseMenu		();

	void			LoadSceneByIdx		( int sceneIdx );

}


public class UI : MonoBehaviour, IUI {
	
	private const	float			TRANSITION_SPEED				= 15f;

	public	static	IUI				Instance						= null;

	private			UI_MainMenu		m_MainMenu						= null;
	private			UI_InGame		m_InGame						= null;
	private			UI_PauseMenu	m_PauseMenu						= null;

	private			Transform		m_Settings						= null;
	private			Transform		m_Settings_Graphics				= null;
	private			Transform		m_Settings_Audio				= null;


	private			Image[]			m_MainMenuImages				= null;
//	private			Image[]			m_SettingsImages				= null;
//	private			Image[]			m_SettingsGraphicsImages		= null;
//	private			Image[]			m_SettingsAudioImages			= null;

	// INTERFACE START
					UI_MainMenu		IUI.MainMenu					{ get { return m_MainMenu; } }
					UI_InGame		IUI.InGame						{ get { return m_InGame; } }
	// INTERFACE END


	private			AsyncOperation	m_AsyncOperation				= null;
	private			Transform		m_CurrentActiveTrasform			= null;
	private			Transform		m_RayCastInterceptor			= null;
	private			bool			m_IsSwitching					= false;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		m_MainMenu	= GetComponentInChildren<UI_MainMenu>( includeInactive : true );
		m_InGame	= GetComponentInChildren<UI_InGame>( includeInactive : true  );
		m_PauseMenu	= GetComponentInChildren<UI_PauseMenu>( includeInactive : true  );

		m_Settings				= transform.Find( "Settings" );
		m_Settings_Graphics		= transform.Find( "Graphics" );
		m_Settings_Audio		= transform.Find( "Audio" );
		m_RayCastInterceptor	= transform.Find( "RayCastInterceptor" );
		m_RayCastInterceptor.gameObject.SetActive( false );

		m_MainMenuImages			= m_MainMenu.GetComponentsInChildren<Image>();
/*		m_SettingsImages			= m_Settings.GetComponentsInChildren<Image>();
		m_SettingsGraphicsImages	= m_Settings_Graphics.GetComponentsInChildren<Image>();
		m_SettingsAudioImages		= m_Settings_Audio.GetComponentsInChildren<Image>();
*/
		m_CurrentActiveTrasform = m_InGame.gameObject.activeSelf ? m_InGame.transform : m_MainMenu.transform;
	}


	public	void	SwitchTo( Transform trasformToShow )
	{
		if ( m_IsSwitching == true )
			return;

		StartCoroutine( SwitchToCO( trasformToShow ) );
	}


	private	IEnumerator	SwitchToCO( Transform trasformToShow )
	{
		m_IsSwitching = true;
		m_RayCastInterceptor.gameObject.SetActive( true );

		Image[] toHide = m_CurrentActiveTrasform.GetComponentOnlyInChildren<Image>();
		Image[] toShow = trasformToShow.GetComponentOnlyInChildren<Image>();

		float interpolant = 0f;
		while( interpolant < 1f )
		{
			interpolant += Time.unscaledDeltaTime * TRANSITION_SPEED;
			for ( int i = 0; i < toHide.Length; i++ )
			{
				Image image = toHide[ i ];
				image.color = Color.Lerp( Color.white, Color.clear, interpolant );
				yield return null;
			}

		}

		m_CurrentActiveTrasform.gameObject.SetActive( false );
		trasformToShow.gameObject.SetActive( true );

		interpolant = 0f;
		while( interpolant < 1f )
		{
			interpolant += Time.unscaledDeltaTime * TRANSITION_SPEED;
			for ( int i = 0; i < toShow.Length; i++ )
			{
				Image image = toShow[ i ];
				image.color = Color.Lerp( Color.clear, Color.white, interpolant );
				yield return null;
			}

		}

		m_CurrentActiveTrasform = trasformToShow;

		m_RayCastInterceptor.gameObject.SetActive( false );
		m_IsSwitching = false;
	}

	private	float prevTimeScale = 1f;
	//////////////////////////////////////////////////////////////////////////
	// ShowPauseMenu ( Interface )
	void	IUI.TooglePauseMenu()
	{
		bool isActive = m_PauseMenu.gameObject.activeSelf;

		// Pausing
		if ( isActive == false )
		{
			prevTimeScale = Time.timeScale;
			Time.timeScale = 0f;
		}
		else
			Time.timeScale = prevTimeScale;

//		if ( isActive == false )
//			Time.timeScale = 0f;

		m_CurrentActiveTrasform.gameObject.SetActive( isActive );
		m_PauseMenu.gameObject.SetActive( !isActive );
		
		CameraControl.Instance.CanParseInput = isActive;
		InputManager.IsEnabled = isActive;
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx ( Interface )
	void	IUI.LoadSceneByIdx( int sceneIdx )
	{
		m_MainMenu.gameObject.SetActive( false );

		m_CurrentActiveTrasform = m_InGame.transform;

		StartCoroutine( LoadSceneByIdxCO( sceneIdx ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadSceneByIdx ( Coroutine )
	private	IEnumerator	LoadSceneByIdxCO( int sceneIdx )
	{
		GameManager.IsChangingScene = true;

		m_AsyncOperation = SceneManager.LoadSceneAsync( sceneIdx );
		m_AsyncOperation.allowSceneActivation = false;

		while ( m_AsyncOperation.progress < 0.9f )
		{
			yield return null;
		}

		m_AsyncOperation.allowSceneActivation = true;

		UI.Instance.InGame.gameObject.SetActive( true );
		UI.Instance.InGame.Hide();

		GameManager.IsChangingScene = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnQuit
	public	void	OnQuit()
	{

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif

	}

}
