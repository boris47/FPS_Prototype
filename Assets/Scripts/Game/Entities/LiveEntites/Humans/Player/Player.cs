﻿
using System;
using UnityEngine;


public partial class Player : Human {

	public	static	Player	CurrentActivePlayer	= null;

	private		Vector3				m_Move				= Vector3.zero;

	private		bool				m_Active			= false;
	public		bool				IsActive
	{
		get { return m_Active; }
		set { m_Active = value; }
	}



	//////////////////////////////////////////////////////////////////////////
	// START
	void Start () {

		m_ID = NewID();
		m_FaceDirection = transform.rotation;

		// Player Components
		{
			m_RigidBody = GetComponent<Rigidbody>();

			// Foots
			Transform pFoots = transform.Find( "FootSpace" );
			if ( pFoots != null ) {
				m_Foots = pFoots.GetComponent<Foots>();
				m_Foots.Parent = this;
			}
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


		m_DragPoint = new GameObject( "GrabPoint" );
		m_DragPoint.transform.SetParent( transform );
		m_DragPoint.transform.localPosition = Vector3.zero;
		m_DragPoint.transform.localRotation = Quaternion.identity;
		m_DragPoint.transform.Translate( 0f, 0f, m_UseDistance );
//		var rb = m_DragPoint.AddComponent<Rigidbody>();
//		rb.useGravity = false;
//		rb.isKinematic = true;

	}

	private GameObject m_DragPoint = null;


	public override void OnInteraction()
	{}




	private	void DropEntityDragged()
	{
		Rigidbody rb	= m_DraggedObject.GetComponent<Rigidbody>();
		rb.useGravity	= m_DraggedObjectUseGravity;
		rb.mass			= m_DraggedObjectMass;
		rb.velocity		= Vector3.zero;
		m_DraggedObject = null;
	}


	//////////////////////////////////////////////////////////////////////////
	// UNITY
	private void FixedUpdate()
	{
		if ( !m_Active )
			return;

		if ( m_DraggedObject == null )
			return;

		float distance = Vector3.Distance( m_DraggedObject.transform.position, m_DragPoint.transform.position );
		if ( distance > m_UseDistance + 0.2f )
		{
			DropEntityDragged();
			return;
		}

		// Dragging Update
		Rigidbody rb = m_DraggedObject.GetComponent<Rigidbody>();
		rb.MovePosition( m_DragPoint.transform.position );
		rb.rotation = CameraControl.Instance.transform.rotation;
		rb.angularVelocity = rb.velocity = Vector3.zero;
	}


	private void Update () {

		if ( !m_Active )
			return;
	
		// Reset "local" states
		m_States.Reset();

		// Update Grab point position
		m_DragPoint.transform.position = CameraControl.Instance.transform.position + ( CameraControl.Instance.transform.forward * m_UseDistance );
		m_DragPoint.transform.rotation = CameraControl.Instance.transform.rotation;

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
		{
			if ( Inputmanager.Inputs.Use )
			{
				if ( m_DraggedObject == null )
				{
					RaycastHit hit;
					if ( Physics.Raycast( CameraControl.Instance.transform.position, CameraControl.Instance.transform.forward, out hit, m_UseDistance ))
					{
						// Interactions
						Interactable interactable = hit.transform.GetComponent<Interactable>();
						if ( interactable != null && interactable.CanInteract )
							interactable.OnInteraction();

						// Drag
						Draggable draggable = hit.transform.GetComponent<Draggable>();
						if ( draggable && interactable.CanInteract )
						{
							m_DraggedObject = hit.transform.gameObject;
							Rigidbody rb = m_DraggedObject.GetComponent<Rigidbody>();
							m_DraggedObjectMass = rb.mass;
							m_DraggedObjectUseGravity = rb.useGravity;
							rb.mass = 1f;
							rb.useGravity = false;
							rb.interpolation = RigidbodyInterpolation.Extrapolate;
						}
					}
				}
				else
				{
					DropEntityDragged();
				}
			}
		}

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

		// rotate the capsule of the player
		transform.rotation = Quaternion.Euler( Vector3.Scale( CameraControl.Instance.transform.rotation.eulerAngles, new Vector3( 0f, 1f, 0f ) ) );
		m_FaceDirection = CameraControl.Instance.transform.rotation;
		// Update flashlight position and rotation
//		pFlashLight->Update();

		// trace previuos states
		m_PreviousStates = m_States;

	}

}
