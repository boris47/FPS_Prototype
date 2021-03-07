﻿
using UnityEngine;
using System.Collections;

public class TurretHard : Turret {

	[Header("Turret Hard Properties")]

//	[SerializeField]
//	private		uint		m_Magazine			= 10;

	[SerializeField]
	private		float		m_RechargeTime		= 2f;

//	private		bool		m_IsRecharging		= false;
//	private		uint		m_FiredBullets		= 0;


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
	/*
	public override void FireLongRange()
	{
		if (m_ShotTimer > 0 )
			return;

		m_ShotTimer = m_ShotDelay;

		IBullet bullet = m_Pool.GetNextComponent();
		bullet.Shoot(m_FirePoint.position, m_FirePoint.forward, velocity: null );

		m_FireAudioSource.Play();

		m_FiredBullets ++;
		if (m_FiredBullets >= m_Magazine )
		{
			m_IsRecharging = true;
			CoroutinesManager.Start(ChargingCO(), "TurretHard::FireLongRange: Start of charging" );
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////

	protected override bool OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = base.OnSave( streamData, ref streamUnit );
		{
		//	streamUnit.SetInternal( "FiredBullets", m_FiredBullets );
		}
		return bResult;
	}


	//////////////////////////////////////////////////////////////////////////

	protected override bool OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = base.OnLoad( streamData, ref streamUnit );
		if (bResult)
		{
	//		m_FiredBullets = ( uint ) streamUnit.GetAsInt( "FiredBullets" );
		}
		return bResult;
	}
	

	//////////////////////////////////////////////////////////////////////////
	
	private	IEnumerator	ChargingCO()
	{
		float	currentTime = 0f;
		float	interpolant	= 0f;

		float	timeStep = (m_RechargeTime / 3f );

		Transform shieldTransform = m_Shield.transform;

		Vector3 savedShieldScale = shieldTransform.localScale;

		// PHASE 1: Shield scaling down
		while (interpolant < 1f)
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;

			shieldTransform.localScale = Vector3.Lerp(savedShieldScale, Vector3.zero, interpolant);
			yield return null;
		}
		interpolant = currentTime = 0f;

		// PHASE 2: Recharge
		while (interpolant < 1f)
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;
			yield return null;
		}
		interpolant = currentTime = 0f;

		// PHASE 3: Shield scaling up
		while (interpolant < 1f)
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / timeStep;

			shieldTransform.localScale = Vector3.Lerp(Vector3.zero, savedShieldScale, interpolant);
			yield return null;
		}

	//	m_IsRecharging = false;
	//	m_FiredBullets = 0;
	}

}
