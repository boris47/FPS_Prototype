using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class CameraControl : MonoBehaviour {

	public static CameraControl Instance				= null;



	const	float			CLAMP_MAX_X_AXIS			= 80.0f;
	const	float			CLAMP_MIN_X_AXIS			= -80.0f;
	private	bool			m_ClmapedXAxis				= true;
	public	bool			ClampedXAxis {
		get { return ClampedXAxis; }
		set { m_ClmapedXAxis = value; }
	}


	// Third person offset max distance
	const	float			MAX_CAMERA_OFFSET			= 15f;
	const	float			MIN_CAMERA_OFFSET			= 1.5f;

	[Tooltip("Camera Target")]
	public	GameObject		m_Target					= null;

	[Tooltip("Camera TPS Offset")]
	public	Vector3			m_TPSOffset					= Vector3.zero;

	[Range( 0.2f, 20.0f )]
	public	float			m_MouseSensitivity			= 1.0f;

	public	bool			m_TPSMode					= false;

	public	bool			m_SmoothedRotation			= true;
	public	bool			m_SmoothedPosition			= true;

	[Range( 1.0f, 10.0f )]
	public float			m_SmoothFactor				= 1.0f;

	public HeadMove			m_HeadMove					= null;
	public HeadBob			m_HeadBob					= null;


	private float			m_CurrentRotation_X_Delta	= 0.0f;
	private float			m_CurrentRotation_Y			= 0.0f;
	private float			m_CurrentRotation_Y_Delta	= 0.0f;

	private float			m_CameraOffset				= 5.0f;
	private float			m_CurrentCameraOffset		= 5.0f;

	private	Vector3			m_CurrentDirection			= Vector3.zero;

	public	bool			PassiveMode {
		get { return this.enabled; }
		set { this.enabled = value; }
	}

	void Start() {
		
		if ( Instance == null )
			Instance = this;
		else {
			Destroy( gameObject );
			return;
		}

		DontDestroyOnLoad( this );

		m_CurrentDirection = transform.rotation.eulerAngles;

		Cursor.visible = false;

	}


	private void Update() {

		if ( GLOBALS.Player1 != null && Input.GetKeyDown( KeyCode.F1 ) ) SwitchToTarget( GLOBALS.Player1.gameObject );
		if ( GLOBALS.Player2 != null && Input.GetKeyDown( KeyCode.F2 ) ) SwitchToTarget( GLOBALS.Player2.gameObject );
		if ( GLOBALS.Player3 != null && Input.GetKeyDown( KeyCode.F3 ) ) SwitchToTarget( GLOBALS.Player3.gameObject );
		if ( GLOBALS.Player4 != null && Input.GetKeyDown( KeyCode.F4 ) ) SwitchToTarget( GLOBALS.Player4.gameObject );

		if ( Input.GetKeyDown( KeyCode.V ) ) {

			if ( m_TPSMode )
				m_TPSMode = false;
			else {
				m_TPSMode = true;
				m_CurrentCameraOffset = 0.0f;
			}
		}

		if ( m_TPSMode ) {

			if ( Input.GetAxis( "Mouse ScrollWheel" ) > 0f && m_CameraOffset > MIN_CAMERA_OFFSET )
				m_CameraOffset -= 0.5f;

			if ( Input.GetAxis( "Mouse ScrollWheel" ) < 0f && m_CameraOffset < MAX_CAMERA_OFFSET )
				m_CameraOffset += 0.5f;

		}


		// Head Effects
		if ( m_TPSMode ) {
			m_HeadMove._Reset( false );
			m_HeadBob._Reset( false );
		}
		else {

			LiveEntity pLiveEnitiy = m_Target.GetComponentInParent<LiveEntity>();
			if ( pLiveEnitiy && pLiveEnitiy.Grounded ) {

				if ( pLiveEnitiy.IsMoving ) {
					m_HeadBob._Update( pLiveEnitiy );
					m_HeadMove._Reset();
				} else {
					m_HeadMove._Update( pLiveEnitiy );
					m_HeadBob._Reset();
				}

			} else {
				m_HeadMove._Reset( true );
				m_HeadBob._Reset( false );
			}
		}


	}


	private void LateUpdate() {

		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Rotation
		{

			float Axis_X_Delta = Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity;
			float Axis_Y_Delta = Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity;

			if ( m_SmoothedRotation )
				m_CurrentRotation_X_Delta = Mathf.Lerp( m_CurrentRotation_X_Delta, Axis_X_Delta, Time.deltaTime * ( 100f / m_SmoothFactor ) );
			else
				m_CurrentRotation_X_Delta = Axis_X_Delta;


			if ( m_SmoothedRotation )
				m_CurrentRotation_Y_Delta = Mathf.Lerp( m_CurrentRotation_Y_Delta, Axis_Y_Delta, Time.deltaTime * ( 100f / m_SmoothFactor ) );
			else
				m_CurrentRotation_Y_Delta = Axis_Y_Delta;
				
			////////////////////////////////////////////////////////////////////////////////
			if ( m_CurrentRotation_X_Delta != 0.0f || m_CurrentRotation_Y_Delta != 0.0f ) {
				if ( m_ClmapedXAxis )
					m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
				else
					m_CurrentDirection.x = m_CurrentDirection.x - m_CurrentRotation_Y_Delta;
				m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_X_Delta;
			}

			// rotation with effect added
			transform.rotation = Quaternion.Euler( m_CurrentDirection + ( m_HeadBob.Direction + m_HeadMove.Direction ) );

			LiveEntity pLiveEnitiy = m_Target.GetComponentInParent<LiveEntity>();
			if ( pLiveEnitiy != null ) {
				pLiveEnitiy.FaceDirection = transform.rotation;
			}

		}



		// Position
		{

			if ( m_TPSMode ) {
				m_CurrentCameraOffset = Mathf.Lerp( m_CurrentCameraOffset, m_CameraOffset, Time.deltaTime * 6f );

				if ( m_SmoothedPosition )
					transform.position = Vector3.Lerp( transform.position, m_Target.transform.position - ( transform.forward * m_CurrentCameraOffset ), Time.deltaTime * 8f );
				else
					transform.position = m_Target.transform.position - ( transform.forward * m_CurrentCameraOffset );

				transform.position = transform.position + transform.TransformDirection( m_TPSOffset );
			}
			else {
				transform.position = m_Target.transform.position;
			}

		}
	
    }
}
