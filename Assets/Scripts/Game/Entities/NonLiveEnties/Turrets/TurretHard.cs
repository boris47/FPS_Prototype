
using UnityEngine;
using System.Collections;

public class TurretHard : Turret {

	[Header("Turret Hard Properties")]

	[SerializeField]
	private		uint		m_Magazine			= 10;

	[SerializeField]
	private		float		m_RechargeTime		= 2f;

	private		bool		m_IsRecharging		= false;
	private		uint		m_FiredBullets		= 0;



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

		if ( m_IsRecharging == false )
			return;

		float damage = Random.Range( bullet.DamageMin, bullet.DamageMax );
		m_Health -= damage;

		if ( m_Health < 0f )
			OnKill();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnKill ( Override )
	public override void OnKill()
	{
		StopAllCoroutines();
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
	// FireLongRange ( Override )
	protected override void FireLongRange( float deltaTime )
	{
		m_ShotTimer -= deltaTime;
		if ( m_ShotTimer > 0 )
			return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetComponent();
		bullet.Shoot( m_FirePoint.position, m_FirePoint.forward );
		
		m_FireAudioSource.Play();

		m_FiredBullets ++;
		if ( m_FiredBullets >= m_Magazine )
		{
			m_IsRecharging = true;
			StartCoroutine( ChargingCO() );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ChargingCO ( Coroutine )
	private	IEnumerator	ChargingCO()
	{
		float	currentTime = 0f;
		float	interpolant	= 0f;

		float	timeStep = ( m_RechargeTime / 3f );

		Vector3 savedShieldScale = m_Shield.transform.localScale;

		// PHASE 1: Shield scaling down
		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;

			m_Shield.transform.localScale = Vector3.Lerp( savedShieldScale, Vector3.zero, interpolant );
			yield return null;
		}
		interpolant = currentTime = 0f;

		// PHASE 2: Recharge
		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;
			yield return null;
		}
		interpolant = currentTime = 0f;

		// PHASE 3: Shield scaling up
		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;

			m_Shield.transform.localScale = Vector3.Lerp( Vector3.zero, savedShieldScale, interpolant );
			yield return null;
		}

		m_IsRecharging = false;
		m_FiredBullets = 0;
	}


	// Update forward direction and gun rotation
	//////////////////////////////////////////////////////////////////////////
	// Update ( Override )
	public override void OnFrame( float deltaTime )
	{
		base.OnFrame( deltaTime );
		
		if ( m_IsRecharging == true )
			return;

		if ( m_TargetInfo.HasTarget == false )
			return;

		if ( m_AllignedGunToPoint == false )
			return;

		FireLongRange( deltaTime );
		
	}


}
