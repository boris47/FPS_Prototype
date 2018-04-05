
using UnityEngine;
using System.Collections;

public class DroneArmored : Drone {

	// Hitted by long range weapon
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

	// Hitted by close range weapon
	//////////////////////////////////////////////////////////////////////////
	// OnHurt ( Override )
	public override void OnHurt( ref IBullet bullet )
	{
		base.OnHurt( ref bullet );


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
	}


	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	public override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		// Not have a target
		if ( m_Brain.CurrentTargetInfo.HasTarget == false )
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
