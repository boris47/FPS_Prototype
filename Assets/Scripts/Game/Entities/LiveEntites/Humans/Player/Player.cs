using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : Human {

	private		Vector3				m_Move				= Vector3.zero;

	private		bool	m_Active	= false;
	public		bool	IsActive {
		get { return m_Active; }
		set { m_Active = value; }
	}

	// Use this for initialization
	void Start () {

		m_ID = NewID();
		m_FaceDirection = transform.rotation;

		// Player Components
		{
			m_RigidBody = GetComponent<Rigidbody>();
			m_Foots = transform.Find( "FootSpace" );
		}

		// Player Data
		{

			m_SectionRef = GLOBALS.Configs.GetSection( m_SectionName = gameObject.name );

			if ( m_SectionRef == null ) {
				Destroy( gameObject );
				return;
			}


			// Walking
			m_SectionRef.AsMultiValue( "Walk", 1, 2, 3, ref m_WalkSpeed, ref m_WalkJumpCoef, ref m_WalkStamina );
			
			// Running
			m_SectionRef.AsMultiValue( "Run", 1, 2, 3, ref m_RunSpeed, ref m_RunJumpCoef, ref m_RunStamina );

			// Crouched
			m_SectionRef.AsMultiValue( "Crouch", 1, 2, 3, ref m_CrouchSpeed, ref m_CrouchJumpCoef, ref m_CrouchStamina );


			// Climbing
///			bool result = m_SectionRef.bAsFloat( "Climb", ref m_ClimbSpeed );
			m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", 0.12f );
//			m_ClimbSpeed				= m_SectionRef[ "Climb" ].Value.ToFloat();

			// Jumping
			{
//				cMultiValue JumpInfos	= m_SectionRef[ "Jump" ].MultiValue;
//				m_JumpForce				= JumpInfos[ 0 ].As<float>();
//				m_JumpForce				= JumpInfos[ 0 ].ToFloat();
//				m_JumpForce				= JumpInfos[ 0 ];

				m_SectionRef.AsMultiValue( "Jump", 1, 2, ref m_JumpForce, ref m_JumpStamina );

			}

			// Stamina
			{
				m_StaminaRestore		= m_SectionRef.AsFloat( "StaminaRestore", 0.0f );
				m_StaminaRunMin			= m_SectionRef.AsFloat( "StaminaRunMin",  0.3f );
				m_StaminaJumpMin		= m_SectionRef.AsFloat( "StaminaJumpMin", 0.4f );
			}

		}
		
		m_Health			= m_SectionRef.AsFloat( "Health", 100.0f );
		m_RigidBody.mass	= m_SectionRef.AsFloat( "phMass", 80.0f  );
		m_Stamina = 1.0f;

		SetMotionType( eMotionType.Walking );

	}



	// Update is called once per frame
	void Update () {

		if ( !m_Active ) return;

		// trace previuos states
		m_PreviousStates = m_States;
	
		// Reset "local" states
		m_States.Reset();


		////////////////////////////////////////////////////////////////////////////////////////
		// Pick eventual collision info from camera to up
		{
			// my check hight formula
///			Leadwerks::Vec3 vCheckHeigth = { 0.0f, ( CamManager()->GetStdHeight() / 2 ) * ( fJumpForce / 10 ), 0.0f };
///			vCheckHeigth *= IsCrouched() ? 0.5f : 1.0f;
///			bIsUnderSomething = World()->GetWorld()->Pick( vCamPos, vCamPos + vCheckHeigth, Leadwerks::PickInfo(), 0.36 );
		}

		////////////////////////////////////////////////////////////////////////////////////////
		// Check for usage
/*		{
			if ( Engine()->InputManager()->Use() ) {
				if ( !IsDragging() ) { // if is not draggind an entity
					Leadwerks::PickInfo pInfo;
					Leadwerks::Vec3 vUsageDistance = Leadwerks::Transform::Point( 0.0f, 0.0f, USE_DISTANCE, CamManager()->GetCamera(), NULL );
					bool bResult = World()->GetWorld()->Pick( vCamPos, vUsageDistance, pInfo, USE_RADIUS, true );
					if ( bResult && pInfo.entity ) {
						Leadwerks::Entity *pEntity = pInfo.entity;

						if ( pEntity->ContainsFunction( "Use" ) )   pEntity->CallFunction( "Use" );
						else
						if ( pEntity->ContainsFunction( "Drag" ) ) { pEntity->CallFunction( "Drag" );
							SetDragging( pEntity );
						}
					}
				}
				else { // if is dragging an entity
					DropEntityDragged();
				}
			}
		}
*/

		////////////////////////////////////////////////////////////////////////////////////////
		// Dragging Update
/*		{
			if ( IsDragging() ) {
				Leadwerks::Vec3 pEntityDraggedPos = Dragged.pEntity->GetPosition( true );
				Leadwerks::Vec3 vNewPosition = Leadwerks::Transform::Point(    Dragged.vPosition, CamManager()->GetCamera(), NULL );
				Leadwerks::Vec3 vNewRotation = Leadwerks::Transform::Rotation( Dragged.vRotation, CamManager()->GetCamera(), NULL );

				float fMaxDifference = 0.5f;
				float fDistance = vNewPosition.DistanceToPoint( pEntityDraggedPos );
				if ( fDistance > 1.5 )  {
					DropEntityDragged();
				}
				else {
					if ( fDistance > fMaxDifference ) {
						vNewPosition = pEntityDraggedPos + ( vNewPosition - pEntityDraggedPos ).Normalize() * fMaxDifference;
						fDistance = fMaxDifference;
					}
					Dragged.pEntity->PhysicsSetPosition( vNewPosition.x, vNewPosition.y, vNewPosition.z, 0.25 * ( IsSwimming() ? 0.5 : 1.0 ) );
					Dragged.pEntity->PhysicsSetRotation( vNewRotation, 0.5 );
				}
			}
		}
*/


		////////////////////////////////////////////////////////////////////////////////////////
		// Water
/*		bool bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater;
		if ( !IsClimbing() && World()->GetWorld()->GetWaterMode() ) {
		
			float fWaterHeight		 = World()->GetWorld()->GetWaterHeight();
			// camera is under water level
			bIsCameraUnderWater = ( vCamPos.y - 0.1f ) < fWaterHeight;
			bIsCameraReallyUnderWater = vCamPos.y < fWaterHeight;
			// entity is under water level, but camera is over water level
			bIsEntityInWater = pEntity->GetPosition().y-0.1 < fWaterHeight && !bIsCameraUnderWater;

			SetInWater( bIsEntityInWater );

			// If now camera is over water level, but prev update was under it
			if ( bIsEntityInWater ) {

				// if distance beetwen ground and parent is minus than camera height
				if ( GetAirbourneHeigth() < CamManager()->GetStdHeight() ) {
					// restore walking state
				//	if ( iMotionType != LIVE_ENTITY::Motion::Walk ) {
						SetMotionType( LIVE_ENTITY::Motion::Walk );
					//	SetCrouched( true );
				//	}
				}

			}

			// If camera go under water level enable underwater state
			if ( bIsCameraUnderWater && iMotionType != LIVE_ENTITY::Motion::Swim ) {
				SetSwimming();
			}

			// if actual motion is 'Swim' but is not entity and camera underwater restore 'walk' motion
			if ( iMotionType == LIVE_ENTITY::Motion::Swim && !bIsEntityInWater && !bIsCameraUnderWater )
				SetMotionType( LIVE_ENTITY::Motion::Walk );

			if ( bIsCameraReallyUnderWater ) {
				SetUnderWater( true );

				// Underwater stamina is consumed as oxygen
				fStamina -= fRunStamina * 2.0f;
			}

		}
*/

		////////////////////////////////////////////////////////////////////////////////////////
		// Movement Update
		{
			switch ( m_MotionType ) {
				case eMotionType.Walking:	{ this.Update_Walk();		break; }
				case eMotionType.Flying:	{ this.Update_Fly();		break; }
				case eMotionType.Swimming:	{ this.Update_Swim();		break; }
//				case eMotionType.Swimming:	{ this->Update_Swim( bIsEntityInWater, bIsCameraUnderWater, bIsCameraReallyUnderWater );	break; }
				case eMotionType.P1ToP2:	{ this.Update_P1ToP2();		break; }
			}
		}

		// Update flashlight position and rotation
//		pFlashLight->Update();

	}

}
