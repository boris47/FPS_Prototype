
using UnityEngine;
using UnityEngine.PostProcessing;

public class FPSEntityCamera : CameraBase
{
	public		const		float				CLAMP_MAX_X_AXIS						=  70.0f;
	public		const		float				CLAMP_MIN_X_AXIS						= -70.0f;

	private		static		FPSEntityCamera		m_Instance								= null;
	public		static		FPSEntityCamera		Instance								=> m_Instance;

	[System.Serializable]
	private class CameraSectionData
	{
		public	float	ViewDistance			= 500f;
	}

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


	[SerializeField]
	private		CameraSectionData				m_CameraSectionData						= new CameraSectionData();

	private		float							m_CurrentRotation_X_Delta				= 0.0f;
	private		float							m_CurrentRotation_Y_Delta				= 0.0f;
	private		float							m_CurrentAngle_X						= 0.0f;

	public		Transform						Target									=> m_Target;

	//////////////////////////////////////////////////////////////////////////
	protected override	void	Awake()
	{
		transform.TryGetChildPath(out string pathToChild, t => t.name == "a_flap");

		// Singleton
		if (Instance.IsNotNull())
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

		if (CustomAssertions.IsTrue(GlobalManager.Configs.TryGetSection("Camera", out Database.Section cameraSection) && GlobalManager.Configs.TrySectionToOuter(cameraSection, m_CameraSectionData)))
		{
			EffectorActiveCondition mainCondition = () => Player.Instance.Motion.CanMove;
			m_CameraEffectorsManager.AddCondition<HeadBob>(mainCondition + (() => Player.Instance.Motion.IsMoving));
			m_CameraEffectorsManager.AddCondition<HeadMove>(mainCondition + (() => !Player.Instance.Motion.IsMoving));

			m_CameraRef.farClipPlane = m_CameraSectionData.ViewDistance;
		}

		OutlineEffectManager.SetEffectCamera(m_CameraRef);

		if (CustomAssertions.IsNotNull(GameManager.UpdateEvents))
		{
			GameManager.UpdateEvents.OnLateFrame += OnLateFrame;
		}
	}
	

	//////////////////////////////////////////////////////////////////////////
	protected override void OnDisable()
	{
		if (GameManager.UpdateEvents.IsNotNull())
		{
			GameManager.UpdateEvents.OnLateFrame -= OnLateFrame;
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
		CustomAssertions.IsNotNull(entity);
		CustomAssertions.IsNotNull(entity.Head);
		CustomAssertions.IsNotNull(entity.Body);

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
	private	void OnLateFrame(float deltaTime)
	{
		var weaponManagerInstance = WeaponManager.Instance;

		float Axis_X_Delta = 0.0f, Axis_Y_Delta = 0.0f;
		if (m_Target)
		{
			Entity.GetRotationsToPoint(m_Entity.Body, m_Entity.Head, m_Target.position, out Axis_Y_Delta, out Axis_X_Delta);
		}
		else if (GlobalManager.InputMgr.HasCategoryEnabled(EInputCategory.CAMERA))
		{
			bool	isZoomed			= weaponManagerInstance.IsZoomed;
			float	wpnZoomSensitivity  = weaponManagerInstance.ZoomSensitivity;
			Axis_X_Delta = Input.GetAxisRaw("Mouse X") * m_MouseSensitivity * ((isZoomed) ? wpnZoomSensitivity : 1.0f);
			Axis_Y_Delta = Input.GetAxisRaw("Mouse Y") * m_MouseSensitivity * ((isZoomed) ? wpnZoomSensitivity : 1.0f);
		}

		Vector3 finalWpnRotationFeedback = new Vector3(0.0f, Axis_Y_Delta, 0.0f);
		if (!Mathf.Approximately(Axis_X_Delta, Mathf.Epsilon) || !Mathf.Approximately(Axis_Y_Delta, Mathf.Epsilon))
		{
			// Interpolate if m_SmoothedRotation is enabled
			if (m_SmoothedRotation)
			{
				float interpolant = Time.unscaledDeltaTime * (100f / m_SmoothFactor);
				m_CurrentRotation_X_Delta = Mathf.LerpUnclamped(m_CurrentRotation_X_Delta, Axis_X_Delta, interpolant);
				m_CurrentRotation_Y_Delta = Mathf.LerpUnclamped(m_CurrentRotation_Y_Delta, Axis_Y_Delta, interpolant);
			}
			else
			{
				m_CurrentRotation_X_Delta = Axis_X_Delta;
				m_CurrentRotation_Y_Delta = Axis_Y_Delta;
			}


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
					head.Rotate(Vector3.right, -m_CurrentRotation_X_Delta);
				}
			}
		}

		// Effectors
		{
			var currentWeapon = WeaponManager.Instance.CurrentWeapon;
			var weaponPivot = WeaponPivot.Instance;
			CameraEffectorData CameraEffectorsData = m_CameraEffectorsManager.CameraEffectorsData;

			// Apply effectors (Just Camera)
			transform.localRotation = Quaternion.Euler(CameraEffectorsData.CameraEffectsDirection + weaponPivot.Dispersion);

			// Weapon
			{
				if (!weaponManagerInstance.IsZoomed && !weaponManagerInstance.IsChangingZoom)
				{
					// Axis_X_Delta -> vertical axis, Axis_Y_Delta -> horizontal axis
					weaponPivot.AddRotationFeedBack(finalWpnRotationFeedback * deltaTime);
				}

				// Weapon Local Position
				currentWeapon.Transform.localPosition = CameraEffectorsData.WeaponPositionDelta + (Vector3.left * weaponPivot.Recoil);

				// Weapon Local Rotation
				currentWeapon.Transform.localEulerAngles = CameraEffectorsData.WeaponDirectionDelta + weaponPivot.RotationFeedback;

				// Optic sight alignment
				if (weaponManagerInstance.IsZoomed)
				{
			//		Vector3 distantPoint = transform.position + currentWeapon.Transform.right;
			//		Vector2 delta = m_CameraRef.WorldToScreenPoint(distantPoint);

					float frameFeedBack = 1.0f + weaponPivot.Recoil; // 1.0f + Because is scale factor
					UIManager.InGame.FrameFeedBack(frameFeedBack/*, delta*/);
				}
			}
		}
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

