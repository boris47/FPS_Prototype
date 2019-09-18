using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponCustomization : MonoBehaviour, IStateDefiner {

	private		Dropdown		m_PrimaryDropDown		= null;
	private		Dropdown		m_SecondaryDropDown		= null;
	private		Dropdown		m_TertiaryDropDown		= null;

	private		Button			m_ReturnToGame			= null;
	private		Button			m_SwitchToInventory		= null;
	private		Button			m_ApplyButton			= null;

	private		bool			m_bIsInitialized		= false;

	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}

	Dictionary<WeaponSlots, Database.Section> m_CurrentAssignedModuleSections = new Dictionary<WeaponSlots, Database.Section>()
	{
		{ WeaponSlots.PRIMARY,		new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) },
		{ WeaponSlots.SECONDARY,	new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) },
		{ WeaponSlots.TERTIARY,		new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) }
	};

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator	IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		Transform child = transform.Find("CustomizationPanel");
		if ( m_bIsInitialized = ( child != null ) )
		{
			m_bIsInitialized &= child.SearchComponentInChild( "ModulePrimaryDropdown", ref m_PrimaryDropDown );
			m_bIsInitialized &= child.SearchComponentInChild( "ModuleSecondaryDropdown", ref m_SecondaryDropDown );
			m_bIsInitialized &= child.SearchComponentInChild( "ModuleTertiaryDropdown", ref m_TertiaryDropDown );

			yield return null;

			// APPLY BUTTON
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ApplyButton", ref m_ApplyButton ) )
			{
				m_ApplyButton.interactable = false;
				m_ApplyButton.onClick.AddListener
				(	
					delegate()
					{
						UIManager.Confirmation.Show( "Apply Changes?", OnApply, delegate { OnEnable(); } );
					}
				);
			}

			yield return null;

			// SWITCH TO INVENTORY
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "SwitchToInventory", ref m_SwitchToInventory ) )
			{
				m_SwitchToInventory.onClick.AddListener
				(	
					OnSwitchToInventory
				);
			}

			yield return null;

			// RETURN TO GAME
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ReturnToGame", ref m_ReturnToGame ) )
			{
				m_ReturnToGame.onClick.AddListener
				(
					OnReturnToGame
				);
			}

			yield return null;
		}

		CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
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
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		Database.Section[] fireModules		= GlobalManager.Configs.GetSectionsByContext( "WeaponFireModules" );
		Database.Section[] utiliyModules	= GlobalManager.Configs.GetSectionsByContext( "WeaponUtilityModules" );

		List<Database.Section> allModules = new List<Database.Section>();
		{
			allModules.AddRange( fireModules );
			allModules.AddRange( utiliyModules );
		}

		// PRIMARY
		FillDropdown( m_PrimaryDropDown,	allModules, WeaponSlots.PRIMARY );

		// SECONDARY
		FillDropdown( m_SecondaryDropDown,	allModules, WeaponSlots.SECONDARY );

		// TERTIARY
		FillDropdown( m_TertiaryDropDown,	allModules, WeaponSlots.TERTIARY );


		CameraControl.Instance.CanParseInput	= false;
//		InputManager.IsEnabled					= false;

		// All categories but not interface
		GlobalManager.InputMgr.DisableCategory( InputCategory.ALL | InputCategory.INTERFACE );

		GlobalManager.SetCursorVisibility( true );

		WeaponManager.Instance.CurrentWeapon.Stash();
	}


	//////////////////////////////////////////////////////////////////////////
	// FillDropdown
	private	void	FillDropdown( Dropdown thisDropdown, List<Database.Section> allModules, WeaponSlots slot )
	{
		thisDropdown.ClearOptions();

		string[] alreadyAssignedModules = WeaponManager.Instance.CurrentWeapon.OtherInfo.Split( ',' );

		// Get weapon module slot
		WeaponModuleSlot slotModule = null;
		WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( slot, ref slotModule);

		// Ask slot if module can be assigned
		List<Database.Section> filtered = new List<Database.Section>();
		foreach( Database.Section current in allModules )
		{
			if ( slotModule.CanAssignModule( current, alreadyAssignedModules: alreadyAssignedModules ) )
			{
				filtered.Add( current );
			}
		}

		// Assign readble names to dropdown options
		List<string> ui_Names = filtered.ConvertAll
		(
			new System.Converter<Database.Section, string>( s => { return s.AsString("Name"); } )
		);
		thisDropdown.AddOptions( ui_Names );

		m_CurrentAssignedModuleSections[slot] = new Database.Section( alreadyAssignedModules[(int)slot], "" );

		// Search current Value
		thisDropdown.value = filtered.FindIndex( s => s.GetName() == alreadyAssignedModules[(int)slot] );

		thisDropdown.onValueChanged.RemoveAllListeners();
		UnityEngine.Events.UnityAction<int> callback = delegate( int moduleIndex )
		{
			OnModuleChanged( slot, filtered[moduleIndex] );
		};
		thisDropdown.onValueChanged.AddListener( callback );
	}

	


	//////////////////////////////////////////////////////////////////////////
	// OnModuleChanged
	private	void	OnModuleChanged( WeaponSlots slot, Database.Section choosenModuleSection )
	{
		m_CurrentAssignedModuleSections[slot] = choosenModuleSection;

		m_ApplyButton.interactable = true;

		/*
		WeaponModuleSlot slotModule = null;
		WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( slot, ref slotModule);

		slotModule.TrySetModule( WeaponManager.Instance.CurrentWeapon, choosenModuleSection );
		*/
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnApply()
	{
		foreach( KeyValuePair<WeaponSlots, Database.Section> pair in m_CurrentAssignedModuleSections )
		{
			WeaponModuleSlot slotModule = null;
			WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( pair.Key, ref slotModule );

			slotModule.TrySetModule( WeaponManager.Instance.CurrentWeapon, pair.Value );
		} 
	} 


	//////////////////////////////////////////////////////////////////////////
	private void	OnSwitchToInventory()
	{
		UIManager.Instance.GoToMenu( UIManager.Inventory );
		GameManager.Instance.RequireFrameSkip();
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnReturnToGame()
	{
		GameManager.Instance.RequireFrameSkip();
		UIManager.Instance.GoToMenu( UIManager.InGame );
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		if ( CameraControl.Instance != null )
		{
			CameraControl.Instance.CanParseInput	= true;
		}

//		InputManager.IsEnabled					= true;
		GlobalManager.InputMgr.EnableCategory( InputCategory.ALL );

		GlobalManager.SetCursorVisibility( false );

		WeaponManager.Instance.CurrentWeapon.Draw();
	}

}
