using UnityEngine;

public interface IBehaviours_Empty : IEntityComponent_Behaviours
{

}

public class Behaviours_Empty : Behaviours_Base, IBehaviours_Empty
{
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}

	public override void OnDestinationReached(in Vector3 position)
	{

	}

	public override void ChangeState(in EBrainState newState)
	{

	}
}