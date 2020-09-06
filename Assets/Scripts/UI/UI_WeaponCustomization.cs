using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_WeaponCustomization : MonoBehaviour, IStateDefiner {

	private		Dropdown		m_PrimaryDropDown		= null;
	private		Dropdown		m_SecondaryDropDown		= null;
	private		Dropdown		m_TertiaryDropDown		= null;

	private		Button			m_ReturnToGame			= null;
	private		Button			m_SwitchToInventory		= null;
	private		Button			m_ApplyButton			= null;

	private		bool			m_IsInitialized		= false;

	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}

	Dictionary<EWeaponSlots, Database.Section> m_CurrentAssignedModuleSections = new Dictionary<EWeaponSlots, Database.Section>()
	{
		{ EWeaponSlots.PRIMARY,		new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) },
		{ EWeaponSlots.SECONDARY,	new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) },
		{ EWeaponSlots.TERTIARY,		new Database.Section( "WPN_BaseModuleEmpty", "Unassigned" ) }
	};

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator	IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		Transform child = this.transform.Find("CustomizationPanel");
		if (this.m_IsInitialized = ( child != null ) )
		{
			this.m_IsInitialized &= child.SearchComponentInChild( "ModulePrimaryDropdown", ref this.m_PrimaryDropDown );
			this.m_IsInitialized &= child.SearchComponentInChild( "ModuleSecondaryDropdown", ref this.m_SecondaryDropDown );
			this.m_IsInitialized &= child.SearchComponentInChild( "ModuleTertiaryDropdown", ref this.m_TertiaryDropDown );

			yield return null;

			// APPLY BUTTON
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "ApplyButton", ref this.m_ApplyButton ) )
			{
				this.m_ApplyButton.interactable = false;
				this.m_ApplyButton.onClick.AddListener
				(	
					delegate()
					{
						UIManager.Confirmation.Show( "Apply Changes?", this.OnApply, delegate { this.OnEnable(); } );
					}
				);
			}

			yield return null;

			// SWITCH TO INVENTORY
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "SwitchToInventory", ref this.m_SwitchToInventory ) )
			{
				this.m_SwitchToInventory.onClick.AddListener
				(
					this.OnSwitchToInventory
				);
			}

			yield return null;

			// RETURN TO GAME
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "ReturnToGame", ref this.m_ReturnToGame ) )
			{
				this.m_ReturnToGame.onClick.AddListener
				(
					this.OnReturnToGame
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
		return this.m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		if (this.m_IsInitialized == false )
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
		this.FillDropdown(this.m_PrimaryDropDown,	allModules, EWeaponSlots.PRIMARY );

		// SECONDARY
		this.FillDropdown(this.m_SecondaryDropDown,	allModules, EWeaponSlots.SECONDARY );

		// TERTIARY
		this.FillDropdown(this.m_TertiaryDropDown,	allModules, EWeaponSlots.TERTIARY );


		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);
//		InputManager.IsEnabled					= false;

		// All categories but not interface
		GlobalManager.InputMgr.DisableCategory( EInputCategory.ALL | EInputCategory.INTERFACE );

		GlobalManager.SetCursorVisibility( true );

		WeaponManager.Instance.CurrentWeapon.Stash();
	}


	//////////////////////////////////////////////////////////////////////////
	// FillDropdown
	private	void	FillDropdown( Dropdown thisDropdown, List<Database.Section> allModules, EWeaponSlots slot )
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

		this.m_CurrentAssignedModuleSections[slot] = new Database.Section( alreadyAssignedModules[(int)slot], "" );

		// Search current Value
		thisDropdown.value = filtered.FindIndex( s => s.GetName() == alreadyAssignedModules[(int)slot] );

		thisDropdown.onValueChanged.RemoveAllListeners();
		UnityEngine.Events.UnityAction<int> callback = delegate( int moduleIndex )
		{
			this.OnModuleChanged( slot, filtered[moduleIndex] );
		};
		thisDropdown.onValueChanged.AddListener( callback );
	}

	


	//////////////////////////////////////////////////////////////////////////
	// OnModuleChanged
	private	void	OnModuleChanged( EWeaponSlots slot, Database.Section choosenModuleSection )
	{
		this.m_CurrentAssignedModuleSections[slot] = choosenModuleSection;

		this.m_ApplyButton.interactable = true;

		/*
		WeaponModuleSlot slotModule = null;
		WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( slot, ref slotModule);

		slotModule.TrySetModule( WeaponManager.Instance.CurrentWeapon, choosenModuleSection );
		*/
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnApply()
	{
		foreach( KeyValuePair<EWeaponSlots, Database.Section> pair in this.m_CurrentAssignedModuleSections )
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
		if (this.m_IsInitialized == false )
		{
			return;
		}

		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

//		InputManager.IsEnabled					= true;
		GlobalManager.InputMgr.EnableCategory( EInputCategory.ALL );

		GlobalManager.SetCursorVisibility( false );

		WeaponManager.Instance.CurrentWeapon.Draw();
	}

}
