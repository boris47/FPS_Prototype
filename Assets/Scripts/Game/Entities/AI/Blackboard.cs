
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityBlackBoardData {

	public	Entity				EntityRef							= null;
	public	Transform			Transform							= null;

	public	Transform			HeadTransform						= null;
	public	Transform			BodyTransform						= null;

	public	LookData			LookData							= null;

	public	TargetInfo			TargetInfo							= null;

	public	Transform			TrasformToLookAt					= null;
	public	Vector3				PointToLookAt						= Vector3.zero;

	public	float				AgentSpeed							= 0.0f;

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
