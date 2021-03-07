using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public abstract partial class Entity
{
	[Header("Entity: Components")]
	[SerializeField]
	protected	Behaviours_Base							m_Behaviours				= null;
	[SerializeField]
	protected	Interactions_Base						m_Interactions				= null;
	[SerializeField]
	protected	Inventory_Base							m_Inventory					= null;
	[SerializeField]
	protected	Memory_Base								m_Memory					= null;
	[SerializeField]
	protected	Motion_Base								m_Motion					= null;
	[SerializeField]
	protected	Navigation_Base							m_Navigation				= null;


	public		IEntityComponent_Behaviours				Behaviours					=> m_Behaviours;
	public		IEntityComponent_Interactions			Interactions				=> m_Interactions;
	public		IEntityComponent_Inventory				Inventory					=> m_Inventory;
	public		IEntityComponent_Memory					Memory						=> m_Memory;
	public		IEntityComponent_Motion					Motion						=> m_Motion;
	public		IEntityComponent_Navigation				Navigation					=> m_Navigation;


	//////////////////////////////////////////////////////////////////////////
	private void SetupComponents()
	{
		// Already assigned base type
		//	List<EntityComponent> alreadyAssigendComponents = gameObject.GetComponents<EntityComponent>().ToList();

		// Ensure one type only for each of them
		List<System.Type> requiredTypes = new HashSet<System.Type>(m_RequiredComponents.Select(c => c.type)).ToList();

		// Add required and Empty component
		foreach (EEntityComponent component in System.Enum.GetValues(typeof(EEntityComponent)))
		{
			// Get the system type of the base entity component
			UnityEngine.Assertions.Assert.IsTrue(TryGetBaseTypeForEntityComponent(component, out System.Type baseType));
			// Get the variable that will hold the created instance of the component
			EntityComponent entityComponent = null;
			UnityEngine.Assertions.Assert.IsTrue(TryGetEntityComponentFromEnum(this, component, ref entityComponent));
			// Get the empty type for this component
			UnityEngine.Assertions.Assert.IsTrue(TryGetEmptyTypeForEntityComponent(component, out System.Type emptyType));

			System.Type typeToInstatiate = requiredTypes.Find(t => t.IsSubclassOf(baseType));

			EntityComponent newComponent = null;

			// Requested concrete
			if (typeToInstatiate.IsNotNull())
			{
				newComponent = gameObject.GetOrAddIfNotFound(typeToInstatiate) as EntityComponent;
			}
			// Empty
			else
			{
				newComponent = gameObject.GetOrAddIfNotFound(emptyType) as EntityComponent;
			}

			// If new component is not the same on this gameobject, destroy it (entityComponent could start with null value)
			if (entityComponent?.GetInstanceID() != newComponent.GetInstanceID())
			{
				Destroy(entityComponent);
			}

			AssignEntityComponent(this, component, newComponent);
		}

		// Let each component initialize itself
		foreach(EntityComponent component in gameObject.GetComponentsInChildren<EntityComponent>(includeInactive: true))
		{
			component.Resolve(this, m_SectionRef);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetBaseTypeForEntityComponent(in EEntityComponent baseType, out System.Type result)
	{
		result = null;
		switch (baseType)
		{
			case EEntityComponent.BEHAVIOURS:	result = typeof(Behaviours_Base);		break;
			case EEntityComponent.INTERACTION:	result = typeof(Interactions_Base);		break;
			case EEntityComponent.INVENTORY:	result = typeof(Inventory_Base);		break;
			case EEntityComponent.MEMORY:		result = typeof(Memory_Base);			break;
			case EEntityComponent.MOTION:		result = typeof(Motion_Base);			break;
			case EEntityComponent.NAVIGATION:	result = typeof(Navigation_Base);		break;
		}
		return result.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetEmptyTypeForEntityComponent(in EEntityComponent baseType, out System.Type result)
	{
		result = null;
		switch (baseType)
		{
			case EEntityComponent.BEHAVIOURS:	result = typeof(Behaviours_Empty);		break;
			case EEntityComponent.INTERACTION:	result = typeof(Interactions_Empty);	break;
			case EEntityComponent.INVENTORY:	result = typeof(Inventory_Empty);		break;
			case EEntityComponent.MEMORY:		result = typeof(Memory_Empty);			break;
			case EEntityComponent.MOTION:		result = typeof(Motion_Empty);			break;
			case EEntityComponent.NAVIGATION:	result = typeof(Navigation_Empty);		break;
		}
		return result.IsNotNull();
	}

	//////////////////////////////////////////////////////////////////////////
	private static bool TryGetEntityComponentFromEnum(in Entity entity, in EEntityComponent type, ref EntityComponent entityComponent)
	{
		entityComponent = null;
		bool bResult = false; // Using this because the entity var can have null value that is legal
		switch (type)
		{
			case EEntityComponent.BEHAVIOURS:	entityComponent = entity.m_Behaviours;		bResult = true; break;
			case EEntityComponent.INTERACTION:	entityComponent = entity.m_Interactions;	bResult = true; break;
			case EEntityComponent.INVENTORY:	entityComponent = entity.m_Inventory;		bResult = true; break;
			case EEntityComponent.MEMORY:		entityComponent = entity.m_Memory;			bResult = true; break;
			case EEntityComponent.MOTION:		entityComponent = entity.m_Motion;			bResult = true; break;
			case EEntityComponent.NAVIGATION:	entityComponent = entity.m_Navigation;		bResult = true; break;
		}
		return bResult;
	}

	//////////////////////////////////////////////////////////////////////////
	private static void AssignEntityComponent(in Entity entity, in EEntityComponent type, in EntityComponent entityComponent)
	{
		switch (type)
		{
			case EEntityComponent.BEHAVIOURS:	entity.m_Behaviours		= entityComponent as Behaviours_Base;		break;
			case EEntityComponent.INTERACTION:	entity.m_Interactions	= entityComponent as Interactions_Base;		break;
			case EEntityComponent.INVENTORY:	entity.m_Inventory		= entityComponent as Inventory_Base;		break;
			case EEntityComponent.MEMORY:		entity.m_Memory			= entityComponent as Memory_Base;			break;
			case EEntityComponent.MOTION:		entity.m_Motion			= entityComponent as Motion_Base;			break;
			case EEntityComponent.NAVIGATION:	entity.m_Navigation		= entityComponent as Navigation_Base;		break;
		}
	}
}
