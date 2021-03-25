
using UnityEngine;


public enum EWeaponSlots : uint
{
	PRIMARY, SECONDARY, TERTIARY, NONE
}


public abstract partial class Weapon
{
	[Header("WeaponModules")]

	[SerializeField]	protected		WeaponModuleSlot		m_PrimaryWeaponModuleSlot		= new WeaponModuleSlot( EWeaponSlots.PRIMARY );
	[SerializeField]	protected		WeaponModuleSlot		m_SecondaryWeaponModuleSlot		= new WeaponModuleSlot( EWeaponSlots.SECONDARY );
	[SerializeField]	protected		WeaponModuleSlot		m_TertiaryWeaponModuleSlot		= new WeaponModuleSlot( EWeaponSlots.TERTIARY );

	//////////////////////////////////////////////////////////////////////////
	private static bool LoadAndConfigureModule(IWeapon wpn, Database.Section section, ref WeaponModuleSlot weaponModuleSlot)
	{
		string wpnModuleSect = null;
		string wpnModuleSectName = GetModuleSlotName(weaponModuleSlot.ThisSlot);

		// Check if slot has module assigned
		if (section.AsBool("Has" + wpnModuleSectName) == false)
		{
			return true;
		}

		// Get Module Section Name
		if (section.TryAsString(wpnModuleSectName, out wpnModuleSect) == false)
		{
			Debug.Log($"Error: Weapon {wpn.Transform.name}: Unable to retrieve module section name {wpnModuleSectName}");
			return false;
		}

		// Get Module Section
		CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection(wpnModuleSect, out Database.Section moduleSection));

		// Try Load up Module into module Slot
		CustomAssertions.IsTrue(LoadWeaponModule(wpn, wpnModuleSect, ref weaponModuleSlot));

		// Apply mods, if assigned to module
		ApplyModuleMods(section, wpnModuleSectName, weaponModuleSlot);
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool LoadWeaponModule(IWeapon wpn, string weaponModuleSection, ref WeaponModuleSlot weaponModuleSlot)
	{
		System.Type type = System.Type.GetType(weaponModuleSection.Trim());
		return weaponModuleSlot.TrySetModule(wpn, type);
	}


	//////////////////////////////////////////////////////////////////////////
	private static void ApplyModuleMods(Database.Section section, string weaponModuleSectionName, WeaponModuleSlot weaponModule)
	{
		if (section.TryGetMultiAsArray($"{weaponModuleSectionName}Mods", out string[] mods))
		{
			foreach (string modifierSectionName in mods)
			{
				if (GlobalManager.Configs.TryGetSection(modifierSectionName, out Database.Section modifierSection))
				{
					weaponModule.WeaponModule.ApplyModifier(modifierSection);
				}
			}

		}
	}


	#region		PREDICATES
	protected	virtual		bool			Predicate_Base()
	{
		return m_WeaponState == EWeaponState.DRAWED && m_IsLocked == false;
	}
	protected	virtual		bool			Predicate_PrimaryFire_Start()		{ return Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_Update()		{ return Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_End()			{ return Predicate_Base() && m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_SecondaryFire_Start()		{ return Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_Update()	{ return Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_End()		{ return Predicate_Base() && m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_TertiaryFire_Start()		{ return Predicate_Base() && m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_Update()		{ return Predicate_Base() && m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_End()		{ return Predicate_Base() && m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_Reload()					{ return Predicate_Base() && m_NeedRecharge == true; }
	#endregion

	#region MODULES EVENTS
	protected	virtual		void			PrimaryFire_Start()		{ m_PrimaryWeaponModuleSlot.WeaponModule.OnStart(); m_WeaponSubState = EWeaponSubState.FIRING;	}
	protected	virtual		void			PrimaryFire_Update()	{ m_PrimaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			PrimaryFire_End()		{ m_PrimaryWeaponModuleSlot.WeaponModule.OnEnd(); m_WeaponSubState = EWeaponSubState.IDLE;	}
	
	protected	virtual		void			SecondaryFire_Start()	{ m_SecondaryWeaponModuleSlot.WeaponModule.OnStart(); m_WeaponSubState = EWeaponSubState.FIRING; 	}
	protected	virtual		void			SecondaryFire_Update()	{ m_SecondaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			SecondaryFire_End()		{ m_SecondaryWeaponModuleSlot.WeaponModule.OnEnd(); m_WeaponSubState = EWeaponSubState.IDLE;	}

	protected	virtual		void			TertiaryFire_Update()	{ m_TertiaryWeaponModuleSlot.WeaponModule.OnStart(); m_WeaponSubState = EWeaponSubState.FIRING;	}
	protected	virtual		void			TertiaryFire_Start()	{ m_TertiaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			TertiaryFire_End()		{ m_TertiaryWeaponModuleSlot.WeaponModule.OnEnd(); m_WeaponSubState = EWeaponSubState.IDLE;	}
	#endregion

}




[System.Serializable]
public class WeaponModuleSlot
{
	[SerializeField]
	public					EWeaponSlots			ThisSlot { get; }			= EWeaponSlots.PRIMARY;

	[SerializeField]
	private					WPN_BaseModule			m_WeaponModule				= null;

	public					WPN_BaseModule			WeaponModule				=> m_WeaponModule;
	
	//////////////////////////////////////////////////////////////////////////
	public WeaponModuleSlot( EWeaponSlots slot )
	{
		ThisSlot = slot;
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool	GetModuleRules( Database.Section moduleSection, ref bool[] allowedSlots, ref int maxCount )
	{
		maxCount = moduleSection.AsInt( "MaxModuleCount", 1 );

		// By default, if not present th key "AllowedSlots", all slot are allowed
		int[] localAllowedSlots = new int[ 3 ] { 1, 2, 3 };
		if ( moduleSection.TryGetMultiAsArray<int>( "AllowedSlots", out localAllowedSlots ) )
		{
			allowedSlots = new bool[3]
			{ // slot - 1 because in config file user use number 1, 2 and 3 but array starts from Zero
				System.Array.Exists( localAllowedSlots, slot => slot - 1 == (int)EWeaponSlots.PRIMARY ),
				System.Array.Exists( localAllowedSlots, slot => slot - 1 == (int)EWeaponSlots.SECONDARY ),
				System.Array.Exists( localAllowedSlots, slot => slot - 1 == (int)EWeaponSlots.TERTIARY )
			};
		}
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	CanAssignModule( Database.Section moduleSection, string[] alreadyAssignedModules = null )
	{
		bool result = true;

		// Is this slot allowed for module
		int maxCount = 0;
		bool[] allowedSlots = null;
		if ( GetModuleRules( moduleSection, ref allowedSlots, ref maxCount ) )
		{
			result &= allowedSlots[(int)ThisSlot];
		}
		
		// Is this module max count less the maximum allowed
		if ( alreadyAssignedModules != null )
		{
			int counter = 0;
			string name = moduleSection.GetSectionName();
			System.Array.ForEach( alreadyAssignedModules, m => { if (m == name) counter++; } );
			result &= !( counter > maxCount );
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySetModule( IWeapon wpn, Database.Section moduleSection )
	{
		System.Type type = System.Type.GetType( moduleSection.GetSectionName() );
		return TrySetModule( wpn, type );
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySetModule( IWeapon wpn, System.Type type )
	{
		if ( type == null )
		{
			Debug.Log( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}, Slot:{Weapon.GetModuleSlotName(ThisSlot)}, Setting invalid weapon module" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_BaseModule ) ) == false )
		{
			Debug.Log( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}, Slot:{Weapon.GetModuleSlotName(ThisSlot)}, Class Requested is not a supported weapon module, \"{type.ToString()}\"" );
			return false;
		}

		if (m_WeaponModule.IsNotNull() )
		{
			if (m_WeaponModule.GetType() == type )
			{
			//	Debug.Log( $"WeaponModuleSlot::TrySetModule: {GetType().Name}, Slot:{Weapon.GetModuleSlotName(ThisSlot)}, the module \"{type.ToString()}\" is already mounted" );
				return true;
			}

			m_WeaponModule.OnDetach();
			Object.Destroy(m_WeaponModule );
		}

		WPN_BaseModule wpnModule = wpn.Transform.gameObject.AddComponent( type ) as WPN_BaseModule;

		// On success assign to internal var
		bool bAttachSuccess = wpnModule.OnAttach( wpn, ThisSlot );
		if ( bAttachSuccess == true )
		{
			m_WeaponModule = wpnModule;
		}
		// On Fail add empty module
		else
		{
			Object.Destroy( wpnModule );
			m_WeaponModule = wpn.Transform.gameObject.AddComponent<WPN_BaseModuleEmpty>();
			Debug.LogError( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}: Module \"{type.ToString()}\" has failed the attach" );
		}

		return bAttachSuccess;
	}

}