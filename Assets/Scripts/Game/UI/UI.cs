using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public interface IUIOptions {
	void	Initialize();

	 void	OnEnable();

	void	ApplyDefaults();
	void	ReadFromRegistry();
	void	UpdateUI();
	void	SaveToRegistry();
	void	OnApplyChanges();
	void	Reset();
}


public interface IUI {

	UI_MainMenu		MainMenu			{ get; }
	UI_InGame		InGame				{ get; }
	UI_Settings		Settings			{ get; }
	UI_Bindings		Bindings			{ get; }
	UI_Graphics		Graphics			{ get; }
	UI_Audio		Audio				{ get; }
	UI_Confirmation	Confirmation		{ get; }

	Image			EffectFrame			{ get; }

	void			SwitchTo			( Transform trasformToShow );

	void			SetPauseMenuState	( bool IsVisible );

	void			DisableInteraction( Transform menu );
	void			EnableInteraction( Transform menu );
}




public class UI : MonoBehaviour, IUI {
	
	private const	float			TRANSITION_SPEED				= 15f;

	public	static	IUI				Instance						= null;

	private			UI_MainMenu		m_MainMenu						= null;
	private			UI_InGame		m_InGame						= null;
	private			UI_Settings		m_Settings						= null;
	private			UI_PauseMenu	m_PauseMenu						= null;
	private			UI_Bindings		m_Bindings						= null;
	private			UI_Graphics		m_Graphics						= null;
	private			UI_Audio		m_Audio							= null;
	private			UI_Confirmation	m_Confirmation					= null;
	private			Image			m_EffectFrame					= null;
	private			Transform		m_RayCastInterceptor			= null;

	// INTERFACE START
					UI_MainMenu		IUI.MainMenu					{ get { return m_MainMenu; } }
					UI_InGame		IUI.InGame						{ get { return m_InGame; } }
					UI_Settings		IUI.Settings					{ get { return m_Settings; } }
					UI_Bindings		IUI.Bindings					{ get { return m_Bindings; } }
					UI_Graphics		IUI.Graphics					{ get { return m_Graphics; } }
					UI_Audio		IUI.Audio						{ get { return m_Audio; } }
					Image			IUI.EffectFrame					{ get { return m_EffectFrame; } }
					UI_Confirmation	IUI.Confirmation				{ get { return m_Confirmation; } }
	// INTERFACE END


	[SerializeField, ReadOnly]
	private			Transform		m_CurrentActiveTrasform			= null;
	private			Transform		m_PrevActiveTransform			= null;


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

		// Get Menu
		m_MainMenu		= GetComponentInChildren<UI_MainMenu>		( includeInactive: true );
		m_InGame		= GetComponentInChildren<UI_InGame>			( includeInactive: true );
		m_Settings		= GetComponentInChildren<UI_Settings>		( includeInactive: true );
		m_PauseMenu		= GetComponentInChildren<UI_PauseMenu>		( includeInactive: true );
		m_Bindings		= GetComponentInChildren<UI_Bindings>		( includeInactive: true );
		m_Graphics		= GetComponentInChildren<UI_Graphics>		( includeInactive: true );
		m_Audio			= GetComponentInChildren<UI_Audio>			( includeInactive: true );
		m_Confirmation	= GetComponentInChildren<UI_Confirmation>	( includeInactive: true );

		// Menu initialization
		m_Audio.Initialize();
		m_Graphics.Initialize();
		m_Confirmation.Initialize();
		
		m_EffectFrame					= transform.Find( "EffectFrame" ).GetComponent<Image>();
		m_RayCastInterceptor			= transform.Find( "RayCastInterceptor" );
		m_RayCastInterceptor.gameObject.SetActive( false );

		m_CurrentActiveTrasform = m_InGame.gameObject.activeSelf ? m_InGame.transform : m_MainMenu.transform;
	}


	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	public	void	SwitchTo( Transform trasformToShow )
	{
		m_CurrentActiveTrasform.gameObject.SetActive( false );
		m_CurrentActiveTrasform	= trasformToShow;
		m_CurrentActiveTrasform.gameObject.SetActive( true );
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
	// DisableInteraction
	public	void	DisableInteraction( Transform menu )
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>( includeInactive: true );
		System.Array.ForEach( selectables, ( s ) => s.interactable = false );
	}


	//////////////////////////////////////////////////////////////////////////
	// EnableInteraction
	public	void	EnableInteraction( Transform menu )
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
