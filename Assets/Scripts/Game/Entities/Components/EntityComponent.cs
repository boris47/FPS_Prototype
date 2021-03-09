using UnityEngine;

/*
Tutti questi diventano component con modello:
interface IIEntityComponent_NAME
abstract base : IIEntityComponent_NAME
	-> Empty
	-> Concrete
Nell awake cio che è richiesto dalla classe concreta vedrà aggiunto il componente concreto, il resto con il rispettivo empty
dopo ciò si cercano i componenti IEntityComponent e si chiama il resolve, in modo che ciascuno di essi finalizzi la
propria inizializzazione sapendo che l'entity adesso avrà tutti i componenti necessari
*/


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

/*
public interface IEntityComponent
{
	void Resolve();
}
*/

public abstract class EntityComponent : MonoBehaviour//, IEntityComponent
{
	//[SerializeField, ReadOnly]
	protected	Entity				m_Entity			= null;

	protected	Database.Section	m_EntitySection		= null;

	public void Resolve(Entity entity, Database.Section entitySection)
	{
		UnityEngine.Assertions.Assert.IsNotNull(entity);
		UnityEngine.Assertions.Assert.IsNotNull(entitySection);

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
