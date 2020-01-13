using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGroup : MonoBehaviour, IIdentificable<System.Guid>
{
	private		System.Guid		m_ID		= new System.Guid();
	public		System.Guid		ID
	{
		get => m_ID;
	}


	private static  List<Entity> m_Collection = new List<Entity>();


	/// <summary> Add a new entity to the group </summary>
	public void RegisterEntity( Entity entity )
	{
		if ( entity && !m_Collection.Contains( entity ) )
		{
			entity.OnEvent_Killed += () => this.UnregisterEntity(entity);
			m_Collection.Add( entity );
			entity.AsInterface.GroupRef.SetGroup( this );
		}
	}


	/// <summary> Removes the entity from the group </summary>
	public void UnregisterEntity( Entity entity )
	{
		if ( !m_Collection.Contains( entity ) )
		{
			entity.AsInterface.GroupRef.SetGroup( null );
			m_Collection.Remove( entity );
		}
	}


	/// <summary> Return true if entity with given id is found in the group </summary>
	public bool GetById( uint id, ref Entity outEntity )
	{
		int index = m_Collection.FindIndex( i => i.AsInterface.ID == id );
		bool bResult = index >= 0;
		if ( bResult )
		{
			outEntity = m_Collection[index];
		}
		return bResult;
	}


	/// <summary> Retrieve the list of entities registered to this group </summary>
	public	Entity[]	GetEntites()
	{
		return m_Collection.ToArray();
	}

}
