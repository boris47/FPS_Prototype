
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker_AI_Behaviour_Attacker : AIBehaviour {
	
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
		this.OnHit( bullet.StartPosition, bullet.WhoRef, bullet.Damage, bullet.CanPenetrate );
	}

	public override void OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
	{
		if (this.EntityData.EntityRef.IsAlive && whoRef.IsAlive && this.EntityData.TargetInfo.CurrentTarget.ID == whoRef.AsInterface.ID )
		{
			this.EntityData.EntityRef.SetPointToLookAt( startPosition );
		}
	}

	public override void OnDestinationReached( Vector3 Destination )
	{
		Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
			planeNormal: this.EntityData.Body_Up,
			planePoint: this.EntityData.Body_Position,
			point: this.EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
		);


		bool IsNotUnderEngageDistance = (this.EntityData.Transform_Position - projectedPoint ).sqrMagnitude > this.EntityData.EntityRef.MinEngageDistance * this.EntityData.EntityRef.MinEngageDistance;
		if ( IsNotUnderEngageDistance )
		{
			this.EntityData.EntityRef.RequestMovement( projectedPoint );
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
		if (this.EntityData.TargetInfo.HasTarget == true )
		{
			this.EntityData.EntityRef.SetPointToLookAt(this.EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position );

			// with a target, if gun alligned, fire
			if (this.EntityData.EntityRef.CanFire() == true )
			{
				this.EntityData.EntityRef.FireLongRange();
			}
		}

		// Update PathFinding and movement along path
		if (this.EntityData.EntityRef.HasDestination && this.EntityData.EntityRef.IsAllignedHeadToPoint )
		{
			float agentFinalSpeed = 0.0f;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: this.EntityData.Body_Up,
				planePoint: this.EntityData.Body_Position,
				point: this.EntityData.LookData.PointToLookAt
			);

			bool IsNotUnderEngageDistance = (this.EntityData.Transform_Position - projectedPoint ).sqrMagnitude > this.EntityData.EntityRef.MinEngageDistance * this.EntityData.EntityRef.MinEngageDistance;

			if (this.EntityData.TargetInfo.HasTarget == true )
			{
				if ( IsNotUnderEngageDistance )
				{
					agentFinalSpeed = this.EntityData.EntityRef.MaxAgentSpeed;
				}
				else
				{
					agentFinalSpeed = 0.0f;
				}
			}
			else
			{
				agentFinalSpeed = this.EntityData.EntityRef.MaxAgentSpeed;
			}

			this.EntityData.AgentSpeed = agentFinalSpeed;
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
				planeNormal: this.EntityData.Body_Up,
				planePoint: this.EntityData.Body_Position,
				point: this.EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
			);


			this.EntityData.EntityRef.RequestMovement( projectedPoint );
		}

		// Orientation
		{
			Vector3 newPointToLookAt = this.EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position + this.EntityData.TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: this.EntityData.Body_Up,
				planePoint: this.EntityData.Head_Position,
				point:			newPointToLookAt
			);

			this.EntityData.EntityRef.SetPointToLookAt( projectedPoint );
		}

		// TODO Set brain to SEKKER mode
		this.EntityData.EntityRef.ChangeState( EBrainState.SEEKER );
	}

	public override void OnKilled()
	{
		
	}

}
