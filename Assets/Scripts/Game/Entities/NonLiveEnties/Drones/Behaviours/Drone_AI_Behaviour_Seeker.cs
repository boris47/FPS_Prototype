
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public	enum EInvestigationDirection : uint
{
	RIGHT, LEFT, BACK, FRONT, END
}

public class Drone_AI_Behaviour_Seeker : AIBehaviour {

	private	EInvestigationDirection		m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;

	private	Coroutine					m_InvestigationCO = null;

	public override void OnEnable()
	{
/*		if ( EntityData.EntityRef.HasDestination == false && m_InvestigationCO == null )
		{
			m_CurrentInvestigationDirection = InvestigationDirection.RIGHT;
			m_InvestigationCO = EntityData.EntityRef.StartCoroutine( InvestigateAroundCO() );
		}
*/
	}

	public override void OnDisable()
	{
		if (this.m_InvestigationCO != null )
		{
			this.EntityData.EntityRef.StopCoroutine(this.m_InvestigationCO );
		}

		this.m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
	}

	public override void OnSave( StreamUnit streamUnit )
	{
		streamUnit.SetInternal( EBrainState.SEEKER.ToString() + "_CurrentInvestigationDirection", this.m_CurrentInvestigationDirection );
	}

	public override void OnLoad( StreamUnit streamUnit )
	{
		this.m_CurrentInvestigationDirection = streamUnit.GetAsEnum<EInvestigationDirection>( EBrainState.SEEKER.ToString() + "_CurrentInvestigationDirection" );
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

	protected	IEnumerator	InvestigateAroundCO()
	{
		while(this.m_CurrentInvestigationDirection < EInvestigationDirection.END )
		{
			yield return new WaitForSecondsRealtime( UnityEngine.Random.Range( 0.7f, 1.3f ) );
			Vector3 newDirection = Vector3.zero;
			switch (this.m_CurrentInvestigationDirection )
			{
				case EInvestigationDirection.RIGHT:	newDirection = this.EntityData.Head_Right; this.print("Right"); break;
				case EInvestigationDirection.LEFT:	newDirection = this.EntityData.Head_Forward * -1f; this.print("left");  break;
				case EInvestigationDirection.BACK:	newDirection = this.EntityData.Head_Right   * -1f; this.print("back");  break;
				case EInvestigationDirection.FRONT:	newDirection = this.EntityData.Head_Forward * -1f; this.print("Front"); break;
			}

			this.m_CurrentInvestigationDirection ++;
			this.EntityData.EntityRef.SetPointToLookAt(this.EntityData.Head_Position + newDirection, ELookTargetMode.HEAD_ONLY );
			yield return new WaitUntil( () => Vector3.Angle(this.EntityData.Head_Forward, newDirection ) < 4.5f );
		}

		this.EntityData.EntityRef.RequestMovement(this.EntityData.EntityRef.SpawnPoint );
		this.EntityData.EntityRef.SetPointToLookAt(this.EntityData.EntityRef.SpawnPoint + this.EntityData.EntityRef.SpawnDirection );
		this.EntityData.EntityRef.ChangeState( EBrainState.NORMAL );
		this.m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
		this.m_InvestigationCO = null;
	}
	

	public override void OnLookRotationReached( Vector3 Direction )
	{
		
	}
	
	public override void OnDestinationReached( Vector3 Destination )
	{
		this.EntityData.EntityRef.NavReset();

		// TODO
		// before returning to normal state should investigate around current position

		if (this.m_InvestigationCO == null )
		{
			this.m_CurrentInvestigationDirection = EInvestigationDirection.RIGHT;
			this.m_InvestigationCO = CoroutinesManager.Start(this.InvestigateAroundCO(), "Drone-Seeker::On DestinationReached: Start of search" );
		}

//		EntityData.EntityRef.StartCoroutine( InnvestigateAroundCO() );

		// Set the point to look just in front ho him
//		EntityData.EntityRef.SetPointToLookAt( EntityData.Head_Position + EntityData.EntityRef.transform.forward );

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
		Debug.DrawLine(this.EntityData.Head_Position, this.EntityData.Head_Position + this.EntityData.Head_Forward, Color.red, 0.0f );

		// Update movement speed along path
		if (this.EntityData.EntityRef.HasDestination )
		{
			if (this.EntityData.EntityRef.IsAllignedHeadToPoint )
			{
				this.EntityData.AgentSpeed = this.EntityData.EntityRef.MaxAgentSpeed;
			}

			if (this.EntityData.EntityRef.IsDisallignedHeadWithPoint )
			{
				this.EntityData.AgentSpeed = 0.0f;
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

