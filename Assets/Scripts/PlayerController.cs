using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public static PlayerController Instance = null;

	public float fPlayerSpeed  = 10.0f;

	[HideInInspector]
	public float fBobbing = 0.0f;

	Rigidbody pRigidBody = null;
	Animator pAnimator = null;

	private Vector3 m_CamForward;
	private Vector3 m_Move;

	private bool bGrounded = true;

	public static	bool IsMoving = false;

	// Use this for initialization
	void Start () {

		if ( Instance == null )
			Instance = this;
		else {
			Destroy( gameObject );
			return;
		}

		DontDestroyOnLoad( this );

		pRigidBody = GetComponent<Rigidbody>();

		pAnimator = GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void Update () {

		if ( bGrounded == false ) return;

		IsMoving = false;

		float 	fMove 			= Input.GetAxis ( "Vertical" );
		float 	fStrafe			= Input.GetAxis ( "Horizontal" );
		bool 	bIsSprinting	= Input.GetKey ( KeyCode.LeftShift );
		bool 	bIsJumping		= Input.GetKeyDown ( KeyCode.Space );


		// calculate camera relative direction to move:
		m_CamForward = Vector3.Scale( CameraControl.Instance.transform.forward, new Vector3( 1.0f, 0.0f, 1.0f ) ).normalized;

		m_Move = ( fMove * m_CamForward ) + ( fStrafe * CameraControl.Instance.transform.right );

		if ( ( m_Move.x != 0.0f ) && ( m_Move.z != 0.0f  ) ) {
			m_Move *= 0.707f;
		}

		if ( m_Move.x != 0.0f || m_Move.z != 0.0f ) IsMoving = true;

		m_Move = transform.InverseTransformDirection( m_Move * fPlayerSpeed * ( bIsSprinting ? 2.0f : 1.0f ) );

		if ( bIsJumping && bGrounded ) pRigidBody.velocity = pRigidBody.velocity + Vector3.up * 10f;

		pRigidBody.velocity = Vector3.Lerp ( 
			pRigidBody.velocity, 
			new Vector3(  m_Move.x, pRigidBody.velocity.y - 0.981f, m_Move.z ),
			Time.deltaTime * 7f
		);

	}

	private void OnCollisionEnter( Collision collision ) {
		
		if ( collision.gameObject.tag == "Terrain" )
			bGrounded = true;

	}

	private void OnCollisionExit( Collision collision ) {
		
		if ( collision.gameObject.tag == "Terrain" )
			bGrounded = false;

	}

}
