using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_SPEED_FACTOR		= 0.5f;

	[Header("Player Properties")]
	[SerializeField]
	private	AnimationCurve	m_DashTimeScaleCurve	= AnimationCurve.Linear( 0f, 1f, 1f, 1f );

	private	Coroutine		m_RotorDashCoroutine	= null;
	

	//////////////////////////////////////////////////////////////////////////
	// FindFinalRotation
	private	Quaternion	FindFinalRotation( Vector3 startPosition, Vector3 destination, Vector3 destinationUp, bool falling, DashTarget target )
	{
		Quaternion finalRotation = Quaternion.identity;

		if ( target != null )
		{
			finalRotation = target.transform.rotation;
		}
		else if ( falling == true)
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
			Vector3 alignedPoint = Utils.Math.ProjectPointOnPlane( planeNormal: -transform.up, planePoint: destination, point: transform.position );
			finalRotation = Quaternion.LookRotation( alignedPoint - startPosition, destinationUp );
		}

		return finalRotation;
	}


	//////////////////////////////////////////////////////////////////////////
	// Dodge ( Coroutine )
	private	IEnumerator	Dodge( Vector3 destination, Vector3 destinationUp, bool falling = false, DashTarget target = null )
	{
		Vector3		startPosition					= transform.position;
		Quaternion	startRotation					= transform.rotation;
		Quaternion	finalRotation					= FindFinalRotation( startPosition, destination, destinationUp, falling, target );
		
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
		AnimationCurve animationCurve	= ( ( target != null && target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			// Flash effect
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime				+= Time.deltaTime;
			interpolant				= currentTime * DASH_SPEED_FACTOR;

			// Time Scale
			float timeScaleNow		= animationCurve.Evaluate( interpolant ) * slowMotionCoeff;
			Time.timeScale			= /*( falling == true ) ? Time.timeScale :*/ timeScaleNow;
			
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

}