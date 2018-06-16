
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour {

	private		Button	m_NewGameResume			= null;
	private		Button	m_ButtonResume			= null;
	private		Button	m_SettingsResume		= null;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private void Awake()
	{
		m_NewGameResume		= transform.Find( "Button_NewGame" ).GetComponent<Button>();
		m_ButtonResume		= transform.Find( "Button_Resume" ).GetComponent<Button>();
		m_SettingsResume	= transform.Find( "Button_Settings" ).GetComponent<Button>();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		if ( Player.Instance != null )
			Destroy( Player.Instance.gameObject );

		Cursor.visible		= true;
		Cursor.lockState	= CursorLockMode.None;
		GameManager.InGame	= false;
		if ( GameManager.Instance != null )
			Destroy( GameManager.Instance.gameObject );

		if ( WeatherSystem.WeatherManager.Instance != null )
			Destroy( WeatherSystem.WeatherManager.Instance.Transform.gameObject );

		if ( CameraControl.Instance != null )
			Destroy( CameraControl.Instance.Transform.gameObject );

		if ( WeaponManager.Instance != null )
			Destroy( WeaponManager.Instance.gameObject );

		if ( EffectManager.Instance != null )
			Destroy( EffectManager.Instance.gameObject );

		m_ButtonResume.interactable = PlayerPrefs.HasKey( "SaveSceneIdx" );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnNewGame()
	{
		if ( GameManager.Instance != null )
			GameManager.Instance.enabled = true;

		UI.Instance.LoadSceneByIdx( 1 );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnNewGame
	public	void	OnResume()
	{
		if ( PlayerPrefs.HasKey( "SaveSceneIdx" ) == true )
		{
			int sceneIdx = PlayerPrefs.GetInt( "SaveSceneIdx" );
			UI.Instance.LoadSceneByIdx( sceneIdx, true );
		}
	}


	private void OnLevelWasLoaded( int level )
	{
		UI.Instance.EffectFrame.color = Color.clear;
	}

}
