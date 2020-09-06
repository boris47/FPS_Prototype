
using UnityEngine;
using Database;
using CutScene;
using UnityEngine.AI;


public partial interface IEntity {
	NavMeshAgent	NavAgent			{ get; }
	void			RequestMovement		( Vector3 Destination );
	void			NavStop				();
}


public abstract partial class Entity : IEntity {
	
	protected	const	float PATH_SEARCH_TIMEOUT		=		3.0f;

	// INTERFACE START
	NavMeshAgent				IEntity.NavAgent						{ get { return this.m_NavAgent; } }
	
	void						IEntity.RequestMovement( Vector3 Destination )
	{
		this.RequestMovement( Destination );
	}

	void						IEntity.NavStop()
	{
		this.NavStop();
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
		this.m_NavAgent.SetDestination( Destination );
		this.m_NavAgent.isStopped = true;

		float currentSearchTime = 0.0f;

		while (this.m_NavAgent.pathPending )
		{
			if (this.m_NavAgent.pathStatus == NavMeshPathStatus.PathPartial )
			{
				print(  "For " + this.name + " destination is unreachable" );
				this.m_HasPendingPathRequest = false;
				yield break;
			}

			currentSearchTime += Time.fixedUnscaledDeltaTime;
			if ( currentSearchTime > PATH_SEARCH_TIMEOUT )
			{
				print( "On Path Search TimeOut Reached" );
				this.OnPathSearchTimeOutReached();
				this.m_HasPendingPathRequest = false;
				yield break;
			}
			yield return null;
		}
		this.m_NavAgent.isStopped			= false;
		this.m_HasPendingPathRequest			= false;
		this.m_PendingPathRequestCO			= null;
		this.m_HasDestination				= true;
		this.m_DestinationToReachPosition	= Destination;
		this.m_NavCanMoveAlongPath			= true;

		m_OnNavigation( Destination );
	}


	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	RequestMovement( Vector3 Destination )
	{
		if (this.m_HasPendingPathRequest == false )
		{
			this.m_PendingPathRequestCO = CoroutinesManager.Start(this.RequestMovementCO( Destination ), "Entity::RequestMovement: Request of movement" );
			this.m_HasPendingPathRequest = true;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	NavStop()
	{
		this.m_NavAgent.isStopped = true;

		// Reset internals
		this.NavReset();
	}


	//////////////////////////////////////////////////////////////////////////

	protected	virtual		void	CheckForNewReachPoint( Vector3 TargetPosition ) // m_TargetInfo.CurrentTarget.Transform.position
	{
		if (this.m_TargetInfo.HasTarget == true )
		{
			// Path search event if not already close enough
			if ( ( TargetPosition - this.m_DestinationToReachPosition ).sqrMagnitude > 180.0f )
			{
				if ( (this.transform.position - TargetPosition ).sqrMagnitude > this.m_MinEngageDistance * this.m_MinEngageDistance )
				{
					///					print( "CheckForNewReachPoint" );
					this.RequestMovement ( TargetPosition );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////

	public	virtual		void	NavReset()
	{
		this.m_HasDestination				= false;
		this.m_DestinationToReachPosition	= Vector3.zero;
		this.m_NavCanMoveAlongPath			= false;

		if (this.m_HasPendingPathRequest )
		{
			if (this.m_PendingPathRequestCO != null )
				this.StopCoroutine(this.m_PendingPathRequestCO );
			this.m_PendingPathRequestCO = null;
			this.m_HasPendingPathRequest = false;
		}
	}

}
