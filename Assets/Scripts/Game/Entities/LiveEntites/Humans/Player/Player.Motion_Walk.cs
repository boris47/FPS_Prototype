﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player {

	private	void	Update_Walk()
	{
		if ( m_Health <= 0.0f )
			return;

		float 	fMove 			= InputManager.Inputs.Forward     ? 1.0f : InputManager.Inputs.Backward   ? -1.0f : 0.0f;
		float 	fStrafe			= InputManager.Inputs.StrafeRight ? 1.0f : InputManager.Inputs.StrafeLeft ? -1.0f : 0.0f;
		bool 	bIsJumping		= InputManager.Inputs.Jump;
		bool 	bSprintInput	= InputManager.Inputs.Run;
		bool	bCrouchInput	= InputManager.Inputs.Crouch;
		bool	bJumpInput		= InputManager.Inputs.Jump;

		float fFinalJump		= 0.0f;
		
		if ( InputManager.HoldCrouch == false )
			m_States.IsCrouched = m_PreviousStates.IsCrouched;
			

		if ( IsGrounded == false )
		{
			if ( m_RigidBody.velocity.y > 0.0f )
				m_States.IsHanging = true;
			else {
				m_States.IsFalling = true;
			}

			/*
			if ( m_PreviousStates.IsHanging && m_States.IsFalling )
				m_LastLandTime = Time.time;


			float iFallTime = Time.time - m_LastLandTime;
			if ( iFallTime > 100 )
				m_Landed = false;
			if ( iFallTime > 400 )
				m_HeavyFall = true;
				*/
			m_Stamina = Mathf.Min( m_Stamina + ( m_StaminaRestore * Time.deltaTime ), 1.0f );

			return;
		}



		////////////////////////////////////////////////////////////////////////////////////////
		// Play step sound when lands over terrain
		{

		}

		   
		////////////////////////////////////////////////////////////////////////////////////////
		// Crouch State
		{  
			// If crouch button is pressed
			if ( bCrouchInput )
			{
				m_States.IsCrouched = !m_PreviousStates.IsCrouched;
				/*
				m_Collider.center = new Vector3
				(
					m_Collider.center.x,
					m_States.IsCrouched ? ( -m_Collider.height * 0.5f ): 0.0f,
					m_Collider.center.z
				);

				m_Collider.height *= m_States.IsCrouched ? 0.5f : 2.0f;
				*/


				// adapt the collider

				/*
				// If is crouched
				if ( m_States.IsCrouched )
				{
					if ( !m_IsUnderSomething )
					{
						CameraControl.Instance.HeadBob.Reset( true );
						m_States.IsCrouched = false;
					}
					else
					{
						if ( bJumpInput ) fFinalJump = m_JumpForce / 2.0f;
						CameraControl.Instance.HeadBob.Reset( true );
						m_States.IsCrouched = false;
					}
				}
				else
				{
					m_States.IsCrouched = true;
				}
				*/
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////
		// Jump
		{
			// If jump button is pressed, has enough space to jump, has stamina requirements and is not dragging an entity
			if ( bJumpInput && !m_IsUnderSomething && ( m_Stamina > m_StaminaJumpMin ) && m_GrabbedObject == null )
			{
				m_Stamina	-= m_JumpStamina;
				fFinalJump	+= m_JumpForce / ( m_States.IsCrouched ? 1.5f : 1.0f );
//				fFinalJump	*= IsInWater() ? 0.8f : 1.0f;
				m_States.IsJumping = true;

			}
		}


		////////////////////////////////////////////////////////////////////////////////////////
		// Run State
		{

			// When stamina reach zero, player became tired
			if ( m_Stamina <= 0.0 ) m_Tiredness = true;

			// When player is tired and stamina overcome a defined value player is no more tired
			if ( m_Tiredness && m_Stamina > m_StaminaRunMin ) m_Tiredness = false;

			// So if run button is pressed, player is not tired and if crouched can stan up
			if ( bSprintInput && !m_Tiredness && ( ( m_States.IsCrouched && !m_IsUnderSomething ) || !m_States.IsCrouched ) ) {

		//		if ( IsDragging() ) DropEntityDragged();

//				CamManager()->GetZoomEffect()->Reset();

				m_States.IsCrouched = false;
				m_States.IsRunning = true;

			}

		}

		float dt = Time.deltaTime;

		////////////////////////////////////////////////////////////////////////////////////////
		// Process inputs
		// If receive input for moving
		if ( ( fMove != 0.0 ) || ( fStrafe != 0.0 ) )
		{
			if ( m_States.IsRunning )
			{
				fMove		*=	m_RunSpeed * ( fMove > 0 ? 1.0f : 0.8f );
				fStrafe		*=	m_RunSpeed * 0.6f;
				m_Stamina	-= m_RunStamina * dt;
			}
			else if ( m_States.IsCrouched )
			{
				fMove		*= m_CrouchSpeed * ( fMove > 0 ? 1.0f : 0.8f );
				fStrafe		*= m_CrouchSpeed * 0.6f;
				m_Stamina	-= m_CrouchStamina * dt;
			}
			else
			{	// walking
				// stamina half restored because we are moving, but just walking
				fMove		*= m_WalkSpeed * ( fMove > 0 ? 1.0f : 0.8f );;
				fStrafe		*= m_WalkSpeed *  0.6f;
				m_Stamina	+= m_StaminaRestore / 2.0f * dt;
				m_States.IsWalking = true;
			}

			if ( m_States.IsJumping == false )
			{
				m_States.IsMoving = true;
			}
		}

		// if don't move stamina regenerates at regular speed
		if ( fMove == 0.0f && fStrafe == 0.0f )
			m_Stamina += m_StaminaRestore * dt;

		// Clamp Stamina between 0.0 and 1.0
		m_Stamina = Mathf.Clamp( m_Stamina, 0.0f, 1.0f );

		// boost movements when Jumping
		if ( m_States.IsJumping ) {
			if ( m_States.IsWalking  )		{ fMove *= m_WalkJumpCoef;	fStrafe *= m_WalkJumpCoef;	fFinalJump *= m_WalkJumpCoef; }
			if ( m_States.IsRunning  )		{ fMove *= m_RunJumpCoef;	fStrafe *= m_RunJumpCoef;	fFinalJump *= m_RunJumpCoef; }
			if ( m_States.IsCrouched )		{ fFinalJump *= m_CrouchJumpCoef; }
		}

		// Apply smoothing on movements
		m_MoveSmooth	= Mathf.Lerp( m_MoveSmooth,   fMove,   dt * 20f );
		m_StrafeSmooth	= Mathf.Lerp( m_StrafeSmooth, fStrafe, dt * 10f );


		// calculate camera relative direction to move:
		{
			Vector3 vCamForward = Vector3.Scale( CameraControl.Instance.transform.forward, new Vector3( 1.0f, 0.0f, 1.0f ) ).normalized;
			m_Move = ( m_MoveSmooth * vCamForward ) + ( m_StrafeSmooth * CameraControl.Instance.transform.right );
		}

		// This prevents "speed hack" strafing
		if ( ( fStrafe != 0.0f ) && ( fMove != 0.0f  ) )
		{
			m_Move *= 0.707f;
		}

		m_Move.y = m_RigidBody.velocity.y;

		// Apply ground speed modifier
		m_Move *= GroundSpeedModifier;

		// Add jump force
		if ( bIsJumping && IsGrounded )
			m_RigidBody.velocity = m_RigidBody.velocity + Vector3.up * fFinalJump;

		m_RigidBody.velocity = new Vector3(  m_Move.x, m_RigidBody.velocity.y, m_Move.z );

		// Update internal time value
		// Used for timed operation such as high jump or other things
		m_LastLandTime = Time.time;

	}

}
