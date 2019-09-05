
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityBlackBoardData {

	public	Transform			m_Transform							= null;
	private	Transform			m_HeadTransform						= null;
	private	Transform			m_BodyTransform						= null;


	//*/////////////////////////////////////////////////////////////////////
	//*/////////////////////////////////////////////////////////////////////

	public	Entity				EntityRef							= null;

//	public	Transform			HeadTransform						= null;
//	public	Transform			BodyTransform						= null;

	// Body
	public	Vector3				Head_Position						{ get { return m_HeadTransform.position; } }
	public	Quaternion			Head_Rotation						{ get { return m_HeadTransform.rotation; } }
	public	Vector3				Head_Forward						{ get { return m_HeadTransform.forward; } }
	public	Vector3				Head_Up								{ get { return m_HeadTransform.up; } }
	public	Vector3				Head_Right							{ get { return m_HeadTransform.right; } }

	// Body
	public	Vector3				Body_Position						{ get { return m_BodyTransform.position; } }
	public	Quaternion			Body_Rotation						{ get { return m_BodyTransform.rotation; } }
	public	Vector3				Body_Forward						{ get { return m_BodyTransform.forward; } }
	public	Vector3				Body_Up								{ get { return m_BodyTransform.up; } }
	public	Vector3				Body_Right							{ get { return m_BodyTransform.right; } }

	// Targettable
	public	Vector3				Transform_Position					{ get { return m_Transform.position; } }
	public	Quaternion			Transform_Rotation					{ get { return m_Transform.rotation; } }
	public	Vector3				Transform_Forward					{ get { return m_Transform.forward; } }
	public	Vector3				Transform_Up						{ get { return m_Transform.up; } }
	public	Vector3				Transform_Right						{ get { return m_Transform.right; } }

	public	LookData			LookData							= null;

	public	TargetInfo			TargetInfo							= null;

	public	Transform			TransformToLookAt					= null;
//	public	Vector3				PointToLookAt						= Vector3.zero;

	public	float				AgentSpeed							= 0.0f;


	public	EntityBlackBoardData( Transform Transform, Transform Head, Transform Body )
	{
		m_Transform			= Transform;
		m_HeadTransform		= Head;
		m_BodyTransform		= Body;
	}

}


public static class Blackboard {

	private static readonly	Dictionary< uint, EntityBlackBoardData >	m_Data						= null;

	private	static	bool												m_bIsInitialized			= false;


	/// <summary>
	/// If not initialized, initialize blackboard data
	/// </summary>
	static	Blackboard()
	{
		if ( m_bIsInitialized == false )
		{
			m_Data = new Dictionary< uint, EntityBlackBoardData >();
			m_bIsInitialized = true;
		}
	}


	/// <summary>
	/// If not already registered, register an entity by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	Register( uint EntityID, EntityBlackBoardData entityData )
	{
		if ( m_bIsInitialized == false )
		{
		//	Initialize();
		}

		if ( m_Data.ContainsKey( EntityID ) )
		{
			return false;
		}

		m_Data.Add( EntityID, entityData );
		return true;
	}


	/// <summary>
	/// If already registered, Un-register an entity by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	UnRegister( IEntity entity )
	{
	//	Initialize();
		if ( IsEntityRegistered( entity.ID ) )
		{
			return m_Data.Remove( entity.ID );
		}
		return false;
	}


	/// <summary>
	/// Check and returns if an entity is registered by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	IsEntityRegistered( uint EntityID )
	{
		return m_bIsInitialized ? m_Data.ContainsKey( EntityID ) : false;
	}

	
	/// <summary>
	/// Return data for a given entity ID if registered
	/// </summary>
	/// <param name="EntityID"></param>
	/// <param name="Key"></param>
	/// <param name="Default"></param>
	/// <returns></returns>
	public	static	EntityBlackBoardData	GetData( uint EntityID )
	{
		return IsEntityRegistered( EntityID ) ? m_Data[ EntityID ] : null;
	}

}
