
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone_AI_Behaviour_Normal : AIBehaviour {

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
		if (this.EntityData.EntityRef.IsAlive )
		{
			this.EntityData.EntityRef.SetPointToLookAt( startPosition );

			this.EntityData.EntityRef.ChangeState( EBrainState.ALARMED );
		}
	}

	public override void OnDestinationReached( Vector3 Destination )
	{
		this.EntityData.EntityRef.NavReset();
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
		if (this.EntityData.EntityRef.HasDestination && this.EntityData.EntityRef.IsAllignedHeadToPoint )
		{
			this.EntityData.AgentSpeed = this.EntityData.EntityRef.MaxAgentSpeed;
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
				planeNormal: this.EntityData.Body_Up,
				planePoint: this.EntityData.Body_Position,
				point: this.EntityData.TargetInfo.CurrentTarget.AsEntity.transform.position
			);


			this.EntityData.EntityRef.RequestMovement( projectedPoint );
		}

		// Switch brain State
		this.EntityData.EntityRef.ChangeState( EBrainState.ATTACKER );
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

