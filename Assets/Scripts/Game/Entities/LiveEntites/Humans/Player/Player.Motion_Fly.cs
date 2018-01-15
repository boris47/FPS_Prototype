using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class Player {
	
	private	void	Update_Fly() {
		
		if ( m_Health <= 0.0f ) return;

		float 	fMove 			= Inputmanager.Inputs.Forward     ? 1.0f : Inputmanager.Inputs.Backward   ? -1.0f : 0.0f;
		float 	fStrafe			= Inputmanager.Inputs.StrafeRight ? 1.0f : Inputmanager.Inputs.StrafeLeft ? -1.0f : 0.0f;
		float	fVertical		= Inputmanager.Inputs.Jump		  ? 1.0f : Inputmanager.Inputs.Crouch	  ? -1.0f : 0.0f;

		float	fSpeed			= Inputmanager.Inputs.Run ? m_RunSpeed : m_WalkSpeed;

		float	fDirMul			= ( fMove > 0 ) ? 1.0f : 0.8f;

		fMove		*= fSpeed * fDirMul;
		fStrafe		*= fSpeed * 0.6f;
		fVertical	*= fSpeed * 0.1f;

		m_MoveSmooth = m_StrafeSmooth = 0.0f;

		// calculate camera relative direction to move:
		{
			m_Move = ( m_MoveSmooth * CameraControl.Instance.transform.forward ) + ( m_StrafeSmooth * CameraControl.Instance.transform.right );
		}

		// This prevents "speed hack" strafing
		if ( ( fStrafe != 0.0f ) && ( fMove != 0.0f  ) ) {
			m_Move *= 0.707f;
		}

		m_RigidBody.velocity = Vector3.Lerp ( m_RigidBody.velocity, new Vector3(  m_Move.x, m_RigidBody.velocity.y, m_Move.z ), Time.deltaTime * 50f );

		m_RigidBody.drag = 2.0f;



		m_RigidBody.AddForce( 0.0f, fVertical, 0.0f );

		// adding floating force
		m_RigidBody.AddForce( 0.0f, 0.01f, 0.0f );
		// TODO  this is to clamp into max and 0.0f values


		// Stamina regenerates at regular speed
		m_Stamina += m_StaminaRestore;

	}

}
