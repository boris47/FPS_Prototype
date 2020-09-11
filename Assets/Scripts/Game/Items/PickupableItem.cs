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
		if ( GlobalManager.Configs.GetSection(this.m_PickUpSectionName, ref this.m_ItemSection ) )
		{
			this.m_Initialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public	bool	SetPickupSectionName( string PickupSectionName )
	{
		Database.Section m_PickupableSection	= null;
		bool bIsSectionFound = GlobalManager.Configs.GetSection(this.m_PickUpSectionName, ref m_PickupableSection );
		if ( bIsSectionFound )
		{
			this.m_PickUpSectionName = PickupSectionName;
		}
		return m_PickupableSection != null;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnTriggerEnter( Collider other )
	{
		if (this.m_Initialized && other.name == "Player" )
		{
//			WeaponManager.Instance.ApplyModifierToWeaponSlot( WeaponManager.Instance.CurrentWeapon, WeaponSlots.PRIMARY, m_PickUpSectionName );

			if ( Utils.Base.SearchComponent( other.transform.gameObject, out IEntityInventary entity, ESearchContext.LOCAL ) )
			{
				entity.AddInventoryItem( this.m_ItemSection, this.m_Texture );
			}

			this.enabled = false;
			Destroy(this.gameObject );
		}

		if (this.m_Initialized == false )
		{
			this.enabled = false;
		}
	}

}