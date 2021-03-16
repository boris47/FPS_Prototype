
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityBlackBoardData
{
	[SerializeField, ReadOnly]
	private	Transform			m_Targettable							= null;
	[SerializeField, ReadOnly]
	private	Transform			m_HeadTransform						= null;
	[SerializeField, ReadOnly]
	private	Transform			m_BodyTransform						= null;


	//*/////////////////////////////////////////////////////////////////////
	//*/////////////////////////////////////////////////////////////////////

	[SerializeField, ReadOnly]
	public	Entity				EntityRef							= null;
	[SerializeField, ReadOnly]
	public	Vector3				SpawnBodyLocation					= Vector3.zero;
	[SerializeField, ReadOnly]
	public	Vector3				SpawnHeadLocation					= Vector3.zero;
	[SerializeField, ReadOnly]
	public	Quaternion			SpawnBodyRotation					= Quaternion.identity;
	[SerializeField, ReadOnly]
	public	Quaternion			SpawnHeadRotation					= Quaternion.identity;

	// Body
	public	Vector3				Head_Position						=> m_HeadTransform.position;
	public	Quaternion			Head_Rotation						=> m_HeadTransform.rotation;
	public	Vector3				Head_Forward						=> m_HeadTransform.forward;
	public	Vector3				Head_Up								=> m_HeadTransform.up;
	public	Vector3				Head_Right							=> m_HeadTransform.right;

	// Body
	public	Vector3				Body_Position						=> m_BodyTransform.position;
	public	Quaternion			Body_Rotation						=> m_BodyTransform.rotation;
	public	Vector3				Body_Forward						=> m_BodyTransform.forward;
	public	Vector3				Body_Up								=> m_BodyTransform.up;
	public	Vector3				Body_Right							=> m_BodyTransform.right;

	// Targettable
	public	Vector3				Targettable_Position				=> m_Targettable.position;
	public	Quaternion			Targettable_Rotation				=> m_Targettable.rotation;
	public	Vector3				Targettable_Forward					=> m_Targettable.forward;
	public	Vector3				Targettable_Up						=> m_Targettable.up;
	public	Vector3				Targettable_Right					=> m_Targettable.right;

	[SerializeField, ReadOnly]
	public	LookData			LookData							= null;

//	[SerializeField, ReadOnly]
//	public	TargetInfo			TargetInfo							= null;

	[SerializeField, ReadOnly]
	public	float				AgentSpeed							= 0.0f;


	public	EntityBlackBoardData(Entity entity)
	{
		m_Targettable		= entity.Targettable;
		m_HeadTransform		= entity.Head;
		m_BodyTransform		= entity.Body;
	}

}


public class Blackboard : OnDemandSingleton<Blackboard>
{
	private readonly	Dictionary<uint, EntityBlackBoardData> m_Data = new Dictionary<uint, EntityBlackBoardData>();


	/// <summary> If not already registered, register an entity by its ID </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	bool	Register( uint EntityID, EntityBlackBoardData entityData )
	{
		if (m_Data.ContainsKey(EntityID))
		{
			return false;
		}

		m_Data.Add(EntityID, entityData);
		return true;
	}


	/// <summary> If already registered, Un-register an entity by its ID </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	bool	UnRegister( Entity entity )
	{
		if (IsEntityRegistered(entity.Id))
		{
			return m_Data.Remove(entity.Id);
		}
		return false;
	}


	/// <summary> Check and returns if an entity is registered by its ID </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	bool	IsEntityRegistered( uint EntityID )
	{
		return m_Data.ContainsKey(EntityID);
	}

	
	/// <summary> Return data for a given entity ID if registered </summary>
	/// <param name="EntityID"></param>
	/// <param name="Key"></param>
	/// <param name="Default"></param>
	/// <returns></returns>
	public	EntityBlackBoardData	GetData( uint EntityID )
	{
		EntityBlackBoardData data;
		m_Data.TryGetValue(EntityID, out data);
		return data;
	}

}
