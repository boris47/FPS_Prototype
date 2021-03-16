
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IWPN_UtilityModule
{

}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModule ( Abstract )
/// <summary> Abstract base class for weapon modules </summary>
[System.Serializable]
public abstract class WPN_BaseModule : MonoBehaviour, IModifiable
{
	protected		Database.Section			m_ModuleSection				= new Database.Section( "Empty", "Unassigned" );
	protected		IWeapon						m_WeaponRef					= null;
	protected		EWeaponSlots				m_ModuleSlot				= EWeaponSlots.NONE;
	protected		List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	protected		GameObject					m_FireModeContainer			= null;

	public virtual	Database.Section			ModuleSection				=> m_ModuleSection;

	/// <summary> Initialize everything about this module </summary>
	public		abstract	bool	OnAttach( IWeapon w, EWeaponSlots slot );

	/// <summary> Unload and clean everything about this module </summary>
	public		abstract	void	OnDetach();

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	InternalSetup( Database.Section moduleSection );


	//////////////////////////////////////////////////////////////////////////
	public	static	bool	GetRules( Database.Section moduleSection, out string[] allowedBullets )
	{
		return moduleSection.TryGetMultiAsArray("AllowedBullets", out allowedBullets);
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool		CanAssignBullet( string bulletName )
	{
		bool result = true;
		if ( result &= GetRules(m_ModuleSection, out string[] allowedBullets ) )
		{
			result &= System.Array.IndexOf( allowedBullets, bulletName ) > -1;
		}
		return result;
	}

	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////


	public		virtual		void	ApplyModifier( Database.Section modifier )	{ }
	public		virtual		void	ResetBaseConfiguration()	{ }
	public		virtual		void	RemoveModifier( Database.Section modifier )	{ }


	//////////////////////////////////////////////////////////////////////////
	protected virtual	void OnEnable()
	{
		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnFrame += InternalUpdate;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected virtual void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnFrame -= InternalUpdate;
		}
	}

	public		abstract	bool	OnSave			( StreamUnit streamUnit );
	public		abstract	bool	OnLoad			( StreamUnit streamUnit );

	public		abstract	bool	CanChangeWeapon	();
	public		abstract	bool	CanBeUsed		();
	public		abstract	void	OnWeaponChange	();

	public		abstract	bool	NeedReload		();
	public		abstract	void	OnAfterReload	();

	protected	abstract	void	InternalUpdate( float DeltaTime );

	//
	public		virtual		void	OnStart		()	{ }
	public		virtual		void	OnUpdate	()	{ }
	public		virtual		void	OnEnd		()	{ }
}
