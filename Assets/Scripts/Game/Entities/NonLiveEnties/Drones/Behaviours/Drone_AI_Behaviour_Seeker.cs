
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public	enum InvestigationDirection : uint {
	RIGHT, LEFT, BACK, FRONT, END
}

public class Drone_AI_Behaviour_Seeker : AIBehaviour {

	private	InvestigationDirection		m_CurrentInvestigationDirection = InvestigationDirection.RIGHT;

	public override void OnEnable()
	{
		
	}

	public override void OnDisable()
	{
		
	}

	public override void OnSave( StreamUnit streamUnit )
	{
		streamUnit.SetInternal( BrainState.SEEKER.ToString() + "_CurrentInvestigationDirection", m_CurrentInvestigationDirection );
	}

	public override void OnLoad( StreamUnit streamUnit )
	{
		m_CurrentInvestigationDirection = streamUnit.GetAsEnum<InvestigationDirection>( BrainState.SEEKER.ToString() + "_CurrentInvestigationDirection" );
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
	/*
	protected	void	InnvestigateAroundCO()
	{
		// Set the point to look just in front ho him
		Vector3 pointToLookAt = EntityData.Head_Position + EntityData.EntityRef.transform.forward;
		EntityData.EntityRef.SetPointToLookAt( pointToLookAt, LookTargetMode.HEAD_ONLY );

		EntityData.EntityRef.ChangeState( BrainState.NORMAL );
	}
	*/

	public override void OnLookRotationReached( Vector3 Direction )
	{
		Vector3 newDirection = Vector3.zero;

		Quaternion rotation = Quaternion.AngleAxis( +90.0f, EntityData.Head_Up );

		switch ( m_CurrentInvestigationDirection )
		{
			case InvestigationDirection.RIGHT:	newDirection = EntityData.Head_Right;				m_CurrentInvestigationDirection ++; print("Right");
				break;
			case InvestigationDirection.LEFT:	newDirection = EntityData.Head_Forward * -1f;		m_CurrentInvestigationDirection ++; print("left");
				break;
			case InvestigationDirection.BACK:	newDirection = EntityData.Head_Right * -1f;			m_CurrentInvestigationDirection ++; print("back");
				break;
			case InvestigationDirection.FRONT:	newDirection = EntityData.Head_Right;				m_CurrentInvestigationDirection ++; print("Front");
				break;
			case InvestigationDirection.END:
				EntityData.EntityRef.RequestMovement( EntityData.EntityRef.SpawnPoint );
				EntityData.EntityRef.SetPointToLookAt( EntityData.EntityRef.SpawnPoint + EntityData.EntityRef.SpawnDirection );
				EntityData.EntityRef.ChangeState( BrainState.NORMAL );
				m_CurrentInvestigationDirection = InvestigationDirection.RIGHT;
				return;
		}
		
		Vector3 newDirectionh = rotation * EntityData.Head_Forward;

		EntityData.EntityRef.SetPointToLookAt( EntityData.Head_Position + newDirection, LookTargetMode.HEAD_ONLY );
	}
	
	public override void OnDestinationReached( Vector3 Destination )
	{
		EntityData.EntityRef.NavReset();

		// TODO
		// before returning to normal state should investigate around current position

//		EntityData.EntityRef.StartCoroutine( InnvestigateAroundCO() );

		// Set the point to look just in front ho him
		EntityData.EntityRef.SetPointToLookAt( EntityData.Head_Position + EntityData.EntityRef.transform.forward );

//		EntityData.EntityRef.ChangeState( BrainState.NORMAL );
	}

	public override void OnThink()
	{
		
	}

	public override void OnPhysicFrame( float FixedDeltaTime )
	{
		
	}

	public override void OnFrame( float DeltaTime )
	{
		// Update movement speed along path
		if ( EntityData.EntityRef.HasDestination )
		{
			if ( EntityData.EntityRef.IsAllignedHeadToPoint )
			{
				EntityData.AgentSpeed = EntityData.EntityRef.MaxAgentSpeed;
			}

			if ( EntityData.EntityRef.IsDisallignedHeadWithPoint )
			{
				EntityData.AgentSpeed = 0.0f;
			}
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

