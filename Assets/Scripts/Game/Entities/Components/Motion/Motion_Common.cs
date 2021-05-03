public interface IMotion_Common : IEntityComponent_Motion
{

}

public class Motion_Common : Motion_Base, IMotion_Common
{
	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{
		
	}
}