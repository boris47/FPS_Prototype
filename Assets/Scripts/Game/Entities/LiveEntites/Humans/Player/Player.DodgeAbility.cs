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
		get { return m_CanDodge; }
	}

	private		bool				m_IsDodging						= false;
	public		bool				IsDodging
	{
		get { return m_IsDodging; }
	}
	private		bool				m_ChosingDodgeRotation			= false;
	public		bool				ChosingDodgeRotation
	{
		get { return m_ChosingDodgeRotation; }
	}

	private	Coroutine		m_DodgeCoroutine		= null;
	private	Vector3			m_DodgeRaycastNormal	= Vector3.zero;
	private	float			m_DodgeInterpolant		= 0f;

	private	bool	AbilityPredcate()
	{
		return m_GrabbedObject == null;
	}

	private	void	AbilityEnableAction()
	{
		OnDodgeAbilityEnable();
	}

	private	void	AbilityContinueAction()
	{
		OnDodgeAbilitySelection();
	}

	private	void	AbilityEndAction()
	{
		OnDodgeAbilityAction();
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
		bool condition = ( m_IsDodging == false && transform.up != Vector3.up ) &&
			m_States.IsJumping == false && m_States.IsHanging == false && IsGrounded == false; //		( m_States.IsJumping || IsGrounded == false );
		if ( condition )
		{
			RaycastHit hit;
			Physics.Raycast( transform.position, Vector3.down, out hit );

			if ( m_CutsceneManager.IsPlaying )
				m_CutsceneManager.Terminate();

			Vector3 alignedForward = Vector3.Cross( transform.right, Vector3.up );
			Quaternion finalRotation = Quaternion.LookRotation( alignedForward, Vector3.up );
			m_DodgeCoroutine = CoroutinesManager.Start( Dodge( destination: hit.point, rotation: finalRotation, falling: true ),
				"Player::CheckForFallOrUserBreak: Start of dodge" );
		}
		return condition;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilityEnable
	private	void	OnDodgeAbilityEnable()
	{
		float angle = Vector3.Angle( m_RaycastHit.normal, transform.up );
		bool validAngle = angle >= 89f && angle < 179f;

		if ( validAngle == true && m_ChosingDodgeRotation == false )
		{
			// return if 
			if ( m_DodgeRaycastNormal != Vector3.zero && m_DodgeRaycastNormal == m_RaycastHit.normal )
			{
				return;
			}

			// Enable dodge target object and scale time
			m_DodgeAbilityTarget.gameObject.SetActive( true );
			m_DodgeAbilityTarget.position	= m_RaycastHit.point;
			m_DodgeAbilityTarget.up			= m_RaycastHit.normal;
			m_DodgeRaycastNormal			= m_RaycastHit.normal;
			GlobalManager.SetTimeScale( SELECTION_TIME_SCALE );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilitySelection
	private	void	OnDodgeAbilitySelection()
	{
		if ( m_DodgeAbilityTarget.gameObject.activeSelf == true )
		{
			// "Abort" because pointing to a point with different normals
			if ( m_DodgeRaycastNormal != m_RaycastHit.normal )
			{
				m_ChosingDodgeRotation = false;
			}
			// pointing on the same surface
			else
			{
				m_ChosingDodgeRotation = true;

				// finding the point in the space where target must look at
				// Pojecting the hit point on same hitted surface plane at the height of dodge ability target
				Vector3 pointToFace = Utils.Math.ProjectPointOnPlane( m_RaycastHit.normal, m_DodgeAbilityTarget.position, m_RaycastHit.point );

				// if found point is different from the current dodge target position
				if ( pointToFace != m_DodgeAbilityTarget.position )
				{
					// Force dodge ability target to look at found point
					m_DodgeAbilityTarget.rotation = Quaternion.LookRotation( ( pointToFace - m_DodgeAbilityTarget.position ), m_RaycastHit.normal );
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnDodgeAbilityAction
	private	void	OnDodgeAbilityAction()
	{
		if ( m_ChosingDodgeRotation == true )
		{
			if ( m_DodgeCoroutine != null )
			{
				StopCoroutine( m_DodgeCoroutine );
			}

			// restore internals
			m_DodgeRaycastNormal = Vector3.zero;
			m_ChosingDodgeRotation = false;

			// restore time scale
			GlobalManager.SetTimeScale( 1.0f );

			// destination is on dodge target position
			Vector3 destination = m_DodgeAbilityTarget.position + m_DodgeAbilityTarget.up;
				
			// Allign actor body to camera vertical axis
			transform.Rotate( Vector3.up, CameraControl.Instance.CurrentDirection.y, Space.Self );

			// remove value of current rotation on veritcal axis of camera
			{
				Vector3 alias = CameraControl.Instance.CurrentDirection;
				alias.y = 0f;
				CameraControl.Instance.CurrentDirection = alias;
			}

			// hide dodge ability target object
			m_DodgeAbilityTarget.gameObject.SetActive( false );

			// finally start dodge coroutine
			m_DodgeCoroutine = CoroutinesManager.Start (
				Dodge (
					destination:		destination,
					rotation:			m_DodgeAbilityTarget.rotation,
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
		Vector3		startPosition					= transform.position;
		Quaternion	startRotation					= transform.rotation;
		Quaternion	finalRotation					= rotation;
		
		var		settings							= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		float	drag								= m_RigidBody.drag;
		m_DodgeInterpolant							= 0f;

		// Enabling dodge ability
		m_IsDodging									= true;

		// Setup
		UnityEngine.UI.Image effectFrame			= UIManager.EffectFrame;
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadBob>(false);
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadMove>(false);
		m_RigidBody.velocity						= Vector3.zero;
		m_RigidBody.constraints						= RigidbodyConstraints.None;

//		float slowMotionCoeff			= WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;
		AnimationCurve animationCurve	= ( ( dodgeTarget != null && dodgeTarget.HasTimeScaleCurveOverride ) ? dodgeTarget.DodgeTimeScaleCurve : m_DodgeTimeScaleCurve );
		while ( m_DodgeInterpolant < 1f )
		{
			// If is paused, wait for resume
			while ( GameManager.IsPaused == true )
				yield return null;

			m_DodgeInterpolant		+= Time.deltaTime;

			if ( bInstantly == true )
				m_DodgeInterpolant = 1.0f;

			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, m_DodgeInterpolant * 6f );

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate( m_DodgeInterpolant );// * slowMotionCoeff;
			if ( m_ChosingDodgeRotation == false )
				Time.timeScale			= ( falling == true ) ? Time.timeScale : timeScaleNow;
			
			// Position and Rotation
			if ( falling == false )
				transform.position	= Vector3.Lerp( startPosition, destination, m_DodgeInterpolant );
			transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, m_DodgeInterpolant * ( ( falling == true ) ? 4f : 1f ) );

			// Motion Blur Intensity
			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			// Audio Global Pitch
			SoundManager.Instance.Pitch = Time.timeScale;
			yield return null;
		}

		m_DodgeInterpolant							= 0f;

		// Reset
		m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity						= Vector3.zero;
		SoundManager.Instance.Pitch					= 1f;
		Time.timeScale								= 1f;
		effectFrame.color							= Color.clear;
		m_DodgeRaycastNormal						= Vector3.zero;
		m_DodgeAbilityTarget.gameObject.SetActive( false );
		m_ChosingDodgeRotation						= false;

		// Final Position and Rotation
		if ( falling == false )
			transform.position						= destination;
		transform.rotation							= finalRotation;

		// Disabling dodge ability
		m_IsDodging									= false;

		// Camera Reset
		CameraControl.Instance.OnCutsceneEnd();
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadBob>(true);
		CameraControl.Instance.CameraEffectorsManager.SetEffectorState<HeadMove>(true);

		SetMotionType( eMotionType.Walking );

		m_DodgeCoroutine = null;
	}

}