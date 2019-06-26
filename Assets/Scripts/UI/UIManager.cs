using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.InteropServices;


public interface IStateDefiner {

	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	/// <summary> Initialize the component </summary>
	IEnumerator Initialize	();

	IEnumerator ReInit		();

	/// <summary> Finalize the component </summary>
	bool Finalize			();
}

public interface IStateDefiner<T1, T2> {

	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	/// <summary> Initialize the component </summary>
	/// <param name="Initializer"> The object initializer </param>
	/// <param name="Callback"> The type used for a goal callback </param>
	/// <returns> A boolean Value for initialization success</returns>
	bool Initialize	( T1 Initializer, System.Action<T2> PositiveCallback = null, System.Action<T2> NegativeCallback = null );

	bool ReInit		();

	/// <summary> Finalize the component </summary>
	bool Finalize			();
}

public interface IUIOptions {

	/// <summary> Apply static default values, save into registry, update UI, apply </summary>
	void	ApplyDefaults();

	/// <summary> Set internal as read from registry </summary>
	void	ReadFromRegistry();

	/// <summary> Update UI elements with current internals </summary>
	void	UpdateUI();

	/// <summary> Save current internals into registry </summary>
	void	SaveToRegistry();

	/// <summary> Callback for changes apply </summary>
	void	OnApplyChanges();

	/// <summary> Remove completely all data from registry </summary>
	void	Reset();
}

public interface IUI {

	UI_MainMenu				MainMenu					{ get; }
	UI_InGame				InGame						{ get; }
	UI_WeaponCustomization	WeaponCustomization			{ get; }
	UI_Inventory			Inventory					{ get; }
	UI_Settings				Settings					{ get; }
	UI_Bindings				Bindings					{ get; }
	UI_Graphics				Graphics					{ get; }
	UI_Audio				Audio						{ get; }
	UI_Confirmation			Confirmation				{ get; }
	UI_Indicators			Indicators					{ get; }
	UI_Minimap				Minimap						{ get; }

	Image					EffectFrame					{ get; }

	void					GoToMenu					( Transform MenuToShow );
	void					GoToSubMenu					( Transform MenuToShow );
	void					GoBack						();


	void					SetPauseMenuState			( bool IsVisible );

	void					DisableInteraction			( Transform menu );
	void					EnableInteraction			( Transform menu );
}




public class UIManager : MonoBehaviour, IUI {

		[DllImport("User32.Dll")]
		public static extern long SetCursorPos(int x, int y);
 
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out POINT lpPoint);
 
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;
		}


	private	static	IUI						m_Instance						= null;
	public	static	IUI						Instance
	{
		get { return m_Instance; }
	}

	private			UI_MainMenu				m_MainMenu						= null;
	private			UI_InGame				m_InGame						= null;
	private			UI_WeaponCustomization	m_WeaponCustomization			= null;
	private			UI_Inventory			m_Inventory						= null;
	private			UI_Settings				m_Settings						= null;
	private			UI_PauseMenu			m_PauseMenu						= null;
	private			UI_Bindings				m_Bindings						= null;
	private			UI_Graphics				m_Graphics						= null;
	private			UI_Audio				m_Audio							= null;
	private			UI_Confirmation			m_Confirmation					= null;
	private			Image					m_EffectFrame					= null;
	private			Transform				m_RayCastInterceptor			= null;
	private			UI_Indicators			m_Indicators					= null;
	private			UI_Minimap				m_UI_Minimap					= null;

	// INTERFACE START
					UI_MainMenu				IUI.MainMenu					{ get { return m_MainMenu; } }
					UI_InGame				IUI.InGame						{ get { return m_InGame; } }
					UI_WeaponCustomization	IUI.WeaponCustomization			{ get { return m_WeaponCustomization; } }
					UI_Inventory			IUI.Inventory					{ get { return m_Inventory; } }
					UI_Settings				IUI.Settings					{ get { return m_Settings; } }
					UI_Bindings				IUI.Bindings					{ get { return m_Bindings; } }
					UI_Graphics				IUI.Graphics					{ get { return m_Graphics; } }
					UI_Audio				IUI.Audio						{ get { return m_Audio; } }
					Image					IUI.EffectFrame					{ get { return m_EffectFrame; } }
					UI_Confirmation			IUI.Confirmation				{ get { return m_Confirmation; } }
					UI_Indicators			IUI.Indicators					{ get { return m_Indicators; } }
					UI_Minimap				IUI.Minimap						{ get { return m_UI_Minimap; } }

	// INTERFACE END


	[SerializeField, ReadOnly]
	private			Transform				m_CurrentActiveTrasform			= null;
	private			Transform				m_PrevActiveTransform			= null;

	[SerializeField]
	private			Stack<Transform>		m_TransformStack					= new Stack<Transform>();

	private	bool							m_bIsInitialized				= false;
	public	bool		IsInitialized
	{
		get { return m_bIsInitialized; }
	}


	private void Awake()
	{
		int sceneIdx = gameObject.scene.buildIndex;
		
		// Singleton
		if ( m_Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;

		GlobalManager.SetCursorVisibility( false );

		m_bIsInitialized = true;

		// Get Menu
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_MainMenu",				ref m_MainMenu );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_InGame",					ref m_InGame );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_WeaponCustomization",		ref m_WeaponCustomization );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Inventory",				ref m_Inventory );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Settings",				ref m_Settings );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_PauseMenu",				ref m_PauseMenu );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Bindings",				ref m_Bindings );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Graphics",				ref m_Graphics );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Audio",					ref m_Audio );
		m_bIsInitialized &= transform.SearchComponentInChild( "UI_Confirmation",			ref m_Confirmation );

		if ( m_bIsInitialized )
		{
			// Indicators
			m_bIsInitialized &= m_InGame.transform.SearchComponent( ref m_Indicators, SearchContext.CHILDREN );
			// Mini map
			m_bIsInitialized &= m_InGame.transform.SearchComponent( ref m_UI_Minimap, SearchContext.CHILDREN );
		}


		Initialize();
//		yield return StartCoroutine(  );

		if ( sceneIdx > 1 )
		{
			SwitchTo( m_InGame.transform );
		}
		else
		{
			SwitchTo( m_MainMenu.transform );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	private	void Initialize()
	{
		// Other Menus initialization
		foreach( IStateDefiner state in transform.GetComponentsInChildren<IStateDefiner>( includeInactive: true ) )
		{
			CoroutinesManager.Start( state.Initialize() );
		}
		
		// Effect Frame
		m_bIsInitialized &= transform.SearchComponentInChild( "EffectFrame", ref m_EffectFrame );

		// Ray cast interceptor
		m_RayCastInterceptor			= transform.Find( "RayCastInterceptor" );
		m_bIsInitialized &= m_RayCastInterceptor != null;

		if ( m_bIsInitialized )
		{
			m_RayCastInterceptor.gameObject.SetActive( false );
			m_CurrentActiveTrasform = m_InGame.gameObject.activeSelf ? m_InGame.transform : m_MainMenu.transform;
		}
		else
		{
			Debug.LogError( "UI: Bad initialization!!!" );
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	private	void	SwitchTo( Transform trasformToShow )
	{
		POINT lastCursorPosition = new POINT();
		GetCursorPos( out lastCursorPosition );
		string previousName = m_CurrentActiveTrasform.name;
			m_CurrentActiveTrasform.gameObject.SetActive( false );
			m_CurrentActiveTrasform	= trasformToShow;
			m_CurrentActiveTrasform.gameObject.SetActive( true );
		string currentName = m_CurrentActiveTrasform.name;
		SetCursorPos( lastCursorPosition.X, lastCursorPosition.Y );
//		print( "Switched from " + previousName + " to " + currentName );
	}


	//////////////////////////////////////////////////////////////////////////
	// GoToMenu
	public	void	GoToMenu( Transform MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		m_TransformStack.Clear();

		SwitchTo( MenuToShow );
	}


	//////////////////////////////////////////////////////////////////////////
	// GoToSubMenu
	public	void	GoToSubMenu( Transform MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		m_TransformStack.Push( m_CurrentActiveTrasform );

		SwitchTo( MenuToShow );
	}


	//////////////////////////////////////////////////////////////////////////
	// GoBack
	public	void GoBack()
	{
		if ( m_TransformStack.Count > 0 )
		{
			Transform t = m_TransformStack.Pop();
			SwitchTo( t );
		}
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
		GameManager.QuitInstanly();
	}

}
