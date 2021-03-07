using UnityEngine;

public interface IInteractions_Common : IEntityComponent_Interactions
{

}

public class Interactions_Common : Interactions_Base, IInteractions_Common
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}
}
