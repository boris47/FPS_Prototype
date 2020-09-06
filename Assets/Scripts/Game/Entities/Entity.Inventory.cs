
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
			get { return this.m_IsSet; }
		}

		public	Database.Section	ItemSection
		{
			get { return this.m_ItemSection; }
		}

		public	Texture2D ItemIcon
		{
			get { return this.m_ItemIcon; }
		}


		public InventoryItem( Database.Section itemSection, Texture2D itemIcon )
		{
			this.m_ItemSection	= null;
			this.m_ItemIcon		= null;
			this.m_IsSet			= false;

			if ( itemSection != null && itemIcon != null )
			{
				this.m_ItemSection	= itemSection;
				this.m_ItemIcon		= itemIcon;
				this.m_IsSet			= true;
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
			this.m_InventoryItems.Add( inventoryItem );

			if (this.m_EntityType == EEntityType.ACTOR )
			{
				result &= UIManager.Inventory.AddItem( itemSection, itemIcon );
			}
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	bool	HasInventoryItem( string sectionName )
	{
		bool bHasBeenFound = this.m_InventoryItems.FindIndex( ii => ii.ItemSection.GetName() == sectionName ) > -1;
		return bHasBeenFound;
	}


	//////////////////////////////////////////////////////////////////////////
	public	virtual	bool	RemoveInventoryItem( string sectionName )
	{
		int index = this.m_InventoryItems.FindIndex( ii => ii.ItemSection.GetName() == sectionName );
		bool result = true;
		if ( result &= index > -1 )
		{
			if (this.m_EntityType == EEntityType.ACTOR )
			{
				result &= UIManager.Inventory.RemoveItem( sectionName );
			}

			this.m_InventoryItems.RemoveAt( index );
		}
		return result;
	}

}
