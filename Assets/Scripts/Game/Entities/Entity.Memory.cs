
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public	class	MemoryUnit {

	public	string	Name					= "MemoryUnit";
	public	Vector3	LastEnemyPosition		= Vector3.zero;
	public	uint	EntityID				= 0;
	public	Entity	EntityRef				= null;
	public	float	Time					= 0;

}


public partial interface IEntityMemory {

	void			ValidateMemories								();

	bool			Add										( Entity entity );

	bool			Contains								( uint EntityID );

	Vector3			GetLastPositionByindex					( int index );
	Entity			GetEntityByindex						( int index );

	Vector3			GetLastPosition							( uint EntityID );
	Entity			GetEntity								( uint EntityID );

	bool			Remove									( Entity entity );

	void			Empty									();
	
}


public abstract partial class Entity : IEntityMemory {

	[Header( "Memory" )]
	[SerializeField, ReadOnly ]
	protected			List<MemoryUnit>			m_Memories						= new List<MemoryUnit>();



	private				IEntityMemory				m_MemoryInstance				= null;
	public				IEntityMemory				Memory							{ get { return m_MemoryInstance; } }

	[SerializeField]
	private				int							MemoriesCount = 0;
	


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	EnableMemory()
	{
		m_MemoryInstance = this as IEntityMemory;
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	DisableMemory()
	{
		m_MemoryInstance.Empty();

		m_MemoryInstance = null;
	}



	/// <summary>
	/// Validate all the memories checking data valid values
	/// </summary>
	void		IEntityMemory.ValidateMemories()
	{
		for ( int i = m_Memories.Count - 1; i >= 0; i-- )
		{
			MemoryUnit unit = m_Memories[ i ];
			if ( unit == null ||  unit.EntityID < 0 || unit.EntityRef == null || unit.EntityRef.IsAlive == false )
			{
				m_Memories.RemoveAt( i );
			}
		}
	}


	/// <summary>
	/// Add a valid entity to memories of this entity
	/// </summary>
	bool		IEntityMemory.Add( Entity entity )
	{
		if ( entity == null || entity.IsAlive == false )
		{
			Debug.Log( "Entity::Memory:AddMemory: Passed invalid entity!!" );
			return false;
		}

		bool bIsMemoryNotPresent = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == entity.ID ) == -1;
		if ( bIsMemoryNotPresent )
		{
			MemoryUnit u = new MemoryUnit()
			{
				LastEnemyPosition	= entity.HeadPosition,
				EntityID			= entity.ID,
				EntityRef			= entity,
				Time				= Time.time
			};
			m_Memories.Add( u );
			MemoriesCount ++;
		}

		return bIsMemoryNotPresent;
	}


	/// <summary>
	/// Check if memory contains this entity
	/// </summary>
	bool		IEntityMemory.Contains( uint EntityID )
	{
		return m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == EntityID ) != -1;
	}


	/// <summary>
	/// Get the last position giving a specific index
	/// </summary>
	Vector3		IEntityMemory.GetLastPositionByindex( int index )
	{
		return ( index > -1 && index < m_Memories.Count ) ? Vector3.zero : m_Memories[ index ].LastEnemyPosition;
	}


	/// <summary>
	/// Get the entity giving a specific index
	/// </summary>
	Entity		IEntityMemory.GetEntityByindex( int index )
	{
		return ( index > -1 && index < m_Memories.Count ) ? null : m_Memories[ index ].EntityRef;
	}



	/// <summary>
	/// Return the last position searched by entity index, vector zero otherwise
	/// </summary>
	Vector3		IEntityMemory.GetLastPosition( uint EntityID )
	{
		int memoryUnitIndex = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == EntityID );
		return ( memoryUnitIndex == -1 ) ? Vector3.zero : m_Memories[ memoryUnitIndex ].LastEnemyPosition;
	}


	/// <summary>
	/// Return the entity searched by index, null otherwise
	/// </summary>
	Entity		IEntityMemory.GetEntity( uint EntityID )
	{
		int memoryUnitIndex = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == EntityID );
		return ( memoryUnitIndex == -1 ) ? null : m_Memories[ memoryUnitIndex ].EntityRef;
	}


	/// <summary>
	/// 
	/// </summary>
	bool		IEntityMemory.Remove( Entity entity )
	{
		if ( entity == null )
		{
			Debug.Log( "Entity::Memory:RemoveMemory: Passed invalid entity!!" );
			return false;
		}

		int memoryUnitIndex = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == entity.ID ) ;
		if ( memoryUnitIndex != -1 )
		{
			m_Memories.RemoveAt( memoryUnitIndex );
			MemoriesCount -- ;
		}

		return memoryUnitIndex != -1;
	}


	/// <summary>
	/// Clear all entity memeory data
	/// </summary>
	void		IEntityMemory.Empty()
	{
		m_Memories.Clear();
		MemoriesCount = 0;
	}

}