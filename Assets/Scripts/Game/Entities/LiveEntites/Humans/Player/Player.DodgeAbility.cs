using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_SPEED_FACTOR		= 0.5f;
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
//	private		DodgeTarget			m_CurrentDodgeTarget			= null;
//	private		DodgeTarget			m_PreviousDodgeTarget			= null;


	private	Coroutine		m_DodgeCoroutine		= null;
	private	Vector3			m_DodgeRaycastNormal	= Vector3.zero;
	private	float			m_DodgeInterpolant		= 0f;

	
	/*
	//////////////////////////////////////////////////////////////////////////
	// FindFinalRotation
	private	Quaternion	FindFinalRotation( Vector3 startPosition, Vector3 destination, Vector3 destinationUp, bool falling )
	{
		Quaternion finalRotation = transform.rotation;
		
//		if ( target != null )
//		{
//			Vector3 alignedForward = Vector3.Cross( CameraControl.Instance.transform.right, target.transform.up );
//			finalRotation = Quaternion.LookRotation( alignedForward, target.transform.up );
//		}
//		else 
		if ( falling == true )
		{
			Vector3 alignedForward = Vector3.Cross( transform.right, Vector3.up );
			finalRotation = Quaternion.LookRotation( alignedForward, destinationUp );
		}
		else if ( m_IsDodging == true )
		{
			Vector3 alignedForward = Vector3.Cross( CameraControl.Instance.transform.right, destinationUp );
			finalRotation = Quaternion.LookRotation( alignedForward, destinationUp );
		}
		else
		{  
			Vector3 alignedPoint = Utils.Math.ProjectPointOnPlane( planeNormal: destinationUp, planePoint: startPosition, point: destination );
			finalRotation = Quaternion.LookRotation( alignedPoint - startPosition, destinationUp );
		}

		return finalRotation;
	}
	*/
	/*
	//////////////////////////////////////////////////////////////////////////
	// Dodge ( Coroutine )
	private	IEnumerator	Dodge( Vector3 destination, Vector3 destinationUp, bool falling = false, DodgeTarget target = null )
	{
		Vector3		startPosition					= transform.position;
		Quaternion	startRotation					= transform.rotation;
		Quaternion	finalRotation					= FindFinalRotation( startPosition, destination, destinationUp, falling );
		
		float	currentTime							= 0f;
		float	interpolant							= 0f;
		var		settings							= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		float	drag								= m_RigidBody.drag;

		// Enabling dodge ability
		m_IsDodging									= true;

		// Setup
		UnityEngine.UI.Image effectFrame			= UI.Instance.InGame.GetEffectFrame();
		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;
		m_RigidBody.velocity						= Vector3.zero;
		m_RigidBody.constraints						= RigidbodyConstraints.None;
		m_RigidBody.drag							= 0f;

		float slowMotionCoeff			= WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;
		AnimationCurve animationCurve	= ( ( target != null && target.HasTimeScaleCurveOverride ) ? target.DodgeTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime				+= Time.deltaTime;
			interpolant				= currentTime * DASH_SPEED_FACTOR * ( ( falling == true ) ? 5f : 1f );

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate( interpolant ) * slowMotionCoeff;
			Time.timeScale			= ( falling == true ) ? Time.timeScale : timeScaleNow;
			
			// Position and Rotation
			if ( falling == false )
				transform.position	= Vector3.Lerp( startPosition, destination, interpolant );
			transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, interpolant * ( ( falling == true ) ? 4f : 1f ) );

			// Motion Blur Intensity
			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			// Audio Global Pitch
			SoundEffectManager.Instance.Pitch = Time.timeScale;
			yield return null;
		}


		// Reset
		m_RigidBody.constraints						= RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity						= Vector3.zero;
		m_RigidBody.drag							= drag;
		SoundEffectManager.Instance.Pitch			= 1f;
		Time.timeScale								= 1f;
		effectFrame.color							= Color.clear;

		// Final Position and Rotation
		if ( falling == false )
			transform.position						= destination;
		transform.rotation							= finalRotation;

		// Disabling dodge ability
		m_IsDodging									= false;

		// Camera Reset
		CameraControl.Instance.OnCutsceneEnd();
		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;
	}

	*/


	
	//////////////////////////////////////////////////////////////////////////
	// CheckForDash
	private	void	CheckForDodge( bool hasHit )
	{
		// forbidden if currently grabbing an object
		if ( m_GrabbedObject != null )
			return;

		// Auto fall or user input
		if ( ( m_IsDodging == false && transform.up != Vector3.up ) && ( InputManager.Inputs.Jump || IsGrounded == false ) && m_DodgeCoroutine == null )
		{
			RaycastHit hit;
			Physics.Raycast( transform.position, Vector3.down, out hit );

			if ( m_CutsceneManager.IsPlaying )
				m_CutsceneManager.Termiante();

			Vector3 alignedForward = Vector3.Cross( transform.right, Vector3.up );
			Quaternion finalRotation = Quaternion.LookRotation( alignedForward, Vector3.up );
			m_DodgeCoroutine = StartCoroutine( Dodge( destination: hit.point, rotation: finalRotation, falling: true ) );
			return;
		}

		// if actually has no hit
		if ( hasHit == false )
		{
/*			// if required reset last target
			if ( m_CurrentDodgeTarget != null )
			{
				m_CurrentDodgeTarget.HideText();
				m_CurrentDodgeTarget = null;
			}
*/
			if ( m_DodgeAbilityTarget.gameObject.activeSelf == true )
			{
				m_DodgeAbilityTarget.gameObject.SetActive( false );
				m_DodgeRaycastNormal = Vector3.zero;
				SoundManager.Instance.Pitch = Time.timeScale = 1f;
			}
			m_ChosingDodgeRotation = false;
			return;
		}
		/*
		// Checking for dodge target
		DodgeTarget currentDodgeTarget = null;
		if ( hasHit == true )
		{
			currentDodgeTarget = m_RaycastHit.transform.GetComponent<DodgeTarget>();

			// Hitting something that is not a dodge target but previously it was
			if ( currentDodgeTarget == null && m_CurrentDodgeTarget != null )
			{
				m_CurrentDodgeTarget.HideText();
				m_CurrentDodgeTarget = null;
				m_ChosingDodgeRotation = false;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 1f;
				return;
			}
		}

		// this is ad odge target and is different from previous value
		if ( currentDodgeTarget != m_CurrentDodgeTarget )
		{
			// First target
			if ( currentDodgeTarget != null && m_CurrentDodgeTarget == null )
			{
				m_CurrentDodgeTarget = currentDodgeTarget;
				m_CurrentDodgeTarget.ShowText();
			}
			// New hit
			if ( currentDodgeTarget != null && m_CurrentDodgeTarget != null && currentDodgeTarget != m_CurrentDodgeTarget )
			{
				m_CurrentDodgeTarget.HideText();
				currentDodgeTarget.ShowText();
				m_CurrentDodgeTarget = currentDodgeTarget;
			}
			// No hit, reset previous
			if ( currentDodgeTarget == null && m_CurrentDodgeTarget != null )
			{
				m_CurrentDodgeTarget.HideText();
				m_CurrentDodgeTarget = null;
			}
		}


		/////////////////////////////////////////////////////
		//////////////  DODGE TARGET POINTED ////////////////
		/////////////////////////////////////////////////////

		if ( m_CurrentDodgeTarget != null )
		{
			if ( InputManager.Inputs.Ability1 && m_ChosingDodgeRotation == false && m_CurrentDodgeTarget != null )		// GetKey Q
			{
				m_DodgeAbilityTarget.gameObject.SetActive( true );
				m_DodgeAbilityTarget.position = m_CurrentDodgeTarget.transform.position;
				m_DodgeAbilityTarget.rotation = m_CurrentDodgeTarget.transform.rotation;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 0.001f;
			}
			if ( InputManager.Inputs.Ability1Loop && m_DodgeAbilityTarget.gameObject.activeSelf == true )				// GetKeyDown Q
			{
				m_ChosingDodgeRotation = true;
			}

			if ( InputManager.Inputs.Ability1Released && m_ChosingDodgeRotation == true )								// GetKeyUp Q
			{
				if ( m_RotorDashCoroutine != null )
					StopCoroutine( m_RotorDashCoroutine );

				SoundEffectManager.Instance.Pitch = Time.timeScale = 1f;
				Vector3 destination = m_CurrentDodgeTarget.transform.position;
				
				transform.Rotate( Vector3.up, CameraControl.Instance.m_CurrentDirection.y, Space.Self );
				CameraControl.Instance.m_CurrentDirection.y = 0f;

				if ( m_PreviousDodgeTarget != null && m_PreviousDodgeTarget != m_CurrentDodgeTarget )
				{
					m_PreviousDodgeTarget.OnReset();
				}
				m_PreviousDodgeTarget = m_CurrentDodgeTarget;

				m_CurrentDodgeTarget.Disable();
				m_CurrentDodgeTarget.HideText();

				m_DodgeAbilityTarget.gameObject.SetActive( false );
				m_RotorDashCoroutine = StartCoroutine
				(
					Dodge
					(
						destination: destination,
						rotation: m_CurrentDodgeTarget.transform.rotation,
						falling: false,
						dodgeTarget: m_CurrentDodgeTarget
					)
				);
				m_ChosingDodgeRotation = false;
			}
		}
		*/

		/////////////////////////////////////////////////////
		///////////// NO DODGE TARGET POINTED ///////////////
		/////////////////////////////////////////////////////

		// hitting somewhere else
//		if ( m_CurrentDodgeTarget == null )
		{
			float angle = Vector3.Angle( m_RaycastHit.normal, transform.up );
			bool validAngle = angle >= 89f && angle < 179f;
			if ( InputManager.Inputs.Ability1 && validAngle == true && m_ChosingDodgeRotation == false )        // GetKeyDown Q one frame
			{
				if ( m_DodgeRaycastNormal != Vector3.zero && m_DodgeRaycastNormal == m_RaycastHit.normal )
				{
					return;
				}

				m_DodgeAbilityTarget.gameObject.SetActive( true );
				m_DodgeAbilityTarget.position = m_RaycastHit.point;
				m_DodgeAbilityTarget.up = m_RaycastHit.normal;
				m_DodgeRaycastNormal = m_RaycastHit.normal;
				SoundManager.Instance.Pitch = Time.timeScale = 0.008f;
			}

			if ( InputManager.Inputs.Ability1Loop && m_DodgeAbilityTarget.gameObject.activeSelf == true )		// GetKey Q more frames
			{
				if ( m_DodgeRaycastNormal != m_RaycastHit.normal )
				{
					m_ChosingDodgeRotation = false;
					return;
				}

				m_ChosingDodgeRotation = true;
				Vector3 pointToFace = Utils.Math.ProjectPointOnPlane( m_RaycastHit.normal, m_DodgeAbilityTarget.position, m_RaycastHit.point );
				if ( pointToFace != m_DodgeAbilityTarget.position )
					m_DodgeAbilityTarget.rotation = Quaternion.LookRotation( ( pointToFace - m_DodgeAbilityTarget.position ), m_RaycastHit.normal );
			}

			if ( InputManager.Inputs.Ability1Released && m_ChosingDodgeRotation == true )						// GetKeyUp Q one frame
			{
				if ( m_DodgeCoroutine != null )
						StopCoroutine( m_DodgeCoroutine );

				m_DodgeRaycastNormal = Vector3.zero;
				SoundManager.Instance.Pitch = Time.timeScale = 1f;
				Vector3 destination = m_DodgeAbilityTarget.position + m_DodgeAbilityTarget.up;
				
				transform.Rotate( Vector3.up, CameraControl.Instance.CurrentDirection.y, Space.Self );
				Vector3 alias = CameraControl.Instance.CurrentDirection;
				alias.y = 0f;
				CameraControl.Instance.CurrentDirection = alias;

				m_DodgeAbilityTarget.gameObject.SetActive( false );
				m_DodgeCoroutine = StartCoroutine( Dodge( destination: destination, rotation: m_DodgeAbilityTarget.rotation ) );
				m_ChosingDodgeRotation = false;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Dodge ( Coroutine )
	private	IEnumerator	Dodge( Vector3 destination, Quaternion rotation, bool falling = false, DodgeTarget dodgeTarget = null )
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
		UnityEngine.UI.Image effectFrame			= UI.Instance.EffectFrame;
		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;
		m_RigidBody.velocity						= Vector3.zero;
		m_RigidBody.constraints						= RigidbodyConstraints.None;

		float slowMotionCoeff			= WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;
		AnimationCurve animationCurve	= ( ( dodgeTarget != null && dodgeTarget.HasTimeScaleCurveOverride ) ? dodgeTarget.DodgeTimeScaleCurve : m_DodgeTimeScaleCurve );
		while ( m_DodgeInterpolant < 1f )
		{
			// If is paused, wait for resume
			while ( GameManager.IsPaused == true )
				yield return null;

			m_DodgeInterpolant		+= Time.deltaTime;

			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, m_DodgeInterpolant * 6f );

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate( m_DodgeInterpolant ) * slowMotionCoeff;
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
		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;

		SetMotionType( eMotionType.Walking );

		m_DodgeCoroutine = null;
	}

}