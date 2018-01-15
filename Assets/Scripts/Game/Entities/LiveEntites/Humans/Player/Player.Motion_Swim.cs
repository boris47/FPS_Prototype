using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player {
	
	private	void	Update_Swim() {

		if ( m_Health <= 0.0f )
			return;

		float   fMove			= Inputmanager.Inputs.Forward     ? 1.0f : Inputmanager.Inputs.Backward   ? -1.0f : 0.0f;
		float   fStrafe			= Inputmanager.Inputs.StrafeRight ? 1.0f : Inputmanager.Inputs.StrafeLeft ? -1.0f : 0.0f;
		float   fUpDown			= Inputmanager.Inputs.Jump        ? 1.0f : Inputmanager.Inputs.Crouch     ? -1.0f : 0.0f;
		bool	bRunButtonHolden= Inputmanager.Inputs.Run;

		float   fSpeed			= Inputmanager.Inputs.Run ? m_RunSpeed : m_WalkSpeed;

		// States.bIsMoving can disable HeadMove in favor of HeadBob, that is disabled while swimming
		bool bIsMoving = false;
		if ( ( fMove != 0.0 ) || ( fStrafe != 0.0 ) || ( fUpDown != 0.0 ) )
			bIsMoving = true;

		// vertical force is relative to x local axis rotation of camera
		float fVertical = ( CameraControl.Instance.transform.rotation.eulerAngles.x / 80.0f );
		// calculate direction
		fVertical *= -fMove;
		// fMove is relative to vertical direction
		fMove *= 1.0f - Mathf.Abs( fVertical );
		// Add inputs from jump - crouch controls
		fVertical += fUpDown;
		// set main speed
		fVertical *= m_WalkSpeed;

		fMove	*= ( bRunButtonHolden ? m_RunSpeed : m_WalkSpeed ) * ( fMove > 0 ? 1.0f : 0.8f );
		fStrafe	*= ( bRunButtonHolden ? m_RunSpeed : m_WalkSpeed ) * 0.6f;

		// Apply smoothing on movements
		m_MoveSmooth	= Mathf.Lerp( m_MoveSmooth,	  fMove,   Time.deltaTime * 20f );
		m_StrafeSmooth	= Mathf.Lerp( m_StrafeSmooth, fStrafe, Time.deltaTime * 10f );

		if ( bIsMoving )
		// calculate camera relative direction to move:
		{
			Vector3 vCamForward = Vector3.Scale( CameraControl.Instance.transform.forward, new Vector3( 1.0f, 0.0f, 1.0f ) ).normalized;
			m_Move = ( m_MoveSmooth * vCamForward ) + ( m_StrafeSmooth * CameraControl.Instance.transform.right );
			m_Move = transform.InverseTransformDirection( m_Move );
		}

		// This prevents "speed hack" strafing
		if ( ( fStrafe != 0.0f ) && ( fMove != 0.0f ) )
		{
			m_Move *= 0.707f;
		}

		m_Move.y = m_RigidBody.velocity.y;

		m_RigidBody.velocity =  Vector3.Lerp( m_RigidBody.velocity, m_Move, Time.deltaTime * 50f );
	
		m_VerticalSmooth = Mathf.Lerp( fVertical, m_RigidBody.velocity.y + m_VerticalSmooth, bRunButtonHolden ? 10.0f : 30.0f );

		// Stamina regenerates at half speed if using sprint otherwise at regular speed
		m_Stamina += ( bRunButtonHolden ? m_StaminaRestore / 2.0f : m_StaminaRestore );

		// Clamp Stamina between 0.0 and 1.0
		m_Stamina = Mathf.Clamp( m_Stamina, 0.0f, 1.0f );

	}

}
