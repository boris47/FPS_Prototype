using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LiveEntity {

	public enum eMotionType {
		None		= 1 << 0,
		Walking		= 1 << 1,
		Flying		= 1 << 2,
		Swimming	= 1 << 3,
		P1ToP2		= 1 << 4
	};

	// This variable control which physic to use on entity
	protected	eMotionType		m_MotionType		= eMotionType.Walking;
	public		eMotionType	MotionType {
		get { return m_MotionType; }
		set { SetMotionType( value ); }
	}


	protected	eMotionType	m_PrevMotionType		= eMotionType.Walking;
	public		eMotionType	PrevMotionType {
		get { return m_PrevMotionType; }
	}



	public		void					SetMotionType( eMotionType i ) {

		m_PrevMotionType = m_MotionType;
		switch( i ) {

			case eMotionType.Walking:	{ this.SetPlainWalking(); return; }
			case eMotionType.Flying:	{ this.SetFlying(); return; }
			case eMotionType.Swimming:	{ this.SetSwimming(); return; }
			case eMotionType.P1ToP2:	{ this.SetClimbing(); return; }
		}

	}


	public		void					SetPlainWalking() {

		if ( m_MotionType == eMotionType.Walking ) return;

		m_PrevMotionType = m_MotionType;
		m_MotionType	 = eMotionType.Walking;

		m_States.Reset();

		if ( m_RigidBody != null )
			m_RigidBody.useGravity = true;

		Inputmanager.HoldCrouch = false;
		Inputmanager.HoldJump	= false;
		Inputmanager.HoldRun = true;

	}

	public		void					SetFlying() {

		if ( m_MotionType == eMotionType.Flying ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType	 = eMotionType.Flying;


		m_States.Reset();

		if ( m_RigidBody != null )
			m_RigidBody.useGravity = false;

		Inputmanager.HoldCrouch = true;
		Inputmanager.HoldJump = true;
		Inputmanager.HoldRun = true;

	}

	public		void					SetSwimming() {

		if ( m_MotionType == eMotionType.Swimming ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType	 = eMotionType.Swimming;

		m_States.Reset();

		if ( m_RigidBody != null )
			m_RigidBody.useGravity = false;

		Inputmanager.HoldCrouch = true;
		Inputmanager.HoldJump = true;
		Inputmanager.HoldRun = true;

	}

	public		void					SetClimbing() {

		if ( m_MotionType == eMotionType.P1ToP2 ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = eMotionType.P1ToP2;

		bool b = IsCrouched;
		m_States.Reset();
		m_States.IsCrouched = b;


		if ( m_RigidBody != null )
			m_RigidBody.useGravity = false;

		Inputmanager.HoldCrouch = false;
		Inputmanager.HoldJump = false;
		Inputmanager.HoldRun = true;

	}






	



}
