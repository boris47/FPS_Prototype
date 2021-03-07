using UnityEngine;
using System.Collections.Generic;

public interface IInventory_Player : IEntityComponent_Inventory
{

}

public class Inventory_Player : Inventory_Base, IInventory_Player
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