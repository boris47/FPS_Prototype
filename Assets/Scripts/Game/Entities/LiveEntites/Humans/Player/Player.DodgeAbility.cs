using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_SPEED_FACTOR		= 0.5f;

	[Header("Player Properties")]
	[SerializeField]
	private	AnimationCurve	m_DashTimeScaleCurve	= AnimationCurve.Linear( 0f, 1f, 1f, 1f );

	private	Coroutine		m_RotorDashCoroutine	= null;

	private	Vector3			m_RaycastNormal			= Vector3.zero;

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
		if ( ( m_IsDodging == false && transform.up != Vector3.up ) && ( InputManager.Inputs.Jump || IsGrounded == false ) && m_RotorDashCoroutine == null )
		{
			RaycastHit hit;
			Physics.Raycast( transform.position, Vector3.down, out hit );

			Vector3 alignedForward = Vector3.Cross( transform.right, Vector3.up );
			Quaternion finalRotation = Quaternion.LookRotation( alignedForward, Vector3.up );
			m_RotorDashCoroutine = StartCoroutine( Dodge( destination: hit.point, rotation: finalRotation, falling: true ) );
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
			if ( m_DashAbilityTarget.gameObject.activeSelf == true )
			{
				m_DashAbilityTarget.gameObject.SetActive( false );
				m_RaycastNormal = Vector3.zero;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 1f;
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
				m_DashAbilityTarget.gameObject.SetActive( true );
				m_DashAbilityTarget.position = m_CurrentDodgeTarget.transform.position;
				m_DashAbilityTarget.rotation = m_CurrentDodgeTarget.transform.rotation;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 0.001f;
			}
			if ( InputManager.Inputs.Ability1Loop && m_DashAbilityTarget.gameObject.activeSelf == true )				// GetKeyDown Q
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

				m_DashAbilityTarget.gameObject.SetActive( false );
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
		if ( m_CurrentDodgeTarget == null )
		{
			float angle = Vector3.Angle( m_RaycastHit.normal, transform.up );
			bool validAngle = angle >= 89f && angle < 179f;
			if ( InputManager.Inputs.Ability1 && validAngle == true && m_ChosingDodgeRotation == false )        // GetKeyDown Q one frame
			{
				if ( m_RaycastNormal != Vector3.zero && m_RaycastNormal == m_RaycastHit.normal )
				{
					return;
				}

				m_DashAbilityTarget.gameObject.SetActive( true );
				m_DashAbilityTarget.position = m_RaycastHit.point;
				m_DashAbilityTarget.up = m_RaycastHit.normal;
				m_RaycastNormal = m_RaycastHit.normal;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 0.001f;
			}

			if ( InputManager.Inputs.Ability1Loop && m_DashAbilityTarget.gameObject.activeSelf == true )		// GetKey Q more frames
			{
				if ( m_RaycastNormal != m_RaycastHit.normal )
				{
					m_ChosingDodgeRotation = false;
					return;
				}

				m_ChosingDodgeRotation = true;
				Vector3 pointToFace = Utils.Math.ProjectPointOnPlane( m_RaycastHit.normal, m_DashAbilityTarget.position, m_RaycastHit.point );
				if ( pointToFace != m_DashAbilityTarget.position )
					m_DashAbilityTarget.rotation = Quaternion.LookRotation( ( pointToFace - m_DashAbilityTarget.position ), m_RaycastHit.normal );
			}

			if ( InputManager.Inputs.Ability1Released && m_ChosingDodgeRotation == true )						// GetKeyUp Q one frame
			{
				if ( m_RotorDashCoroutine != null )
						StopCoroutine( m_RotorDashCoroutine );

				m_RaycastNormal = Vector3.zero;
				SoundEffectManager.Instance.Pitch = Time.timeScale = 1f;
				Vector3 destination = m_DashAbilityTarget.position + m_DashAbilityTarget.up;
				
				transform.Rotate( Vector3.up, CameraControl.Instance.m_CurrentDirection.y, Space.Self );
				CameraControl.Instance.m_CurrentDirection.y = 0f;

				m_DashAbilityTarget.gameObject.SetActive( false );
				m_RotorDashCoroutine = StartCoroutine( Dodge( destination: destination, rotation: m_DashAbilityTarget.rotation ) );
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
		
		float	currentTime							= 0f;
		var		settings							= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		float	drag								= m_RigidBody.drag;

		m_DodgeInterpolant							*= 0.2f;

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
		AnimationCurve animationCurve	= ( ( dodgeTarget != null && dodgeTarget.HasTimeScaleCurveOverride ) ? dodgeTarget.DodgeTimeScaleCurve : m_DashTimeScaleCurve );
		while ( m_DodgeInterpolant < 1f )
		{
			if ( m_ChosingDodgeRotation == true )
				yield return null;

			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, m_DodgeInterpolant * 6f );
			currentTime				+= Time.deltaTime;
			m_DodgeInterpolant				= currentTime * DASH_SPEED_FACTOR * ( ( falling == true ) ? 5f : 1f );

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate( m_DodgeInterpolant ) * slowMotionCoeff;
			Time.timeScale			= ( falling == true ) ? Time.timeScale : timeScaleNow;
			
			// Position and Rotation
			if ( falling == false )
				transform.position	= Vector3.Lerp( startPosition, destination, m_DodgeInterpolant );
			transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, m_DodgeInterpolant * ( ( falling == true ) ? 4f : 1f ) );

			// Motion Blur Intensity
			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			// Audio Global Pitch
			SoundEffectManager.Instance.Pitch = Time.timeScale;
			yield return null;
		}

		m_DodgeInterpolant							= 0f;

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

		m_RotorDashCoroutine = null;
	}

}