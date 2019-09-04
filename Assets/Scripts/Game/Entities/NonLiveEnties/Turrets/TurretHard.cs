
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
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}

	//////////////////////////////////////////////////////////////////////////
	/*
	public override void OnHit( IBullet bullet )
	{
		if ( m_IsRecharging == false )
			return;

		base.OnHit( bullet );
	}
	*/

	//////////////////////////////////////////////////////////////////////////

	protected override void OnKill()
	{
		StopAllCoroutines();
		base.OnKill();
	}
	

	//////////////////////////////////////////////////////////////////////////

	public override void FireLongRange()
	{
		if ( m_ShotTimer > 0 )
			return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetNextComponent();
		bullet.Shoot( m_FirePoint.position, m_FirePoint.forward );
		
		m_FireAudioSource.Play();

		m_FiredBullets ++;
		if ( m_FiredBullets >= m_Magazine )
		{
			m_IsRecharging = true;
			CoroutinesManager.Start( ChargingCO(), "TurretHard::FireLongRange: Start of charging" );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected override StreamUnit OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnSave( streamData );
		if ( streamUnit == null )
			return null;

		streamUnit.SetInternal( "FiredBullets", m_FiredBullets );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////

	protected override StreamUnit OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		m_FiredBullets = ( uint ) streamUnit.GetAsInt( "FiredBullets" );
		
		return streamUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	
	private	IEnumerator	ChargingCO()
	{
		float	currentTime = 0f;
		float	interpolant	= 0f;

		float	timeStep = ( m_RechargeTime / 3f );

		Transform shiledTransform = ( m_Shield as Shield ).transform;

		Vector3 savedShieldScale = shiledTransform.localScale;

		// PHASE 1: Shield scaling down
		while( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;

			shiledTransform.localScale = Vector3.Lerp( savedShieldScale, Vector3.zero, interpolant );
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

			shiledTransform.localScale = Vector3.Lerp( Vector3.zero, savedShieldScale, interpolant );
			yield return null;
		}

		m_IsRecharging = false;
		m_FiredBullets = 0;
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected override void OnFrame( float deltaTime )
	{
		if ( m_IsRecharging == false )
		{
			base.OnFrame( deltaTime );
		}
	}

}
