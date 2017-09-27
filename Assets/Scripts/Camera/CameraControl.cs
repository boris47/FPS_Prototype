using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraControl : MonoBehaviour {

	public static CameraControl Instance				= null;

	const	float			MAX_CAMERA_OFFSET			= 15f;
	const	float			MIN_CAMERA_OFFSET			= 1.5f;

	[Tooltip("Camera Target")]
	public	Transform		m_Target					= null;

	[Tooltip("Camera Offset")]
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

	private	Vector3			m_CurrentDirection			 = Vector3.zero;

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

		if ( Input.GetKeyDown( KeyCode.V ) ) m_TPSMode = !m_TPSMode;

		if ( m_TPSMode ) {

			if ( Input.GetAxis( "Mouse ScrollWheel" ) > 0f && m_CameraOffset > MIN_CAMERA_OFFSET )
				m_CameraOffset -= 0.5f;

			if ( Input.GetAxis( "Mouse ScrollWheel" ) < 0f && m_CameraOffset < MAX_CAMERA_OFFSET )
				m_CameraOffset += 0.5f;

		}


		// Effects

		if ( m_TPSMode ) {
			m_HeadMove._Reset( false );
			m_HeadBob._Reset( false );
		}
		else {

			LiveEntity pLiveEnitiy = m_Target.GetComponentInParent<LiveEntity>();
			if ( pLiveEnitiy && pLiveEnitiy.Grounded ) {

				if ( pLiveEnitiy.IsMoving ) {
					m_HeadBob._Update();
				} else {
					m_HeadMove._Update();
				}

			} else {
				m_HeadMove._Reset( true );
				m_HeadBob._Reset( false );
			}
		}


	}


	private void LateUpdate() {

//		if ( Input.GetMouseButton( 0 ) ) {

			m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

			// Internal Rotation
			if ( m_SmoothedRotation )
				m_CurrentRotation_X_Delta = Mathf.Lerp( m_CurrentRotation_X_Delta, Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity, Time.deltaTime * ( 100f / m_SmoothFactor ) );
			else
				m_CurrentRotation_X_Delta = Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity;


			if ( m_SmoothedRotation )
				m_CurrentRotation_Y_Delta = Mathf.Lerp( m_CurrentRotation_Y_Delta, Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity, Time.deltaTime * ( 100f / m_SmoothFactor ) );
			else
				m_CurrentRotation_Y_Delta = Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity;



			if ( m_CurrentRotation_X_Delta != 0.0f || m_CurrentRotation_Y_Delta != 0.0f ) {
				m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, -80.0f, 80.0f );
				m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_X_Delta;
				m_CurrentDirection.z = 0.0f;
			}

//		}

		// rotation with effect added
		transform.rotation = Quaternion.Euler( m_CurrentDirection + ( m_HeadBob.Direction + m_HeadMove.Direction ) );

		

		// Position
		if ( m_TPSMode ) {
			m_CurrentCameraOffset = Mathf.Lerp( m_CurrentCameraOffset, m_CameraOffset, Time.deltaTime * 6f );

			if ( m_SmoothedPosition )
				transform.position = Vector3.Lerp( transform.position, m_Target.position - ( transform.forward * m_CurrentCameraOffset ), Time.deltaTime * 8f );
			else
				transform.position = m_Target.position - ( transform.forward * m_CurrentCameraOffset );

			transform.position = transform.position + transform.TransformDirection( m_TPSOffset );
		}
		else {
			transform.position = m_Target.position;
		}
	
    }
}
