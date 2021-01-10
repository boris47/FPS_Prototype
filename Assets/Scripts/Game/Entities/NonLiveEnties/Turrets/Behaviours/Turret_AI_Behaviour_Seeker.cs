
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_AI_Behaviour_Seeker : AIBehaviour {

	public override void OnEnable()
	{
		
	}

	public override void OnDisable()
	{
		
	}

	public override void OnSave( StreamUnit streamUnit )
	{
		
	}

	public override void OnLoad( StreamUnit streamUnit )
	{
		
	}

	public override void OnHit( IBullet bullet )
	{
		OnHit( bullet.StartPosition, bullet.WhoRef, bullet.Damage, bullet.CanPenetrate );
	}

	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		if (EntityData.EntityRef.IsAlive )
		{
			EntityData.EntityRef.SetPointToLookAt( startPosition );

			EntityData.EntityRef.ChangeState( EBrainState.ALARMED );
		}
	}

	public override void OnDestinationReached( Vector3 Destination )
	{
		
	}

	public override void OnThink()
	{
		
	}

	public override void OnPhysicFrame( float FixedDeltaTime )
	{
		
	}

	public override void OnFrame( float DeltaTime )
	{
		
	}

	public override void OnPauseSet( bool isPaused )
	{
		
	}

	public override void OnTargetAcquired()
	{
		// Switch brain State
		EntityData.EntityRef.ChangeState( EBrainState.ATTACKER );
	}

	public override void OnTargetChange()
	{
		
	}

	public override void OnTargetLost()
	{

	}

	public override void OnKilled()
	{
		
	}

}

