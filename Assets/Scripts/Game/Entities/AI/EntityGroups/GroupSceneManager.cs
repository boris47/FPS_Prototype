using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroupSceneManager : SingletonMonoBehaviour<GroupSceneManager> {

	private static  List<EntityGroup> m_Collection = new List<EntityGroup>();


	protected override void OnBeforeSplashScreen()
	{
		// Add anì empty group
		m_Collection.Add( new EntityGroup() );
	}


	/// <summary> Register a new group if not already contained </summary>
	public void RegisterGroup( EntityGroup group )
	{
		if ( !m_Collection.Exists( g => g.ID == group.ID ) )
		{
			m_Collection.Add( group );
		}
	}


	/// <summary> Remove the group </summary>
	public void UnregisterGroup( EntityGroup group )
	{
		if ( !m_Collection.Exists( g => g.ID == group.ID ) )
		{
			m_Collection.Remove( group );
		}
	}

	/// <summary> Removes the assigned group to all entities belonging the given group </summary>
	public void DisgregateGroup( EntityGroup group )
	{
		foreach( Entity entity in group.GetEntites() )
		{
			entity.AsInterface.GroupRef.SetGroup(null);
		}
	}


	/// <summary> Returns true if group with the given id is found </summary>
	public bool GetById( System.Guid id, ref EntityGroup outGroup )
	{
		int index = m_Collection.FindIndex( i => i.ID == id );
		bool  bResult = index >= 0;
		if ( bResult )
		{
			outGroup = m_Collection[index];
		}
		return bResult;
	}

}


