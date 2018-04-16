﻿
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
	// Awake ( Override )
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}


	//////////////////////////////////////////////////////////////////////////
	// OnHit ( Override )
	public override void OnHit( ref IBullet bullet )
	{

		// Avoid friendly fire
		if ( bullet.WhoRef is NonLiveEntity )
			return;

		// Hit event, set ALARMED State if actual is NORMAL
		if ( m_Brain.State == BrainState.NORMAL )
		{
			m_Brain.ChangeState( BrainState.ALARMED );
		}

		// if is not attacking
		if ( m_Brain.State != BrainState.ATTACKING )
		{
			// set start bullet position as point to face at
			m_PointToFace	= bullet.StartPosition;	
			m_HasFaceTarget = true;
		}

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
		// Update internal timer
		m_ShotTimer -= deltaTime;
		
		if ( m_TargetInfo.HasTarget == true )
		{
			if ( m_Brain.State != BrainState.ATTACKING )
				m_Brain.ChangeState( BrainState.ATTACKING );
			
			m_PointToFace = m_TargetInfo.CurrentTarget.transform.position;
			m_HasFaceTarget = true;

			m_Destination = m_TargetInfo.CurrentTarget.transform.position;
			m_HasDestination = true;

			m_DistanceToTravel	= ( transform.position - m_PointToFace ).sqrMagnitude;
		}

		// if has target point to face at set
		if ( m_HasFaceTarget )
		{
			FaceToPoint( deltaTime );   // m_PointToFace
		}

		// if body is alligned with target start moving
		if ( m_IsAllignedBodyToDestination && m_IsMoving == false )
		{
			m_IsMoving = true;
			m_StartMovePosition = transform.position;
		}

		// if has destination set
		if ( m_HasDestination && m_IsAllignedBodyToDestination )
		{
			GoAtPoint( deltaTime );	// m_Destination
		}

		// if gun alligned, fire
		if ( m_AllignedGunToPoint == true && m_TargetInfo.HasTarget == true && m_IsRecharging == false )
		{
			FireLongRange( deltaTime );
		}
	}


}