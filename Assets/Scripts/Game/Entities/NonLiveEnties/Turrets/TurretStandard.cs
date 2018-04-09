
using UnityEngine;


public class TurretStandard : Turret {


	public override void OnTargetAquired( TargetInfo_t targetInfo )	{ }
	public override void OnTargetChanged( TargetInfo_t targetInfo ) { }
	public override void OnTargetLost( TargetInfo_t targetInfo )	{ }


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( ref IBullet bullet )
	{
		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		base.OnHit( ref bullet );

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
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
		
		if ( m_TargetInfo.HasTarget == false )
			return;

		if ( m_AllignedGunToPoint == false )
			return;

		FireLongRange( deltaTime );
		
	}

}
