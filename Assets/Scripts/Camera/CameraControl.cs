
using UnityEngine;
using UnityEngine.PostProcessing;

public interface ICameraSetters {
	Transform		Target						{ set; }
}

public partial class CameraControl : MonoBehaviour, ICameraSetters {

	private	const	float	WPN_ROTATION_CLAMP_VALUE	= 2f;

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

	public	Vector3			m_CurrentDirection			= Vector3.zero;

	// WEAPON
	private	Vector3			m_WpnCurrentDeviation		= Vector3.zero;
	private	Vector3			m_WpnCurrentDispersion		= Vector3.zero;
	private	Vector3			m_WpnRotationfeedback		= Vector3.zero;


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
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private	void	Start()
	{
		Cursor.visible = false;

		CanParseInput = true;

		GameManager.Instance.OnSave += OnSave;
		GameManager.Instance.OnLoad += OnLoad;

		m_ViewPoint = Player.Instance.transform.Find( "ViewPivot" );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamingUnit	OnSave( StreamingData streamingData )
	{
		StreamingUnit streamingUnit	= new StreamingUnit();
		streamingUnit.InstanceID	= gameObject.GetInstanceID();
		streamingUnit.Name			= gameObject.name;
		streamingUnit.Internals		= m_CurrentDirection.x + ", " + m_CurrentDirection.y + ", " + m_CurrentDirection.z;

		streamingData.Data.Add( streamingUnit );

		return streamingUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamingUnit	OnLoad( StreamingData streamingData )
	{
		int instanceID				= gameObject.GetInstanceID();
		StreamingUnit streamingUnit	= streamingData.Data.Find( ( StreamingUnit data ) => data.InstanceID == instanceID );
		if ( streamingUnit == null )
			return null;

		Utils.Converters.StringToVector( streamingUnit.Internals, ref m_CurrentDirection );
		return streamingUnit;
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
		m_WpnCurrentDeviation = m_WpnCurrentDispersion = Vector3.zero;

		if ( m_CurrentDirection.x > CLAMP_MAX_X_AXIS )	m_CurrentDirection.x = m_CurrentDirection.x - 360f;
		if ( m_CurrentDirection.x < CLAMP_MIN_X_AXIS )	m_CurrentDirection.x = m_CurrentDirection.x + 360f;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDeviation
	public	void	ApplyDeviation( float deviation, float weightX = 1f, float weightY = 1f )
	{
		if ( Player.Instance.IsDodging )
			return;

		m_WpnCurrentDeviation.x += Random.Range( -deviation, -deviation * 0.5f ) * weightX;
		m_WpnCurrentDeviation.y += Random.Range( -deviation,  deviation ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	public	void	ApplyDispersion( float dispersion, float weightX = 1f, float weightY = 1f )
	{
		m_WpnCurrentDispersion.x += Random.Range( -dispersion, -dispersion * 0.5f ) * weightX;
		m_WpnCurrentDispersion.y += Random.Range( -dispersion,  dispersion ) * weightY;
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
	// LateUpdate
	private	void	LateUpdate()
	{
		if ( m_ViewPoint == null )
			return;

		float dt = Time.deltaTime;

		// CAMERA EFFECTS
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

		// Used for view smotthness
		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Weapon movements
		if ( WeaponManager.Instance.CurrentWeapon != null )
		{
			m_WpnRotationfeedback	= Vector3.Lerp( m_WpnRotationfeedback, Vector3.zero, dt );
			m_WpnCurrentDeviation	= Vector3.Lerp( m_WpnCurrentDeviation,  Vector3.zero, dt * 3.7f );
			m_WpnCurrentDispersion	= Vector3.Lerp( m_WpnCurrentDispersion, Vector3.zero, dt * 3.7f );

			Vector3 wpnPositionDelta = m_HeadBob.WeaponPositionDelta + m_HeadMove.WeaponPositionDelta;
			Vector3 wpnRotationDelta = m_HeadBob.WeaponRotationDelta + m_HeadMove.WeaponRotationDelta;

			if ( WeaponManager.Instance.CurrentWeapon.IsFiring == true )
			{
				wpnRotationDelta += m_WpnCurrentDispersion;

//				float fireDispersion = WeaponManager.Instance.CurrentWeapon.FireDispersion;
//				Vector3 finalRotationDelta = m_WeaponRotationDelta + ( m_CurrentDispersion * fireDispersion );
//				m_WeaponRotationDelta = Vector3.Lerp( m_WeaponRotationDelta, finalRotationDelta, Time.deltaTime );
			}
			else
			{
//				wpnPositionDelta += m_WpnRotationfeedback.x;
				wpnRotationDelta += m_WpnRotationfeedback;
			}
			WeaponManager.Instance.CurrentWeapon.Transform.localPosition	 = wpnPositionDelta;
			WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	 = wpnRotationDelta;
		}

		// Look at target is assigned
		if ( m_Target != null )
			return;

		// Cam Dispersion
		m_CurrentDirection		= Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_WpnCurrentDeviation, dt * 8f );
		m_CurrentDirection.x	= Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		

		// Rotation
		if ( CanParseInput == true )
		{
			bool	isZoomed			= WeaponManager.Instance.Zoomed;
			float	wpnZoomSensitivity  = WeaponManager.Instance.CurrentWeapon.ZommSensitivity;
			float	Axis_X_Delta		= Input.GetAxisRaw ( "Mouse X" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );
			float	Axis_Y_Delta		= Input.GetAxisRaw ( "Mouse Y" ) * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );

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

			if ( Player.Instance.ChosingDodgeRotation == false )
			{
				m_CurrentDirection += m_HeadBob.Direction + m_HeadMove.Direction;
			}
			transform.rotation = m_ViewPoint.transform.rotation * Quaternion.Euler( m_CurrentDirection );

			// rotation with effect added
			{
				Vector3 finalWpnRotationFeedback = Vector3.right * Axis_Y_Delta + Vector3.up * Axis_X_Delta;
				m_WpnRotationfeedback = Vector3.Lerp( m_WpnRotationfeedback, finalWpnRotationFeedback, dt * 5f );
			}
		}

		// Position
		{
			// manage camera height while crouching
			m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( Player.Instance.IsCrouched ) ? 0.5f : 1.0f, dt * 10f );
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
