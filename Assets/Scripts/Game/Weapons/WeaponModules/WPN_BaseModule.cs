
using System.Collections;
using System.Collections.Generic;
using Database;
using UnityEngine;


public interface IWPN_FireModule {

	EFireMode				FireMode						{ get; }

	uint					Magazine						{ get; }
	uint					MagazineCapacity				{ get; }

	float					CamDeviation					{ get; }
	float					FireDispersion					{ get; }

	bool					NeedReload						();
	bool					ChangeFireMode					( string FireMode );
	bool					ChangeFireMode				<T>	();
}

public interface IWPN_UtilityModule {

}


//////////////////////////////////////////////////////////////////////////
// WPN_BaseModule ( Abstract )
/// <summary> Abstract base class for weapon modules </summary>
[System.Serializable]
public abstract class WPN_BaseModule : MonoBehaviour, IModifiable {

	protected		Database.Section			m_ModuleSection				= new Database.Section( "Empty", "Unassigned" );
	protected		IWeapon						m_WeaponRef					= null;
	protected		EWeaponSlots					m_ModuleSlot				= EWeaponSlots.NONE;
	protected		List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	protected		GameObject					m_FireModeContainer			= null;


	public virtual		Database.Section			ModuleSection
	{
		get { return this.m_ModuleSection; }
	}

	public		abstract	bool	Setup			( IWeapon w, EWeaponSlots slot );

	//////////////////////////////////////////////////////////////////////////
	protected	abstract	bool	InternalSetup( Database.Section moduleSection );


	//////////////////////////////////////////////////////////////////////////
	public	static	bool	GetRules( Database.Section moduleSection, ref string[] allowedBullets )
	{
		bool result = true;

		string[] localAllowedBullets = null;
		if ( result &= moduleSection.bGetMultiAsArray<string>( "AllowedBullets", ref localAllowedBullets ) )
		{
			allowedBullets = localAllowedBullets;
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool		CanAssignBullet( string bulletName )
	{
		bool result = true;

		string[] allowedBullets = null;
		if ( result &= GetRules(this.m_ModuleSection, ref allowedBullets ) )
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


	protected	virtual	void OnEnable()
	{
		GameManager.UpdateEvents.OnFrame += this.InternalUpdate;
	}

	protected	virtual void OnDisable()
	{
		if ( GameManager.UpdateEvents.IsNotNull() )
			GameManager.UpdateEvents.OnFrame -= this.InternalUpdate;
	}

	public		abstract	bool	OnSave			( StreamUnit streamUnit );
	public		abstract	bool	OnLoad			( StreamUnit streamUnit );

	public		abstract	bool	CanChangeWeapon	();
	public		abstract	bool	CanBeUsed		();
	public		abstract	void	OnWeaponChange	();

	public		abstract	bool	NeedReload		();
	public		abstract	void	OnAfterReload	();

	public		abstract	void	InternalUpdate( float DeltaTime );

	//
	public		virtual		void	OnStart		()	{ }
	public		virtual		void	OnUpdate	()	{ }
	public		virtual		void	OnEnd		()	{ }

}
