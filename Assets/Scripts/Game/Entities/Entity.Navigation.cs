
using UnityEngine;
using CFG_Reader;
using CutScene;
using UnityEngine.AI;


public partial interface IEntity {
	NavMeshAgent	NavAgent	{ get; }
	bool			NavGoto		( Vector3 Destination );
	void			NavStop		();
}


public abstract partial class Entity : MonoBehaviour, IEntity, IEntitySimulation {
	
	// INTERFACE START
	NavMeshAgent				IEntity.NavAgent						{ get { return m_NavAgent; } }
	
	bool						IEntity.NavGoto( Vector3 Destination )
	{
		return NavGoto( Destination );
	}

	void						IEntity.NavStop()
	{
		NavStop();
	}
	// INTERFACE END


	protected	NavMeshAgent				m_NavAgent						= null;

	protected	bool						m_NavCanMoveAlongPath			= true;


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	NavUpdate()
	{
		if ( m_HasDestination == true && m_NavAgent.hasPath == true )
		{
			OnDestinationReached();
			NavReset();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		bool	NavGoto( Vector3 Destination )
	{
		bool result = true;
		if ( result &= m_NavAgent.SetDestination( Destination ) )
		{
			m_NavAgent.isStopped			= false;
			m_NavCanMoveAlongPath			= true;
			m_HasDestination				= true;
			m_DestinationToReachPosition	= Destination;
		} 

		return result;
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
					NavGoto ( TargetPosition );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected	virtual		void	NavReset()
	{
		m_HasDestination				= false;
		m_DestinationToReachPosition	= Vector3.zero;
		m_DestinationToReachRotation	= Vector3.zero;
		m_NavCanMoveAlongPath			= false;
	}

}
