using UnityEngine;
using System.Collections.Generic;

public interface IMemory_Common : IEntityComponent_Memory
{

}

public class Memory_Common : Memory_Base, IMemory_Common
{
	[System.Serializable]
	public	class	MemoryUnit
	{
		public	string	Name					= "MemoryUnit";
		public	Vector3	LastEnemyPosition		= Vector3.zero;
		public	Vector3 LastEnemyDirection		= Vector3.zero;
		public	uint	EntityId				= 0;
		public	Entity	EntityRef				= null;
		public	float	Time					= 0;
	}


	[Header("Entity: Memory")]

	[SerializeField, ReadOnly]
	protected			List<MemoryUnit>			m_Memories						= new List<MemoryUnit>();
	[SerializeField]
	private				int							m_MemoriesCount					= 0;

	public	override	int							MemoriesCount					=> m_MemoriesCount;


	//////////////////////////////////////////////////////////////////////////
	protected override void Resolve_Internal(Entity entity, Database.Section entitySection)
	{
		CustomAssertions.IsNotNull(entity.Behaviours);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnSave(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void OnLoad(StreamUnit streamUnit)
	{
		base.OnLoad(streamUnit);
	}

	//////////////////////////////////////////////////////////////////////////
	public override void EnableMemory()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public override void DisableMemory()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	public override void UpdateMemory()
	{
		CleanInvalidMemories();

		// This entity has no target at the moment
	//	if (!m_Entity.Behaviours.TargetInfo.HasTarget)  // TODO
		{
			for (int i = m_Memories.Count - 1; i >= 0; i--)
			{
				MemoryUnit unit = m_Memories[i];
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void CleanInvalidMemories()
	{
		for (int i = m_Memories.Count - 1; i >= 0; i--)
		{
			MemoryUnit unit = m_Memories[i];
			if (unit == null || unit.EntityId < 0 || unit.EntityRef == null || unit.EntityRef.IsAlive == false)
			{
				m_Memories.RemoveAt(i);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool Add(Entity entity)
	{
		if (entity == null || entity.IsAlive == false)
		{
			Debug.Log("Entity::Memory:Add: Passed entity is invalid!!");
			return false;
		}

		bool bIsMemoryNotPresent = !Contains(entity.Id);
		if (bIsMemoryNotPresent)
		{
			MemoryUnit u = new MemoryUnit()
			{
				LastEnemyPosition = entity.Targettable.position,
				LastEnemyDirection = entity.EntityRigidBody.velocity,
				EntityId = entity.Id,
				EntityRef = entity,
				Time = Time.time
			};
			m_Memories.Add(u);
			m_MemoriesCount++;
		}

		return bIsMemoryNotPresent;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool Contains(uint EntityId)
	{
		return m_Memories.FindIndex((MemoryUnit u) => u.EntityId == EntityId) != -1;
	}

	//////////////////////////////////////////////////////////////////////////
	public override Vector3 GetLastPositionByIndex(int index)
	{
		return m_Memories.IsValidIndex(index) ? Vector3.zero : m_Memories[index].LastEnemyPosition;
	}

	//////////////////////////////////////////////////////////////////////////
	public override Vector3 GetLastDirectionByIndex(int index)
	{
		return m_Memories.IsValidIndex(index) ? Vector3.zero : m_Memories[index].LastEnemyDirection;
	}

	//////////////////////////////////////////////////////////////////////////
	public override Entity GetEntityByIndex(int index)
	{
		return m_Memories.IsValidIndex(index) ? null : m_Memories[index].EntityRef;
	}

	//////////////////////////////////////////////////////////////////////////
	public override Vector3 GetLastPosition(uint EntityId)
	{
		int memoryUnitIndex = m_Memories.FindIndex((MemoryUnit u) => u.EntityId == EntityId);
		return (memoryUnitIndex == -1) ? Vector3.zero : m_Memories[memoryUnitIndex].LastEnemyPosition;
	}

	//////////////////////////////////////////////////////////////////////////
	public override Entity GetEntity(uint EntityId)
	{
		int memoryUnitIndex = m_Memories.FindIndex((MemoryUnit u) => u.EntityId == EntityId);
		return (memoryUnitIndex == -1) ? null : m_Memories[memoryUnitIndex].EntityRef;
	}

	//////////////////////////////////////////////////////////////////////////
	public override bool Remove(Entity entity)
	{
		if (entity == null)
		{
			Debug.Log("Entity::Memory:RemoveMemory: Passed invalid entity!!");
			return false;
		}

		int memoryUnitIndex = m_Memories.FindIndex((MemoryUnit u) => u.EntityId == entity.Id);
		if (memoryUnitIndex != -1)
		{
			m_Memories.RemoveAt(memoryUnitIndex);
			m_MemoriesCount--;
		}

		return memoryUnitIndex != -1;
	}

	//////////////////////////////////////////////////////////////////////////
	public override void Empty()
	{
		m_Memories.Clear();
		m_MemoriesCount = 0;
	}
}