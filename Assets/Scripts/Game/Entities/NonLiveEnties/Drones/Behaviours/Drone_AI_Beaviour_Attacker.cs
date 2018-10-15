
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Drone {

	protected partial class Drone_AI_Beaviour_Attacker {

		public		override	StreamUnit	OnSave( StreamData streamData )
		{
			return null;
		}

		public		override	StreamUnit	OnLoad( StreamData streamData )
		{
			return null;
		}

		public		override	void		OnHit( IBullet bullet )
		{
			// Avoid friendly fire
			if ( bullet.WhoRef is NonLiveEntity )
				return;

			Debug.Log( "hitted with bullet" );

			float damage = UnityEngine.Random.Range( bullet.DamageMin, bullet.DamageMax );
			this.OnHit( bullet.StartPosition, bullet.WhoRef, damage, bullet.CanPenetrate );
		}

		public		override	void		OnHit( Vector3 startPosition, Entity whoRef, float damage, bool canPenetrate = false )
		{
			Debug.Log( "hitted with details" );

			// Hit event, set ALARMED State
			if ( m_ThisEntity.m_Health > 0.0f )
			{
				m_ThisEntity.TakeDamage( damage );

				m_ThisEntity.SetPointToLookAt( startPosition );
			}
		}

		public		override	void		OnThink()
		{
			
		}

		public		override	void		OnPhysicFrame( float FixedDeltaTime )
		{
			
		}

		public		override	void		OnFrame( float DeltaTime )
		{
			// Update internal timer
			m_ThisEntity.m_ShotTimer -= DeltaTime;

			// Update targeting
			if ( m_ThisEntity.m_TargetInfo.HasTarget == true )
			{
				m_ThisEntity.SetPointToLookAt( m_ThisEntity.m_TargetInfo.CurrentTarget.Transform.position );

				// with a target, if gun alligned, fire
				if ( m_ThisEntity.m_IsAllignedGunToPoint == true )
				{
					m_ThisEntity.FireLongRange( DeltaTime );
				}
			}

			// if has point to face, update entity orientation
			if ( m_ThisEntity.m_HasLookAtObject )
			{
				m_ThisEntity.FaceToPoint( DeltaTime );   // m_PointToFace
			}


			m_ThisEntity.m_NavCanMoveAlongPath = false;

			// Update PathFinding and movement along path
			if ( m_ThisEntity.m_HasDestination && m_ThisEntity.m_IsAllignedHeadToPoint )
			{
				float agentFinalSpeed = 0.0f;
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, m_ThisEntity.m_PointToFace );
				bool IsNotUnderEngageDistance = ( m_ThisEntity.transform.position - projectedPoint ).sqrMagnitude > m_ThisEntity.m_MinEngageDistance * m_ThisEntity.m_MinEngageDistance;
				if ( m_ThisEntity.m_TargetInfo.HasTarget )
				{
					if ( IsNotUnderEngageDistance )
					{
						agentFinalSpeed = m_ThisEntity.m_MoveMaxSpeed;
					}
					else
					{
						agentFinalSpeed = 0.0f;
					}
				}
				else
				{
					agentFinalSpeed = m_ThisEntity.m_MoveMaxSpeed;
				}

				m_ThisEntity.m_NavAgent.speed = agentFinalSpeed;
			}
		}

		public		override	void		OnPauseSet( bool isPaused )
		{
			
		}

		public		override	void		OnTargetAcquired( TargetInfo_t targetInfo )
		{
			
		}

		public		override	void		OnTargetUpdate( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetChange( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetLost( TargetInfo_t targetInfo )
		{
			
			// SEEKING MODE

			// TODO Set brain to SEKKER mode
			m_ThisEntity.ChangeState( BrainState.SEEKER );

			// Destination
			{
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, targetInfo.CurrentTarget.Transform.position );
				m_ThisEntity.RequestMovement( projectedPoint );
			}

			// Orientation
			{
				Vector3 newPointToLookAt = targetInfo.CurrentTarget.Transform.position + targetInfo.CurrentTarget.RigidBody.velocity.normalized;
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, newPointToLookAt );
				m_ThisEntity.SetPointToLookAt( projectedPoint );
			}
		
		}

		public		override	void		OnDestinationReached( Vector3 Destination )
		{
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, m_ThisEntity.m_TargetInfo.CurrentTarget.Transform.position );
			if ( ( m_ThisEntity.transform.position - projectedPoint ).sqrMagnitude > m_ThisEntity.m_MinEngageDistance * m_ThisEntity.m_MinEngageDistance )
			{
				m_ThisEntity.RequestMovement( projectedPoint );
			}
		}

		public		override	void		OnKilled()
		{
			
		}

	}

}
