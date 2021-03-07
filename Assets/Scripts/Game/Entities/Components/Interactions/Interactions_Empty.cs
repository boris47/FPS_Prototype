using UnityEngine;

public interface IInteractions_Empty : IEntityComponent_Interactions
{

}

public class Interactions_Empty : Interactions_Base, IInteractions_Empty
{
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}
}