using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class LiveEntity {

	public	Entity_States_Temp		States		= new Entity_States_Temp();
	public	Entity_States_Temp		PrevStates	= new Entity_States_Temp();

	// This variable control which physic to use on entity
	protected	LIVE_ENTITY.MotionType	m_MotionType		= LIVE_ENTITY.MotionType.Walking;
	protected	LIVE_ENTITY.MotionType	m_PrevMotionType	= LIVE_ENTITY.MotionType.None;

	public	LIVE_ENTITY.MotionType	GetMotionType()					{ return m_MotionType; }
	public	LIVE_ENTITY.MotionType	GetPrevMotionType()				{ return m_PrevMotionType; }


	public	void					SetMotionType( LIVE_ENTITY.MotionType i ) {

		m_PrevMotionType = m_MotionType;
		switch( i ) {

			case LIVE_ENTITY.MotionType.Walking:	{ this.SetPlainWalking(); return; }
			case LIVE_ENTITY.MotionType.Flying:		{ this.SetFlying(); return; }
			case LIVE_ENTITY.MotionType.Swimming:	{ this.SetSwimming(); return; }
			case LIVE_ENTITY.MotionType.P1ToP2:		{ this.SetClimbing(); return; }
		}

	}





	public void SetPlainWalking() {

		if ( m_MotionType == LIVE_ENTITY.MotionType.Walking ) return;

		m_PrevMotionType = m_MotionType;
		m_MotionType = LIVE_ENTITY.MotionType.Walking;

//		this.ClearState();

//		pEntity->SetGravityMode( true );
//		pEntity->SetFriction( 0.0f, 1.0f );

//		Engine()->InputManager()->SetHoldCrouch( false );
//		Engine()->InputManager()->SetHoldJump( false );
//		Engine()->InputManager()->SetHodRun( true );

	}

	public void SetFlying() {

		if ( m_MotionType == LIVE_ENTITY.MotionType.Flying ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = LIVE_ENTITY.MotionType.Flying;

//		this->ClearState();
//		pEntity->SetGravityMode( false );
//		pEntity->SetFriction( 0.0f, 10.0f );

//		Engine()->InputManager()->SetHoldCrouch( true );
//		Engine()->InputManager()->SetHoldJump( true );
//		Engine()->InputManager()->SetHodRun( true );

	}

	public void SetSwimming() {

		if ( m_MotionType == LIVE_ENTITY.MotionType.Swimming ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = LIVE_ENTITY.MotionType.Swimming;

//		this->ClearState();
//		SetCrouched( true );

//		pEntity->SetGravityMode( false );
//		pEntity->SetFriction( 0.1f, 0.1f ) ;
	//	pEntity->SetVelocity( EMPTY_VEC3 );
	
//		Engine()->InputManager()->SetHoldCrouch( true );
//		Engine()->InputManager()->SetHoldJump( true );
//		Engine()->InputManager()->SetHodRun( true );
	
	}

	public void SetClimbing() {

		if ( m_MotionType == LIVE_ENTITY.MotionType.P1ToP2 ) return;
	
		m_PrevMotionType = m_MotionType;
		m_MotionType = LIVE_ENTITY.MotionType.P1ToP2;

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
