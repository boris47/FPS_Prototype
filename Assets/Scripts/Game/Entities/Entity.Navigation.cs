
using UnityEngine;
using Database;
using CutScene;
using UnityEngine.AI;


public partial interface IEntity
{
	NavMeshAgent	NavAgent			{ get; }
	void			RequestMovement		( Vector3 Destination );
	void			NavStop				();
}


public abstract partial class Entity : IEntity {
	
	protected	const	float PATH_SEARCH_TIMEOUT		=		3.0f;

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
	protected	bool					m_HasNavAgent					= false;

	[System.NonSerialized]
	protected	bool					m_NavCanMoveAlongPath			= true;

	protected	bool					m_HasPendingPathRequest			= false;

	protected	Coroutine				m_PendingPathRequestCO			= null;



	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void		OnPathSearchTimeOutReached()
	{

	}


	//////////////////////////////////////////////////////////////////////////

	protected	virtual System.Collections.IEnumerator RequestMovementCO( Vector3 Destination )
	{
		m_NavAgent.SetDestination( Destination );
		m_NavAgent.isStopped = true;

		float currentSearchTime = 0.0f;

		while (m_NavAgent.pathPending )
		{
			if (m_NavAgent.pathStatus == NavMeshPathStatus.PathPartial )
			{
				print(  "For " + name + " destination is unreachable" );
				m_HasPendingPathRequest = false;
				yield break;
			}

			currentSearchTime += Time.fixedUnscaledDeltaTime;
			if ( currentSearchTime > PATH_SEARCH_TIMEOUT )
			{
				print( "On Path Search TimeOut Reached" );
				OnPathSearchTimeOutReached();
				m_HasPendingPathRequest = false;
				yield break;
			}
			yield return null;
		}
		m_NavAgent.isStopped			= false;
		m_HasPendingPathRequest			= false;
		m_PendingPathRequestCO			= null;
		m_HasDestination				= true;
		m_DestinationToReachPosition	= Destination;
		m_NavCanMoveAlongPath			= true;

		m_OnNavigation( Destination );
	}


	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	RequestMovement( Vector3 Destination )
	{
		if (m_HasPendingPathRequest == false )
		{
			m_PendingPathRequestCO = CoroutinesManager.Start(RequestMovementCO( Destination ), "Entity::RequestMovement: Request of movement" );
			m_HasPendingPathRequest = true;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	NavStop()
	{
		m_NavAgent.isStopped = true;

		// Reset internals
		NavReset();
	}


	//////////////////////////////////////////////////////////////////////////
	/*
	protected	virtual		void	CheckForNewReachPoint(Vector3 TargetPosition) // m_TargetInfo.CurrentTarget.Transform.position
	{
		if (m_TargetInfo.HasTarget == true)
		{
			// Path search event if not already close enough
			if ((TargetPosition - m_DestinationToReachPosition).sqrMagnitude > 180.0f)
			{
				if ((transform.position - TargetPosition).sqrMagnitude > m_MinEngageDistance * m_MinEngageDistance)
				{
				//	print( "CheckForNewReachPoint" );
					RequestMovement(TargetPosition);
				}
			}
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	NavReset()
	{
		m_HasDestination				= false;
		m_DestinationToReachPosition	= Vector3.zero;
		m_NavCanMoveAlongPath			= false;

		if (m_HasPendingPathRequest )
		{
			if (m_PendingPathRequestCO != null )
				StopCoroutine(m_PendingPathRequestCO );
			m_PendingPathRequestCO = null;
			m_HasPendingPathRequest = false;
		}
	}

}
