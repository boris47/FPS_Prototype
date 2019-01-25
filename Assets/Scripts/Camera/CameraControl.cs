
using UnityEngine;
using UnityEngine.PostProcessing;

public interface ICameraControl {

	Transform					Transform							{ get; }
	bool						Enabled								{ get; set; } 
	bool						ClampedXAxis						{ get; set; }
	bool						CanParseInput						{ get; set; }
	PostProcessingProfile		GetPP_Profile						{ get; }
	Camera						MainCamera							{ get; }
	Transform					Target								{ get; set; }
	Transform					WeaponPivot							{ get; }
	Vector3						CurrentDirection					{ get; set; }

	HeadBob						HeadBob								{ get; }
	HeadMove					HeadMove							{ get; }

	void						SetViewPoint						( Transform viewPoint );
	bool						IsParentedWith						( Transform transform );
	void						OnCutsceneEnd						();
	void						ApplyDeviation						( float deviation, float weightX = 1f, float weightY = 1f );
	void						ApplyDispersion						( float dispersion, float weightX = 1f, float weightY = 1f );
	void						ApplyFallFeedback					( float delta, float weightX = 1f, float weightY = 1f );
}

public class CameraControl : MonoBehaviour, ICameraControl {

	private		const		float				WPN_ROTATION_CLAMP_VALUE				= 0.6f;
	private		const		float				WPN_FALL_FEEDBACK_CLAMP_VALUE			= 5f;

	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	public		static		ICameraControl		Instance								= null;

	// INTERFACE START
				Transform						ICameraControl.Transform				{ get { return transform; } }
				bool							ICameraControl.Enabled					{ get { return enabled; } set { enabled = value; } }
				bool							ICameraControl.ClampedXAxis				{ get { return m_ClampedXAxis; } set { m_ClampedXAxis = value; } }
				bool							ICameraControl.CanParseInput			{ get { return m_CanParseInput; } set { m_CanParseInput = value; } }
				PostProcessingProfile			ICameraControl.GetPP_Profile			{ get { return m_PP_Profile; } }
				Camera							ICameraControl.MainCamera				{ get { return m_CameraRef; } }
				Transform						ICameraControl.Target					{ get { return m_Target; } set { OnTargetSet( value ); } }
				Transform						ICameraControl.WeaponPivot				{ get { return m_WeaponPivot; } }
				Vector3							ICameraControl.CurrentDirection			{ get { return m_CurrentDirection; } set{ m_CurrentDirection = value; } }
				HeadMove						ICameraControl.HeadMove					{ get { return m_HeadMove; } }
				HeadBob							ICameraControl.HeadBob					{ get { return m_HeadBob; } }
	// INTERFACE END
	
	[SerializeField, Tooltip("Camera Target"), ReadOnly]
	private		Transform						m_Target								= null;

	[SerializeField, Range( 0.2f, 20.0f )]
	private		float							m_MouseSensitivity						= 1.0f;

	[SerializeField]
	private		bool							m_SmoothedRotation						= true;

	[SerializeField, Range( 1.0f, 10.0f )]
	private		float							m_SmoothFactor							= 1.0f;

	[SerializeField]
	private		HeadMove						m_HeadMove								= null;

	[SerializeField]
	private		HeadBob							m_HeadBob								= null;

	[SerializeField]
	private		Transform						m_WeaponPivot							= null;

	private		Vector3							m_CurrentDirection						= Vector3.zero;
	private		bool							m_ClampedXAxis							= true;
	private		bool							m_CanParseInput							= true;
	private		PostProcessingProfile			m_PP_Profile							= null;
	private		Camera							m_CameraRef								= null;
	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;
	private		float							m_CameraFPS_Shift						= 0.0f;

	// WEAPON
	private		Vector3							m_WpnCurrentDeviation					= Vector3.zero;
	private		Vector3							m_WpnCurrentDispersion					= Vector3.zero;
	private		Vector3							m_WpnRotationFeedback					= Vector3.zero;
	private		Vector3							m_WpnFallFeedback						= Vector3.zero;



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		// Sinlgeton
		if ( Instance != null )
		{
			print( "Camera Awake: Instance already found" );
			gameObject.SetActive( false );
			return;
		}
		Instance = this  as ICameraControl;
		DontDestroyOnLoad( this );

		m_WeaponPivot = transform.Find( "WeaponPivot" );

		m_CameraRef = GetComponent<Camera>();
		m_PP_Profile = GetComponent<PostProcessingBehaviour>().profile;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnDisable()
	{
		if ( GameManager.Instance != null )
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Start
	private	void	Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );

		// Current internal direction
		streamUnit.AddInternal( "CurrentDirection", Utils.Converters.Vector3ToString( m_CurrentDirection ) );

		// Can parse input
		streamUnit.AddInternal( "CanParseInput", m_CanParseInput );

		// Headbob
		streamUnit.AddInternal( "HeadbobActive", m_HeadBob.IsActive );

		// Headmove
		streamUnit.AddInternal( "HeadmoveActive", m_HeadBob.IsActive );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	StreamUnit	OnLoad( StreamData streamData )
	{
		StreamUnit streamUnit = null;
		if ( streamData.GetUnit( gameObject, ref streamUnit ) == false )
			return null;

		// Camera internals
		m_WpnCurrentDeviation	= Vector3.zero;
		m_WpnCurrentDispersion	= Vector3.zero;
		m_WpnRotationFeedback	= Vector3.zero;
		m_WpnFallFeedback		= Vector3.zero;

		// Current internal direction
		m_CurrentDirection		= streamUnit.GetAsVector( "CurrentDirection" );

		// Can parse input
		m_CanParseInput			= streamUnit.GetAsBool( "CanParseInput" );

		// Headbob
		m_HeadBob.IsActive		= streamUnit.GetAsBool( "HeadbobActive" );

		// Headmove
		m_HeadMove.IsActive		= streamUnit.GetAsBool( "HeadmoveActive" );

		return streamUnit;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTargetSet
	private	void	OnTargetSet( Transform value )
	{
		if ( value == null )
		{
			float x = transform.localEulerAngles.x;

			while( x > CLAMP_MAX_X_AXIS ) x -= 360f;
            while( x < CLAMP_MIN_X_AXIS ) x += 360f;

			m_CurrentDirection.x = x;
			m_CurrentDirection.y = transform.parent.localEulerAngles.y;
			m_CurrentDirection.z = 0.0f;
		}
		m_Target = value;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetViewPoint
	void	ICameraControl.SetViewPoint( Transform viewPoint )
	{
		if ( viewPoint == null )
			return;

		transform.SetParent( viewPoint );
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetViewPoint
	bool	ICameraControl.IsParentedWith( Transform transform )
	{
		return transform.parent == transform;
	}

	//////////////////////////////////////////////////////////////////////////
	// OnCutsceneEnd
	void	ICameraControl.OnCutsceneEnd()
	{
		OnTargetSet( null );
		m_CurrentRotation_X_Delta	= 0.0f;
		m_CurrentRotation_Y_Delta	= 0.0f;
		m_WpnCurrentDeviation		= m_WpnCurrentDispersion = Vector3.zero;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDeviation
	void	ICameraControl.ApplyDeviation( float deviation, float weightX, float weightY )
	{
		if ( Player.Instance.IsDodging )
			return;

		m_WpnCurrentDeviation.x += Random.Range( -deviation, deviation ) * weightX;
		m_WpnCurrentDeviation.y += Random.Range( -deviation, deviation ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	void	ICameraControl.ApplyDispersion( float dispersion, float weightX, float weightY )
	{
		m_WpnCurrentDispersion.x += Random.Range( -dispersion, dispersion ) * weightX;
		m_WpnCurrentDispersion.y += Random.Range( -dispersion, dispersion ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyFallFeedback
	void	ICameraControl.ApplyFallFeedback( float delta, float weightX, float weightY )
	{
		m_WpnFallFeedback.x += delta * weightX;
		m_WpnFallFeedback.y += delta * weightY;
//		m_WpnFallFeedback = Vector3.ClampMagnitude( m_WpnCurrentDeviation, WPN_FALL_FEEDBACK_CLAMP_VALUE );
	}


	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		if ( transform.parent == null || GameManager.IsPaused )
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

		// Used for view smoothness
		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Weapon movements
		{
			Vector3 wpnPositionDelta = m_HeadBob.WeaponPositionDelta + m_HeadMove.WeaponPositionDelta;
			Vector3 wpnRotationDelta = m_HeadBob.WeaponRotationDelta + m_HeadMove.WeaponRotationDelta;

			m_WpnCurrentDispersion	= Vector3.Lerp( m_WpnCurrentDispersion, Vector3.zero, dt * 3.7f );
			m_WpnRotationFeedback	= Vector3.Lerp( m_WpnRotationFeedback,	Vector3.zero, dt );
			m_WpnFallFeedback		= Vector3.Lerp( m_WpnFallFeedback,		Vector3.zero, dt * 3.7f );
			m_WpnCurrentDeviation	= Vector3.Lerp( m_WpnCurrentDeviation,  Vector3.zero, dt * 3.7f );

			wpnRotationDelta += m_WpnCurrentDispersion + m_WpnRotationFeedback + m_WpnFallFeedback;

			WeaponManager.Instance.CurrentWeapon.Transform.localPosition	 = wpnPositionDelta;
			WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	 = wpnRotationDelta;
		}

		// Look at target is assigned
		if ( m_Target != null )
		{
			// Position
			m_CameraFPS_Shift		= Mathf.Lerp( m_CameraFPS_Shift, ( Player.Instance.IsCrouched ) ? 0.5f : 1.0f, dt * 10f );

			// Camera Rotation
			Quaternion rotation		= Quaternion.LookRotation( m_Target.position - transform.position, transform.parent.up );
			transform.rotation		= Quaternion.Slerp( transform.rotation, rotation, Time.unscaledDeltaTime * 8f ) * Quaternion.Euler( m_HeadBob.Direction + m_HeadMove.Direction );
			
			// Head Rotation
			Vector3 projectedPoint	= Utils.Math.ProjectPointOnPlane( transform.parent.up, transform.parent.position, m_Target.position );
			transform.parent.rotation	= Quaternion.LookRotation( projectedPoint - transform.parent.position, transform.parent.up );
			return;
		}

		// Cam Dispersion
		m_CurrentDirection		= Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_WpnCurrentDeviation, dt * 8f );
		m_CurrentDirection.x	= Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );

		// Rotation
		if ( m_CanParseInput == true )
		{
			bool	isZoomed			= WeaponManager.Instance.IsZoomed;
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
				if ( m_ClampedXAxis )
					m_CurrentDirection.x = Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
				else
					m_CurrentDirection.x = m_CurrentDirection.x - m_CurrentRotation_Y_Delta;

				m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_X_Delta;
			}

			// Apply effects only if not chosing dodge rotation
//			if ( Player.Instance.ChosingDodgeRotation == false )
			{
				m_CurrentDirection += m_HeadBob.Direction + m_HeadMove.Direction;
			}

			// Horizonatal rotatation
			transform.parent.localRotation = Quaternion.Euler( Vector3.up * m_CurrentDirection.y );

			// Vertical rotation
			transform.localRotation = Quaternion.Euler( Vector3.right * m_CurrentDirection.x );

			// rotation with effect added
			{
				Vector3 finalWpnRotationFeedback = Vector3.right * Axis_Y_Delta + Vector3.up * Axis_X_Delta;
				m_WpnRotationFeedback = Vector3.ClampMagnitude( Vector3.Lerp( m_WpnRotationFeedback, finalWpnRotationFeedback, dt * 8f ), WPN_ROTATION_CLAMP_VALUE );
			}
		}

		// Position
		{
			// manage camera height while crouching
			m_CameraFPS_Shift = Mathf.Lerp( m_CameraFPS_Shift, ( Player.Instance.IsCrouched ) ? 0.5f : 1.0f, dt * 10f );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		var settings = m_PP_Profile.vignette.settings;
		settings.intensity = 0f;
		m_PP_Profile.vignette.settings = settings;

		Instance = null;
	}
}
