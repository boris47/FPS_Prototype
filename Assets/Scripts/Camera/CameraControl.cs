
using UnityEngine;
using UnityEngine.PostProcessing;

public interface ICameraSetters {
	Transform		Target						{ set; }
	Vector3			CurrentDirection			{ set; }
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
	public	PostProcessingProfile GetPP_Profile
	{
		get { return GetComponent<PostProcessingBehaviour>().profile; }
	}


	[SerializeField, Tooltip("Camera ViewPoint"), ReadOnly]
	private	Transform		m_ViewPoint					= null;
	public	Transform		ViewPoint					{ get { return m_ViewPoint; } }

	[SerializeField, Tooltip("Camera Target"), ReadOnly]
	private	Transform		m_Target					= null;
	public	Transform		Target						{ get { return m_Target; } }
			Transform		ICameraSetters.Target		{ set { m_Target = value; } }

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
	public	Camera			MainCamera					{ get { return m_CameraRef == null ? m_CameraRef = GetComponent<Camera>() : m_CameraRef; } }




	private float			m_CurrentRotation_X_Delta	= 0.0f;
	private float			m_CurrentRotation_Y_Delta	= 0.0f;

	private	float			m_CameraFPS_Shift			= 0.0f;

	private	Vector3			m_CurrentDirection			= Vector3.zero;
			Vector3			ICameraSetters.CurrentDirection		{ set { m_CurrentDirection = value; } }
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
//			Destroy( gameObject );	// this prevent missing weapons transform
			gameObject.SetActive( false );
			return;
		}
		Instance = this;
		DontDestroyOnLoad( this );

		ClampedXAxis = true;

		m_WeaponPivot = transform.Find( "WeaponPivot" );

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
		m_CurrentDirection = transform.rotation.eulerAngles;

		Cursor.visible = false;

		CanParseInput = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	public	void	ApplyDispersion( float range )
	{
		m_CurrentDispersion.x += Random.Range( -range, -range * 0.5f );
		m_CurrentDispersion.y += Random.Range( -range, range );
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private	void	Update()
	{
		if ( m_ViewPoint == null )
			return;
		
		// if Target is assigned force to stop camera effects
		if ( m_Target != null )
		{
//			m_HeadBob.Reset ( bInstantly : true );
//			m_HeadMove.Reset( bInstantly : false );
//			return;
		}
		
		m_WpnRotationfeedbackX = Vector3.Lerp( m_WpnRotationfeedbackX, Vector3.zero, Time.deltaTime );
		m_WpnRotationfeedbackY = Vector3.Lerp( m_WpnRotationfeedbackY, Vector3.zero, Time.deltaTime );

		// Cam Dispersion
		m_CurrentDirection = Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_CurrentDispersion, Time.deltaTime * 8f );
		m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		m_CurrentDispersion = Vector3.Lerp ( m_CurrentDispersion, Vector3.zero, Time.deltaTime * 3.7f );

		LiveEntity pLiveEnitiy = m_ViewPoint.parent.GetComponent<LiveEntity>();
		if ( pLiveEnitiy && pLiveEnitiy.IsGrounded )
		{
			m_HeadBob.Update ( liveEntity : ref pLiveEnitiy, weight : pLiveEnitiy.IsMoving == true ? 1f : 0f );
			m_HeadMove.Update( liveEntity : ref pLiveEnitiy, weight : pLiveEnitiy.IsMoving == true ? 0f : 1f );
			
			// Weapon movements
			if ( WeaponManager.Instance.CurrentWeapon != null )
			{
				m_WeaponPositionDelta = m_HeadBob.WeaponPositionDelta + m_HeadMove.WeaponPositionDelta;
				m_WeaponRotationDelta = m_HeadBob.WeaponRotationDelta + m_HeadMove.WeaponRotationDelta;
			}
		}
		else
		{
			m_HeadBob.Reset ( bInstantly : true );
			m_HeadMove.Reset( bInstantly : false );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		if ( m_ViewPoint == null )
			return;

		// Look at target is assigned
		if ( m_Target != null )
		{
			Vector3 direction = ( m_Target.position - transform.position );
			transform.forward = Vector3.Slerp( transform.forward, direction.normalized, Time.deltaTime );
		}

		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Rotation
		if ( CanParseInput == true )
		{
			bool	isZoomed			= WeaponManager.Instance.Zoomed;
			float	wpnZoomSensitivity  = WeaponManager.Instance.CurrentWeapon.ZommSensitivity;

			float Axis_X_Delta = Input.GetAxis ( "Mouse X" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );
			float Axis_Y_Delta = Input.GetAxis ( "Mouse Y" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );

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

			transform.rotation = Quaternion.Euler( m_CurrentDirection + m_HeadBob.Direction + m_HeadMove.Direction );

			// rotation with effect added
			{
				Vector3 m_WpnRotationfeedbackXfinal = Vector3.ClampMagnitude(  Vector3.up		* Axis_X_Delta, WPN_ROTATION_CLAMP_VALUE * ( isZoomed ? 0.2f : 1.0f ) );
				Vector3 m_WpnRotationfeedbackYfinal = Vector3.ClampMagnitude(  Vector3.forward  * Axis_Y_Delta, WPN_ROTATION_CLAMP_VALUE * ( isZoomed ? 0.2f : 1.0f ) );

				m_WpnRotationfeedbackX = Vector3.Lerp( m_WpnRotationfeedbackX, m_WpnRotationfeedbackXfinal, Time.deltaTime * 5f );
				m_WpnRotationfeedbackY = Vector3.Lerp( m_WpnRotationfeedbackY, m_WpnRotationfeedbackYfinal, Time.deltaTime * 5f );

				m_WeaponRotationDelta += m_WpnRotationfeedbackX;
				m_WeaponRotationDelta += m_WpnRotationfeedbackY;
			}
		}


		// Position
		{
			LiveEntity pLiveEnitiy	= m_ViewPoint.parent.GetComponent<LiveEntity>();
			bool isCrouched = pLiveEnitiy.IsCrouched;
			// manage camera height while crouching
			m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( isCrouched ) ? 0.5f : 1.0f, Time.deltaTime * 10f );
			transform.position = m_ViewPoint.transform.parent.transform.TransformPoint( m_ViewPoint.transform.localPosition * m_CameraFPS_Shift );
		}

		WeaponManager.Instance.CurrentWeapon.Transform.localPosition	 = m_WeaponPositionDelta;
		WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	 = m_WeaponRotationDelta;
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
