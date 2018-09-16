
using UnityEngine;
using CFG_Reader;
using CutScene;
using AI;

public partial interface IEntity {
	
	// Set The path to follow in order to reach the destination
	void					NavSetPath( uint nodeCount, Vector3[] path );

	void					NavStop();
	
}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {
	

	protected	Vector3[]					m_NavPath						= null;
	protected	uint						m_NavPathLength					= 0;
	protected	Vector3						m_NavPrevPosition				= Vector3.zero;
	protected	uint						m_NavCurrentNodeIdx				= 0;
	protected	Vector3						m_NavCurrentNodePosition		= Vector3.zero;
	protected	float						m_NavNextNodeDistance			= 0.0f;
	protected	bool						m_NavHasDestination				= false;
	protected	Vector3						m_NavDestination				= Vector3.zero;
	protected	bool						m_NavCanMoveAlongPath			= true;

	protected	Vector3						CurrentNodeToReachPosition {
		get {
			if ( m_NavHasDestination && m_NavPath != null && m_NavPath.Length > 0 )
			{
				return m_NavPath[ m_NavCurrentNodeIdx ];
			}
			return Vector3.zero;
		}
	}

	// Position saved at start of movement ( used for distances check )
	protected	Vector3						m_StartMovePosition				= Vector3.zero;

	// Questa funzione viene chiamata durante il caricamento dello script o quando si modifica un valore nell'inspector (chiamata solo nell'editor)
	private void OnValidate()
	{

	}




	//////////////////////////////////////////////////////////////////////////
	// SetNavPath ( virtual )
	public		virtual		void	NavSetPath( uint nodeCount, Vector3[] path )
	{
		// path check
		if ( path == null || path.Length == 0 )
		{
			m_NavCanMoveAlongPath = false;
			return;
		}

		// Reset internals
		NavReset();

		// Fills navigation data
		m_NavHasDestination		= true;
		m_NavPrevPosition		= transform.position;
		m_NavCurrentNodeIdx		= 0;
		m_NavPath				= path;
		m_NavPathLength			= nodeCount;

		Vector3 projectedNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ 0 ] );
		m_NavNextNodeDistance	= ( projectedNodePosition - transform.position ).sqrMagnitude;

		m_NavDestination			= projectedNodePosition;
		m_NavCanMoveAlongPath		= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// NavStop ( virtual )
	public		virtual		void	NavStop()
	{
		// Reset internals
		NavReset();

		m_RigidBody.velocity = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// UpdateNavigation ( virtual )
	protected	virtual		void	NavUpdate( float deltaTime, float Speed )
	{
		// check
		if ( m_NavHasDestination == false || m_NavCanMoveAlongPath == false )
			return;

		// check traveled distance;
		float traveledDistance = ( m_NavPrevPosition - transform.position ).sqrMagnitude;
		if ( traveledDistance >= m_NavNextNodeDistance )
		{
			m_NavCurrentNodeIdx ++;

			print( "NodeReached" );

			// Arrived
			if ( m_NavCurrentNodeIdx == m_NavPathLength )
			{
				NavReset();
				print( "Path completed" );
				return;
			}

			// distance To Travel
			Vector3 projectedNextNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ m_NavCurrentNodeIdx ] );
			m_NavNextNodeDistance = ( projectedNextNodePosition - transform.position ).sqrMagnitude;
			m_NavPrevPosition = transform.position;
		}

		// go to node

		Vector3 projectedDestination = Utils.Math.ProjectPointOnPlane( transform.up, m_RigidBody.position, m_NavPath[ m_NavCurrentNodeIdx ] );
		Vector3 targetDirection = ( projectedDestination - transform.position ).normalized;

		SetPoinToFace( projectedDestination );

		// TODO Implement three parts entities body: Foots, body, head

		if ( m_Brain.State == BrainState.SEEKER || m_Brain.State == BrainState.NORMAL )
		{
			m_RigidBody.velocity = transform.forward * Speed * 10f * deltaTime;
		}
		else
		{
			m_RigidBody.velocity = targetDirection * Speed * 10f * deltaTime;
		}

//		m_RigidBody.AddForce( targetDirection * Speed, ForceMode.VelocityChange );

//		transform.position += targetDirection * Speed * 10f * Time.deltaTime;

//		m_RigidBody.velocity = targetDirection * Speed * 10f * Time.deltaTime;
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForNewReachPoint ( virtual )
	protected	virtual		void	CheckForNewReachPoint( Vector3 TargetPosition )
	{
		if ( m_TargetInfo.HasTarget == true )
		{
			// TODO find a way to no spawm path finding request

			// Path search event if not already near enough
			int targetNodeIndex = AI.Pathfinding.PathFinder.GetNearestNodeIdx( TargetPosition );
			if ( m_TargetNodeIndex != targetNodeIndex && ( transform.position - TargetPosition ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
			{
				if ( m_Brain.TryToReachPoint( targetNodeIndex ) )
				{
					m_TargetNodeIndex = targetNodeIndex;
					print( "found path" );
				}
			}
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// Resetnavigation ( virtual )
	protected	virtual		void	NavReset()
	{
		m_NavHasDestination					= false;
		m_NavPath							= null;
		m_NavPathLength						= 0;
		m_NavPrevPosition					= Vector3.zero;
		m_NavCurrentNodeIdx					= 0;
		m_NavNextNodeDistance				= 0.0f;
	}

}
