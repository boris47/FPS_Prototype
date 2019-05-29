using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour {

	[SerializeField]
	protected	string					m_PickUpSectionName		= string.Empty;

	[SerializeField]
	protected	Texture2D				m_Texture				= null;


	private		Database.Section		m_ItemSection			= null;
	private		bool					m_Initialized			= true;


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if ( GameManager.Configs.bGetSection( m_PickUpSectionName, ref m_ItemSection ) )
		{
			m_Initialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	SetPickupSectionName( string PickupSectionName )
	{
		Database.Section m_PickupableSection	= null;
		bool bIsSectionFound = GameManager.Configs.bGetSection( m_PickUpSectionName, ref m_PickupableSection );
		if ( bIsSectionFound )
		{
			m_PickUpSectionName = PickupSectionName;
		}
		return m_PickupableSection != null;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if ( m_Initialized && other.name == "Player" )
		{
//			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );

			IEntityInventary entity = null;
			if ( Utils.Base.SearchComponent( other.transform.gameObject, ref entity, SearchContext.LOCAL ) )
			{
				entity.AddInventoryItem( m_ItemSection, m_Texture );
			}

			enabled = false;
			Destroy( gameObject );
		}

		if ( m_Initialized == false )
		{
			enabled = false;
		}
	}

}