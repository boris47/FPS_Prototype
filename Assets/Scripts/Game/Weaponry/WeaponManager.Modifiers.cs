
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial interface IWeaponManager {

	bool					ApplyModifierToWeapon					( IWeapon wpn, string modifier );
	bool					RemoveModifierFromWeapon				( IWeapon wpn, string modifier );

	
	bool					ApplyModifierToWeaponSlot				( IWeapon wpn, EWeaponSlots slot, string modifier );
	bool					RemoveModifierToWeaponSlot				( IWeapon wpn, EWeaponSlots slot, string modifier );


	List<Database.Section>	ListCompatibleModifiersWithModule		( IWeapon wpn, EWeaponSlots slot );
}


public sealed partial class WeaponManager : IWeaponManager {

	private			List<string>	m_AvailableWeaponModifiers		= new List<string>();
	private			List<string>	m_AvailableModuleModifiers		= new List<string>();
	private			List<string>	m_AvailableFireModeModifiers	= new List<string>();
	/*
	public	string[]	AvailableWeaponModifiers
	{
		get { return m_AvailableWeaponModifiers.ToArray(); }
	}
	public	string[]	AvailableModuleModifiers
	{
		get { return m_AvailableModuleModifiers.ToArray(); }
	}
	public	string[]	AvailableFireModeModifiers
	{
		get { return m_AvailableFireModeModifiers.ToArray(); }
	}
	*/



	bool			IWeaponManager.ApplyModifierToWeapon( IWeapon wpn, string modifierSectionName )
	{
		if ( GlobalManager.Configs.TryGetSection( modifierSectionName, out Database.Section modifierSection ) )
		{
			( wpn as IModifiable ).ApplyModifier( modifierSection );
			return true;
		}
		return false;
	}

	bool			IWeaponManager.RemoveModifierFromWeapon( IWeapon wpn, string modifierSectionName )
	{
		if ( GlobalManager.Configs.TryGetSection( modifierSectionName, out Database.Section modifierSection ) )
		{
			( wpn as IModifiable ).RemoveModifier( modifierSection );
			return true;
		}
		return false;
	}


	bool			IWeaponManager.ApplyModifierToWeaponSlot( IWeapon wpn, EWeaponSlots slot, string modifier )
	{
		if ( wpn.TryGetModuleBySlot( slot, out WPN_BaseModule weaponModule ) && GlobalManager.Configs.TryGetSection( modifier, out Database.Section modifierSection ) )
		{
			weaponModule.ApplyModifier( modifierSection );
			return true;
		}

		return false;
	}


	bool			IWeaponManager.RemoveModifierToWeaponSlot( IWeapon wpn, EWeaponSlots slot, string modifier )
	{
		if ( wpn.TryGetModuleBySlot( slot, out WPN_BaseModule weaponModule) && GlobalManager.Configs.TryGetSection( modifier, out Database.Section modifierSection ) )
		{
			weaponModule.RemoveModifier( modifierSection );
			return true;
		}

		return false;
	}


	List<Database.Section>	IWeaponManager.ListCompatibleModifiersWithModule( IWeapon wpn, EWeaponSlots slot )
	{
		List<Database.Section> result = null;

		if ( wpn.TryGetModuleSlot( slot, out WeaponModuleSlot moduleSlot ) )
		{
			foreach( Database.Section section in GlobalManager.Configs.GetSectionsByContext( "WeaponModulesModifiers" ) )
			{
				if ( moduleSlot.CanAssignModule( section, null ) )
				{
					result.Add( section );
				}
			}
		}


		return result;
	}

}