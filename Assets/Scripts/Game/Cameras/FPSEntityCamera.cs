﻿
using UnityEngine;
using UnityEngine.PostProcessing;

public class FPSEntityCamera : CameraBase
{
	private		const		float				WPN_ROTATION_CLAMP_VALUE				= 0.28f;
	private		const		float				WPN_FALL_FEEDBACK_CLAMP_VALUE			= 5f;

	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	public		static		FPSEntityCamera		m_Instance								= null;
	public		static		FPSEntityCamera		Instance								=> m_Instance;

	protected override		string				PostProcessResourcePath					=> "Scriptables/CameraPostProcesses";

	[Header("FPS Camera")]

	[SerializeField, ReadOnly]
	private		float							m_MouseSensitivity						= 1.0f;

	[SerializeField]
	private		bool							m_SmoothedRotation						= true;

	[SerializeField, ReadOnly]
	private		float							m_SmoothFactor							= 1.0f;

	[SerializeField, ReadOnly]
	private		Entity							m_Entity								= null;

	[SerializeField]
	private		Transform						m_Target								= null;
	public		Transform						Target									=> m_Target;

	[SerializeField]
	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;
	[SerializeField]
	private		float							m_CurrentAngle_X						= 0.0f;


	[System.Serializable]
	private class CameraSectionData
	{
		public	float	ViewDistance			= 500f;
	}

	[SerializeField]
	private		CameraSectionData				m_CameraSectionData						= new CameraSectionData();

	//////////////////////////////////////////////////////////////////////////
	protected override	void	Awake()
	{
		// Singleton
		if (Instance != null)
		{
			print("Instance already found");
			gameObject.SetActive(false);
			return;
		}
		m_Instance = this;

		base.Awake();
	}

	//////////////////////////////////////////////////////////////////////////
	protected override void OnEnable()
	{
		base.OnEnable();

		if (!GlobalManager.Configs.TryGetSection("Camera", out Database.Section cameraSection) || !GlobalManager.Configs.TrySectionToOuter(cameraSection, m_CameraSectionData))
		{
			UnityEngine.Assertions.Assert.IsTrue(false, "Cannot correctly enable the camera");
		}
		else
		{
			EffectorActiveCondition mainCondition = () => Player.Instance.Motion.CanMove;
			m_CameraEffectorsManager.AddCondition<HeadBob>(mainCondition + (() => Player.Instance.Motion.IsMoving));
			m_CameraEffectorsManager.AddCondition<HeadMove>(mainCondition + (() => !Player.Instance.Motion.IsMoving));

			m_CameraRef.farClipPlane = m_CameraSectionData.ViewDistance;
		}

		UnityEngine.Assertions.Assert.IsNotNull(GameManager.StreamEvents);

		GameManager.StreamEvents.OnSave += OnSave;
		GameManager.StreamEvents.OnLoad += OnLoad;

		OutlineEffectManager.SetEffectCamera(m_CameraRef);
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		if ( GameManager.StreamEvents.IsNotNull() )
		{
			GameManager.StreamEvents.OnSave -= OnSave;
			GameManager.StreamEvents.OnLoad -= OnLoad;
		}
		OutlineEffectManager.SetEffectCamera( null );

		base.OnDisable();
	}


	//////////////////////////////////////////////////////////////////////////
	private	bool	OnSave( StreamData streamData, ref StreamUnit streamUnit )
	{
		streamUnit = streamData.NewUnit(gameObject );

		// Current internal direction
	//	streamUnit.SetInternal( "CurrentDirection", Utils.Converters.Vector3ToString(m_CurrentDirection) );

		// Can parse input
		streamUnit.SetInternal( "CanParseInput", GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA) );
		
		// TODO load effectors

		return true;
	}


	//////////////////////////////////////////////////////////////////////////
	private	bool	OnLoad( StreamData streamData, ref StreamUnit streamUnit )
	{
		bool bResult = streamData.TryGetUnit(gameObject, out streamUnit);
		if ( bResult )
		{
			MotionBlurModel.Settings settings = m_PP_Profile.motionBlur.settings;
			settings.frameBlending = 0f;
			m_PP_Profile.motionBlur.settings = settings;

			// Current internal direction
			//	m_CurrentDirection = streamUnit.GetAsVector( "CurrentDirection" );

			// Can parse input
			GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, streamUnit.GetAsBool( "CanParseInput" ));

			// TODO Save Effectors
		}
		return bResult;
	}


	/// <summary> Enable or disable the smooth effecton the rotation of this camera </summary>
	//////////////////////////////////////////////////////////////////////////
	public void		SetSmooth(bool bIsEnabled)
	{
		m_SmoothedRotation = bIsEnabled;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> The value will be clamper in a [0.001f - 20f] range </summary>
	public void		SetSmoothFactor(float newFactor)
	{
		m_SmoothFactor = Mathf.Clamp(newFactor, 1.0f, 20.0f);
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnOwnerDeath(Entity entitykilled)
	{
		// reset effect
		VignetteModel.Settings settings = m_PP_Profile.vignette.settings;
		settings.intensity = 0f;
		m_PP_Profile.vignette.settings = settings;
	}


	//////////////////////////////////////////////////////////////////////////
	public void		SetViewPoint(Entity entity)
	{
		UnityEngine.Assertions.Assert.IsNotNull(entity);
		UnityEngine.Assertions.Assert.IsNotNull(entity.Head);
		UnityEngine.Assertions.Assert.IsNotNull(entity.Body);

		if (m_Entity)
		{
			m_Entity.OnEvent_Killed -= OnOwnerDeath;
		}

		m_Entity = entity;

		transform.SetParent(entity.Head);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;

		m_Entity.OnEvent_Killed += OnOwnerDeath;
	}

	//////////////////////////////////////////////////////////////////////////
	public void		SetTarget(Transform target)
	{
		m_Target = target;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public void		OnCutsceneEnd()
	{
		SetTarget(null);
	}

	//////////////////////////////////////////////////////////////////////////
	private	void	LateUpdate()
	{
		CameraEffectorData CameraEffectorsData = m_CameraEffectorsManager.CameraEffectorsData;
		IWeapon currentWeapon = WeaponManager.Instance.CurrentWeapon;

		// Weapon Local Position
		currentWeapon.Transform.localPosition = CameraEffectorsData.WeaponPositionDelta + ( Vector3.left * currentWeapon.Recoil * 0.1f );

		// Headbob effect on weapon
		Vector3 headbobEffectOnWeapon = Vector3.zero;
		if (m_CameraEffectorsManager.TryGetEffectorData<HeadBob>(out CameraEffectorData data))
		{
			headbobEffectOnWeapon = data.CameraEffectsDirection * -1f;
		}

		// Weapon Local Rotation
		currentWeapon.Transform.localEulerAngles = Vector3.zero
			+ CameraEffectorsData.WeaponDirectionDelta // like headbob
			+ currentWeapon.Dispersion				// Delta for each shoot
			+ currentWeapon.FallFeedback
			+ currentWeapon.RotationFeedback
			+ headbobEffectOnWeapon
		;

		// Optic sight alignment
		if (WeaponManager.Instance.IsZoomed)
		{
			Vector3 distantPoint = currentWeapon.Transform.position + (currentWeapon.Transform.right * 1000f);
			Vector2 delta = m_CameraRef.WorldToScreenPoint(distantPoint);

			float frameFeedBack = 1.0f + currentWeapon.Recoil; // 1.0f + Because is scale factor
			UIManager.InGame.FrameFeedBack(frameFeedBack, delta);
		}

		float Axis_X_Delta = 0.0f, Axis_Y_Delta = 0.0f;
		if (m_Target)
		{
			Entity.GetRotationsToPoint(m_Entity.Body, m_Entity.Head, m_Target.position, out Axis_Y_Delta, out Axis_X_Delta);

			/*
			// Rotate the body
			{
				Vector3 projctedPoint = Utils.Math.ProjectPointOnPlane(body.up, body.position, targetPosition);
				Vector3 directionToPoint = (projctedPoint - body.position);
				Axis_Y_Delta = Vector3.SignedAngle(body.forward, directionToPoint, body.up);
			}

			// Rotate the head
			{
				Vector3 directionToPoint = (targetPosition - head.position);
				float angle = Vector3.Angle(body.up, directionToPoint);
				Axis_X_Delta = Mathf.DeltaAngle(angle, m_CurrentAngle_X) + 90f; // +90 because of m_EntityBody.up
			}
			*/
		}
		else if (GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA))
		{
			bool	isZoomed			= WeaponManager.Instance.IsZoomed;
			float	wpnZoomSensitivity  = WeaponManager.Instance.ZoomSensitivity;
			Axis_X_Delta = Input.GetAxisRaw("Mouse X") * m_MouseSensitivity * ((isZoomed) ? wpnZoomSensitivity : 1.0f);
			Axis_Y_Delta = Input.GetAxisRaw("Mouse Y") * m_MouseSensitivity * ((isZoomed) ? wpnZoomSensitivity : 1.0f);
		}

		if (!Mathf.Approximately(Axis_X_Delta, Mathf.Epsilon) || !Mathf.Approximately(Axis_Y_Delta, Mathf.Epsilon))
		{
			// Interpolate if m_SmoothedRotation is enabled
			float interpolant = m_SmoothedRotation ? (Time.unscaledDeltaTime * (100f / m_SmoothFactor)) : 1f;
			m_CurrentRotation_X_Delta = Mathf.LerpUnclamped(m_CurrentRotation_X_Delta, Axis_X_Delta, interpolant);
			m_CurrentRotation_Y_Delta = Mathf.LerpUnclamped(m_CurrentRotation_Y_Delta, Axis_Y_Delta, interpolant);

			Vector3 finalWpnRotationFeedback = new Vector3(0.0f, Axis_Y_Delta, 0.0f);

			Transform head = m_Entity.Head;
			Transform body = m_Entity.transform; // m_Entity.Body is a sub-gameObject, rotating it does not affect the entity rotation itself

			// Apply rotation
			{
				// Horizontal rotation (Body)
				body.Rotate(Vector3.up, m_CurrentRotation_Y_Delta, Space.Self);

				// Vertical rotation (Head)
				if (Utils.Math.ClampResult(ref m_CurrentAngle_X, m_CurrentAngle_X - m_CurrentRotation_X_Delta, CLAMP_MIN_X_AXIS, CLAMP_MAX_X_AXIS))
				{
					finalWpnRotationFeedback.z = Axis_X_Delta;
				}
				head.localRotation = Quaternion.Euler(Vector3.right * m_CurrentAngle_X);
			}

			// Rotation with effect added
			{
				// Axis_X_Delta -> vertical axis, Axis_Y_Delta -> horizontal axis
				Vector3 finalWpnRotationFeedbackClamped = Vector3.ClampMagnitude(finalWpnRotationFeedback, WPN_ROTATION_CLAMP_VALUE);
				currentWeapon.AddRotationFeedBack(finalWpnRotationFeedbackClamped);
			}
		}

		// Apply effectors (Just Camera)
		m_CameraRef.transform.localRotation = Quaternion.Euler(CameraEffectorsData.CameraEffectsDirection + currentWeapon.Deviation);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		if ((Object)m_Instance != this)
			return;

		m_Instance = null;

		base.OnDestroy();
	}

}

