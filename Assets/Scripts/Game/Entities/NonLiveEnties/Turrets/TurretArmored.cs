﻿
using UnityEngine;


public class TurretArmored : Turret {


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
			OnKill();
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

	
	// Update forward direction and gun rotation
	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	public override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );

		if ( m_Brain.CurrentTargetInfo.HasTarget == false )
			return;

		if ( m_AllignedGunToPoint == false )
			return;

		FireLongRange( deltaTime );
	}

}
