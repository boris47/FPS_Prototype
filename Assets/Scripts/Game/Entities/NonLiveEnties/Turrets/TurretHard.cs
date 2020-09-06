
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
		this.m_SectionName = this.GetType().FullName;

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
		this.StopAllCoroutines();
		base.OnKill();
	}
	

	//////////////////////////////////////////////////////////////////////////

	public override void FireLongRange()
	{
		if (this.m_ShotTimer > 0 )
			return;

		this.m_ShotTimer = this.m_ShotDelay;

		IBullet bullet = this.m_Pool.GetNextComponent();
		bullet.Shoot(this.m_FirePoint.position, this.m_FirePoint.forward );

		this.m_FireAudioSource.Play();

		this.m_FiredBullets ++;
		if (this.m_FiredBullets >= this.m_Magazine )
		{
			this.m_IsRecharging = true;
			CoroutinesManager.Start(this.ChargingCO(), "TurretHard::FireLongRange: Start of charging" );
		}
	}
	

	//////////////////////////////////////////////////////////////////////////

	protected override StreamUnit OnSave( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnSave( streamData );
		if ( streamUnit == null )
			return null;

		streamUnit.SetInternal( "FiredBullets", this.m_FiredBullets );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////

	protected override StreamUnit OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = base.OnLoad( streamData );
		if ( streamUnit == null )
			return null;

		this.m_FiredBullets = ( uint ) streamUnit.GetAsInt( "FiredBullets" );
		
		return streamUnit;
	}
	

	//////////////////////////////////////////////////////////////////////////
	
	private	IEnumerator	ChargingCO()
	{
		float	currentTime = 0f;
		float	interpolant	= 0f;

		float	timeStep = (this.m_RechargeTime / 3f );

		Transform shiledTransform = (this.m_Shield as Shield ).transform;

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

		this.m_IsRecharging = false;
		this.m_FiredBullets = 0;
	}

	
	//////////////////////////////////////////////////////////////////////////

	protected override void OnFrame( float deltaTime )
	{
		if (this.m_IsRecharging == false )
		{
			base.OnFrame( deltaTime );
		}
	}

}
