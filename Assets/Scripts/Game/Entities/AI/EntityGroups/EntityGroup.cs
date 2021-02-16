using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGroup : MonoBehaviour, IIdentificable<System.Guid>
{
	public System.Guid ID { get; } = System.Guid.NewGuid();

	[SerializeField]
	private		List<Entity>	m_Collection		= new List<Entity>();

	

	//////////////////////////////////////////////////////////////////////////
	private void Awake()
	{
		// If some already assigned 
		m_Collection.ForEach( e =>
		{
			if ( e )
			{
				// Assign this as their group
				e.AsInterface.GroupRef.SetGroup( this );
					
				// register on death callback
				e.OnEvent_Killed += OnEntityKilled;
			}
		});

		GroupSceneManager.Instance.RegisterGroup( this );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		m_Collection.ForEach( e =>
		{
			if ( e )
			{
				// Assign this as their group
				e.AsInterface.GroupRef.SetGroup( null );
					
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
	public void RegisterEntity( Entity entity )
	{
		if ( entity && entity.AsInterface.GroupRef.Group == null && !m_Collection.Contains( entity ) )
		{
			entity.OnEvent_Killed += OnEntityKilled;
			m_Collection.Add( entity );
			entity.AsInterface.GroupRef.SetGroup( this );
		}
	}


	/// <summary> Removes the entity from the group </summary>
	public void UnregisterEntity( Entity entity )
	{
		if ( !m_Collection.Contains( entity ) )
		{
			entity.OnEvent_Killed -= OnEntityKilled;
			entity.AsInterface.GroupRef.SetGroup( null );
			m_Collection.Remove( entity );
		}
	}


	/// <summary> Return true if entity with given id is found in the group </summary>
	public bool TryGetById( uint id, out Entity outEntity )
	{
		outEntity = null;
		int index = m_Collection.FindIndex( i => i.AsInterface.ID == id );
		bool bResult = index >= 0;
		if ( bResult )
		{
			outEntity = m_Collection[index];
		}
		return bResult;
	}


	/// <summary> Retrieve the list of entities registered to this group </summary>
	public	Entity[]	GetEntites() => m_Collection.ToArray();


	/// <summary> Search for the other entites in the group </summary>
	public List<Entity> GetOthers( IEntity entity ) => m_Collection.FindAll( e => e.AsInterface.ID != entity.ID );

}
