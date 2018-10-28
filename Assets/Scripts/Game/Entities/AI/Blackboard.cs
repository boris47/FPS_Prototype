
using UnityEngine;
using System.Collections.Generic;

/* TODO
--- * make getter m_IsAllignedHeadToPoint
--- * make getter m_IsAllignedBodyToPoint
--- * make getter m_IsAllignedGunToPoint
--- * make getter m_HasLookAtObject
--- * make getter m_HasDestination
--- * make getter m_MinEngageDistance
--- * make method bool CanFire()		USUALLY CHECK m_IsAllignedGunToPoint
--- * make public Entity.UpdateHeadRotation
--- * make public Entity.FireLongRange
--- * make public Entity.RequestMovement
--- * make public Entity.SetPointToLookAt
--- * make public Entity.TakeDamage
 * make public 
 */

public class EntityBlackBoardData {

	public	Entity				EntityRef							= null;
	public	Transform			Transform							= null;

	public	Transform			HeadTransform						= null;
	public	Transform			BodyTransform						= null;

	public	TargetInfo_t		TargetInfo							= default( TargetInfo_t );

	public	Transform			TrasformToLookAt					= null;
	public	Vector3				PointToLookAt						= Vector3.zero;

	public	BrainState			BrainState							= BrainState.COUNT;

	public	float				AgentSpeed							= 0.0f;

}


public static class Blackboard {

	private static	Dictionary< uint, EntityBlackBoardData >	m_Data						= null;

	private	static	bool										m_bIsInitialized			= false;


	/// <summary>
	/// If not initialized, initialize blackboard data
	/// </summary>
	public	static	void	Initialize()
	{
		if ( m_bIsInitialized == false )
		{
			m_Data = new Dictionary< uint, EntityBlackBoardData >();
		}
	}


	/// <summary>
	/// If not already registered, register an entity by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	Register( uint EntityID )
	{
		Initialize();

		if ( m_Data.ContainsKey( EntityID ) )
		{
			return false;
		}

		EntityBlackBoardData entityData = new EntityBlackBoardData();
		m_Data.Add( EntityID, entityData );
		return true;
	}


	/// <summary>
	/// If already registered, Un-register an entity by its ID
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	UnRegister( uint EntityID )
	{
		Initialize();
		if ( IsEntityRegistered( EntityID ) )
		{
			return m_Data.Remove( EntityID );
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
