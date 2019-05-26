using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour {

	[SerializeField]
	protected	string		m_PickUpSectionName		= string.Empty;

	[SerializeField]
	protected	Texture2D	m_Texture				= null;

	private		bool		m_Initialized			= true;


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
		if ( m_Initialized && other.name == "Player" )
		{
			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );

			Database.Section section = null;
			GameManager.Configs.bGetSection( m_PickUpSectionName, ref section );

			UI.Instance.Inventory.AddItem( m_Texture, section );
			enabled = false;
			Destroy( gameObject );
		}

		if ( m_Initialized == false )
		{
			enabled = false;
		}
	}

}