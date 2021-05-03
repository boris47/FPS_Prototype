public interface IEntityComponent_Inventory
{
	/// <summary>  </summary>
	void			AddInventoryItem				(string ItemSectionName);
	/// <summary>  </summary>
	bool			HasInventoryItem				(string sectionName);
	/// <summary>  </summary>
	bool			RemoveInventoryItem				(string sectionName);
}

public abstract class Inventory_Base : EntityComponent, IEntityComponent_Inventory
{
	//////////////////////////////////////////////////////////////////////////
	public abstract void AddInventoryItem(string ItemSectionName);

	//////////////////////////////////////////////////////////////////////////
	public abstract bool HasInventoryItem(string sectionName);

	//////////////////////////////////////////////////////////////////////////
	public abstract bool RemoveInventoryItem(string sectionName);
}

public class EntityComponentContainer_Inventory<T> : EntityComponentContainer where T : Inventory_Base, new()
{
	public override System.Type type { get; } = typeof(T);
}
