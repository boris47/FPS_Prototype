
using UnityEngine;
using CFG_Reader;
using CutScene;
using UnityEngine.AI;


public partial interface IEntity {
	NavMeshAgent	NavAgent			{ get; }
	void			RequestMovement		( Vector3 Destination );
	void			NavStop				();
}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {
	
	// INTERFACE START
	NavMeshAgent				IEntity.NavAgent						{ get { return m_NavAgent; } }
	
	void						IEntity.RequestMovement( Vector3 Destination )
	{
		RequestMovement( Destination );
	}

	void						IEntity.NavStop()
	{
		NavStop();
	}
	// INTERFACE END


	protected	NavMeshAgent			m_NavAgent						= null;

	protected	bool					m_NavCanMoveAlongPath			= true;


	protected	virtual		void	OnPathSearchTimeOutReached()
	{

	}


	//////////////////////////////////////////////////////////////////////////

	protected	const	float PATH_SEARCH_TIMEOUT = 3.0f;
	protected	System.Collections.IEnumerator RequestMovementCO( Vector3 Destination )
	{
		m_NavAgent.SetDestination( Destination );
		m_NavAgent.isStopped = true;

		float currentSearchTime = 0.0f;

		while ( m_NavAgent.pathPending )
		{
			if ( m_NavAgent.pathStatus == NavMeshPathStatus.PathPartial )
			{
				print(  "For " + name + " destination is unreachable" );
				yield break;
			}

			currentSearchTime += Time.fixedUnscaledDeltaTime;
			if ( currentSearchTime > PATH_SEARCH_TIMEOUT )
			{
				print( "On Path Search TimeOut Reached" );
				OnPathSearchTimeOutReached();
				yield break;
			}
			yield return null;
		}


		print( "path successifully found" );
		m_NavAgent.isStopped			= false;
		m_NavCanMoveAlongPath			= true;
		m_HasDestination				= true;
		m_DestinationToReachPosition	= Destination;
	}


	//////////////////////////////////////////////////////////////////////////

	protected	virtual		void	RequestMovement( Vector3 Destination )
	{
		StartCoroutine( RequestMovementCO( Destination ) );
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected	virtual		void	NavStop()
	{
		m_NavAgent.isStopped = true;

		// Reset internals
		NavReset();
	}


	//////////////////////////////////////////////////////////////////////////

	protected	virtual		void	CheckForNewReachPoint( Vector3 TargetPosition ) // m_TargetInfo.CurrentTarget.Transform.position
	{
		if ( m_TargetInfo.HasTarget == true )
		{
			// Path search event if not already near enough
			if ( ( TargetPosition - m_DestinationToReachPosition ).sqrMagnitude > 180.0f )
			{
				if ( ( transform.position - TargetPosition ).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance )
				{
					print( "CheckForNewReachPoint" );
					RequestMovement ( TargetPosition );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////

	protected	virtual		void	NavReset()
	{
		m_HasDestination				= false;
		m_DestinationToReachPosition	= Vector3.zero;
		m_NavCanMoveAlongPath			= false;
	}

}
