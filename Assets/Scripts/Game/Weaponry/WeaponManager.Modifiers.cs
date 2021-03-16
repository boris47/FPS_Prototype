using UnityEngine;
using System.Linq;

public partial interface IWeaponManager
{
	bool					ApplyModifierToWeapon					(in IWeapon wpn, in string modifier);
	bool					RemoveModifierFromWeapon				(in IWeapon wpn, in string modifier);
	
	bool					ApplyModifierToWeaponSlot				(in IWeapon wpn, in EWeaponSlots slot, string modifier);
	bool					RemoveModifierToWeaponSlot				(in IWeapon wpn, in EWeaponSlots slot, string modifier);

	bool					ListCompatibleModifiersWithModule		(in IWeapon wpn, in EWeaponSlots slot, out string[] sectionNames);
}


public sealed partial class WeaponManager
{
	bool IWeaponManager.ApplyModifierToWeapon(in IWeapon wpn, in string modifierSectionName)
	{
		if (GlobalManager.Configs.TryGetSection(modifierSectionName, out Database.Section modifierSection))
		{
			(wpn as IModifiable).ApplyModifier(modifierSection);
			return true;
		}
		return false;
	}

	bool IWeaponManager.RemoveModifierFromWeapon(in IWeapon wpn, in string modifierSectionName)
	{
		if (GlobalManager.Configs.TryGetSection(modifierSectionName, out Database.Section modifierSection))
		{
			(wpn as IModifiable).RemoveModifier(modifierSection);
			return true;
		}
		return false;
	}


	bool IWeaponManager.ApplyModifierToWeaponSlot(in IWeapon wpn, in EWeaponSlots slot, string modifier)
	{
		if (Weapon.TryGetModuleBySlot(wpn, slot, out WPN_BaseModule weaponModule) && GlobalManager.Configs.TryGetSection(modifier, out Database.Section modifierSection))
		{
			weaponModule.ApplyModifier(modifierSection);
			return true;
		}

		return false;
	}


	bool IWeaponManager.RemoveModifierToWeaponSlot(in IWeapon wpn, in EWeaponSlots slot, string modifier)
	{
		if (Weapon.TryGetModuleBySlot(wpn, slot, out WPN_BaseModule weaponModule) && GlobalManager.Configs.TryGetSection(modifier, out Database.Section modifierSection))
		{
			weaponModule.RemoveModifier(modifierSection);
			return true;
		}

		return false;
	}


	bool IWeaponManager.ListCompatibleModifiersWithModule(in IWeapon wpn, in EWeaponSlots slot, out string[] sectionNames)
	{
		sectionNames = default;
		if (Weapon.TryGetModuleSlot(wpn, slot, out WeaponModuleSlot moduleSlot))
		{
			sectionNames = GlobalManager.Configs.GetSectionsByContext("WeaponModulesModifiers")
				.Where(s => moduleSlot.CanAssignModule(s, null))
				.Select(s => s.GetSectionName())
				.ToArray();
			return true;
		}
		return false;
	}

}