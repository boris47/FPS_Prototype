
using UnityEngine;
using System.Collections.Generic;


public interface IEntityInventary {

	bool	AddInventoryItem( Database.Section itemSection, Texture2D itemIcon );

	bool	HasInventoryItem( string sectionName );

	bool	RemoveInventoryItem( string sectionName );

}


public abstract partial class Entity : MonoBehaviour, IEntityInventary {

	protected struct InventoryItem {

		private	bool				m_IsSet;
		private	Database.Section	m_ItemSection;
		private	Texture2D			m_ItemIcon;

		public	bool	IsSet
		{
			get { return m_IsSet; }
		}

		public	Database.Section	ItemSection
		{
			get { return m_ItemSection; }
		}

		public	Texture2D ItemIcon
		{
			get { return m_ItemIcon; }
		}


		public InventoryItem( Database.Section itemSection, Texture2D itemIcon )
		{
			m_ItemSection	= null;
			m_ItemIcon		= null;
			m_IsSet			= false;

			if ( itemSection != null && itemIcon != null )
			{
				m_ItemSection	= itemSection;
				m_ItemIcon		= itemIcon;
				m_IsSet			= true;
			}
		}
	}


	protected	List<InventoryItem>	m_InventoryItems = new List<InventoryItem>();




	//////////////////////////////////////////////////////////////////////////
	public	virtual	bool	AddInventoryItem( Database.Section itemSection, Texture2D itemIcon )
	{
		bool result = true;

		InventoryItem inventoryItem = new InventoryItem( itemSection, itemIcon );
		if ( result &= inventoryItem.IsSet )
		{
			m_InventoryItems.Add( inventoryItem );

			if ( m_EntityType == ENTITY_TYPE.ACTOR )
			{
				result &= UI.Instance.Inventory.AddItem( itemSection, itemIcon );
			}
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	bool	HasInventoryItem( string sectionName )
	{
		bool bHasBeenFound = m_InventoryItems.FindIndex( ii => ii.ItemSection.GetName() == sectionName ) > -1;
		return bHasBeenFound;
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	bool	RemoveInventoryItem( string sectionName )
	{
		int index = m_InventoryItems.FindIndex( ii => ii.ItemSection.GetName() == sectionName );
		bool result = true;
		if ( result &= index > -1 )
		{
			if ( m_EntityType == ENTITY_TYPE.ACTOR )
			{
				result &= UI.Instance.Inventory.RemoveItem( sectionName );
			}

			m_InventoryItems.RemoveAt( index );
		}
		return result;
	}

}
