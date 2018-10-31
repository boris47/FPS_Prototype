﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker_AI_Behaviour_Attacker : AIBehaviour {
	



	public override StreamUnit OnSave( StreamData streamData )
	{
		return null;
	}

	public override StreamUnit OnLoad( StreamData streamData )
	{
		return null;
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
		}
	}

	public override void OnDestinationReached( Vector3 Destination )
	{
		Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
			planeNormal:	EntityData.BodyTransform.up,
			planePoint:		EntityData.BodyTransform.position,
			point:			EntityData.TargetInfo.CurrentTarget.Transform.position
		);


		bool IsNotUnderEngageDistance = ( EntityData.Transform.position - projectedPoint ).sqrMagnitude > EntityData.EntityRef.MinEngageDistance * EntityData.EntityRef.MinEngageDistance;
		if ( IsNotUnderEngageDistance )
		{
			EntityData.EntityRef.RequestMovement( projectedPoint );
		}
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
		if ( EntityData.TargetInfo.HasTarget == true )
		{
			EntityData.EntityRef.SetPointToLookAt( EntityData.TargetInfo.CurrentTarget.Transform.position );

			// with a target, if gun alligned, fire
			if ( EntityData.EntityRef.CanFire() == true )
			{
				EntityData.EntityRef.FireLongRange();
			}
		}

		// Update PathFinding and movement along path
		if ( EntityData.EntityRef.HasDestination && EntityData.EntityRef.IsAllignedHeadToPoint )
		{
			float agentFinalSpeed = 0.0f;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal:	EntityData.BodyTransform.up,
				planePoint:		EntityData.BodyTransform.position,
				point:			EntityData.PointToLookAt
			);

			bool IsNotUnderEngageDistance = ( EntityData.Transform.position - projectedPoint ).sqrMagnitude > EntityData.EntityRef.MinEngageDistance * EntityData.EntityRef.MinEngageDistance;

			if ( EntityData.TargetInfo.HasTarget == true )
			{
				if ( IsNotUnderEngageDistance )
				{
					agentFinalSpeed = EntityData.EntityRef.MaxAgentSpeed;
				}
				else
				{
					agentFinalSpeed = 0.0f;
				}
			}
			else
			{
				agentFinalSpeed = EntityData.EntityRef.MaxAgentSpeed;
			}

			EntityData.AgentSpeed = agentFinalSpeed;
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
		// SEEKING MODE

		// Destination
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal:	EntityData.BodyTransform.up,
				planePoint:		EntityData.BodyTransform.position,
				point:			EntityData.TargetInfo.CurrentTarget.Transform.position
			);


			EntityData.EntityRef.RequestMovement( projectedPoint );
		}

		// Orientation
		{
			Vector3 newPointToLookAt = EntityData.TargetInfo.CurrentTarget.Transform.position + EntityData.TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal:	EntityData.BodyTransform.up,
				planePoint:		EntityData.HeadTransform.position,
				point:			newPointToLookAt
			);

			EntityData.EntityRef.SetPointToLookAt( projectedPoint );
		}

		// TODO Set brain to SEKKER mode
		EntityData.EntityRef.ChangeState( BrainState.SEEKER );
	}

	public override void OnKilled()
	{
		
	}

}