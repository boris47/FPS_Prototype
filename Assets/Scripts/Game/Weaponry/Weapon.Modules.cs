
using UnityEngine;

[System.Serializable]
public enum EWeaponSlots : uint
{
	PRIMARY, SECONDARY, NONE
}


public abstract partial class WeaponBase
{
	[Header("WeaponModules")]

	[SerializeField]
	protected		WeaponModuleSlot		m_PrimaryWeaponModuleSlot		= null;
	[SerializeField]
	protected		WeaponModuleSlot		m_SecondaryWeaponModuleSlot		= null;


	//////////////////////////////////////////////////////////////////////////
	private static void LoadAndConfigureModule(IWeapon wpn, Database.Section section, WeaponModuleSlot weaponModuleSlot)
	{
		string wpnModuleSectName = GetModuleSlotName(weaponModuleSlot.Slot);
		
		// Check if slot has module assigned
		if (section.AsBool("Has" + wpnModuleSectName))
		{
			// Get Module Section Name
			if (CustomAssertions.IsTrue(section.TryAsString(wpnModuleSectName, out string wpnModuleSect), $"Weapon {wpn.Transform.name}: Unable to retrieve module section name {wpnModuleSectName}"))
			{
				// Get Module Section
				CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(wpnModuleSect, out Database.Section moduleSection));

				// Try Load up Module into module Slot
				LoadWeaponModule(wpn, wpnModuleSect, weaponModuleSlot);

				// Apply mods, if assigned to module
				ApplyModuleMods(section, wpnModuleSectName, weaponModuleSlot);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private static void LoadWeaponModule(in IWeapon wpn, in string weaponModuleSection, WeaponModuleSlot weaponModuleSlot)
	{
		System.Type type = System.Type.GetType(weaponModuleSection.Trim());
		weaponModuleSlot.SetModule(wpn, type);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void ApplyModuleMods(in Database.Section section, in string weaponModuleSectionName, in WeaponModuleSlot weaponModule)
	{
		if (section.TryGetMultiAsArray($"{weaponModuleSectionName}Mods", out string[] mods))
		{
			using (var modifiable = new Modifiable(weaponModule.WeaponModule))
			{
				foreach (string modifierSectionName in mods)
				{
					if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(modifierSectionName, out Database.Section modifierSection)))
					{
						modifiable.AddModifier(modifierSection);
					}
				}
			}
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetModuleBySlot(in IWeapon wpn, in EWeaponSlots slot, out WPN_BaseModule weaponModule)
	{
		weaponModule = null;
		WeaponBase w = wpn as WeaponBase;
		switch (slot)
		{
			case EWeaponSlots.PRIMARY:		weaponModule = w.m_PrimaryWeaponModuleSlot.WeaponModule;		break;
			case EWeaponSlots.SECONDARY:	weaponModule = w.m_SecondaryWeaponModuleSlot.WeaponModule;		break;
			default:	break;
		}
		return weaponModule.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	public static bool TryGetModuleSlot(in IWeapon wpn, in EWeaponSlots slot, out WeaponModuleSlot moduleSlot)
	{
		moduleSlot = null;
		WeaponBase w = wpn as WeaponBase;
		switch (slot)
		{
			case EWeaponSlots.PRIMARY:		moduleSlot = w.m_PrimaryWeaponModuleSlot;		break;
			case EWeaponSlots.SECONDARY:	moduleSlot = w.m_SecondaryWeaponModuleSlot;		break;
			default:	break;
		}
		return moduleSlot.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	public static string GetModuleSlotName(EWeaponSlots slot)
	{
		string result = "";
		switch (slot)
		{
			case EWeaponSlots.PRIMARY:		result = "PrimaryWeaponModule";		break;
			case EWeaponSlots.SECONDARY:	result = "SecondaryWeaponModule";	break;
			default:	break;
		}
		return result;
	}


	protected	virtual		bool			Predicate_Base						() => m_WeaponState == EWeaponState.DRAWED && !m_IsLocked;
	protected	virtual		bool			Predicate_PrimaryFire_Start			() => Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed();
	protected	virtual		bool			Predicate_PrimaryFire_Update		() => Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed();
	protected	virtual		bool			Predicate_PrimaryFire_End			() => Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed();

	protected	virtual		bool			Predicate_SecondaryFire_Start		() => Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed();
	protected	virtual		bool			Predicate_SecondaryFire_Update		() => Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed();
	protected	virtual		bool			Predicate_SecondaryFire_End			() => Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed();

	protected	virtual		bool			Predicate_Reload					() => Predicate_Base() && m_NeedRecharge;


	protected	virtual		void			PrimaryFire_Start					()	{ m_PrimaryWeaponModuleSlot.WeaponModule.OnStart(); m_WeaponSubState = EWeaponSubState.FIRING;	}
	protected	virtual		void			PrimaryFire_Update					()	{ m_PrimaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			PrimaryFire_End						()	{ m_PrimaryWeaponModuleSlot.WeaponModule.OnEnd(); m_WeaponSubState = EWeaponSubState.IDLE;	}
	
	protected	virtual		void			SecondaryFire_Start					()	{ m_SecondaryWeaponModuleSlot.WeaponModule.OnStart(); m_WeaponSubState = EWeaponSubState.FIRING; 	}
	protected	virtual		void			SecondaryFire_Update				()	{ m_SecondaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			SecondaryFire_End					()	{ m_SecondaryWeaponModuleSlot.WeaponModule.OnEnd(); m_WeaponSubState = EWeaponSubState.IDLE;	}
}