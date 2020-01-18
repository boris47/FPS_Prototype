using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.InteropServices;


public interface IStateDefiner {

	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	string StateName		{ get; }

	/// <summary> Initialize the component </summary>
	IEnumerator Initialize	();

	IEnumerator ReInit		();

	/// <summary> Finalize the component </summary>
	bool Finalize			();
}

public interface IStateDefiner<T1, T2> {

	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	string StateName		{ get; }

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

	void					GoToMenu					( Transform MenuToShow );
	void					GoToMenu					( MonoBehaviour MenuToShow );
	void					EnableMenuByScene			( SceneEnumeration scene );
	bool					IsCurrentActive				( MonoBehaviour menu );
	void					GoToSubMenu					( Transform MenuToShow );
	void					GoBack						();


	void					DisableInteraction			( Transform menu );
	void					EnableInteraction			( Transform menu );
}




public sealed class UIManager : MonoBehaviour, IUI {

	[DllImport("User32.Dll")]
	internal static extern long SetCursorPos(int x, int y);
 
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetCursorPos(out POINT lpPoint);
 
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

	private	static	UI_MainMenu				m_MainMenu						= null;
	private	static	UI_InGame				m_InGame						= null;
	private	static	UI_WeaponCustomization	m_WeaponCustomization			= null;
	private	static	UI_Inventory			m_Inventory						= null;
	private	static	UI_Settings				m_Settings						= null;
	private	static	UI_PauseMenu			m_PauseMenu						= null;
	private	static	UI_Bindings				m_Bindings						= null;
	private	static	UI_Graphics				m_Graphics						= null;
	private	static	UI_Audio				m_Audio							= null;
	private	static	UI_Confirmation			m_Confirmation					= null;
	private	static	UI_Indicators			m_Indicators					= null;
	private	static	UI_Minimap				m_UI_Minimap					= null;
	private	static	UI_ComInterface			m_UI_ComInterface				= null;
	private	static	Image					m_EffectFrame					= null;
	private			Transform				m_RayCastInterceptor			= null;

	// INTERFACE START
	public	static	UI_MainMenu				MainMenu					{ get { return m_MainMenu; } }
	public	static	UI_InGame				InGame						{ get { return m_InGame; } }
	public	static	UI_WeaponCustomization	WeaponCustomization			{ get { return m_WeaponCustomization; } }
	public	static	UI_Inventory			Inventory					{ get { return m_Inventory; } }
	public	static	UI_Settings				Settings					{ get { return m_Settings; } }
	public	static	UI_PauseMenu			PauseMenu					{ get { return m_PauseMenu; } }
	public	static	UI_Bindings				Bindings					{ get { return m_Bindings; } }
	public	static	UI_Graphics				Graphics					{ get { return m_Graphics; } }
	public	static	UI_Audio				Audio						{ get { return m_Audio; } }
	public	static	UI_Confirmation			Confirmation				{ get { return m_Confirmation; } }
	public	static	UI_Indicators			Indicators					{ get { return m_Indicators; } }
	public	static	UI_Minimap				Minimap						{ get { return m_UI_Minimap; } }
	public	static	UI_ComInterface			ComInterface				{ get { return m_UI_ComInterface; } }
	public	static	Image					EffectFrame					{ get { return m_EffectFrame; } }
	// INTERFACE END


//	[SerializeField, ReadOnly]
	private			Transform				m_CurrentActiveTransform		= null;
//	private			Transform				m_PrevActiveTransform			= null;

	[SerializeField]
	private			Stack<Transform>		m_TransformStack				= new Stack<Transform>();

	private	bool							m_bIsInitialized				= false;
	public	bool		IsInitialized
	{
		get { return m_bIsInitialized; }
	}




	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		
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

		// Get Menus
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

			m_bIsInitialized &= m_InGame.transform.SearchComponent( ref m_UI_ComInterface, SearchContext.CHILDREN );
		}

		// Effect Frame
		m_bIsInitialized &= transform.SearchComponentInChild( "EffectFrame", ref m_EffectFrame );

		// Ray cast interceptor
		m_RayCastInterceptor			= transform.Find( "RayCastInterceptor" );
		m_bIsInitialized &= m_RayCastInterceptor != null;


		m_RayCastInterceptor.gameObject.SetActive( false );

		if ( m_bIsInitialized == false )
		{
			Debug.LogError( "UI: Bad initialization!!!" );
			return;
		}

		CoroutinesManager.Start( Initialize(), "UIMananger::Initialize() Initializing substates" );
	}



	//////////////////////////////////////////////////////////////////////////
	// Initialize
	private	IEnumerator Initialize()
	{
		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		yield return null;

		m_CurrentActiveTransform =	m_InGame.gameObject.activeSelf ? m_InGame.transform : 
									m_MainMenu.gameObject.activeSelf ? m_MainMenu.transform : null;

		// Other Menus initialization
		IStateDefiner[] states = transform.GetComponentsInChildren<IStateDefiner>( includeInactive: true );
//		uint statesCount = (uint)states.Length;
//		CoroutinesManager.AddCoroutineToPendingCount( statesCount );
		foreach( IStateDefiner state in states )
		{
			yield return CoroutinesManager.Start( state.Initialize(), "UIMananger::Initialize() Initializing substate " + state.StateName );
		}
//		CoroutinesManager.RemoveCoroutineFromPendingCount( statesCount );

		yield return null;
		yield return null;

		int sceneIdx = CustomSceneManager.CurrentSceneIndex; // gameObject.scene.buildIndex;
		if ( sceneIdx == (int)SceneEnumeration.LOADING )
		{
	//		SwitchTo( m_Loading.transform );
		}
		else if ( sceneIdx == (int)SceneEnumeration.MAIN_MENU )
		{
			SwitchTo( m_MainMenu.transform );
		}
		else if ( sceneIdx == (int)SceneEnumeration.INTRO )
		{

		}
		else
		{
			SwitchTo( m_InGame.transform );
		}

		CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
	}



	//////////////////////////////////////////////////////////////////////////
	// EnableMenuByScene
	public void	EnableMenuByScene( SceneEnumeration scene )
	{
		switch ( scene )
		{
			case SceneEnumeration.NEXT:
			case SceneEnumeration.PREVIOUS:
			case SceneEnumeration.NONE:
			case SceneEnumeration.COUNT:
			case SceneEnumeration.INTRO:
				break;
			case SceneEnumeration.LOADING:
//				GoToMenu( Loading );
				break;
			case SceneEnumeration.MAIN_MENU:
				GoToMenu( MainMenu );
				break;
			case SceneEnumeration.OPENWORLD1:
			case SceneEnumeration.OPENWORLD2:
			case SceneEnumeration.OPENWORLD3:
				GoToMenu( InGame );
				break;
			case SceneEnumeration.ENDING:
				break;
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// IsCurrentActive
	public	bool		IsCurrentActive( MonoBehaviour menu )
	{
		return m_CurrentActiveTransform == menu.transform;
	}



	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	private	void	SwitchTo( Transform TransformToShow )
	{
		if ( m_CurrentActiveTransform == TransformToShow )
			return;

		POINT lastCursorPosition = new POINT();

		// SAve the current cursor position on the screen
		GetCursorPos( out lastCursorPosition );

		// Disable current active menu gameobject
		if ( m_CurrentActiveTransform )
			m_CurrentActiveTransform.gameObject.SetActive( false );

		// Swicth to new menu
		m_CurrentActiveTransform	= TransformToShow;

		// Enable current active menu gameobject
		m_CurrentActiveTransform.gameObject.SetActive( true );

//		string currentName = m_CurrentActiveTransform.name;

		// Re-set the cursor position
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
	// GoToMenu
	public	void	GoToMenu( MonoBehaviour MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		m_TransformStack.Clear();

		SwitchTo( MenuToShow.transform );
	}



	//////////////////////////////////////////////////////////////////////////
	// GoToSubMenu
	public	void	GoToSubMenu( Transform MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		m_TransformStack.Push( m_CurrentActiveTransform );

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
