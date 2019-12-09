
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
	void						OnCutsceneEnd						();
	void						ApplyDeviation						( float deviation, float weightX = 1f, float weightY = 1f );
	void						ApplyDispersion						( float dispersion, float weightX = 1f, float weightY = 1f );
	void						ApplyFallFeedback					( float delta, float weightX = 1f, float weightY = 1f );
	void						AddRecoil							( float recoil );
}

public class CameraControl : MonoBehaviour, ICameraControl {

	private		const		float				WPN_ROTATION_CLAMP_VALUE				= 0.6f;
	private		const		float				WPN_FALL_FEEDBACK_CLAMP_VALUE			= 5f;

	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	public		static		ICameraControl		m_Instance								= null;
	public		static		ICameraControl		Instance
	{
		get { return m_Instance; }
	}
	

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

	[SerializeField]
	private		bool							m_WeaponMoveEffectEnabled				= true;

	[SerializeField, Range( 0.2f, 20.0f )]
	private		float							m_MouseSensitivity						= 1.0f;

	[SerializeField]
	private		bool							m_SmoothedRotation						= true;

	[SerializeField, Range( 1.0f, 10.0f )]
	private		float							m_SmoothFactor							= 1.0f;

	[SerializeField]
	private		HeadMove						m_HeadMove								= new HeadMove();

	[SerializeField]
	private		HeadBob							m_HeadBob								= new HeadBob();

	[SerializeField]
	private		Transform						m_WeaponPivot							= null;

	private		Vector3							m_CurrentDirection						= Vector3.zero;
	private		bool							m_ClampedXAxis							= true;
	private		bool							m_CanParseInput							= true;
	private		PostProcessingProfile			m_PP_Profile							= null;
	private		Camera							m_CameraRef								= null;
	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;

	// WEAPON
	private		Vector3							m_WpnCurrentDeviation					= Vector3.zero;
	private		Vector3							m_WpnCurrentDispersion					= Vector3.zero;
	private		Vector3							m_WpnRotationFeedback					= Vector3.zero;
	private		Vector3							m_WpnFallFeedback						= Vector3.zero;
	private		float							m_Recoil								= 0.0f;


	[System.Serializable]
	private class CameraSectionData {
		public	float	ViewDistance					= 500f;
	}

	[SerializeField]
	private		CameraSectionData			m_CameraSectionData = new CameraSectionData();



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		// Singleton
		if ( Instance != null )
		{
			print( "Camera Awake: Instance already found" );
			gameObject.SetActive( false );
			return;
		}
		m_Instance = this;

		m_WeaponPivot = transform.Find( "WeaponPivot" );

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		ResourceManager.LoadedData<PostProcessingProfile> cameraPostProcesses = new ResourceManager.LoadedData<PostProcessingProfile>();
		bool bLoadResult = ResourceManager.LoadResourceSync
		(
			ResourcePath:			"Scriptables/CameraPostProcesses",
			loadedResource:			cameraPostProcesses
		);

		UnityEngine.Assertions.Assert.IsTrue
		(
			bLoadResult,
			"CameraControl::Awake: Failed the load of camera post processes profile"
		);

		m_CameraRef = GetComponent<Camera>();
		m_PP_Profile = gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>().profile = cameraPostProcesses.Asset;


		if ( GlobalManager.Configs.bGetSection( "Camera", m_CameraSectionData ) == false )
		{
			Debug.Log( "UI_Indicators::Initialize:Cannot load m_CameraSectionData" );
		}
		else
		{
			CameraEffectBase.EffectActiveCondition mainCondition = delegate()
			{
				return Player.Instance.IsGrounded;
			};

			m_HeadMove.Setup( mainCondition + delegate() { return Player.Instance.IsMoving == false; } );
			m_HeadBob.Setup( mainCondition + delegate() { return Player.Instance.IsMoving == true; } );
			m_CameraRef.farClipPlane = m_CameraSectionData.ViewDistance;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		UnityEngine.Assertions.Assert.IsNotNull
		(
			GameManager.StreamEvents,
			"CameraControl::OnEnable : GameManager.StreamEvents is null"
		);

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		OutlineEffectManager.SetEffectCamera( m_CameraRef );
	}
	

	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnDisable()
	{
		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
		OutlineEffectManager.SetEffectCamera( null );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnSave
	private	StreamUnit	OnSave( StreamData streamData )
	{
		StreamUnit streamUnit	= streamData.NewUnit( gameObject );

		// Current internal direction
		streamUnit.SetInternal( "CurrentDirection", Utils.Converters.Vector3ToString( m_CurrentDirection ) );

		// Can parse input
		streamUnit.SetInternal( "CanParseInput", m_CanParseInput );

		// Headbob
		streamUnit.SetInternal( "HeadbobActive", m_HeadBob.IsActive );

		// Headmove
		streamUnit.SetInternal( "HeadmoveActive", m_HeadBob.IsActive );

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
			m_CurrentDirection.y = m_ViewPoint ? m_ViewPoint.localEulerAngles.y : 0.0f;
			m_CurrentDirection.z = 0.0f;
		}
		m_Target = value;
	}

	private	Transform m_ViewPoint = null;


	//////////////////////////////////////////////////////////////////////////
	// SetViewPoint
	void	ICameraControl.SetViewPoint( Transform viewPoint )
	{
		m_ViewPoint = viewPoint ?? m_ViewPoint;
//		transform.SetParent( viewPoint );
//		transform.localPosition = Vector3.zero;
//		transform.localRotation = Quaternion.identity;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	private void Update()
	{
		if ( m_ViewPoint )
		{
			transform.SetPositionAndRotation( m_ViewPoint.position, m_ViewPoint.rotation );
		}
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

		m_WpnCurrentDeviation.x -= Random.Range( 0.001f, deviation ) * weightX;
		m_WpnCurrentDeviation.y += Random.Range( -deviation, deviation ) * weightY;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDispersion
	void	ICameraControl.ApplyDispersion( float dispersion, float weightX, float weightY )
	{
		m_WpnCurrentDispersion.x += Random.Range( 0.001f, dispersion ) * weightX;
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
	// AddRecoil
	void	ICameraControl.AddRecoil( float recoil )
	{
		m_Recoil += recoil;
	}

	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		if ( m_ViewPoint == null )
			return;

		// Look at target is assigned
		if ( m_Target )
		{
			// Camera Rotation
//			Quaternion rotation		= Quaternion.LookRotation( m_Target.position - transform.position, transform.parent.up );
//			transform.rotation		= Quaternion.Slerp( transform.rotation, rotation, Time.unscaledDeltaTime * 8f ) * Quaternion.Euler( m_HeadBob.Direction /*+ m_HeadMove.Direction*/ );

			m_ViewPoint.LookAt( m_Target );
			
			// Head Rotation
		//	Vector3 projectedPoint	= Utils.Math.ProjectPointOnPlane( transform.parent.up, transform.parent.position, m_Target.position );
		//	transform.parent.rotation	= Quaternion.LookRotation( projectedPoint - transform.parent.position, transform.parent.up );
			return;
		}

		if ( GameManager.IsPaused || GlobalManager.InputMgr.HasCategoryEnabled( InputCategory.CAMERA ) == false )
			return;

		float dt = Time.deltaTime;

		// CAMERA EFFECTS
		m_HeadMove.Update();
		m_HeadBob.Update();

		// Used for view smoothness
		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Weapon movements
		{
			m_WpnCurrentDispersion	= Vector3.Lerp( m_WpnCurrentDispersion, Vector3.zero, dt * 3.7f );
			m_WpnRotationFeedback	= Vector3.Lerp( m_WpnRotationFeedback,	Vector3.zero, dt );
			m_WpnFallFeedback		= Vector3.Lerp( m_WpnFallFeedback,		Vector3.zero, dt * 3.7f );
			m_WpnCurrentDeviation	= Vector3.Lerp( m_WpnCurrentDeviation,  Vector3.zero, dt * 3.7f );
			m_Recoil				= Mathf.Lerp( m_Recoil, 0.0f, dt * 3.7f );
			m_Recoil				= Mathf.Clamp( m_Recoil, 0.0f, 0.05f );

			if ( m_WeaponMoveEffectEnabled && WeaponManager.Instance.CurrentWeapon != null )
			{
				// Position
				Vector3 localPosition		= HeadBob.WeaponPositionDelta + HeadMove.WeaponPositionDelta + ( Vector3.left * m_Recoil );

				WeaponManager.Instance.CurrentWeapon.Transform.localPosition = localPosition;

				// Rotation
				Vector3 localEulerAngles	= HeadBob.WeaponRotationDelta + HeadMove.WeaponRotationDelta + m_WpnCurrentDispersion + m_WpnRotationFeedback + m_WpnFallFeedback;
				WeaponManager.Instance.CurrentWeapon.Transform.localEulerAngles	= localEulerAngles;

//				Vector3 basePivotRotation = Vector3.up * -90f;
//				m_WeaponPivot.localEulerAngles = m_HeadBob.Direction*10f + basePivotRotation;
			}

			// Optic sight alignment
			if ( WeaponManager.Instance.IsZoomed )
			{
				Vector2 delta = Vector2.zero;
				{
					Vector3 wpnDir = WeaponManager.Instance.CurrentWeapon.Transform.right;
					Vector3 wpnPos = WeaponManager.Instance.CurrentWeapon.Transform.position;
					Vector3 distantPoint = wpnPos + wpnDir * 1000f;
					Vector3 projectedPoint = m_CameraRef.WorldToScreenPoint( distantPoint );

					delta =  projectedPoint;
				}

				float frameFeedBack = m_Recoil * 10.0f;
				UIManager.InGame.FrameFeedBack( 1.0f + frameFeedBack, delta ); // 1.0f + Because is scale factor
			}
		}

		// Cam Dispersion
		m_CurrentDirection		= Vector3.Lerp( m_CurrentDirection, m_CurrentDirection + m_WpnCurrentDeviation, dt * 13f );
		m_CurrentDirection.x	= Utils.Math.Clamp( m_CurrentDirection.x - m_CurrentRotation_Y_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );

		// Rotation
		if ( m_CanParseInput == true )
		{
			bool	isZoomed			= WeaponManager.Instance.IsZoomed;
			float	wpnZoomSensitivity  = WeaponManager.Instance.ZoomSensitivity;
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

			// Apply effects
			{
//				m_CurrentDirection += m_HeadBob.Direction + m_HeadMove.Direction;
			}

			Vector3 effects = ( m_CurrentDirection + m_HeadBob.Direction + m_HeadMove.Direction );

			// Horizontal and Vertical rotation
			m_ViewPoint.localRotation = Quaternion.Euler(  ( Vector3.up * effects.y ) + ( Vector3.right * effects.x ) );

			// rotation with effect added
			{
				Vector3 finalWpnRotationFeedback = Vector3.right * Axis_Y_Delta + Vector3.up * Axis_X_Delta;
				m_WpnRotationFeedback = Vector3.ClampMagnitude( Vector3.Lerp( m_WpnRotationFeedback, finalWpnRotationFeedback, dt * 8f ), WPN_ROTATION_CLAMP_VALUE );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDestroy
	private void OnDestroy()
	{
		if ( (Object)m_Instance != this )
			return;

//		var settings = m_PP_Profile.vignette.settings;
//		settings.intensity = 0f;
//		m_PP_Profile.vignette.settings = settings;

		m_Instance = null;
	}
}
