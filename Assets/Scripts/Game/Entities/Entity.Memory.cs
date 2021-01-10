
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public	class	MemoryUnit {

	public	string	Name					= "MemoryUnit";
	public	Vector3	LastEnemyPosition		= Vector3.zero;
	public	Vector3 LastEnemyDirection		= Vector3.zero;
	public	uint	EntityID				= 0;
	public	Entity	EntityRef				= null;
	public	float	Time					= 0;

}


public partial interface IEntityMemory {

	void			CleanInvalidMemories								();

	bool			Add										( Entity entity );
	int				Count									{ get; }
	bool			Contains								( uint EntityID );

	Vector3			GetLastPositionByindex					( int index );
	Vector3			GetLastDirectionByindex					( int index );
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
	private				int							m_MemoriesCount = 0;


	int		IEntityMemory.Count
	{
		get { return m_MemoriesCount; }
	}
	


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


	//////////////////////////////////////////////////////////////////////////
	protected	virtual	void	UpdateMemory()
	{
		Memory.CleanInvalidMemories();

		// This entity has no target at the moment
		if (m_TargetInfo.HasTarget == false )
		{
			for ( int i = m_Memories.Count - 1; i >= 0; i-- )
			{
				MemoryUnit unit = m_Memories[ i ];

			}
		}
	}



	/// <summary> Validate all the memories checking data valid values </summary>
	void		IEntityMemory.CleanInvalidMemories()
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


	/// <summary> Add a valid entity to memories of this entity </summary>
	bool		IEntityMemory.Add( Entity entity )
	{
		if ( entity == null || entity.IsAlive == false )
		{
			Debug.Log( "Entity::Memory:Add: Passed entity is invalid!!" );
			return false;
		}

		bool bIsMemoryNotPresent = Memory.Contains( entity.AsInterface.ID ) == false;
		if ( bIsMemoryNotPresent )
		{
			MemoryUnit u = new MemoryUnit()
			{
				LastEnemyPosition	= entity.HeadPosition,
				LastEnemyDirection	= entity.AsInterface.RigidBody.velocity,
				EntityID			= entity.AsInterface.ID,
				EntityRef			= entity,
				Time				= Time.time
			};
			m_Memories.Add( u );
			m_MemoriesCount ++;
		}

		return bIsMemoryNotPresent;
	}


	/// <summary> Check if memory contains this entity </summary>
	bool		IEntityMemory.Contains( uint EntityID )
	{
		return m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == EntityID ) != -1;
	}


	/// <summary> Get the last position giving a specific index </summary>
	Vector3		IEntityMemory.GetLastPositionByindex( int index )
	{
		return ( index > -1 && index < m_Memories.Count ) ? Vector3.zero : m_Memories[ index ].LastEnemyPosition;
	}


	/// <summary> Get the last direction giving a specific index </summary>
	Vector3		IEntityMemory.GetLastDirectionByindex( int index )
	{
		return ( index > -1 && index < m_Memories.Count ) ? Vector3.zero : m_Memories[ index ].LastEnemyDirection;
	}


	/// <summary> Get the entity giving a specific index </summary>
	Entity		IEntityMemory.GetEntityByindex( int index )
	{
		return ( index > -1 && index < m_Memories.Count ) ? null : m_Memories[ index ].EntityRef;
	}



	/// <summary> Return the last position searched by entity index, vector zero otherwise </summary>
	Vector3		IEntityMemory.GetLastPosition( uint EntityID )
	{
		int memoryUnitIndex = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == EntityID );
		return ( memoryUnitIndex == -1 ) ? Vector3.zero : m_Memories[ memoryUnitIndex ].LastEnemyPosition;
	}


	/// <summary> Return the entity searched by index, null otherwise </summary>
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

		int memoryUnitIndex = m_Memories.FindIndex( ( MemoryUnit u ) => u.EntityID == entity.AsInterface.ID ) ;
		if ( memoryUnitIndex != -1 )
		{
			m_Memories.RemoveAt( memoryUnitIndex );
			m_MemoriesCount -- ;
		}

		return memoryUnitIndex != -1;
	}


	/// <summary> Clear all entity memeory data </summary>
	void		IEntityMemory.Empty()
	{
		m_Memories.Clear();
		m_MemoriesCount = 0;
	}

}