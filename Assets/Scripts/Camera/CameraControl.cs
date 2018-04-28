
using UnityEngine;
using UnityEngine.PostProcessing;

public interface ICameraSetters {
	Transform		Target						{ set; }
}

public partial class CameraControl : MonoBehaviour, ICameraSetters {

	private	const	float	WPN_ROTATION_CLAMP_VALUE	= 5f;

	public	const	float	CLAMP_MAX_X_AXIS			=  80.0f;
	public	const	float	CLAMP_MIN_X_AXIS			= -80.0f;

	// Third person offset max distance
	public	const	float	MAX_CAMERA_OFFSET			= 15f;
	public	const	float	MIN_CAMERA_OFFSET			= 1.5f;

	public static CameraControl Instance				= null;


	public	bool			ClampedXAxis				{ get; set; }
	public	bool			CanParseInput				{ get; set; }

	private	PostProcessingProfile m_PP_Profile			= null;
	public	PostProcessingProfile GetPP_Profile			{ get { return m_PP_Profile; } }

	[SerializeField, Tooltip("Camera ViewPoint")]
	private	Transform		m_ViewPoint					= null;
	public	Transform		ViewPoint					{ get { return m_ViewPoint; } }
	
	[SerializeField, Tooltip("Camera Target"), ReadOnly]
	private	Transform		m_Target					= null;
	public	Transform		Target						{ get { return m_Target; } }
			Transform		ICameraSetters.Target		{ set { OnTargetSet( m_Target = value ); } }

	[SerializeField, Range( 0.2f, 20.0f )]
	private	float			m_MouseSensitivity			= 1.0f;

	[SerializeField]
	private bool			m_SmoothedRotation			= true;

	[SerializeField, Range( 1.0f, 10.0f )]
	private float			m_SmoothFactor				= 1.0f;

	[SerializeField]
	private HeadMove		m_HeadMove					= null;
	public	HeadMove		HeadMove					{ get { return m_HeadMove; } }

	[SerializeField]
	private HeadBob			m_HeadBob					= null;
	public	HeadBob			HeadBob						{ get { return m_HeadBob; } }

	[SerializeField]
	private Transform		m_WeaponPivot				= null;
	public	Transform		WeaponPivot					{ get { return m_WeaponPivot; } }

	private	Camera			m_CameraRef					= null;
	public	Camera			MainCamera					{ get { return m_CameraRef; } }

	private float			m_CurrentRotation_X_Delta	= 0.0f;
	private float			m_CurrentRotation_Y_Delta	= 0.0f;

	private	float			m_CameraFPS_Shift			= 0.0f;

	private	Vector3			m_CurrentDirection			= Vector3.zero;

	// WEAPON
	private	Vector3			m_CurrentDispersion			= Vector3.zero;
	private	Vector3			m_WeaponPositionDelta		= Vector3.zero;
	private	Vector3			m_WeaponRotationDelta		= Vector3.zero;
	private	Vector3			m_WpnRotationfeedbackX		= Vector3.zero;
	private	Vector3			m_WpnRotationfeedbackY		= Vector3.zero;


	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		// Sinlgeton
		if ( Instance != null )
		{
			gameObject.SetActive( false );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		ClampedXAxis = true;

		m_WeaponPivot = transform.Find( "WeaponPivot" );

		m_CameraRef = GetComponent<Camera>();
		m_PP_Profile = GetComponent<PostProcessingBehaviour>().profile;

		Player player = FindObjectOfType<Player>();
		if ( player == null )
		{
			enabled = false;
			return;
		}
		m_ViewPoint = player.transform.Find( "ViewPivot" );
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private	void	Start()
	{
		Cursor.visible = false;

		CanParseInput = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetSet
	private	void	OnTargetSet( Transform value )
	{
		if ( value == null )
			m_CurrentDirection = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCutsceneEnd
	public	void	OnCutsceneEnd()
	{
		m_CurrentRotation_X_Delta = 0f;
		m_CurrentRotation_Y_Delta = 0f;
		m_CurrentDispersion = Vector3.zero;

		if ( m_CurrentDirection.x > CLAMP_MAX_X_AXIS )
		{
			m_CurrentDirection.x = m_CurrentDirection.x - 360f;

		}

		if ( m_CurrentDirection.x < CLAMP_MIN_X_AXIS )
		{
			m_CurrentDirection.x = m_CurrentDirection.x + 360f;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	public	void	ApplyDispersion( float range )
	{
		if ( Player.Instance.IsDodging )
			return;

		m_CurrentDispersion.x = Random.Range( -range, -range * 0.5f );
		m_CurrentDispersion.y = Random.Range( -range,  range );
	}

	//////////////////////////////////////////////////////////////////////////
	// FixedUpdate
	private void FixedUpdate()
	{
		if ( m_Target != null )
		{
			// Position
			transform.position		= Player.Instance.transform.TransformPoint( m_ViewPoint.transform.localPosition * m_CameraFPS_Shift );

			// Rotation
			Quaternion rotation		= Quaternion.LookRotation( m_Target.position - transform.position, Player.Instance.transform.up );
			transform.rotation		= Quaternion.Slerp( transform.rotation, rotation, Time.unscaledDeltaTime * 5f ) * Quaternion.Euler( m_HeadBob.Direction + m_HeadMove.Direction );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		if ( m_ViewPoint == null )
			return;

		m_WpnRotationfeedbackX = Vector3.Lerp( m_WpnRotationfeedbackX, Vector3.zero, Time.deltaTime );
		m_WpnRotationfeedbackY = Vector3.Lerp( m_WpnRotationfeedbackY, Vector3.zero, Time.deltaTime );

		if ( Player.Instance.IsGrounded )
		{
			m_HeadBob.Update ( weight : Player.Instance.IsMoving == true ? 1f : 0f );
			m_HeadMove.Update( weight : Player.Instance.IsMoving == true ? 0f : 1f );
		}
		else
		{
			m_HeadBob.Reset ( bInstantly : true );
			m_HeadMove.Reset( bInstantly : false );
		}

		m_WeaponPositionDelta = m_HeadBob.WeaponPositionDelta + m_HeadMove.WeaponPositionDelta;
		m_WeaponRotationDelta = m_HeadBob.WeaponRotationDelta + m_HeadMove.WeaponRotationDelta;
	}


	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		if ( m_ViewPoint == null )
			return;

		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );
		
		m_WeaponRotationDelta += m_WpnRotationfeedbackX;
		m_WeaponRotationDelta += m_WpnRotationfeedbackY;

		// Weapon movements
		if ( WeaponManager.Instance.CurrentWeapon != null )
		{
			WeaponManager.Instance.CurrentWeapon.Transform.localPosition	 = m_WeaponPositionDelta;
			WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	 = m_WeaponRotationDelta;
		}

		// Look at target is assigned
		if ( m_Target != null )
			return;

		// Cam Dispersion
		m_CurrentDirection		= Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_CurrentDispersion, Time.deltaTime * 8f );
		m_CurrentDirection.x	= Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		m_CurrentDispersion		= Vector3.Lerp ( m_CurrentDispersion, Vector3.zero, Time.deltaTime * 3.7f );

		// Rotation
		if ( CanParseInput == true )
		{
			bool	isZoomed			= WeaponManager.Instance.Zoomed;
			float	wpnZoomSensitivity  = WeaponManager.Instance.CurrentWeapon.ZommSensitivity;
			float	Axis_X_Delta		= Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );
			float	Axis_Y_Delta		= Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );

			if ( m_SmoothedRotation )
			{
				m_CurrentRotation_X_Delta = Mathf.Lerp( m_CurrentRotation_X_Delta, Axis_X_Delta, Time.unscaledDeltaTime * ( 100f / m_SmoothFactor ) );
				m_CurrentRotation_Y_Delta = Mathf.Lerp( m_CurrentRotation_Y_Delta, Axis_Y_Delta, Time.unscaledDeltaTime * ( 100f / m_SmoothFactor ) );
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

			m_CurrentDirection += m_HeadBob.Direction + m_HeadMove.Direction;
			transform.rotation = m_ViewPoint.transform.rotation * Quaternion.Euler( m_CurrentDirection );

			// rotation with effect added
			{
				Vector3 m_WpnRotationfeedbackXfinal = Vector3.ClampMagnitude(  Vector3.up		* Axis_X_Delta, WPN_ROTATION_CLAMP_VALUE * ( isZoomed ? 0.2f : 1.0f ) );
				Vector3 m_WpnRotationfeedbackYfinal = Vector3.ClampMagnitude(  Vector3.forward  * Axis_Y_Delta, WPN_ROTATION_CLAMP_VALUE * ( isZoomed ? 0.2f : 1.0f ) );

				m_WpnRotationfeedbackX = Vector3.Lerp( m_WpnRotationfeedbackX, m_WpnRotationfeedbackXfinal, Time.deltaTime * 5f );
				m_WpnRotationfeedbackY = Vector3.Lerp( m_WpnRotationfeedbackY, m_WpnRotationfeedbackYfinal, Time.deltaTime * 5f );
			}
		}

		// Position
		{
			// manage camera height while crouching
			m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( Player.Instance.IsCrouched ) ? 0.5f : 1.0f, Time.deltaTime * 10f );
			transform.position = Player.Instance.transform.TransformPoint( m_ViewPoint.transform.localPosition * m_CameraFPS_Shift );
		}
    }


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		var settings = GetPP_Profile.vignette.settings;
		settings.intensity = 0f;
		GetPP_Profile.vignette.settings = settings;
	}
}
