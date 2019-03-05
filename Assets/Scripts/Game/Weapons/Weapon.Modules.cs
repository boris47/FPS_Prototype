
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/////////////////////////////////////////
/////////////////////////////////////////

public enum WeaponSlots : uint {
	PRIMARY, SECONDARY, TERTIARY, NONE
}


public abstract partial class Weapon {

	[Header("WeaponModules")]

	[SerializeField, ReadOnly]	protected		WeaponModuleSlot		m_PrimaryWeaponModuleSlot		= new WeaponModuleSlot( WeaponSlots.PRIMARY );
	[SerializeField, ReadOnly]	protected		WeaponModuleSlot		m_SecondaryWeaponModuleSlot		= new WeaponModuleSlot( WeaponSlots.SECONDARY );
	[SerializeField, ReadOnly]	protected		WeaponModuleSlot		m_TertiaryWeaponModuleSlot		= new WeaponModuleSlot( WeaponSlots.TERTIARY );


	//////////////////////////////////////////////////////////////////////////
	private	static	bool					LoadAndConfigureModule( IWeapon wpn, Database.Section section, ref WeaponModuleSlot weaponModuleSlot )
	{
		string wpnModuleSect = null;
		string wpnModuleSectName = GetModuleSlotName( weaponModuleSlot.ThisSlot );
		Database.Section moduleSection = null;
		if ( section.AsBool( "Has" + wpnModuleSectName ) && section.bAsString( wpnModuleSectName, ref wpnModuleSect ) && GameManager.Configs.bGetSection( wpnModuleSect, ref moduleSection ) )
		{
			if ( LoadWeaponModule( wpn, wpnModuleSect, ref weaponModuleSlot ) == false )
			{
				Destroy( weaponModuleSlot.WeaponModule );
				weaponModuleSlot.TrySetModule( wpn, typeof( WPN_BaseModuleEmpty ) );
				return false;
			}

			ApplyModuleMods( section, wpnModuleSectName, weaponModuleSlot );
			
		}
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
				if ( GameManager.Configs.bGetSection( modifierSectionName, ref modifierSection ) )
				{
					weaponModule.WeaponModule.ApplyModifier( modifierSection );
				}
			}

		}
	}


	#region		PREDICATES
	// PREDICATES	START
	protected virtual		bool			Predicate_Base() { return m_WeaponState == WeaponState.DRAWED && Player.Instance.ChosingDodgeRotation == false && m_IsLocked == false; }
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
	// PREDICATES	END
	#endregion

	#region MODULES EVENTS
	//////////////////////////////////////////////////////////////////////////
	protected virtual		void			PrimaryFire_Start()		{	m_PrimaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			PrimaryFire_Update()	{	m_PrimaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			PrimaryFire_End()		{	m_PrimaryWeaponModuleSlot.WeaponModule.OnEnd();		}
	
	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			SecondaryFire_Start()	{	m_SecondaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			SecondaryFire_Update()	{	m_SecondaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			SecondaryFire_End()		{	m_SecondaryWeaponModuleSlot.WeaponModule.OnEnd();	}

	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void			TertiaryFire_Update()	{	m_TertiaryWeaponModuleSlot.WeaponModule.OnStart();	}
	protected	virtual		void			TertiaryFire_Start()	{	m_TertiaryWeaponModuleSlot.WeaponModule.OnUpdate();	}
	protected	virtual		void			TertiaryFire_End()		{	m_TertiaryWeaponModuleSlot.WeaponModule.OnEnd();		}
	#endregion

}




[System.Serializable]
public class WeaponModuleSlot {

	[SerializeField]
	private	readonly		WeaponSlots				m_ThisSlot					= WeaponSlots.PRIMARY;

	public					WeaponSlots				ThisSlot
	{
		get { return m_ThisSlot; }
	}

	[SerializeField]
	private					WPN_BaseModule			m_WeaponModule				= null;

	public					WPN_BaseModule			WeaponModule
	{
		get { return m_WeaponModule; }
	}



	//////////////////////////////////////////////////////////////////////////
	public WeaponModuleSlot( WeaponSlots slot )
	{
		m_ThisSlot = slot;
	}


	//////////////////////////////////////////////////////////////////////////
	private static bool	GetModuleRules( Database.Section moduleSection, ref Dictionary<WeaponSlots, bool> allowedSlots, ref int maxCount )
	{
		bool result = true;
		if ( result &= moduleSection.bAsInt( "MaxModuleCount", ref maxCount ) )
		{

		}

		int[] localAllowedSlots = null;
		if ( result &= moduleSection.bGetMultiAsArray<int>( "AllowedSlots", ref localAllowedSlots ) )
		{
			allowedSlots = new Dictionary<WeaponSlots, bool>();
			allowedSlots.Add( WeaponSlots.PRIMARY,		false );
			allowedSlots.Add( WeaponSlots.SECONDARY,	false );
			allowedSlots.Add( WeaponSlots.TERTIARY,		false );

		
			foreach( int slot in localAllowedSlots )
			{
				switch ( slot )
				{
					case 1:	allowedSlots[ WeaponSlots.PRIMARY ]		= true;	break;
					case 2:	allowedSlots[ WeaponSlots.SECONDARY ]	= true;	break;
					case 3:	allowedSlots[ WeaponSlots.TERTIARY ]	= true;	break;
					default: break;
				}
			}
		}
		return result;
	}



	//////////////////////////////////////////////////////////////////////////
	public	bool	CanAssignModule( Database.Section moduleSection, string[] alreadyAssignedModules = null )
	{
		Dictionary<WeaponSlots, bool> allowedSlots = null;
		int maxCount = 0;

		bool result = true;

		// Is this slot allowed for module
		if ( GetModuleRules( moduleSection, ref allowedSlots, ref maxCount ) )
		{
			result &= allowedSlots[ m_ThisSlot ];
		}
		
		// Is this module max count less the maxximum allowed
		if ( alreadyAssignedModules != null )
		{
			int counter = 0;
			System.Array.ForEach( alreadyAssignedModules, m => { if (m == moduleSection.Name()) counter++; } );

			result &= counter < maxCount;
		}
		
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	TrySetModule( IWeapon wpn, System.Type type )
	{
		if ( type == null )
		{
			Debug.Log( "WeaponModuleSlot::TrySetModule: " + wpn.Section.Name() + ", Slot:" + Weapon.GetModuleSlotName(m_ThisSlot) + ", Setting invalid weapon module \"" + type.ToString() + "\"" );
			return false;
		}
			
		// Check module type as child of WPN_BaseModule
		if ( type.IsSubclassOf( typeof( WPN_BaseModule ) ) == false )
		{
			Debug.Log( "WeaponModuleSlot::TrySetModule: " + wpn.Section.Name() + ", Slot:" + Weapon.GetModuleSlotName(m_ThisSlot) + ", Class Requested is not a supported weapon module, \"" + type.ToString() + "\"" );
			return false;
		}

		if ( m_WeaponModule != null )
		{
			Object.Destroy( m_WeaponModule );
		}

		WPN_BaseModule wpnModule = wpn.Transform.gameObject.AddComponent( type ) as WPN_BaseModule;

		// On success assign to internal var
		bool bSetupSuccess = wpnModule.Setup( wpn, m_ThisSlot );
		if ( bSetupSuccess == true )
		{
			m_WeaponModule = wpnModule;
		}
		// On Fail add empty module
		else
		{
			Object.Destroy( wpnModule );
			wpn.Transform.gameObject.AddComponent<WPN_BaseModuleEmpty>();
			Debug.Log( "WeaponModuleSlot::TrySetModule: " + wpn.Section.Name() + ": Class Requested is not a supported weapon module, \"" + type.ToString() + "\"" );
		}

		return bSetupSuccess;
	}

}