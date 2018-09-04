
using UnityEngine;
using CFG_Reader;
using CutScene;
using AI;

public partial interface IEntity {
	
	// Set The path to follow in order to reach the destination
	void					NavSetPath( uint nodeCount, IAINode[] path );

	void					NavStop();
	
}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {
	

	protected	IAINode[]					m_NavPath						= null;
	protected	uint						m_NavPathLength					= 0;
	protected	Vector3						m_NavPrevPosition				= Vector3.zero;
	protected	uint						m_NavCurrentNodeIdx				= 0;
	protected	float						m_NavNextNodeDistance			= 0.0f;
	protected	bool						m_HasDestination				= false;
	protected	Vector3						m_Destination					= Vector3.zero;
	protected	bool						m_IsMoving						= false;

	// Position saved at start of movement ( used for distances check )
	protected	Vector3						m_StartMovePosition				= Vector3.zero;



	//////////////////////////////////////////////////////////////////////////
	// SetNavPath ( virtual )
	public		virtual		void	NavSetPath( uint nodeCount, IAINode[] path )
	{
		// path check
		if ( path == null || path.Length == 0 )
		{
			return;
		}

		// Reset internals
		NavReset();

		// Fills navigation data
		m_HasDestination		= true;
		m_NavPrevPosition		= transform.position;
		m_NavCurrentNodeIdx		= 0;
		m_NavPath				= path;
		m_NavPathLength			= nodeCount;

		Vector3 projectedNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ 0 ].Position );
		m_NavNextNodeDistance	= ( projectedNodePosition - transform.position ).sqrMagnitude;

		m_Destination			= projectedNodePosition;
	}


	public		virtual		void	NavStop()
	{
		// Reset internals
		NavReset();

		m_RigidBody.velocity = Vector3.zero;
	}

	//////////////////////////////////////////////////////////////////////////
	// UpdateNavigation ( virtual )
	protected	virtual		void	NavUpdate()
	{
		// check
		if ( m_HasDestination == false )
			return;

		// check traveled distance;
		float traveledDistance = ( m_NavPrevPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance >= m_NavNextNodeDistance )
		{
			m_NavPath[ m_NavCurrentNodeIdx ].OnNodeReached( this );
			m_NavCurrentNodeIdx ++;

			print( "NodeReached" );

			// Arrived
			if ( m_NavCurrentNodeIdx == m_NavPathLength || m_NavPath[ m_NavCurrentNodeIdx ] == null )
			{
				NavReset();
				print( "Path completed" );
				return;
			}

			// distance To Travel
			Vector3 projectedNextNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ m_NavCurrentNodeIdx ].Position );
			m_NavNextNodeDistance = ( projectedNextNodePosition - transform.position ).sqrMagnitude;
			m_NavPrevPosition = transform.position;
		}

		// go to node

		Vector3 projectedDestination = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ m_NavCurrentNodeIdx ].Position );
		Vector3 targetDirection = ( projectedDestination - transform.position ).normalized;

		m_RigidBody.velocity	= targetDirection * 82f * Time.deltaTime;
	}


	//////////////////////////////////////////////////////////////////////////
	// Resetnavigation ( virtual )
	protected	virtual		void	NavReset()
	{
		m_HasDestination					= false;
		m_NavPath							= null;
		m_NavPathLength						= 0;
		m_NavPrevPosition					= Vector3.zero;
		m_NavCurrentNodeIdx					= 0;
		m_NavNextNodeDistance				= 0.0f;
	}

}
