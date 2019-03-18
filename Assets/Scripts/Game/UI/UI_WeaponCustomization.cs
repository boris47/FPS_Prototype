using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponCustomization : MonoBehaviour {

	private	Dropdown		m_PrimaryDropDown		= null;
	private	Dropdown		m_SecondaryDropDown		= null;
	private	Dropdown		m_TertiaryDropDown		= null;



	//////////////////////////////////////////////////////////////////////////
	// Initialize
	public	 void Initialize()
	{
		Transform child = transform.Find("CustomizationPanel");
		child.SearchComponentInChild( "ModulePrimaryDropdown", ref m_PrimaryDropDown );
		child.SearchComponentInChild( "ModuleSecondaryDropdown", ref m_SecondaryDropDown );
		child.SearchComponentInChild( "ModuleTertiaryDropdown", ref m_TertiaryDropDown );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		string[] currentModuleNames = WeaponManager.Instance.CurrentWeapon.OtherInfo.Split( ',' );

		Database.Section[] fireModules		= GameManager.Configs.GetSectionsByContext( "WeaponFireModules" );
		Database.Section[] utiliyModules	= GameManager.Configs.GetSectionsByContext( "WeaponUtilityModules" );

		List<Database.Section> allModules = new List<Database.Section>();
		{
			allModules.AddRange( fireModules );
			allModules.AddRange( utiliyModules );
		}

		List<string> allNames = allModules.ConvertAll( 
			new System.Converter<Database.Section, string>( ( Database.Section res ) => { return res.AsString("Name"); } )
		);

		List<string>moduleNames = allModules.ConvertAll( 
			new System.Converter<Database.Section, string>( ( Database.Section res ) => { return res.Name(); } )
		);

		// PRIMARY
		{
			UnityEngine.Events.UnityAction<int> onPrimaryAction = delegate( int index )
			{
				OnModuleChanged( WeaponSlots.PRIMARY, moduleNames[index] );
			};
			m_PrimaryDropDown.ClearOptions();
			m_PrimaryDropDown.AddOptions( allNames );
			m_PrimaryDropDown.value = moduleNames.FindIndex( s => s == currentModuleNames[(int)WeaponSlots.PRIMARY] );
			m_PrimaryDropDown.onValueChanged.AddListener( onPrimaryAction );
		}

		// SECONDARY
		{
			UnityEngine.Events.UnityAction<int> onSecondaryAction = delegate( int index )
			{
				OnModuleChanged( WeaponSlots.SECONDARY, moduleNames[index] );
			};
			m_SecondaryDropDown.ClearOptions();
			m_SecondaryDropDown.AddOptions( allNames );
			m_SecondaryDropDown.value = moduleNames.FindIndex( s => s == currentModuleNames[(int)WeaponSlots.SECONDARY] );
			m_SecondaryDropDown.onValueChanged.AddListener( onSecondaryAction );
		}

		// TERTIARY
		{
			UnityEngine.Events.UnityAction<int> onTertiaryAction = delegate( int index )
			{
				OnModuleChanged( WeaponSlots.TERTIARY, moduleNames[index] );
			};
			m_TertiaryDropDown.ClearOptions();
			m_TertiaryDropDown.AddOptions( allNames );
			m_TertiaryDropDown.value = moduleNames.FindIndex( s => s == currentModuleNames[(int)WeaponSlots.TERTIARY] );
			m_TertiaryDropDown.onValueChanged.AddListener( onTertiaryAction );
		}

		CameraControl.Instance.CanParseInput	= false;
		InputManager.IsEnabled					= false;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnModuleChanged
	private	void	OnModuleChanged( WeaponSlots slot, string choosenModuleName )
	{
		WeaponModuleSlot slotModule = null;
		WeaponManager.Instance.CurrentWeapon.bGetModuleSlot( slot, ref slotModule);

		System.Type type = System.Type.GetType( choosenModuleName );
		slotModule.TrySetModule( WeaponManager.Instance.CurrentWeapon, type );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDisable
	private void OnDisable()
	{
		CameraControl.Instance.CanParseInput	= true;
		InputManager.IsEnabled					= true;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

}
