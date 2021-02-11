
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone_AI_Behaviour_Attacker : AIBehaviour {

	public override void OnEnable()
	{
		if (EntityData.TargetInfo.HasTarget == false )
		{
			print( "Behaviour: Drone_AI_Behaviour_Attacker, Entity: " + EntityData.EntityRef.name + " Enabled without target\nGoing to ALARMED state" );
			EntityData.EntityRef.ChangeState( EBrainState.ALARMED );
		}
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
		Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
			planeNormal: EntityData.Body_Up,
			planePoint: EntityData.Body_Position,
			point: EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
		);


		bool IsNotUnderEngageDistance = (EntityData.Transform_Position - projectedPoint ).sqrMagnitude > Mathf.Pow(EntityData.EntityRef.MinEngageDistance, 2.0f);
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
//		if ( EntityData.TargetInfo.HasTarget == true )
		{
			EntityData.EntityRef.SetPointToLookAt(EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position );

			// with a target, if gun alligned, fire
			if (EntityData.EntityRef.CanFire() == true )
			{
				EntityData.EntityRef.FireWeapon();
			}
		}

		// Update PathFinding and movement along path
		if (EntityData.EntityRef.HasDestination && EntityData.EntityRef.IsAllignedHeadToPoint )
		{
			float agentFinalSpeed = 0.0f;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Body_Position,
				point: EntityData.LookData.PointToLookAt
			);

			bool IsNotUnderEngageDistance = (EntityData.Transform_Position - projectedPoint ).sqrMagnitude > EntityData.EntityRef.MinEngageDistance * EntityData.EntityRef.MinEngageDistance;

//			if ( EntityData.TargetInfo.HasTarget == true )
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
			/*			else
						{
							agentFinalSpeed = EntityData.EntityRef.MaxAgentSpeed;
						}
			*/
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
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Body_Position,
				point: EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
			);


			EntityData.EntityRef.RequestMovement( projectedPoint );
		}

		// Orientation
		{
			Vector3 newPointToLookAt = EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position + EntityData.TargetInfo.CurrentTarget.RigidBody.velocity.normalized;
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( 
				planeNormal: EntityData.Body_Up,
				planePoint: EntityData.Body_Position,
				point:			newPointToLookAt
			);

			EntityData.EntityRef.SetPointToLookAt( projectedPoint );
		}

		EntityData.EntityRef.ChangeState( EBrainState.SEEKER );
	}

	public override void OnKilled()
	{
		
	}

}
