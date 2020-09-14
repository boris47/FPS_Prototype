
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
	private	static	bool					LoadAndConfigureModule( IWeapon wpn, Database.Section section, ref WeaponModuleSlot weaponModuleSlot )
	{
		string wpnModuleSect = null;
		string wpnModuleSectName = GetModuleSlotName( weaponModuleSlot.ThisSlot );
		Database.Section moduleSection = null;

		// Check if slot has module assigned
		if ( section.AsBool( "Has" + wpnModuleSectName ) == false )
		{
			return true;
		}

		// Get Module Section Name
		if ( section.bAsString( wpnModuleSectName, ref wpnModuleSect ) == false )
		{
			Debug.Log( $"Error: Weapon {wpn.Transform.name}: Unable to retrieve module section name {wpnModuleSectName}" );
			return false;
		}

		// Get Module Section
		if ( GlobalManager.Configs.GetSection( wpnModuleSect, ref moduleSection ) == false )
		{
			Debug.Log( $"Error: Weapon {wpn.Transform.name}: Unable to retrieve {wpnModuleSect} for module {wpnModuleSectName}" );
			return false;
		}

		// Try Load up Module into module Slot
		if ( LoadWeaponModule( wpn, wpnModuleSect, ref weaponModuleSlot ) == false )
		{
			Debug.Log( $"Error: Weapon {wpn.Transform.name}: Unable to load module {wpnModuleSectName}" );
			return false;
		}

		// Apply mods, if assigned to module
		ApplyModuleMods( section, wpnModuleSectName, weaponModuleSlot );
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private	static	bool					LoadWeaponModule( IWeapon wpn, string weaponModuleSection, ref WeaponModuleSlot weaponModuleSlot )
	{
		System.Type type = System.Type.GetType( weaponModuleSection.Trim() );
		return weaponModuleSlot.TrySetModule( wpn, type );
	}


	//////////////////////////////////////////////////////////////////////////
	private static	void					ApplyModuleMods( Database.Section section, string weaponModuleSectionName, WeaponModuleSlot weaponModule )
	{
		string[] mods = null;
		if ( section.bGetMultiAsArray( weaponModuleSectionName + "Mods", ref mods ) )
		{
			Database.Section modifierSection = null;
			foreach( string modifierSectionName in mods )
			{
				if ( GlobalManager.Configs.GetSection( modifierSectionName, ref modifierSection ) )
				{
					weaponModule.WeaponModule.ApplyModifier( modifierSection );
				}
			}

		}
	}


	#region		PREDICATES
	protected	virtual		bool			Predicate_Base() { return this.m_WeaponState == EWeaponState.DRAWED /*&& Player.Instance.ChosingDodgeRotation == false*/ && this.m_IsLocked == false; }
	protected	virtual		bool			Predicate_PrimaryFire_Start()		{ return this.Predicate_Base() && this.m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_Update()		{ return this.Predicate_Base() && this.m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_PrimaryFire_End()			{ return this.Predicate_Base() && this.m_PrimaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_SecondaryFire_Start()		{ return this.Predicate_Base() && this.m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_Update()	{ return this.Predicate_Base() && this.m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_SecondaryFire_End()		{ return this.Predicate_Base() && this.m_SecondaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_TertiaryFire_Start()		{ return this.Predicate_Base() && this.m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_Update()		{ return this.Predicate_Base() && this.m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }
	protected	virtual		bool			Predicate_TertiaryFire_End()		{ return this.Predicate_Base() && this.m_TertiaryWeaponModuleSlot.WeaponModule.CanBeUsed(); }

	protected	virtual		bool			Predicate_Reload()					{ return this.Predicate_Base() && this.m_NeedRecharge == true; }
	#endregion

	#region MODULES EVENTS
	protected	virtual		void			PrimaryFire_Start()		{ this.m_PrimaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			PrimaryFire_Update()	{ this.m_PrimaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			PrimaryFire_End()		{ this.m_PrimaryWeaponModuleSlot.WeaponModule.OnEnd();		}
	
	protected	virtual		void			SecondaryFire_Start()	{ this.m_SecondaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			SecondaryFire_Update()	{ this.m_SecondaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			SecondaryFire_End()		{ this.m_SecondaryWeaponModuleSlot.WeaponModule.OnEnd();	}

	protected	virtual		void			TertiaryFire_Update()	{ this.m_TertiaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			TertiaryFire_Start()	{ this.m_TertiaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			TertiaryFire_End()		{ this.m_TertiaryWeaponModuleSlot.WeaponModule.OnEnd();		}
	#endregion

}




[System.Serializable]
public class WeaponModuleSlot
{
	[SerializeField]
	public					EWeaponSlots			ThisSlot { get; }			= EWeaponSlots.PRIMARY;

	[SerializeField]
	private					WPN_BaseModule			m_WeaponModule				= null;

	public					WPN_BaseModule			WeaponModule
	{
		get { return this.m_WeaponModule; }
	}

	//////////////////////////////////////////////////////////////////////////
	public WeaponModuleSlot( EWeaponSlots slot )
	{
		this.ThisSlot = slot;
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool	GetModuleRules( Database.Section moduleSection, ref bool[] allowedSlots, ref int maxCount )
	{
		maxCount = moduleSection.AsInt( "MaxModuleCount", 1 );

		// By default, if not present th key "AllowedSlots", all slot are allowed
		int[] localAllowedSlots = new int[ 3 ] { 1, 2, 3 };
		if ( moduleSection.bGetMultiAsArray<int>( "AllowedSlots", ref localAllowedSlots ) )
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
			result &= allowedSlots[(int)this.ThisSlot];
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
		return this.TrySetModule( wpn, type );
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySetModule( IWeapon wpn, System.Type type )
	{
		if ( type == null )
		{
			Debug.Log( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}, Slot:{Weapon.GetModuleSlotName(this.ThisSlot)}, Setting invalid weapon module" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_BaseModule ) ) == false )
		{
			Debug.Log( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}, Slot:{Weapon.GetModuleSlotName(this.ThisSlot)}, Class Requested is not a supported weapon module, \"{type.ToString()}\"" );
			return false;
		}

		if (this.m_WeaponModule.IsNotNull() )
		{
			if (this.m_WeaponModule.GetType() == type )
			{
				Debug.Log( $"WeaponModuleSlot::TrySetModule: {this.GetType().Name}, Slot:{Weapon.GetModuleSlotName(this.ThisSlot)}, the module \"{type.ToString()}\" is already mounted" );
				return true;
			}

			this.m_WeaponModule.OnDetach();
			Object.Destroy(this.m_WeaponModule );
		}

		WPN_BaseModule wpnModule = wpn.Transform.gameObject.AddComponent( type ) as WPN_BaseModule;

		// On success assign to internal var
		bool bAttachSuccess = wpnModule.OnAttach( wpn, this.ThisSlot );
		if ( bAttachSuccess == true )
		{
			this.m_WeaponModule = wpnModule;
		}
		// On Fail add empty module
		else
		{
			Object.Destroy( wpnModule );
			this.m_WeaponModule = wpn.Transform.gameObject.AddComponent<WPN_BaseModuleEmpty>();
			Debug.LogError( $"WeaponModuleSlot::TrySetModule: {wpn.Section.GetSectionName()}: Module \"{type.ToString()}\" has failed the attach" );
		}

		return bAttachSuccess;
	}

}