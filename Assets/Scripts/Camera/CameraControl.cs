
using UnityEngine;
using UnityEngine.PostProcessing;

public partial class CameraControl : MonoBehaviour {

	public	const	float	CLAMP_MAX_X_AXIS			= 80.0f;
	public	const	float	CLAMP_MIN_X_AXIS			= -80.0f;

	// Third person offset max distance
	public	const	float	MAX_CAMERA_OFFSET			= 15f;
	public	const	float	MIN_CAMERA_OFFSET			= 1.5f;

	public static CameraControl Instance				= null;


	public	bool			ClampedXAxis				{ get; set; }
	public	bool			CanParseInput				{ get; set; }
	public	PostProcessingProfile GetPP_Profile
	{
		get { return GetComponent<PostProcessingBehaviour>().profile; }
	}


	[SerializeField, Tooltip("Camera ViewPoint"), ReadOnly]
	private	Transform		m_ViewPoint					= null;
	public	Transform		ViewPoint
	{
		get { return m_ViewPoint; }
	}

	[SerializeField, Tooltip("Camera Target"), ReadOnly]
	private	Transform		m_Target					= null;
	public	Transform		Target
	{
		get { return m_Target; }
	}

	[SerializeField, Range( 0.2f, 20.0f )]
	private	float			m_MouseSensitivity			= 1.0f;

	[SerializeField]
	private bool			m_SmoothedRotation			= true;

	[SerializeField, Range( 1.0f, 10.0f )]
	private float			m_SmoothFactor				= 1.0f;

	[SerializeField]
	private HeadMove		m_HeadMove					= null;
	public	HeadMove		HeadMove
	{
		get { return m_HeadMove; }
	}

	[SerializeField]
	private HeadBob			m_HeadBob					= null;
	public	HeadBob			HeadBob
	{
		get { return m_HeadBob; }
	}

	private	Camera			m_CameraRef					= null;
	public	Camera			MainCamera
	{
		get { return m_CameraRef == null ? m_CameraRef = GetComponent<Camera>() : m_CameraRef; }
	}

	private float			m_CurrentRotation_X_Delta	= 0.0f;
//	private float			m_CurrentRotation_Y			= 0.0f;
	private float			m_CurrentRotation_Y_Delta	= 0.0f;

	private	float			m_CameraFPS_Shift			= 0.0f;

	private	Vector3			m_CurrentDirection			= Vector3.zero;
	private	Vector3			m_CurrentDispersion			= Vector3.zero;


	private void Awake()
	{

		// Sinlgeton
		if ( Instance != null )
		{
			Destroy( gameObject );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		ClampedXAxis = true;

		Player player = FindObjectOfType<Player>();
		if ( player == null )
		{
			enabled = false;
			return;
		}
		m_ViewPoint = player.transform.Find( "ViewPivot" );

	}

	void Start()
	{

		m_CurrentDirection = transform.rotation.eulerAngles;

		Cursor.visible = false;

		CanParseInput = true;
	}

	           

	public	void	ApplyDispersion( float range )
	{
		m_CurrentDispersion.x += Random.Range( -range, -range * 0.5f );
		m_CurrentDispersion.y += Random.Range( -range, range );
	}


	private void Update()
	{
		if ( m_ViewPoint == null )
			return;
		
		// if Target is assigned force to stop camera effects
		if ( m_Target != null )
		{
			m_HeadBob.Reset ( bInstantly : true );
			m_HeadMove.Reset( bInstantly : false );
			return;
		}

		// Cam Dispersion
		m_CurrentDirection = Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_CurrentDispersion, Time.deltaTime * 8f );
		m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		m_CurrentDispersion = Vector3.Lerp ( m_CurrentDispersion, Vector3.zero, Time.deltaTime * 3.7f );

		LiveEntity pLiveEnitiy = m_ViewPoint.parent.GetComponent<LiveEntity>();
		if ( pLiveEnitiy && pLiveEnitiy.IsGrounded )
		{
			m_HeadBob.Update ( liveEntity : ref pLiveEnitiy, weight : pLiveEnitiy.IsMoving == true ? 1f : 0f );
			m_HeadMove.Update( liveEntity : ref pLiveEnitiy, weight : pLiveEnitiy.IsMoving == true ? 0f : 1f );
		}
		else
		{
			m_HeadBob.Reset ( bInstantly : true );
			m_HeadMove.Reset( bInstantly : false );
		}

	}


	private void LateUpdate()
	{
		if ( m_ViewPoint == null )
			return;

		// Look at target is assigned
		if ( m_Target != null )
		{
			transform.position = m_ViewPoint.position;
			transform.LookAt( m_Target );
			return;
		}


		// User Control

		LiveEntity pLiveEnitiy	= m_ViewPoint.parent.GetComponent<LiveEntity>();
		m_SmoothFactor			= Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Rotation
		if ( CanParseInput == true )
		{
			float Axis_X_Delta = Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity;
			float Axis_Y_Delta = Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity;

			if ( m_SmoothedRotation )
			{
				m_CurrentRotation_X_Delta = Mathf.Lerp( m_CurrentRotation_X_Delta, Axis_X_Delta, Time.deltaTime * ( 100f / m_SmoothFactor ) );
				m_CurrentRotation_Y_Delta = Mathf.Lerp( m_CurrentRotation_Y_Delta, Axis_Y_Delta, Time.deltaTime * ( 100f / m_SmoothFactor ) );
			}
			else
			{
				m_CurrentRotation_X_Delta = Axis_X_Delta;
				m_CurrentRotation_Y_Delta = Axis_Y_Delta;
			}
			
			
			////////////////////////////////////////////////////////////////////////////////
			if ( ( m_CurrentRotation_X_Delta != 0.0f || m_CurrentRotation_Y_Delta != 0.0f ) )
			{
				if ( ClampedXAxis )
					m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
				else
					m_CurrentDirection.x = m_CurrentDirection.x - m_CurrentRotation_Y_Delta;

				m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_X_Delta;
			}

			// rotation with effect added
			transform.rotation = Quaternion.Euler( m_CurrentDirection + m_HeadBob.Direction + m_HeadMove.Direction );

		}


		// Position
		{
			bool isCrouched = pLiveEnitiy.IsCrouched;
			// manage camera height while crouching
			m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( isCrouched ) ? 0.5f : 1.0f, Time.deltaTime * 10f );
			transform.position = m_ViewPoint.transform.parent.transform.TransformPoint( m_ViewPoint.transform.localPosition * m_CameraFPS_Shift );
		}
	
    }
}
