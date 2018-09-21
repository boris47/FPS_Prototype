
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

	protected	bool						m_NavCanMoveAlongPath			= true;
	protected	int							m_NavTargetNodeIndex			= 0;

	protected	Vector3						CurrentNodeToReachPosition {
		get {
			if ( m_HasDestination && m_NavPath != null && m_NavPath.Length > 0 )
			{
				return m_NavPath[ m_NavCurrentNodeIdx ];
			}
			return Vector3.zero;
		}
	}


	// Questa funzione viene chiamata durante il caricamento dello script o quando si modifica un valore nell'inspector (chiamata solo nell'editor)
	private void OnValidate()
	{
		// get call 3 times plus 1 on application quit
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
		m_HasDestination		= true;
		m_NavPrevPosition		= transform.position;
		m_NavCurrentNodeIdx		= 0;
		m_NavPath				= path;
		m_NavPathLength			= nodeCount;

		Vector3 projectedNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, m_NavPath[ 0 ] );
		m_NavNextNodeDistance	= ( projectedNodePosition - transform.position ).sqrMagnitude;

		m_DestinationToReach		= m_NavPath[nodeCount - 1];
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
	protected	virtual		void	NavUpdate( float Speed, float DeltaTime )
	{
		// check
		if ( m_HasDestination == false || m_NavCanMoveAlongPath == false )
			return;

		// check traveled distance;
		if ( ( m_NavPrevPosition - transform.position ).sqrMagnitude >= m_NavNextNodeDistance )
		{
//			print( "NodeReached" );
			m_NavCurrentNodeIdx ++;

			// Arrived
			if ( m_NavCurrentNodeIdx == m_NavPathLength )
			{
				NavReset();
				print( "Path completed" );
				return;
			}

			// distance To Travel
			Vector3 projectedNextNodePosition = Utils.Math.ProjectPointOnPlane( transform.up, transform.position, CurrentNodeToReachPosition );
			m_NavNextNodeDistance = ( projectedNextNodePosition - transform.position ).sqrMagnitude;
			m_NavPrevPosition = transform.position;
		}

		// go to node with custom movement, if present
		NavMove( CurrentNodeToReachPosition, Speed, DeltaTime );

		////
		#region

		//		Vector3 projectedDestination = Utils.Math.ProjectPointOnPlane( transform.up, m_RigidBody.position, m_NavPath[ m_NavCurrentNodeIdx ] );
		//		Vector3 targetDirection = ( projectedDestination - transform.position ).normalized;

		//		SetPoinToFace(  m_NavPath[ m_NavCurrentNodeIdx ] );

		// TODO Implement three parts entities body: Foots, body, head
		/*
		NavMove( m_NavPath[ m_NavCurrentNodeIdx ] );

		/*
		if ( m_Brain.State == BrainState.SEEKER || m_Brain.State == BrainState.NORMAL )
		{
			m_RigidBody.velocity = transform.forward * Speed * 10f * deltaTime;
		}
		else
		{
			m_RigidBody.velocity = targetDirection * Speed * 10f * deltaTime;
		}
		*/
		//		m_RigidBody.AddForce( targetDirection * Speed, ForceMode.VelocityChange );

		//		transform.position += targetDirection * Speed * 10f * Time.deltaTime;

		//		m_RigidBody.velocity = targetDirection * Speed * 10f * Time.deltaTime;
		#endregion
	}


	//////////////////////////////////////////////////////////////////////////
	// NavMove ( virtual )
	protected	virtual	void	NavMove( Vector3 CurrentDestination, float Speed, float DeltaTime )
	{
		print( "You should not call this function" );
	}


	//////////////////////////////////////////////////////////////////////////
	// CheckForNewReachPoint ( virtual )
	protected	virtual		void	CheckForNewReachPoint( Vector3 TargetPosition ) // m_TargetInfo.CurrentTarget.Transform.position
	{

		if ( m_TargetInfo.HasTarget == true )
		{
			// TODO find a way to no spawm path finding request

			// Path search event if not already near enough
			if ( ( TargetPosition - m_DestinationToReach ).sqrMagnitude > 180.0f )
			{
				if ( ( transform.position - TargetPosition ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
				{
					int targetNodeIndex = AI.Pathfinding.PathFinder.GetNearestNodeIdx( TargetPosition );
					if ( m_Brain.TryToReachPoint( targetNodeIndex ) )
					{
						print( "Re-found path" );
					}
				}
			}

		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// Resetnavigation ( virtual )
	protected	virtual		void	NavReset()
	{
//		if ( m_Brain.State != BrainState.ATTACKING )
		{
			m_NavPath							= null;
			m_NavPathLength						= 0;
			m_NavPrevPosition					= Vector3.zero;
			m_NavCurrentNodeIdx					= 0;
			m_NavCurrentNodePosition			= Vector3.zero;
			m_NavNextNodeDistance				= 0.0f;

			m_NavCanMoveAlongPath				= false;

			m_HasDestination					= false;
		}
	}

}
