
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {

	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;

		// now point to face is target position
		m_PointToFace = m_TargetInfo.CurrentTarget.transform.position;
		m_HasFaceTarget = true;

		// now point to reach is target position
		m_Destination = m_TargetInfo.CurrentTarget.transform.position;
		m_HasDestination = true;

		m_Brain.ChangeState( BrainState.ATTACKING );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	public override void OnTargetChanged( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		m_TargetInfo = default( TargetInfo_t );

		m_Brain.ChangeState( BrainState.NORMAL );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( IBullet bullet )
	{
		OnHit( bullet.StartPosition, bullet.WhoRef, 0f );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public	override	void	OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		// Hit event, set ALARMED State if actual is NORMAL
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
		}

		// if is not attacking
		if ( m_Brain.State != BrainState.ATTACKING )
		{
			// set start bullet position as point to face at
			m_PointToFace	= startPosition;	
			m_HasFaceTarget = true;
		}
	}

}
