using UnityEngine;

public interface IInventory_Empty: IEntityComponent_Inventory
{

}

public class Inventory_Empty : Inventory_Base, IInventory_Empty
{
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
}
