using System.Collections.Generic;
using UnityEngine;

public class EntityGroup : MonoBehaviour//, IIdentificable<System.Guid>
{
	public System.Guid Id { get; } = System.Guid.NewGuid();

	[SerializeField]
	private		List<Entity>	m_Entities		= new List<Entity>();

	
	/// <summary> Retrieve the list of entities registered to this group </summary>
	public		Entity[]		GetEntites() => m_Entities.ToArray();


	/// <summary> Search for the other entites in the group </summary>
	public		List<Entity>	GetOthers(Entity entity) => m_Entities.FindAll(e => e.Id != entity.Id);


	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// If some already assigned 
		m_Entities.ForEach( e =>
		{
			if ( e )
			{
				// Assign this as their group
				e.SetGroup(this);
					
				// register on death callback
				e.OnEvent_Killed += OnEntityKilled;
			}
		});

		GroupSceneManager.Instance.RegisterGroup( this );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		m_Entities.ForEach( e =>
		{
			if ( e )
			{
				// Assign this as their group
				e.SetGroup(null);
					
				// register on death callback
				e.OnEvent_Killed -= OnEntityKilled;
			}
		});

		GroupSceneManager.Instance.UnregisterGroup( this );
	}


	private	void OnEntityKilled( Entity entityKilled )
	{
		UnregisterEntity( entityKilled );
	}


	/// <summary> Add a new entity to the group </summary>
	public void RegisterEntity(Entity entity)
	{
		if (entity && entity.EntityGroup == null && !m_Entities.Contains(entity))
		{
			entity.OnEvent_Killed += OnEntityKilled;
			m_Entities.Add(entity);
			entity.SetGroup(this);
		}
	}


	/// <summary> Removes the entity from the group </summary>
	public void UnregisterEntity(Entity entity)
	{
		if (m_Entities.Contains(entity))
		{
			entity.OnEvent_Killed -= OnEntityKilled;
			entity.SetGroup(null);
			m_Entities.Remove(entity);
		}
	}


	/// <summary> Return true if entity with given id is found in the group </summary>
	public bool TryGetById(uint id, out Entity outEntity)
	{
		outEntity = null;
		int index = m_Entities.FindIndex(i => i.Id == id);
		bool bResult = index >= 0;
		if (bResult)
		{
			outEntity = m_Entities[index];
		}
		return bResult;
	}
}
