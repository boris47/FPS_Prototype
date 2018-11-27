﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker_AI_Behaviour_Seeker : AIBehaviour {
	
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
		float damage = UnityEngine.Random.Range( bullet.DamageMin, bullet.DamageMax );
		this.OnHit( bullet.StartPosition, bullet.WhoRef, damage, bullet.CanPenetrate );
	}

	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		if ( EntityData.EntityRef.IsAlive )
		{
			EntityData.EntityRef.SetPointToLookAt( startPosition );

			EntityData.EntityRef.ChangeState( BrainState.ALARMED );
		}
	}

	public override void OnDestinationReached( Vector3 Destination )
	{
		EntityData.EntityRef.NavReset();

		// Set the point to look just in front ho him
		EntityData.EntityRef.SetPointToLookAt( EntityData.Head_Position + EntityData.EntityRef.transform.forward );

		EntityData.EntityRef.ChangeState( BrainState.NORMAL );
	}

	public override void OnThink()
	{
		
	}

	public override void OnPhysicFrame( float FixedDeltaTime )
	{
		
	}

	public override void OnFrame( float DeltaTime )
	{
		// Update PathFinding and movement along path
		if ( EntityData.EntityRef.HasDestination && EntityData.EntityRef.IsAllignedHeadToPoint )
		{
			EntityData.AgentSpeed = EntityData.EntityRef.MaxAgentSpeed;
		}
	}

	public override void OnPauseSet( bool isPaused )
	{
		
	}

	public override void OnTargetAcquired()
	{
		// Destination
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal:	EntityData.Body_Up,
				planePoint:		EntityData.Body_Position,
				point:			EntityData.TargetInfo.CurrentTarget.Transform.position
			);


			EntityData.EntityRef.RequestMovement( projectedPoint );
		}

		// Switch brain State
		EntityData.EntityRef.ChangeState( BrainState.ATTACKER );
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

