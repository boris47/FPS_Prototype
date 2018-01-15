using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class CameraControl : MonoBehaviour {

	public static CameraControl Instance				= null;

	const	float			CLAMP_MAX_X_AXIS			= 80.0f;
	const	float			CLAMP_MIN_X_AXIS			= -80.0f;
	private	bool			m_ClampedXAxis				= true;
	public	bool			ClampedXAxis {
		get { return m_ClampedXAxis; }
		set { m_ClampedXAxis = value; }
	}


	// Third person offset max distance
	const	float			MAX_CAMERA_OFFSET			= 15f;
	const	float			MIN_CAMERA_OFFSET			= 1.5f;

	[SerializeField][Tooltip("Camera Target")]
	private	GameObject		m_Target					= null;
	public	GameObject		Target {
		get { return m_Target; }
	}


	[SerializeField][Tooltip("Camera TPS Offset")]
	private	Vector3			m_TPSOffset					= Vector3.zero;

	[SerializeField][Range( 0.2f, 20.0f )]
	private	float			m_MouseSensitivity			= 1.0f;

	[SerializeField]
	private	bool			m_TPSMode					= false;

	[SerializeField]
	private bool			m_SmoothedRotation			= true;
	[SerializeField]
	private	bool			m_SmoothedPosition			= true;

	[SerializeField][Range( 1.0f, 10.0f )]
	private float			m_SmoothFactor				= 1.0f;

	[SerializeField]
	private HeadMove		m_HeadMove					= null;
	public	HeadMove		HeadMove {
		get { return m_HeadMove; }
	}

	[SerializeField]
	private HeadBob			m_HeadBob					= null;
	public	HeadBob			HeadBob {
		get { return m_HeadBob; }
	}

	public	bool			PassiveMode
	{
		get { return this.enabled; }
		set { this.enabled = value; }
	}

	private float			m_CurrentRotation_X_Delta	= 0.0f;
	private float			m_CurrentRotation_Y			= 0.0f;
	private float			m_CurrentRotation_Y_Delta	= 0.0f;

	private float			m_CameraOffset				= 5.0f;
	private float			m_CurrentCameraOffset		= 5.0f;
	[SerializeField]
	private	float			m_CameraFPS_Shift			= 0.0f;

	private	Vector3			m_CurrentDirection			= Vector3.zero;



	void Start()
	{
		// Sinlgeton
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;

		DontDestroyOnLoad( this );

		m_CurrentDirection = transform.rotation.eulerAngles;

		Cursor.visible = false;

	}


	private void Update()
	{

		if ( m_Target == null )
			return;

		if ( Input.GetKeyDown( KeyCode.V ) )
		{
			if ( m_TPSMode )
				m_TPSMode = false;
			else {
				m_TPSMode = true;
				m_CurrentCameraOffset = 0.0f;
			}
		}

		if ( m_TPSMode )
		{
			if ( Input.GetAxis( "Mouse ScrollWheel" ) > 0f && m_CameraOffset > MIN_CAMERA_OFFSET )
				m_CameraOffset -= 0.5f;

			if ( Input.GetAxis( "Mouse ScrollWheel" ) < 0f && m_CameraOffset < MAX_CAMERA_OFFSET )
				m_CameraOffset += 0.5f;
		}


		// Head Effects
		if ( m_TPSMode )
		{
			m_HeadMove.Reset( false );
			m_HeadBob.Reset( false );
		}
		else
		{
			LiveEntity pLiveEnitiy = m_Target.GetComponentInParent<LiveEntity>();
			if ( pLiveEnitiy && pLiveEnitiy.Grounded )
			{
				if ( pLiveEnitiy.IsMoving ) {
					m_HeadBob.Update( pLiveEnitiy );
					m_HeadMove.Reset();
				}
				else
				{
					m_HeadMove.Update( pLiveEnitiy );
					m_HeadBob.Reset();
				}
			}
			else
			{
				m_HeadMove.Reset( true );
				m_HeadBob.Reset( false );
			}
		}

	}


	private void LateUpdate()
	{

		if ( m_Target == null )
			return;

		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		LiveEntity pLiveEnitiy = m_Target.GetComponentInParent<LiveEntity>();

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
			if ( m_CurrentRotation_X_Delta != 0.0f || m_CurrentRotation_Y_Delta != 0.0f )
			{
				if ( m_ClampedXAxis )
					m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
				else
					m_CurrentDirection.x = m_CurrentDirection.x - m_CurrentRotation_Y_Delta;
				m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_X_Delta;
			}

			// rotation with effect added
			transform.rotation = Quaternion.Euler( m_CurrentDirection + ( m_HeadBob.Direction + m_HeadMove.Direction ) );

			if ( pLiveEnitiy != null )
			{
				pLiveEnitiy.FaceDirection = transform.rotation;
			}

		}



		// Position
		{
			if ( m_TPSMode )
			{
				m_CurrentCameraOffset = Mathf.Lerp( m_CurrentCameraOffset, m_CameraOffset, Time.deltaTime * 6f );

				if ( m_SmoothedPosition )
					transform.position = Vector3.Lerp( transform.position, m_Target.transform.position - ( transform.forward * m_CurrentCameraOffset ), Time.deltaTime * 8f );
				else
					transform.position = m_Target.transform.position - ( transform.forward * m_CurrentCameraOffset );

				transform.position = transform.position + transform.TransformDirection( m_TPSOffset );
			}
			else
			{
				bool isCrouched = pLiveEnitiy.IsCrouched;
				m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( isCrouched ) ? 0.5f : 1.0f, Time.deltaTime * 10f );

				transform.position = m_Target.transform.parent.transform.TransformPoint( m_Target.transform.localPosition * m_CameraFPS_Shift );
				
			}

		}
	
    }
}
