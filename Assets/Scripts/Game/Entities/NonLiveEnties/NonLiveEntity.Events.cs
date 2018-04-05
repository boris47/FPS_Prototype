
using UnityEngine;


public abstract partial class NonLiveEntity : Entity {


	// Hitted by long range weapon
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
		m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;	
		
		if ( bullet is GranadeBase )
		{
			m_PointToFace = bullet.Transform.position;
			m_DistanceToTravel	= 0f;	
		}	
	}


	// Hitted by close range weapon
	//////////////////////////////////////////////////////////////////////////
	// OnHurt ( Override )
	public override void OnHurt( ref IBullet bullet )
	{
		// Hit event, ALARMED State
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
		}

		m_PointToFace = bullet.StartPosition;
		m_DistanceToTravel = ( transform.position - m_PointToFace ).sqrMagnitude;		
	}


	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public override void OnThink()
	{	
		bool hasBestResult = m_Brain.CurrentTargetInfo.HasTarget;
		if ( hasBestResult == true )
		{
			m_Brain.ChangeState( BrainState.ATTACKING );
			m_Target = m_Brain.CurrentTargetInfo.CurrentTarget.transform;
			m_PointToFace = m_Brain.CurrentTargetInfo.CurrentTarget.transform.position;
			m_DistanceToTravel = ( transform.position - m_PointToFace ).sqrMagnitude;
		}
		else
		{
			if ( m_Target != null )
			{
				m_Target = null;
				m_AllignedToPoint = false;
				m_AllignedGunToPoint = false;
				m_IsMoving = false;
				m_StartMovePosition = m_PointToFace = Vector3.zero;
				if ( m_Brain.State == BrainState.ATTACKING )
				{
					m_Brain.ChangeState( BrainState.NORMAL );
				}
			}

		}
		
	}

}
