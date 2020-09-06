using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_SPEED_FACTOR		= 0.5f;
	private	const	float	SELECTION_TIME_SCALE	= 0.008f;

	// DODGING

	[Header("Dodge Properties")]

	[SerializeField]
	private		AnimationCurve		m_DodgeTimeScaleCurve			= AnimationCurve.Linear( 0f, 1f, 1f, 1f );

	private		Transform			m_DodgeAbilityTarget			= null;

	private		bool				m_CanDodge						= false;
	public		bool				CanDodge
	{
		get { return this.m_CanDodge; }
	}

	private		bool				m_IsDodging						= false;
	public		bool				IsDodging
	{
		get { return this.m_IsDodging; }
	}
	private		bool				m_ChosingDodgeRotation			= false;
	public		bool				ChosingDodgeRotation
	{
		get { return this.m_ChosingDodgeRotation; }
	}

	private	Coroutine		m_DodgeCoroutine		= null;
	private	Vector3			m_DodgeRaycastNormal	= Vector3.zero;
	private	float			m_DodgeInterpolant		= 0f;

	private	bool	AbilityPredcate()
	{
		return this.m_GrabbedObject == null;
	}

	private	void	AbilityEnableAction()
	{
		this.OnDodgeAbilityEnable();
	}

	private	void	AbilityContinueAction()
	{
		this.OnDodgeAbilitySelection();
	}

	private	void	AbilityEndAction()
	{
		this.OnDodgeAbilityAction();
	}

	/*
	//////////////////////////////////////////////////////////////////////////
	// CheckForDash
	private	void	CheckForDodge( bool hasInteractiveRayCastHit )
	{
		// forbidden if currently grabbing an object
		if ( m_GrabbedObject != null )
			return;

		// if actually has no hit so reset dodge target
		if ( hasInteractiveRayCastHit == false )
		{
			if ( m_DodgeAbilityTarget.gameObject.activeSelf == true )
			{
				m_DodgeAbilityTarget.gameObject.SetActive( false );
				m_DodgeRaycastNormal = Vector3.zero;
				GameManager.SetTimeScale( 1.0f );
			}
			m_ChosingDodgeRotation = false;
		}
	}
	*/

	//////////////////////////////////////////////////////////////////////////
	// CheckForFallOrUserBreck
	public bool	CheckForFallOrUserBreak()
	{
		bool condition = (this.m_IsDodging == false && this.transform.up != Vector3.up ) &&
			this.m_States.IsJumping == false && this.m_States.IsHanging == false && this.IsGrounded == false; //		( m_States.IsJumping || IsGrounded == false );
		if ( condition )
		{
			Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit );

			if (this.m_CutsceneManager.IsPlaying )
				this.m_CutsceneManager.Terminate();

			Vector3 alignedForward = Vector3.Cross(this.transform.right, Vector3.up );
			Quaternion finalRotation = Quaternion.LookRotation( alignedForward, Vector3.up );
			this.m_DodgeCoroutine = CoroutinesManager.Start(this.Dodge( destination: hit.point, rotation: finalRotation, falling: true ),
				"Player::CheckForFallOrUserBreak: Start of dodge" );
		}
		return condition;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilityEnable
	private	void	OnDodgeAbilityEnable()
	{
		float angle = Vector3.Angle(this.m_RaycastHit.normal, this.transform.up );
		bool validAngle = angle >= 89f && angle < 179f;

		if ( validAngle == true && this.m_ChosingDodgeRotation == false )
		{
			// return if 
			if (this.m_DodgeRaycastNormal != Vector3.zero && this.m_DodgeRaycastNormal == this.m_RaycastHit.normal )
			{
				return;
			}

			// Enable dodge target object and scale time
			this.m_DodgeAbilityTarget.gameObject.SetActive( true );
			this.m_DodgeAbilityTarget.position	= this.m_RaycastHit.point;
			this.m_DodgeAbilityTarget.up			= this.m_RaycastHit.normal;
			this.m_DodgeRaycastNormal			= this.m_RaycastHit.normal;
			GlobalManager.SetTimeScale( SELECTION_TIME_SCALE );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilitySelection
	private	void	OnDodgeAbilitySelection()
	{
		if (this.m_DodgeAbilityTarget.gameObject.activeSelf == true )
		{
			// "Abort" because pointing to a point with different normals
			if (this.m_DodgeRaycastNormal != this.m_RaycastHit.normal )
			{
				this.m_ChosingDodgeRotation = false;
			}
			// pointing on the same surface
			else
			{
				this.m_ChosingDodgeRotation = true;

				// finding the point in the space where target must look at
				// Pojecting the hit point on same hitted surface plane at the height of dodge ability target
				Vector3 pointToFace = Utils.Math.ProjectPointOnPlane(this.m_RaycastHit.normal, this.m_DodgeAbilityTarget.position, this.m_RaycastHit.point );

				// if found point is different from the current dodge target position
				if ( pointToFace != this.m_DodgeAbilityTarget.position )
				{
					// Force dodge ability target to look at found point
					this.m_DodgeAbilityTarget.rotation = Quaternion.LookRotation( ( pointToFace - this.m_DodgeAbilityTarget.position ), this.m_RaycastHit.normal );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilityAction
	private	void	OnDodgeAbilityAction()
	{
		if (this.m_ChosingDodgeRotation == true )
		{
			if (this.m_DodgeCoroutine != null )
			{
				this.StopCoroutine(this.m_DodgeCoroutine );
			}

			// restore internals
			this.m_DodgeRaycastNormal = Vector3.zero;
			this.m_ChosingDodgeRotation = false;

			// restore time scale
			GlobalManager.SetTimeScale( 1.0f );

			// destination is on dodge target position
			Vector3 destination = this.m_DodgeAbilityTarget.position + this.m_DodgeAbilityTarget.up;

			// Allign actor body to camera vertical axis
			this.transform.Rotate( Vector3.up, CameraControl.Instance.CurrentDirection.y, Space.Self );

			// remove value of current rotation on veritcal axis of camera
			{
				Vector3 alias = CameraControl.Instance.CurrentDirection;
				alias.y = 0f;
				CameraControl.Instance.CurrentDirection = alias;
			}

			// hide dodge ability target object
			this.m_DodgeAbilityTarget.gameObject.SetActive( false );

			// finally start dodge coroutine
			this.m_DodgeCoroutine = CoroutinesManager.Start (
				this.Dodge (
					destination:		destination,
					rotation: this.m_DodgeAbilityTarget.rotation,
					falling :			false,
					dodgeTarget :		null,
					bInstantly :		false
				),
				"Player::OnDodgeAbilityAction: Start of dodge"
			);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Dodge ( Coroutine )
	private	IEnumerator	Dodge( Vector3 destination, Quaternion rotation, bool falling = false, DodgeTarget dodgeTarget = null, bool bInstantly = false )
	{
		Vector3		startPosition					= this.transform.position;
		Quaternion	startRotation					= this.transform.rotation;
		Quaternion	finalRotation					= rotation;

		UnityEngine.PostProcessing.MotionBlurModel.Settings settings							= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		float	drag								= this.m_RigidBody.drag;
		this.m_DodgeInterpolant							= 0f;

		// Enabling dodge ability
		this.m_IsDodging									= true;

		// Setup
		UnityEngine.UI.Image effectFrame			= UIManager.EffectFrame;
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadBob>(false);
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadMove>(false);
		this.m_RigidBody.velocity						= Vector3.zero;
		this.m_RigidBody.constraints						= RigidbodyConstraints.None;

//		float slowMotionCoeff			= WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;
		AnimationCurve animationCurve	= ( ( dodgeTarget != null && dodgeTarget.HasTimeScaleCurveOverride ) ? dodgeTarget.DodgeTimeScaleCurve : this.m_DodgeTimeScaleCurve );
		while (this.m_DodgeInterpolant < 1f )
		{
			// If is paused, wait for resume
			while ( GameManager.IsPaused == true )
				yield return null;

			this.m_DodgeInterpolant		+= Time.deltaTime;

			if ( bInstantly == true )
				this.m_DodgeInterpolant = 1.0f;

			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, this.m_DodgeInterpolant * 6f );

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate(this.m_DodgeInterpolant );// * slowMotionCoeff;
			if (this.m_ChosingDodgeRotation == false )
				Time.timeScale			= ( falling == true ) ? Time.timeScale : timeScaleNow;
			
			// Position and Rotation
			if ( falling == false )
				this.transform.position	= Vector3.Lerp( startPosition, destination, this.m_DodgeInterpolant );
			this.transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, this.m_DodgeInterpolant * ( ( falling == true ) ? 4f : 1f ) );

			// Motion Blur Intensity
			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			// Audio Global Pitch
			SoundManager.Pitch = Time.timeScale;
			yield return null;
		}

		this.m_DodgeInterpolant							= 0f;

		// Reset
		this.m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		this.m_RigidBody.velocity						= Vector3.zero;
		SoundManager.Pitch					= 1f;
		Time.timeScale								= 1f;
		effectFrame.color							= Color.clear;
		this.m_DodgeRaycastNormal						= Vector3.zero;
		this.m_DodgeAbilityTarget.gameObject.SetActive( false );
		this.m_ChosingDodgeRotation						= false;

		// Final Position and Rotation
		if ( falling == false )
			this.transform.position						= destination;
		this.transform.rotation							= finalRotation;

		// Disabling dodge ability
		this.m_IsDodging									= false;

		// Camera Reset
		CameraControl.Instance.OnCutsceneEnd();
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadBob>(true);
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadMove>(true);

		this.SetMotionType( EMotionType.Walking );

		this.m_DodgeCoroutine = null;
	}

}