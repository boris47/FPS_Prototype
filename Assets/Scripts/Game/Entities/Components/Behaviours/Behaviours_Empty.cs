using UnityEngine;

public interface IBehaviours_Empty : IEntityComponent_Behaviours
{

}

public class Behaviours_Empty : Behaviours_Base, IBehaviours_Empty
{
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}

	protected override void SetBehaviour(EBrainState brainState, string behaviourId)
	{

	}

	public override void OnDestinationReached(Vector3 position)
	{

	}

	public override void ChangeState(EBrainState newState)
	{

	}
}