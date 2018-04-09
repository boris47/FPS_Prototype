
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {


	//////////////////////////////////////////////////////////////////////////
	// OnTargetAquired ( Override )
	public override void OnTargetAquired( TargetInfo_t targetInfo )
	{
		base.OnTargetAquired( targetInfo );		// m_TargetInfo = targetInfo;

		m_Brain.ChangeState( BrainState.ATTACKING );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetChanged ( Override )
	public override void OnTargetChanged( TargetInfo_t targetInfo )
	{
		base.OnTargetChanged( targetInfo );		// m_TargetInfo = targetInfo;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetLost ( Override )
	public override void OnTargetLost( TargetInfo_t targetInfo )
	{
		base.OnTargetLost( targetInfo );		// m_TargetInfo = default( TargetInfo_t );

		m_IsMoving = false;
		m_StartMovePosition = m_PointToFace = Vector3.zero;

		if ( m_Brain.State == BrainState.ATTACKING )
		{
			m_Brain.ChangeState( BrainState.NORMAL );
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( ref IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		base.OnHit( ref bullet );

		if ( m_Shield != null && m_Shield.Status > 0f && m_Shield.IsUnbreakable == false )
		{
			m_Shield.OnHit( ref bullet );
			return;
		}

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health < 0f )
		{
			OnKill();
			return;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public override void OnKill()
	{
		base.OnKill();
		m_Pool.Destroy();
		Destroy( gameObject );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnThink ( Override )
	public override void OnThink()
	{
		base.OnThink();
		
		if ( m_TargetInfo.HasTarget == true )
		{
			m_DistanceToTravel	= ( transform.position - m_TargetInfo.CurrentTarget.transform.position ).sqrMagnitude;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	public override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		if ( m_Brain.State != BrainState.NORMAL )
		{
			if ( m_AllignedToPoint && m_IsMoving == false )
			{
				m_IsMoving = true;
				m_StartMovePosition = transform.position;
			}

			GoAtPoint( deltaTime );
		}

		// Not have a target
		if ( m_TargetInfo.HasTarget == false )
			return;

		// Not aligned to target
		if ( m_AllignedGunToPoint == false )
			return;
		
/*		// CLOSE RANGE COMBAT
		if ( m_Brain.CurrentTargetInfo.TargetSqrDistance < m_CloseCombatRange * m_CloseCombatRange )
		{
			FireCloseRange();
		}
		// LONG RANGE COMBAT
		else
*/		{
			FireLongRange( deltaTime );
		}
		
	}

}
