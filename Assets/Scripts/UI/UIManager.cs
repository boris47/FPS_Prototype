using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.InteropServices;


public interface IStateDefiner
{
	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	string StateName		{ get; }


	void		PreInit		();

	/// <summary> Initialize the component </summary>
	IEnumerator Initialize	();

	IEnumerator ReInit		();

	/// <summary> Finalize the component </summary>
	bool Finalize			();
}
/*
public interface IStateDefiner<T1, T2>
{
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
*/
public interface IUIOptions
{
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


public interface IUI
{
	void					GoToMenu					( UI_Base MenuToShow );
	void					EnableMenuByScene			( ESceneEnumeration scene );
	bool					IsCurrentActive				( UI_Base menu );
	void					GoToSubMenu					( UI_Base MenuToShow );
	void					GoBack						();


	void					DisableInteraction			( UI_Base menu );
	void					EnableInteraction			( UI_Base menu );
}


public sealed class UIManager : InGameSingleton<UIManager>, IUI
{
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
	private			UI_Base					m_CurrentActiveUI			= null;
//	private			Transform				m_PrevActiveTransform		= null;

	[SerializeField]
	private			Stack<UI_Base>			m_History					= new Stack<UI_Base>();

	private	bool							m_IsInitialized				= false;
	public	bool		IsInitialized
	{
		get { return m_IsInitialized; }
	}




	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		GlobalManager.SetCursorVisibility( false );

		m_IsInitialized = true;

		foreach( IStateDefiner state in transform.GetComponentsInChildren<IStateDefiner>(includeInactive: true))
		{
			state.PreInit();
		}

		// Get Menus
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_MainMenu",				out m_MainMenu );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_InGame",				out m_InGame );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_WeaponCustomization",	out m_WeaponCustomization );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Inventory",				out m_Inventory );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Settings",				out m_Settings );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_PauseMenu",				out m_PauseMenu );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Bindings",				out m_Bindings );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Graphics",				out m_Graphics );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Audio",					out m_Audio );
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Confirmation",			out m_Confirmation );

		if (m_IsInitialized )
		{
			// Indicators
			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Indicators );
			// Mini map
			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_UI_Minimap );

			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_UI_ComInterface );
		}

		// Effect Frame
		m_IsInitialized &= transform.TrySearchComponentByChildName( "EffectFrame", out m_EffectFrame );

		// Ray cast interceptor
		m_RayCastInterceptor = transform.Find( "RayCastInterceptor" );
		m_IsInitialized &= m_RayCastInterceptor.IsNotNull();

		m_RayCastInterceptor.gameObject.SetActive( false );

		if (m_IsInitialized == false )
		{
			Debug.LogError( "UI: Bad initialization!!!" );
			return;
		}

		CoroutinesManager.Start(Initialize(), "UIMananger::Initialize() Initializing substates" );
	}



	//////////////////////////////////////////////////////////////////////////
	// Initialize
	private	IEnumerator Initialize()
	{
		CoroutinesManager.AddCoroutineToPendingCount(1);

		yield return null;

		m_CurrentActiveUI =	m_InGame.gameObject.activeSelf
			? m_InGame as UI_Base
			: m_MainMenu.gameObject.activeSelf
			? m_MainMenu as UI_Base
			: null;

		uint numCoroutines = CoroutinesManager.PendingRoutines;

		// Other Menus initialization
		foreach( IStateDefiner state in transform.GetComponentsInChildren<IStateDefiner>(includeInactive: true))
		{
			CoroutinesManager.Start( state.Initialize(), "UIMananger::Initialize() Initializing substate " + state.StateName );
		}
		yield return new WaitUntil( () => CoroutinesManager.PendingRoutines <= numCoroutines);

		int sceneIdx = CustomSceneManager.CurrentSceneIndex; // gameObject.scene.buildIndex;
		if ( sceneIdx == (int)ESceneEnumeration.LOADING )
		{
	//		SwitchTo( m_Loading.transform );
		}
		else if ( sceneIdx == (int)ESceneEnumeration.MAIN_MENU )
		{
			SwitchTo( m_MainMenu );
		}
		else if ( sceneIdx == (int)ESceneEnumeration.INTRO )
		{

		}
		else
		{
			SwitchTo( m_InGame );
		}

		CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
	}



	//////////////////////////////////////////////////////////////////////////
	// EnableMenuByScene
	public void EnableMenuByScene(ESceneEnumeration scene)
	{
		switch ( scene )
		{
			case ESceneEnumeration.NEXT:
			case ESceneEnumeration.PREVIOUS:
			case ESceneEnumeration.NONE:
			case ESceneEnumeration.COUNT:
			case ESceneEnumeration.INTRO:
			case ESceneEnumeration.LOADING:
				GoToMenu(null);
				break;
			case ESceneEnumeration.MAIN_MENU:
				GoToMenu(MainMenu);
				break;
			case ESceneEnumeration.OPENWORLD1:
			case ESceneEnumeration.OPENWORLD2:
			case ESceneEnumeration.OPENWORLD3:
				GoToMenu(InGame);
				break;
			case ESceneEnumeration.ENDING:
				break;
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// IsCurrentActive
	public	bool		IsCurrentActive(UI_Base menu)
	{
		return m_CurrentActiveUI == menu;
	}



	//////////////////////////////////////////////////////////////////////////
	// SwitchTo
	private	void	SwitchTo(UI_Base uiToShow)
	{
		if (m_CurrentActiveUI?.GetInstanceID() == uiToShow?.GetInstanceID())
			return;

		POINT lastCursorPosition = new POINT();

		// SAve the current cursor position on the screen
		GetCursorPos( out lastCursorPosition );

		// Disable current active menu gameobject
		m_CurrentActiveUI?.gameObject.SetActive( false );

		// Swicth to new menu
		m_CurrentActiveUI = uiToShow;

		// Enable current active menu gameobject
		m_CurrentActiveUI?.gameObject.SetActive( true );

//		string currentName = m_CurrentActiveTransform.name;

		// Re-set the cursor position
		SetCursorPos( lastCursorPosition.X, lastCursorPosition.Y );
//		print( "Switched from " + previousName + " to " + currentName );
	}



	//////////////////////////////////////////////////////////////////////////
	public	void	GoToMenu( UI_Base MenuToShow )
	{
		m_History.Clear();

		SwitchTo( MenuToShow );
	}



	//////////////////////////////////////////////////////////////////////////
	// GoToSubMenu
	public	void	GoToSubMenu(UI_Base MenuToShow)
	{
		if ( MenuToShow == null )
			return;

		m_History.Push(m_CurrentActiveUI );

		SwitchTo( MenuToShow );
	}

	//////////////////////////////////////////////////////////////////////////
	// GoBack
	public	void GoBack()
	{
		if (m_History.Count > 0 )
		{
			UI_Base t = m_History.Pop();
			SwitchTo( t );
		}
	}



	//////////////////////////////////////////////////////////////////////////
	// DisableInteraction
	public	void	DisableInteraction( UI_Base menu )
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>( includeInactive: true );
		System.Array.ForEach( selectables, ( s ) => s.interactable = false );
	}



	//////////////////////////////////////////////////////////////////////////
	// EnableInteraction
	public	void	EnableInteraction( UI_Base menu )
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
