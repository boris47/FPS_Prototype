using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public interface IStateDefiner
{
	/// <summary> Return the current initialized state </summary>
	bool IsInitialized		{ get; }

	/// <summary> Pre-Initialize the component </summary>
	void		PreInit		();

	/// <summary> Initialize the component </summary>
	void		Initialize	();

	/// <summary> Re-Initialize the component </summary>
	void		ReInit		();

	/// <summary> Finalize the component </summary>
	bool		Finalize	();
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


public sealed class UIManager : MonoBehaviourSingleton<UIManager>, IUI
{
	[DllImport("User32.Dll")]
	internal static extern long SetCursorPos(int x, int y);
 
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool GetCursorPos(out POINT lpPoint);
 
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
	private	static	UI_Notifications		m_UI_Notifications				= null;
	private	static	Image					m_EffectFrame					= null;

	public	static	UI_MainMenu				MainMenu						=> m_MainMenu;
	public	static	UI_InGame				InGame							=> m_InGame;
	public	static	UI_WeaponCustomization	WeaponCustomization				=> m_WeaponCustomization;
	public	static	UI_Inventory			Inventory						=> m_Inventory;
	public	static	UI_Settings				Settings						=> m_Settings;
	public	static	UI_PauseMenu			PauseMenu						=> m_PauseMenu;
	public	static	UI_Bindings				Bindings						=> m_Bindings;
	public	static	UI_Graphics				Graphics						=> m_Graphics;
	public	static	UI_Audio				Audio							=> m_Audio;
	public	static	UI_Confirmation			Confirmation					=> m_Confirmation;
	public	static	UI_Indicators			Indicators						=> m_Indicators;
	public	static	UI_Minimap				Minimap							=> m_UI_Minimap;
	public	static	UI_Notifications		Notifications					=> m_UI_Notifications;
	public	static	Image					EffectFrame						=> m_EffectFrame;


	private struct OnScreenInfo
	{
		public string message;
		public float showTime;
	}

	[SerializeField, ReadOnly]
	private			UI_Base					m_CurrentActiveUI				= null;

	[SerializeField, ReadOnly]
	private			UI_Base					m_PreviousActiveUI				= null;

	[SerializeField]
	private			Stack<UI_Base>			m_History						= new Stack<UI_Base>();

	private			Transform				m_RayCastInterceptor			= null;
	private			bool					m_IsInitialized					= false;

	private			List<OnScreenInfo>		m_OnScreenInfo					= new List<OnScreenInfo>();
	private	GUIStyle						m_OnScreenInfoGUIData			= null;

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int X;
		public int Y;
	}

	public			bool					IsInitialized					=> m_IsInitialized;

	public			UI_Base					PreviousActiveUI				=> m_PreviousActiveUI;


	//////////////////////////////////////////////////////////////////////////
	public void	AddScreenInfo(int index, string message, float showTime = 1f)
	{
		m_OnScreenInfo[index] = new OnScreenInfo()
		{
			message = message,
			showTime = showTime
		};
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnGUI()
	{
		m_OnScreenInfoGUIData = m_OnScreenInfoGUIData ?? new GUIStyle(GUI.skin.label)
		{
			fontSize = 10
		};

		for (int i = 0; i < m_OnScreenInfo.Count; i++)
		{
			var item = m_OnScreenInfo[i];
			if (item.showTime >= 0f)
			{
				GUI.Label(new Rect(20f, 5f + (float)i, 200f, 50f), item.message, m_OnScreenInfoGUIData);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		for (int i = m_OnScreenInfo.Count - 1; i >= 0; i--)
		{
			var item = m_OnScreenInfo[i];
			if (item.showTime > 0f)
			{
				item.showTime -= deltaTime;
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		m_OnScreenInfo.Resize(20);

		m_IsInitialized = true;

		foreach (IStateDefiner state in transform.GetComponentsInChildren<IStateDefiner>(includeInactive: true))
		{
			state.PreInit();
		}

		// Get Menus
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_MainMenu",				out m_MainMenu);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_InGame",					out m_InGame);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_WeaponCustomization",	out m_WeaponCustomization);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_Inventory",				out m_Inventory);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_Settings",				out m_Settings);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_PauseMenu",				out m_PauseMenu);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_Bindings",				out m_Bindings);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_Graphics",				out m_Graphics);
		m_IsInitialized &= transform.TrySearchComponentByChildName("UI_Audio",					out m_Audio);
		m_IsInitialized &= transform.TrySearchComponentByChildName( "UI_Confirmation",			out m_Confirmation);

		if (m_IsInitialized)
		{
			// Indicators
			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_Indicators);
			// Mini map
			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_UI_Minimap);

			m_IsInitialized &= m_InGame.transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_UI_Notifications);
		}

		// Effect Frame
		m_IsInitialized &= transform.TrySearchComponentByChildName("EffectFrame", out m_EffectFrame);

		// Ray cast interceptor
		m_RayCastInterceptor = transform.Find("RayCastInterceptor");
		m_IsInitialized &= m_RayCastInterceptor.IsNotNull();

		m_RayCastInterceptor.gameObject.SetActive(false);

		if (m_IsInitialized == false)
		{
			Debug.LogError("UI: Bad initialization!!!");
			return;
		}

		m_CurrentActiveUI = m_InGame.gameObject.activeSelf
			? m_InGame as UI_Base
			: m_MainMenu.gameObject.activeSelf
			? m_MainMenu as UI_Base : null;

		// Other Menus initialization
		foreach (IStateDefiner state in transform.GetComponentsInChildren<IStateDefiner>(includeInactive: true))
		{
			state.Initialize();
			CustomAssertions.IsTrue(state.IsInitialized);
		}
		
		EnableMenuByScene((ESceneEnumeration)CustomSceneManager.CurrentSceneIndex);
		/*
		int sceneIdx = CustomSceneManager.CurrentSceneIndex;
		if (sceneIdx == (int)ESceneEnumeration.LOADING)
		{
			//		SwitchTo( m_Loading.transform );
		}
		else if (sceneIdx == (int)ESceneEnumeration.MAIN_MENU)
		{
			SwitchTo(m_MainMenu);
		}
		else if (sceneIdx == (int)ESceneEnumeration.INTRO)
		{

		}
		else
		{
			SwitchTo(m_InGame);
		}
		*/
	}

	//////////////////////////////////////////////////////////////////////////
	public void EnableMenuByScene(ESceneEnumeration scene)
	{
		switch (scene)
		{
			case ESceneEnumeration.NEXT:
			case ESceneEnumeration.PREVIOUS:
			case ESceneEnumeration.NONE:
			case ESceneEnumeration.COUNT:
			{
				Debug.LogError("Shoud never happen");
				break;
			}
			case ESceneEnumeration.MAIN_MENU:
			{
				GoToMenu(MainMenu);
				break;
			}
			case ESceneEnumeration.OPENWORLD1:
			case ESceneEnumeration.OPENWORLD2:
			case ESceneEnumeration.OPENWORLD3:
			{
				GoToMenu(InGame);
				break;
			}
			case ESceneEnumeration.INTRO:
			case ESceneEnumeration.LOADING:
			case ESceneEnumeration.ENDING:
			{
				GoToMenu(null);
				break;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public bool IsCurrentActive(UI_Base menu)
	{
		return m_CurrentActiveUI == menu;
	}


	//////////////////////////////////////////////////////////////////////////
	private void SwitchTo(UI_Base uiToShow)
	{
		if (m_CurrentActiveUI?.GetInstanceID() == uiToShow?.GetInstanceID())
			return;

		POINT lastCursorPosition = new POINT();

		// Save the current cursor position on the screen
		GetCursorPos(out lastCursorPosition);

		// Disable current active menu gameobject
		m_CurrentActiveUI?.gameObject.SetActive(false);

		m_PreviousActiveUI = m_CurrentActiveUI;

		// Swicth to new menu
		m_CurrentActiveUI = uiToShow;

		// Enable current active menu gameobject
		m_CurrentActiveUI?.gameObject.SetActive(true);

		//string currentName = m_CurrentActiveTransform.name;

		// Re-set the cursor position
		SetCursorPos(lastCursorPosition.X, lastCursorPosition.Y);
		//print( "Switched from " + previousName + " to " + currentName );
	}


	//////////////////////////////////////////////////////////////////////////
	public void GoToMenu(UI_Base menu)
	{
		if (m_CurrentActiveUI?.GetInstanceID() != menu?.GetInstanceID())
		{
			m_History.Clear();

			SwitchTo(menu);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void GoToSubMenu(UI_Base MenuToShow)
	{
		if (MenuToShow)
		{
			m_History.Push(m_CurrentActiveUI);

			SwitchTo(MenuToShow);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void GoBack()
	{
		if (m_History.Count > 0)
		{
			UI_Base t = m_History.Pop();
			SwitchTo(t);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void DisableInteraction(UI_Base menu)
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>(includeInactive: true);
		System.Array.ForEach(selectables, (s) => s.interactable = false);
	}


	//////////////////////////////////////////////////////////////////////////
	public void EnableInteraction(UI_Base menu)
	{
		Selectable[] selectables = menu.GetComponentsInChildren<Selectable>(includeInactive: true);
		System.Array.ForEach(selectables, (s) => s.interactable = true);
	}
}
