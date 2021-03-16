using UnityEngine;

public enum EEntityComponent
{
	BEHAVIOURS,
	INTERACTION,
	INVENTORY,
	MEMORY,
	MOTION,
	NAVIGATION,
}

public abstract class EntityComponentContainer
{
	public abstract System.Type type { get; }
}

public abstract class EntityComponent : MonoBehaviour//, IEntityComponent
{
	//[SerializeField, ReadOnly]
	protected	Entity				m_Entity			= null;

	protected	Database.Section	m_EntitySection		= null;

	public void Resolve(Entity entity, Database.Section entitySection)
	{
		CustomAssertions.IsNotNull(entity);
		CustomAssertions.IsNotNull(entitySection);

		m_Entity = entity;
		m_EntitySection = entitySection;

		Resolve_Internal(entity, entitySection);
	}

	//////////////////////////////////////////////////////////////////////////
	protected abstract void Resolve_Internal(Entity entity, Database.Section entitySection);

	//////////////////////////////////////////////////////////////////////////
	public virtual void Enable()
	{
		enabled = true;
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void Disable()
	{
		enabled = false;
	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void OnSave(StreamUnit streamUnit)
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public virtual void OnLoad(StreamUnit streamUnit)
	{

	}
}
