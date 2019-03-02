using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WPN_WeaponModule_Shield : WPN_BaseModule, IWPN_UtilityModule {

	protected	float		m_ShieldLife		= 1f;

	//////////////////////////////////////////////////////////////////////////
	public	override	bool	Setup			( IWeapon w, WeaponSlots slot )
	{
		string moduleSectionName = this.GetType().FullName;
		m_WeaponRef = w;
		if ( GameManager.Configs.bGetSection( moduleSectionName, ref m_ModuleSection ) == false )			// Get Module Section
			return false;

		string modulePrefabPath = null;
		if ( m_ModuleSection.bAsString( "Module_Prefab", ref modulePrefabPath ) )
		{
			GameObject modulePrefab = Resources.Load( modulePrefabPath ) as GameObject;
			if ( modulePrefab )
			{
				modulePrefab = Instantiate<GameObject>( modulePrefab, transform );
				modulePrefab.transform.localPosition = Vector3.zero;
				modulePrefab.transform.localRotation = Quaternion.identity;
			}
		}

		m_ShieldLife = m_ModuleSection.AsFloat( "ShieldLife", 50f );

		if ( InternalSetup( m_ModuleSection ) == false )
			return false;

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	override	bool	InternalSetup( Database.Section moduleSection )
	{
		return true;
	}


	//		MODIFIERS
	//////////////////////////////////////////////////////////////////////////
	public override void ApplyModifier( Database.Section modifier )
	{
		// Do actions here

		base.ApplyModifier( modifier );
	}


	public	override	void	ResetBaseConfiguration()
	{
		// Do actions here

		base.ResetBaseConfiguration();
	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		// Do Actions here

		base.RemoveModifier( modifier );
	}


		//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		return true;
	}
	
	public	override	bool	CanChangeWeapon	() {  return true; }
	public	override	bool	CanBeUsed		() {  return true; }
	public	override	void	OnWeaponChange	() { }
	public	override	bool	NeedReload		() { return false; }
	public	override	void	OnAfterReload	() { }
	public	override	void	InternalUpdate	( float DeltaTime ) { }



	public override		void	OnStart()
	{

	}



	public override void OnEnd()
	{
		
	}

}
