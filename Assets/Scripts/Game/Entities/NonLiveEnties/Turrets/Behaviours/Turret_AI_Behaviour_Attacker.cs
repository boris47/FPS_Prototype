﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret_AI_Behaviour_Attacker : AIBehaviour {

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
		if (EntityData.EntityRef.IsAlive && whoRef.IsAlive && EntityData.TargetInfo.CurrentTarget.ID == whoRef.AsInterface.ID )
		{
			EntityData.EntityRef.SetPointToLookAt( startPosition );
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
		// Update targeting
		if (EntityData.TargetInfo.HasTarget == true )
		{
			EntityData.EntityRef.SetPointToLookAt(EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position );

			// with a target, if gun alligned, fire
			if (EntityData.EntityRef.CanFire() == true )
			{
				EntityData.EntityRef.FireLongRange();
			}
		}
	}

	public override void OnPauseSet( bool isPaused )
	{
		
	}

	public override void OnTargetAcquired()
	{
		
	}

	public override void OnTargetChange()
	{
		
	}

	public override void OnTargetLost()
	{
		// Orientation
		{
			Vector3 newPointToLookAt = EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position + EntityData.TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Head_Position,
				point:			newPointToLookAt
			);

			EntityData.EntityRef.SetPointToLookAt( projectedPoint );
		}

		// TODO Set brain to SEKKER mode
		EntityData.EntityRef.ChangeState( EBrainState.SEEKER );
	}

	public override void OnKilled()
	{
		
	}

}

