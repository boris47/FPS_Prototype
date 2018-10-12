
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Behaviours {

	public class Walker_AI_Beaviour_Alarmed : Behaviour_Alarmed {

		protected		float				m_BodyRotationSpeed			= 5.0f;
		protected       float               m_HeadRotationSpeed			= 5.0f;
		protected		float				m_GunRotationSpeed			= 5.0f;

		// Transforms
		protected		Transform			m_HeadTransform				= null;
		protected		Transform			m_BodyTransform				= null;
		protected		Transform			m_GunTransform				= null;
		protected		Transform			m_FirePoint					= null;

		protected	bool						m_HasLookAtObject				= false;
		protected	Vector3						m_PointToFace					= Vector3.zero;
		protected	Transform					m_TrasformToLookAt				= null;
		protected	Quaternion					m_RotationToAllignTo			= Quaternion.identity;

		protected		GameObjectsPool<Bullet> m_Pool					= null;

		// Flag set if body of entity is aligned with target
		protected	bool						m_IsAllignedBodyToPoint			= false;

		// Flag set if head of entity is aligned with target
		protected	bool						m_IsAllignedHeadToPoint			= false;

		// Flag set if gun of entity is aligned with target
		protected   bool                        m_IsAllignedGunToPoint			= false;

		public Walker_AI_Beaviour_Alarmed()
		{
			
		}

		public	override	void	Setup( Brain brain, IEntity ThisEntity, BehaviourSetupData Data )
		{
			base.Setup( brain, ThisEntity, null );

			m_Pool = Data.BulletstPool;
		}


		public	override	void	Enable()
		{
			print( "Switched to Walker_AI_Beaviour_Alarmed" );
		}


		public	override	void	Disable()
		{
			
		}


		public	override	void	OnThink()
		{
			
		}


		public	override	void	OnPhysicFrame( float FixedDeltaTime )
		{
			
		}


		public	override	void	OnFrame( float DeltaTime )
		{
			// if has point to face, update entity orientation
			if ( m_HasLookAtObject )
			{
				FaceToPoint( DeltaTime );   // m_PointToFace
			}
		}


		public	override	void	OnSave( StreamUnit streamUnit )
		{

			return;
		}


		public	override	void	OnLoad( StreamUnit streamUnit )
		{

			return;
		}


		protected	void		FaceToPoint( float DeltaTime )
		{
			// ORIENTATION
			// BODY
			{
				// Nothing, rotation not allowed here
			}
			// HEAD
			{
				Vector3 pointOnThisPlane = Utils.Math.ProjectPointOnPlane( m_BodyTransform.up, m_HeadTransform.position, m_PointToFace );
				Vector3 dirToPosition = ( pointOnThisPlane - m_HeadTransform.position );

				m_IsAllignedHeadToPoint = Vector3.Angle( m_HeadTransform.forward, dirToPosition ) < 12f;
				{
					float speed = m_HeadRotationSpeed * ( ( m_ThisEntity.TargetInfo.HasTarget ) ? 3.0f : 1.0f );

					m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
					m_HeadTransform.rotation = Quaternion.RotateTowards( m_HeadTransform.rotation, m_RotationToAllignTo, speed * DeltaTime );
				}
			}
		
			// GUN
			{
				Vector3 pointToLookAt = m_PointToFace;
				if ( m_ThisEntity.TargetInfo.HasTarget == true ) // PREDICTION
				{
					// Vector3 shooterPosition, Vector3 shooterVelocity, float shotSpeed, Vector3 targetPosition, Vector3 targetVelocity
					pointToLookAt = Utils.Math.CalculateBulletPrediction
					(
						shooterPosition:	m_GunTransform.position,
						shooterVelocity:	m_ThisEntity.NavAgent.velocity,
						shotSpeed:			m_Pool.GetAsModel().Velocity,
						targetPosition:		m_ThisEntity.TargetInfo.CurrentTarget.Transform.position,
						targetVelocity:		m_ThisEntity.TargetInfo.CurrentTarget.RigidBody.velocity
					);
				}

				Vector3 dirToPosition = ( pointToLookAt - m_GunTransform.position );
				if ( m_IsAllignedHeadToPoint == true )
				{
					m_RotationToAllignTo.SetLookRotation( dirToPosition, m_BodyTransform.up );
					m_GunTransform.rotation = Quaternion.RotateTowards( m_GunTransform.rotation, m_RotationToAllignTo, m_GunRotationSpeed * DeltaTime );
				}
				m_IsAllignedGunToPoint = Vector3.Angle( m_GunTransform.forward, dirToPosition ) < 16f;
			}
		}

	}

}
