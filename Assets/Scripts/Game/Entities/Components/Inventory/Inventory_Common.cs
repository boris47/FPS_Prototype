public interface IInventory_Common : IEntityComponent_Inventory
{

}

public class Inventory_Common : Inventory_Base, IInventory_Common
{/*
	[SerializeField]
	protected class InventoryItem
	{
		public string ItemSectionName { get; } = string.Empty;

		public InventoryItem(string ItemSectionName)
		{
			ItemSectionName = this.ItemSectionName;
		}
	}

	[SerializeField]
	protected List<InventoryItem> m_InventoryItems = new List<InventoryItem>();
*/

	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public override void AddInventoryItem(string ItemSectionName)
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public override bool HasInventoryItem(string sectionName)
	{
		return false;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool RemoveInventoryItem(string sectionName)
	{
		return true;
	}
	/*
		//////////////////////////////////////////////////////////////////////////
		public override void AddInventoryItem(string ItemSectionName)
		{
			InventoryItem inventoryItem = new InventoryItem(ItemSectionName);
			m_InventoryItems.Add(inventoryItem);
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool HasInventoryItem(string sectionName)
		{
			bool bHasBeenFound = m_InventoryItems.FindIndex(ii => ii.ItemSectionName == sectionName) > -1;
			return bHasBeenFound;
		}


		//////////////////////////////////////////////////////////////////////////
		public override bool RemoveInventoryItem(string sectionName)
		{
			int index = m_InventoryItems.FindIndex(ii => ii.ItemSectionName == sectionName);
			bool result = true;
			if (result &= index > -1)
			{
				if (m_Entity.EntityType == EEntityType.ACTOR)
				{
					result &= UIManager.Inventory.RemoveItem(sectionName);
				}

				m_InventoryItems.RemoveAt(index);
			}
			return result;
		}
		*/
}