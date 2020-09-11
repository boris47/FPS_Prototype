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
//	void	ApplyDefaults();

	/// <summary> Set internal as read from registry </summary>
//	void	ReadFromRegistry();

	/// <summary> Update UI elements with current internals </summary>
	void	UpdateUI();

	/// <summary> Save current internals into registry </summary>
//	void	SaveToRegistry();

	/// <summary> Callback for changes apply </summary>
	void	OnApplyChanges();

	/// <summary> Remove completely all data from registry </summary>
	void	Reset();
}



public interface IUI {

	void					GoToMenu					( Transform MenuToShow );
	void					GoToMenu					( MonoBehaviour MenuToShow );
	void					EnableMenuByScene			( ESceneEnumeration scene );
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

	private	bool							m_IsInitialized				= false;
	public	bool		IsInitialized
	{
		get { return this.m_IsInitialized; }
	}




	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		
		// Singleton
		if ( m_Instance != null )
		{
			Destroy(this.gameObject );
			return;
		}
		DontDestroyOnLoad( this );
		m_Instance = this;

		GlobalManager.SetCursorVisibility( false );

		this.m_IsInitialized = true;

		// Get Menus
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_MainMenu",				ref m_MainMenu );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_InGame",				ref m_InGame );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_WeaponCustomization",	ref m_WeaponCustomization );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Inventory",				ref m_Inventory );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Settings",				ref m_Settings );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_PauseMenu",				ref m_PauseMenu );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Bindings",				ref m_Bindings );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Graphics",				ref m_Graphics );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Audio",					ref m_Audio );
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "UI_Confirmation",			ref m_Confirmation );

		if (this.m_IsInitialized )
		{
			// Indicators
			this.m_IsInitialized &= m_InGame.transform.SearchComponent( ref m_Indicators, ESearchContext.CHILDREN );
			// Mini map
			this.m_IsInitialized &= m_InGame.transform.SearchComponent( ref m_UI_Minimap, ESearchContext.CHILDREN );

			this.m_IsInitialized &= m_InGame.transform.SearchComponent( ref m_UI_ComInterface, ESearchContext.CHILDREN );
		}

		// Effect Frame
		this.m_IsInitialized &= this.transform.SearchComponentInChild( "EffectFrame", ref m_EffectFrame );

		// Ray cast interceptor
		this.m_RayCastInterceptor			= this.transform.Find( "RayCastInterceptor" );
		this.m_IsInitialized &= this.m_RayCastInterceptor != null;


		this.m_RayCastInterceptor.gameObject.SetActive( false );

		if (this.m_IsInitialized == false )
		{
			Debug.LogError( "UI: Bad initialization!!!" );
			return;
		}

		CoroutinesManager.Start(this.Initialize(), "UIMananger::Initialize() Initializing substates" );
	}



	//////////////////////////////////////////////////////////////////////////
	// Initialize
	private	IEnumerator Initialize()
	{
		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		yield return null;

		this.m_CurrentActiveTransform =	m_InGame.gameObject.activeSelf
			? m_InGame.transform
			: m_MainMenu.gameObject.activeSelf
			? m_MainMenu.transform
			: null;

		uint numCoroutines = CoroutinesManager.PendingRoutines;

		// Other Menus initialization
		foreach( IStateDefiner state in this.transform.GetComponentsInChildren<IStateDefiner>(includeInactive: true))
		{
			CoroutinesManager.Start( state.Initialize(), "UIMananger::Initialize() Initializing substate " + state.StateName );
		}
		yield return new WaitUntil( () => CoroutinesManager.PendingRoutines <= numCoroutines);

		yield return null;
		yield return null;

		int sceneIdx = CustomSceneManager.CurrentSceneIndex; // gameObject.scene.buildIndex;
		if ( sceneIdx == (int)ESceneEnumeration.LOADING )
		{
	//		SwitchTo( m_Loading.transform );
		}
		else if ( sceneIdx == (int)ESceneEnumeration.MAIN_MENU )
		{
			this.SwitchTo( m_MainMenu.transform );
		}
		else if ( sceneIdx == (int)ESceneEnumeration.INTRO )
		{

		}
		else
		{
			this.SwitchTo( m_InGame.transform );
		}

		CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
	}



	//////////////////////////////////////////////////////////////////////////
	// EnableMenuByScene
	public void EnableMenuByScene( ESceneEnumeration scene )
	{
		switch ( scene )
		{
			case ESceneEnumeration.NEXT:
			case ESceneEnumeration.PREVIOUS:
			case ESceneEnumeration.NONE:
			case ESceneEnumeration.COUNT:
			case ESceneEnumeration.INTRO:
				break;
			case ESceneEnumeration.LOADING:
//				GoToMenu( Loading );
				break;
			case ESceneEnumeration.MAIN_MENU:
				this.GoToMenu( MainMenu );
				break;
			case ESceneEnumeration.OPENWORLD1:
			case ESceneEnumeration.OPENWORLD2:
			case ESceneEnumeration.OPENWORLD3:
				this.GoToMenu( InGame );
				break;
			case ESceneEnumeration.ENDING:
				break;
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// IsCurrentActive
	public	bool		IsCurrentActive( MonoBehaviour menu )
	{
		return this.m_CurrentActiveTransform == menu.transform;
	}



	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	private	void	SwitchTo( Transform TransformToShow )
	{
		if (this.m_CurrentActiveTransform?.GetInstanceID() == TransformToShow?.GetInstanceID() )
			return;

		POINT lastCursorPosition = new POINT();

		// SAve the current cursor position on the screen
		GetCursorPos( out lastCursorPosition );

		// Disable current active menu gameobject
		if (this.m_CurrentActiveTransform )
			this.m_CurrentActiveTransform.gameObject.SetActive( false );

		// Swicth to new menu
		this.m_CurrentActiveTransform	= TransformToShow;

		// Enable current active menu gameobject
		this.m_CurrentActiveTransform.gameObject.SetActive( true );

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

		this.m_TransformStack.Clear();

		this.SwitchTo( MenuToShow );
	}



	//////////////////////////////////////////////////////////////////////////
	// GoToMenu
	public	void	GoToMenu( MonoBehaviour MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		this.m_TransformStack.Clear();

		this.SwitchTo( MenuToShow.transform );
	}



	//////////////////////////////////////////////////////////////////////////
	// GoToSubMenu
	public	void	GoToSubMenu( Transform MenuToShow )
	{
		if ( MenuToShow == null )
			return;

		this.m_TransformStack.Push(this.m_CurrentActiveTransform );

		this.SwitchTo( MenuToShow );
	}



	//////////////////////////////////////////////////////////////////////////
	// GoBack
	public	void GoBack()
	{
		if (this.m_TransformStack.Count > 0 )
		{
			Transform t = this.m_TransformStack.Pop();
			this.SwitchTo( t );
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
