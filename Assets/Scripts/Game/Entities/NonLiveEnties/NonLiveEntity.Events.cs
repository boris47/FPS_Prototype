
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {

	
	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		m_TargetInfo = targetInfo;
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

		m_AllignedToPoint = false;
		m_AllignedGunToPoint = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( ref IBullet bullet )
	{
		// Hit event, set ALARMED State if actual is NORMAL
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
		}

		m_PointToFace		= bullet.StartPosition;	
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public override void OnThink()
	{	
		if ( m_TargetInfo.HasTarget == true )
		{
			m_PointToFace		= m_TargetInfo.CurrentTarget.transform.position;
		}
	}

}
