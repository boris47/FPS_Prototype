
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Walker {

	protected partial class Walker_AI_Beaviour_Alarmed {

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

			// if has point to face, update entity orientation
			if ( m_ThisEntity.m_HasLookAtObject )
			{
				m_ThisEntity.FaceToPoint( DeltaTime );
			}

			// Update PathFinding and movement along path
			if ( m_ThisEntity.m_HasDestination && m_ThisEntity.m_IsAllignedHeadToPoint )
			{
				float agentFinalSpeed = 0.0f;
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, m_ThisEntity.m_PointToFace );

				agentFinalSpeed = m_ThisEntity.m_MoveMaxSpeed;

				m_ThisEntity.m_NavAgent.speed = agentFinalSpeed;
			}
		}

		public		override	void		OnPauseSet( bool isPaused )
		{
			
		}

		public		override	void		OnTargetAcquired( TargetInfo_t targetInfo )
		{
			 // PathFinding
			Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( m_ThisEntity.m_BodyTransform.up, m_ThisEntity.m_BodyTransform.position, m_ThisEntity.m_TargetInfo.CurrentTarget.Transform.position );
			m_ThisEntity.RequestMovement( projectedPoint );

			// Switch brain State
			m_ThisEntity.ChangeState( BrainState.ATTACKER );
		}

		public		override	void		OnTargetUpdate( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetChange( TargetInfo_t targetInfo )
		{
			
		}
		
		public		override	void		OnTargetLost( TargetInfo_t targetInfo )
		{
			
		}

		public		override	void		OnDestinationReached( Vector3 Destination )
		{
			m_ThisEntity.NavReset();
		}

		public		override	void		OnKilled()
		{
			
		}

	}

}
