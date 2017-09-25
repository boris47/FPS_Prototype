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
		m_MotionType = eMotionType.Walking;

//		this.ClearState();

//		pEntity->SetGravityMode( true );
//		pEntity->SetFriction( 0.0f, 1.0f );

//		Engine()->InputManager()->SetHoldCrouch( false );
//		Engine()->InputManager()->SetHoldJump( false );
//		Engine()->InputManager()->SetHodRun( true );

	}

	public		void					SetFlying() {

		if ( m_MotionType == eMotionType.Flying ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = eMotionType.Flying;

//		this->ClearState();
//		pEntity->SetGravityMode( false );
//		pEntity->SetFriction( 0.0f, 10.0f );

//		Engine()->InputManager()->SetHoldCrouch( true );
//		Engine()->InputManager()->SetHoldJump( true );
//		Engine()->InputManager()->SetHodRun( true );

	}

	public		void					SetSwimming() {

		if ( m_MotionType == eMotionType.Swimming ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = eMotionType.Swimming;

//		this->ClearState();
//		SetCrouched( true );

//		pEntity->SetGravityMode( false );
//		pEntity->SetFriction( 0.1f, 0.1f ) ;
	//	pEntity->SetVelocity( EMPTY_VEC3 );
	
//		Engine()->InputManager()->SetHoldCrouch( true );
//		Engine()->InputManager()->SetHoldJump( true );
//		Engine()->InputManager()->SetHodRun( true );
	
	}

	public		void					SetClimbing() {

		if ( m_MotionType == eMotionType.P1ToP2 ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = eMotionType.P1ToP2;

//		bool b = IsCrouched();
//		this->ClearState();
//		SetCrouched( b );
	
//		pEntity->SetGravityMode( false );
//		pEntity->SetFriction( 0.0f, 1000.0f );

//		Engine()->InputManager()->SetHoldCrouch( false );
//		Engine()->InputManager()->SetHoldJump( false );
//		Engine()->InputManager()->SetHodRun( true );

	}






	



}
