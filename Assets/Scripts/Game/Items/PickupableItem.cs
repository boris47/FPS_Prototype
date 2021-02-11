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
		if ( GlobalManager.Configs.TryGetSection(m_PickUpSectionName, out m_ItemSection ) )
		{
			m_Initialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	SetPickupSectionName( string PickupSectionName )
	{
		bool bIsSectionFound = GlobalManager.Configs.TryGetSection(m_PickUpSectionName, out Database.Section pickupableSection);
		if ( bIsSectionFound )
		{
			m_PickUpSectionName = PickupSectionName;
		}
		return bIsSectionFound;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if (m_Initialized && other.name == "Player" )
		{
//			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );

			if ( Utils.Base.TrySearchComponent( other.transform.gameObject, ESearchContext.LOCAL, out IEntityInventary entity ) )
			{
				entity.AddInventoryItem( m_ItemSection, m_Texture );
			}

			enabled = false;
			Destroy(gameObject );
		}

		if (m_Initialized == false )
		{
			enabled = false;
		}
	}

}