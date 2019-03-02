using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour {

	[SerializeField]
	protected	string		m_PickUpSectionName		= string.Empty;

	private		bool		m_Initialized			= false;


	//////////////////////////////////////////////////////////////////////////
	public	bool	SetPickupSectionName( string PickupSectionName )
	{
		Database.Section m_PickupableSection	= null;
		if ( GameManager.Configs.bGetSection( m_PickUpSectionName, ref m_PickupableSection ) )
		{
			m_PickUpSectionName = PickupSectionName;
			m_Initialized = true;
		}
		return m_PickupableSection != null;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if ( m_Initialized && other.isTrigger && other.name == "Player" )
		{
			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );
			enabled = false;
			Destroy( gameObject );
		}

		if ( m_Initialized == false )
		{
			enabled = false;
		}
	}

}