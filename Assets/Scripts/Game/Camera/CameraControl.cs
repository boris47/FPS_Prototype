
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;


public sealed class CameraControl : MonoBehaviour
{
	private		const		float				WPN_ROTATION_CLAMP_VALUE				= 0.6f;
	private		const		float				WPN_FALL_FEEDBACK_CLAMP_VALUE			= 5f;

	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	public		static		CameraControl		m_Instance								= null;
	public		static		CameraControl		Instance
	{
		get { return m_Instance; }
	}
	
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
	private		Transform						m_WeaponPivot							= null;

	[SerializeField]
	private		Transform						m_ViewPoint								= null;

	[SerializeField]
	private		Transform						m_ViewPointBody							= null;

	private		Vector2							m_RotationFeedback						= Vector2.zero;

	public		PostProcessingProfile			PP_Profile								=> m_PP_Profile;
	public		Camera							MainCamera								=> m_CameraRef;
	public		Transform						WeaponPivot								=> m_WeaponPivot;
	public		CameraEffectorsManager			CameraEffectorsManager					=> m_CameraEffectorsManager;
	public		Transform						Target									{ get => m_Target; set => OnTargetSet( value ); }


	// EFFECTORS	
	[SerializeField]
	private		CameraEffectorsManager			m_CameraEffectorsManager				= new CameraEffectorsManager();

	private		Vector3							m_CurrentDirection						= Vector3.zero;
	private		PostProcessingProfile			m_PP_Profile							= null;
	private		Camera							m_CameraRef								= null;
	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;


	[System.Serializable]
	private class CameraSectionData
	{
		public	float	ViewDistance			= 500f;
	}

	[SerializeField]
	private		CameraSectionData				m_CameraSectionData						= new CameraSectionData();



	//////////////////////////////////////////////////////////////////////////
	// Awake
	private	void	Awake()
	{
		// Singleton
		if (Instance != null)
		{
			print("Camera Awake: Instance already found");
			gameObject.SetActive(false);
			return;
		}
		m_Instance = this;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	private void OnEnable()
	{
		m_WeaponPivot = transform.Find("WeaponPivot");

		// Sprites for TargetToKill, LocationToReach or ObjectToInteractWith
		bool bLoadResult = ResourceManager.LoadResourceSync("Scriptables/CameraPostProcesses", out PostProcessingProfile cameraPostProcesses);
		UnityEngine.Assertions.Assert.IsTrue(bLoadResult, "CameraControl::OnEnable: Failed the load of camera post processes profile");

		TryGetComponent(out m_CameraRef);
		m_PP_Profile = gameObject.GetOrAddIfNotFound<PostProcessingBehaviour>().profile = cameraPostProcesses;

		if (!(GlobalManager.Configs.TryGetSection("Camera", out Database.Section cameraSection) && GlobalManager.Configs.TrySectionToOuter(cameraSection, m_CameraSectionData)))
		{
			Debug.Log("UI_Indicators::Initialize:Cannot load m_CameraSectionData");
		}
		else
		{
			EffectActiveCondition mainCondition = () => Player.Instance.IsGrounded;

			m_CameraEffectorsManager.Add<HeadBob>(mainCondition + (() => Player.Instance.IsMoving));
			m_CameraEffectorsManager.Add<HeadMove>(mainCondition + (() => !Player.Instance.IsMoving));

			m_CameraRef.farClipPlane = m_CameraSectionData.ViewDistance;
		}

		UnityEngine.Assertions.Assert.IsNotNull
		(
			GameManager.StreamEvents,
			"CameraControl::OnEnable : GameManager.StreamEvents is null"
		);

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		OutlineEffectManager.SetEffectCamera(m_CameraRef);
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
	private	bool	OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		streamUnit = streamData.NewUnit(gameObject );

		// Current internal direction
		streamUnit.SetInternal( "CurrentDirection", Utils.Converters.Vector3ToString(m_CurrentDirection ) );

		// Can parse input
		streamUnit.SetInternal( "CanParseInput", GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA) );
		
		// TODO load effectors

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnLoad
	private	bool	OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if ( bResult )
		{
			// Camera internals
			m_RotationFeedback = Vector3.zero;

			// Current internal direction
			m_CurrentDirection = streamUnit.GetAsVector( "CurrentDirection" );

			// Can parse input
			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, streamUnit.GetAsBool( "CanParseInput" ));

			// TODO Save Effectors
		}
		return bResult;
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

		if ( value && !m_Target && m_ViewPoint )
		{
			m_ViewPoint.up = m_ViewPointBody?.up ?? Vector3.up;
		}

		m_Target = value;
	}


	//////////////////////////////////////////////////////////////////////////
	// SetViewPoint
	public void	SetViewPoint( Transform viewPoint, Transform viewPointBody )
	{
		m_ViewPoint = viewPoint ?? m_ViewPoint;
		m_ViewPointBody = viewPointBody ?? m_ViewPointBody;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnCutsceneEnd
	public void	OnCutsceneEnd()
	{
		OnTargetSet( null );
		m_CurrentRotation_X_Delta	= 0.0f;
		m_CurrentRotation_Y_Delta	= 0.0f;
		( WeaponManager.Instance?.CurrentWeapon as Weapon ).OnCutsceneEnd();
	}


	//////////////////////////////////////////////////////////////////////////
	// LateUpdate
	private	void	LateUpdate()
	{
		float deltaTime = GameManager.IsPaused ? 0f : Time.deltaTime;

		// Update effectors
		m_CameraEffectorsManager.Update( deltaTime );

		CameraEffectorData CameraEffectorsData = m_CameraEffectorsManager.CameraEffectorsData;
		Transform viewPoint = m_ViewPoint ?? transform;
		Transform viewPointBody = m_ViewPointBody ?? transform;
		bool bHasTargetAssigned = m_Target;
		IWeapon currentWeapon = WeaponManager.Instance.CurrentWeapon;

		// By Default Camera follow viewPoint
		transform.SetPositionAndRotation( viewPoint.position, viewPoint.rotation );

		// Used for view smoothness
		m_SmoothFactor = Mathf.Clamp( m_SmoothFactor, 1.0f, 10.0f );

		// Weapon movements
		{
			m_RotationFeedback = Vector3.Lerp( m_RotationFeedback, Vector3.zero, deltaTime );

			if ( m_WeaponMoveEffectEnabled )
			{
				// Weapon Local Position
				currentWeapon.Transform.localPosition = CameraEffectorsData.WeaponPositionDelta + ( Vector3.left * currentWeapon.Recoil * 0.1f );

				// Headbob effect on weapon
				Vector3 headbobEffectOnWeapon = ( m_CameraEffectorsManager.GetEffectorData<HeadBob>()?.CameraEffectsDirection ?? Vector3.zero ) * -1f;

				// Weapon Local Rotation
				currentWeapon.Transform.localEulerAngles = Vector3.zero
					+ CameraEffectorsData.WeaponRotationDelta // like headbob
					+ currentWeapon.Dispersion				// Delta for each shoot
					+ currentWeapon.FallFeedback
				//	+ currentWeapon.RotationFeedback
					+ headbobEffectOnWeapon
				;

				// Optic sight alignment
				if ( WeaponManager.Instance.IsZoomed )
				{
					Vector2 delta = Vector2.zero;
					{
						Vector3 distantPoint = currentWeapon.Transform.position + ( currentWeapon.Transform.right * 1000f );
						delta = m_CameraRef.WorldToScreenPoint( distantPoint );
					}

					float frameFeedBack = 1.0f + currentWeapon.Recoil; // 1.0f + Because is scale factor
					UIManager.InGame.FrameFeedBack( frameFeedBack, delta );
				}
			}
		}

		// Look at target if assigned ( override previous assignment )
		if ( bHasTargetAssigned )
		{
			// Body Rotation
			if (m_ViewPointBody )
			{
				Vector3 projectedPoint = Utils.Math.ProjectPointOnPlane( viewPointBody.up, viewPointBody.position, m_Target.position );
				viewPointBody.LookAt( projectedPoint, viewPointBody.up );
			}
			
//			viewPointBody.rotation = Quaternion.LookRotation( projectedPoint - viewPointBody.position, viewPointBody.up );

			// Camera Rotation
//			viewPoint.LookAt( m_Target, viewPointBody.up );

			Vector3 direction = m_Target.position - viewPoint.position;
			Vector3 DirectionPlusEffects = direction + CameraEffectorsData.CameraEffectsDirection + currentWeapon.Deviation;
//			viewPoint.rotation = Quaternion.Euler( DirectionPlusEffects.x, 0f, 0f );
			return;
		}

		// Return if input is disabled
		if ( GlobalManager.InputMgr.HasCategoryEnabled( EInputCategory.CAMERA ) == false )
			return;

		bool	isZoomed			= WeaponManager.Instance.IsZoomed;
		float	wpnZoomSensitivity  = WeaponManager.Instance.ZoomSensitivity;
		float	Axis_X_Delta		= Input.GetAxisRaw("Mouse X") * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );
		float	Axis_Y_Delta		= Input.GetAxisRaw("Mouse Y") * m_MouseSensitivity * ( ( isZoomed ) ? wpnZoomSensitivity : 1.0f );

		// Interpolate if m_SmoothedRotation is enabled
		float interpolant = m_SmoothedRotation ? ( Time.unscaledDeltaTime * ( 100f / m_SmoothFactor ) ) : 1f;
		m_CurrentRotation_X_Delta = Mathf.LerpUnclamped(m_CurrentRotation_X_Delta, Axis_X_Delta, interpolant );
		m_CurrentRotation_Y_Delta = Mathf.LerpUnclamped(m_CurrentRotation_Y_Delta, Axis_Y_Delta, interpolant );

		m_CurrentDirection.x = Utils.Math.Clamp(m_CurrentDirection.x - m_CurrentRotation_X_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS );
		m_CurrentDirection.y = m_CurrentDirection.y + m_CurrentRotation_Y_Delta;

		// Apply rotation
		{
			// Apply effects
			Vector3 DirectionPlusEffects = ( m_CurrentDirection + CameraEffectorsData.CameraEffectsDirection + currentWeapon.Deviation );

			// Horizonatal rotation
			m_ViewPointBody.localRotation = Quaternion.Euler( Vector3.up * DirectionPlusEffects.y );

			// Vertical rotation
			viewPoint.localRotation = Quaternion.Euler( Vector3.right * DirectionPlusEffects.x );
		}

		// rotation with effect added
		{
			m_RotationFeedback.Set( Axis_X_Delta, Axis_Y_Delta );
//			Vector3 finalWpnRotationFeedback = new Vector3( Axis_Y_Delta, Axis_X_Delta );
//			m_WpnRotationFeedback = Vector3.Lerp( m_WpnRotationFeedback, finalWpnRotationFeedback, deltaTime * 8f ).normalized * WPN_ROTATION_CLAMP_VALUE;
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

