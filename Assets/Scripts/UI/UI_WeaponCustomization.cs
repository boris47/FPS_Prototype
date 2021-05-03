using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_WeaponCustomization : UI_Base, IStateDefiner
{
	private				Dropdown							m_PrimaryDropDown					= null;
	private				Dropdown							m_SecondaryDropDown					= null;

	private				Button								m_ReturnToGame						= null;
	private				Button								m_SwitchToInventory					= null;
	private				Button								m_ApplyButton						= null;

	private				bool								m_IsInitialized						= false;
						bool								IStateDefiner.IsInitialized			=> m_IsInitialized;

	private readonly Dictionary<EWeaponSlots, Database.Section> m_CurrentAssignedModuleSections = new Dictionary<EWeaponSlots, Database.Section>()
//	{
//		{ EWeaponSlots.PRIMARY,		new Database.Section( "WPN_BaseModuleEmpty" ) },
//		{ EWeaponSlots.SECONDARY,	new Database.Section( "WPN_BaseModuleEmpty" ) },
//	}
	;

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("CustomizationPanel", out Transform customizationPanel)))
			{
				CustomAssertions.IsTrue(customizationPanel.TrySearchComponentByChildName("ModulePrimaryDropdown", out m_PrimaryDropDown));
				CustomAssertions.IsTrue(customizationPanel.TrySearchComponentByChildName("ModuleSecondaryDropdown", out m_SecondaryDropDown));
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ApplyButton", out m_ApplyButton)))
			{
				void OnConfirm()
				{
					var currenWeapon = WeaponManager.Instance.CurrentWeapon;
					foreach (KeyValuePair<EWeaponSlots, Database.Section> pair in m_CurrentAssignedModuleSections)
					{
						if (WeaponBase.TryGetModuleSlot(currenWeapon, pair.Key, out WeaponModuleSlot slotModule))
						{
							slotModule.SetModule(currenWeapon, pair.Value);
						}
					}
				}
				m_ApplyButton.onClick.AddListener(() => UIManager.Confirmation.Show("Apply Changes?", OnConfirm));
				m_ApplyButton.interactable = false;
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("SwitchToInventory", out m_SwitchToInventory)))
			{
				m_SwitchToInventory.onClick.AddListener(OnSwitchToInventory);
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ReturnToGame", out m_ReturnToGame)))
			{
				m_ReturnToGame.onClick.AddListener(OnReturnToGame);
			}

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		Database.Section[] fireModules		= GlobalManager.Configs.GetSectionsByContext("WeaponFireModules");
		Database.Section[] utilityModules	= GlobalManager.Configs.GetSectionsByContext("WeaponUtilityModules");
		Database.Section[] allModules		= Enumerable.Concat(fireModules, utilityModules).ToArray();

		// PRIMARY
		FillDropdown(m_PrimaryDropDown,		allModules, EWeaponSlots.PRIMARY);

		// SECONDARY
		FillDropdown(m_SecondaryDropDown,	allModules, EWeaponSlots.SECONDARY);

		// TODo Handle actually disable categories
		GlobalManager.InputMgr.DisableCategory(EInputCategory.ALL);

		GlobalManager.InputMgr.SetCategory(EInputCategory.INTERFACE, true);

		GlobalManager.SetCursorVisibility(true);

		WeaponManager.Instance.CurrentWeapon.Stash();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		// TODo Handle actually disable categories
		GlobalManager.InputMgr.EnableCategory(EInputCategory.ALL);

		GlobalManager.InputMgr.SetCategory(EInputCategory.INTERFACE, false);

		WeaponManager.Instance.CurrentWeapon.Draw();
	}


	//////////////////////////////////////////////////////////////////////////
	private void FillDropdown(in Dropdown thisDropdown, in Database.Section[] allModules, EWeaponSlots slot)
	{
		thisDropdown.ClearOptions();

		IWeapon currentWeapon = WeaponManager.Instance.CurrentWeapon;

		// Get weapon module slot
		if (CustomAssertions.IsTrue(WeaponBase.TryGetModuleSlot(currentWeapon, slot, out WeaponModuleSlot slotModule)))
		{
			string[] alreadyAssignedModules = currentWeapon.OtherInfo.Split(',');

			// Ask slot if module can be assigned
			List<Database.Section> filtered = allModules.Where(s => slotModule.CanAssignModule(s, alreadyAssignedModules)).ToList();

			// Assign readable names to dropdown options
			List<string> ui_Names = filtered.Select(section => section.AsString("Name")).ToList();
			thisDropdown.AddOptions(ui_Names);

			string currentAssigned = alreadyAssignedModules[(int)slot];
			m_CurrentAssignedModuleSections[slot] = new Database.Section(currentAssigned);

			// Search current Value
			thisDropdown.value = filtered.FindIndex(s => s.GetSectionName() == currentAssigned);

			void callback(int moduleIndex)
			{
				m_CurrentAssignedModuleSections[slot] = filtered[moduleIndex];
				m_ApplyButton.interactable = true;
			}
			thisDropdown.onValueChanged.RemoveAllListeners();
			thisDropdown.onValueChanged.AddListener(callback);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnSwitchToInventory()
	{
		GlobalManager.Instance.RequireFrameSkip();

		UIManager.Instance.GoToMenu(UIManager.Inventory);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnReturnToGame()
	{
		GlobalManager.Instance.RequireFrameSkip();

		UIManager.Instance.GoToMenu(UIManager.InGame);

		UIManager.InGame.UpdateUI();
	}
}
