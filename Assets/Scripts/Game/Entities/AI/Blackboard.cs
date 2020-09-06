
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

	// Body
	public	Vector3				Head_Position						=> this.m_HeadTransform.position;
	public	Quaternion			Head_Rotation						=> this.m_HeadTransform.rotation;
	public	Vector3				Head_Forward						=> this.m_HeadTransform.forward;
	public	Vector3				Head_Up								=> this.m_HeadTransform.up;
	public	Vector3				Head_Right							=> this.m_HeadTransform.right;

	// Body
	public	Vector3				Body_Position						=> this.m_BodyTransform.position;
	public	Quaternion			Body_Rotation						=> this.m_BodyTransform.rotation;
	public	Vector3				Body_Forward						=> this.m_BodyTransform.forward;
	public	Vector3				Body_Up								=> this.m_BodyTransform.up;
	public	Vector3				Body_Right							=> this.m_BodyTransform.right;

	// Targettable
	public	Vector3				Transform_Position					=> this.m_Transform.position;
	public	Quaternion			Transform_Rotation					=> this.m_Transform.rotation;
	public	Vector3				Transform_Forward					=> this.m_Transform.forward;
	public	Vector3				Transform_Up						=> this.m_Transform.up;
	public	Vector3				Transform_Right						=> this.m_Transform.right;

	public	LookData			LookData							= null;

	public	TargetInfo			TargetInfo							= null;

	public	Transform			TransformToLookAt					= null;
//	public	Vector3				PointToLookAt						= Vector3.zero;

	public	float				AgentSpeed							= 0.0f;


	public	EntityBlackBoardData( Transform Transform, Transform Head, Transform Body )
	{
		this.m_Transform			= Transform;
		this.m_HeadTransform		= Head;
		this.m_BodyTransform		= Body;
	}

}


public static class Blackboard {

	private static readonly	Dictionary< uint, EntityBlackBoardData >	m_Data						= null;

	private	static	bool												m_IsInitialized			= false;


	/// <summary>
	/// If not initialized, initialize blackboard data
	/// </summary>
	static	Blackboard()
	{
		if ( m_IsInitialized == false )
		{
			m_Data = new Dictionary< uint, EntityBlackBoardData >();
			m_IsInitialized = true;
		}
	}


	/// <summary>
	/// If not already registered, register an entity by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	Register( uint EntityID, EntityBlackBoardData entityData )
	{
		if ( m_IsInitialized == false )
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
		return m_IsInitialized ? m_Data.ContainsKey( EntityID ) : false;
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
