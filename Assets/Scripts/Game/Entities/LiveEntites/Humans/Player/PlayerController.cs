using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Human {

	public static PlayerController	Instance			= null;

	private		Rigidbody			m_RigidBody			= null;

	private		Vector3				m_Move				= Vector3.zero;

	// Use this for initialization
	void Start () {

		// Singleton
		{
			if ( Instance == null )
				Instance = this;
			else {
				Destroy( gameObject );
				return;
			}
		}

		// Player Components
		{
			m_RigidBody = GetComponent<Rigidbody>();
		}

		// Player Data
		{

			m_SectionRef = GLOBALS.Configs.GetSection( m_SectionName = "Actor" );

			if ( m_SectionRef == null ) {
				Destroy( gameObject );
				return;
			}


			// Walking
			try {
				// // GetMultiValue( "Walk",	1 );
				cMultiValue WalkInfos	= m_SectionRef[ "Walk" ].MultiValue;
				m_WalkSpeed				= WalkInfos[ 0 ];
				m_WalkJumpCoef			= WalkInfos[ 1 ];
				m_WalkStamina			= WalkInfos[ 2 ];
			} catch { }
			
			// Running
			{
				m_SectionRef.AsMultiValue( "Run", 1, 2, 3, ref m_RunSpeed, ref m_RunJumpCoef, ref m_RunStamina );
			}

			// Crouched
			{
				m_SectionRef.AsMultiValue( "Crouch", 1, 2, 3, ref m_CrouchSpeed, ref m_CrouchJumpCoef, ref m_CrouchStamina );
			}


			// Climbing
///			m_SectionRef.bAsFloat( "Climb", ref m_ClimbSpeed );
			m_ClimbSpeed				= m_SectionRef.AsFloat( "Climb", 0.12f );
			m_ClimbSpeed				= m_SectionRef[ "Climb" ].Value.ToFloat();

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
				m_StaminaRunMin			= m_SectionRef.AsFloat( "StaminaRunMin", 0.3f );
				m_StaminaJumpMin		= m_SectionRef.AsFloat( "StaminaJumpMin", 0.4f );
			}

		}
		
		DontDestroyOnLoad( this );

	}

//	private void FixedUpdate() {
		
		// Apply grvity
//		pRigidBody.velocity = pRigidBody.velocity + ( Vector3.down * 0.981f );

//	}

	// Update is called once per frame
	void Update () {

		if ( m_Grounded == false ) return;

		float 	fMove 			= Input.GetAxis ( "Vertical" );
		float 	fStrafe			= Input.GetAxis ( "Horizontal" );
		bool 	bIsJumping		= Input.GetKeyDown ( KeyCode.Space );
		bool 	bSprintInput	= Input.GetKey ( KeyCode.LeftShift );
		bool	bCrouchInput	= Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl );


		bool bIsMoving = false;


		// Apply correct speed
		if ( bSprintInput ) {

			fMove	*=	m_RunSpeed * ( fMove > 0 ? 1.0f : 0.8f );
			fStrafe	*=	m_RunSpeed * 0.6f;
			m_Stamina -= m_RunStamina;

		}
		else if ( bCrouchInput ) {

			fMove		*= m_CrouchSpeed * ( fMove > 0 ? 1.0f : 0.8f );
			fStrafe		*= m_CrouchSpeed * 0.6f;
			m_Stamina	-= m_CrouchStamina;


		}
		else {	// walking
				// stamina half restored because we are moving, but just walking
			fMove		*= m_WalkSpeed * ( fMove > 0 ? 1.0f : 0.8f );;
			fStrafe		*= m_WalkSpeed *  0.6f;
			m_Stamina	+= m_StaminaRestore / 2;

		}

		// This prevent speedhack
		if ( ( fStrafe != 0.0f ) && ( fMove != 0.0f  ) ) {
			m_Move *= 0.707f;
		}


		if ( fStrafe != 0.0f || fMove != 0.0f ) bIsMoving = true;


		// While not moving stamina regenerates at maximum speed
		if ( fStrafe == 0.0f && fMove == 0.0f )
			m_Stamina += m_StaminaRestore;

		// Clamp Stamina between 0.0 and 1.0
		m_Stamina = Mathf.Clamp( m_Stamina, 0.0f, 1.0f );


		m_MoveSmooth	= Mathf.Lerp( m_MoveSmooth, fMove, Time.deltaTime * 50f );
		m_StrafeSmooth	= Mathf.Lerp( m_StrafeSmooth, fStrafe, Time.deltaTime * 50f );


		// calculate camera relative direction to move:
		{
			Vector3 vCamForward = Vector3.Scale( CameraControl.Instance.transform.forward, new Vector3( 1.0f, 0.0f, 1.0f ) ).normalized;
			m_Move = ( m_MoveSmooth * vCamForward ) + ( m_StrafeSmooth * CameraControl.Instance.transform.right );
			m_Move = transform.InverseTransformDirection( m_Move *( bSprintInput ? 2.0f : 1.0f ) );
		}

		m_Move.y = m_RigidBody.velocity.y;

		if ( bIsJumping && m_Grounded ) m_RigidBody.velocity = m_RigidBody.velocity + Vector3.up * 10f;

		m_RigidBody.velocity = m_Move;

		/*
		m_RigidBody.velocity = Vector3.Lerp ( 
			m_RigidBody.velocity, 
			new Vector3(  m_Move.x, m_RigidBody.velocity.y, m_Move.z ),
			Time.deltaTime * 7f
		);
		*/

		// Apply states
		m_States.SetState( ( byte )LIVE_ENTITY.States.Moving, bIsMoving );

	}

	private void OnCollisionEnter( Collision collision ) {
		
		if ( collision.gameObject.tag == "Terrain" )
			m_Grounded = true;

	}

	private void OnCollisionExit( Collision collision ) {
		
		if ( collision.gameObject.tag == "Terrain" )
			m_Grounded = false;

	}

}
