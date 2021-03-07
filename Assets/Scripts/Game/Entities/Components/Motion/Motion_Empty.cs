using UnityEngine;

public interface IMotion_Empty : IEntityComponent_Motion
{

}

public class Motion_Empty : Motion_Base, IMotion_Empty
{

	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{

	}
}
