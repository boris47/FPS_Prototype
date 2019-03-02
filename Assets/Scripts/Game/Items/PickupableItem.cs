using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour {

	public	string	m_PickUpSectionName = "";

	private void Awake()
	{
		Database.Section m_PickupableSection	= null;
		if ( GameManager.Configs.bGetSection( m_PickUpSectionName, ref m_PickupableSection ) == false )
		{
			enabled = false;
		}
	}

	private void OnTriggerEnter( Collider other )
	{
		if ( other.isTrigger && other.name == "Player" )
		{
			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );
			enabled = false;
			Destroy( gameObject );
		}
	}


}
