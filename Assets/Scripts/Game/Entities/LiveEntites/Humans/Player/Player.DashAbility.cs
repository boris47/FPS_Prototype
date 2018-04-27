using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_SPEED_FACTOR	= 0.5f;

	[Header("Player Properties")]
	[SerializeField]
	private	AnimationCurve	m_DashTimeScaleCurve = AnimationCurve.Linear( 0f, 1f, 1f, 1f );

	private	Coroutine		m_RotorDashCoroutine	= null;

	private	IEnumerator	DashRotator( Vector3 destination, Vector3 destinationUp, bool falling = false, DashTarget target = null )
	{
		Vector3		startPosition			= transform.position;
		Quaternion	startRotation			= transform.rotation;
		Quaternion	finalRotation			= Quaternion.identity;
		if ( target != null )
		{
			finalRotation = target.transform.rotation;
		}
		else if ( falling == true)
		{
			Vector3		alignedForward		= Vector3.Cross( transform.right, Vector3.up );
			finalRotation					= Quaternion.LookRotation( alignedForward, destinationUp );
		}
		else
		{
			Vector3		alignedPoint		= Utils.Math.ProjectPointOnPlane( -transform.up, destination, transform.position );
			finalRotation					= Quaternion.LookRotation( alignedPoint - startPosition, destinationUp );
		}
		
		float	currentTime					= 0f;
		float	interpolant					= 0f;
		var		settings					= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		UnityEngine.UI.Image effectFrame	= UI.Instance.InGame.GetEffectFrame();

		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;

		m_RigidBody.velocity = Vector3.zero;
		m_RigidBody.constraints = RigidbodyConstraints.None;

		float slowMotionCoeff = WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;

		AnimationCurve animationCurve = ( ( target != null && target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime				+= Time.deltaTime;
			interpolant				= currentTime * DASH_SPEED_FACTOR;

			float	timeScaleNow	= animationCurve.Evaluate( interpolant ) * slowMotionCoeff;
			if ( falling == false )
			Time.timeScale			= timeScaleNow;
			
			transform.position		= Vector3.Lerp( startPosition, destination, interpolant );
			transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, interpolant );

			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			SoundEffectManager.Instance.Pitch = Time.timeScale;
			yield return null;
		}

		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
		m_RigidBody.velocity = Vector3.zero;

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;
		effectFrame.color = Color.clear;

		CameraControl.Instance.OnCutsceneEnd();

		transform.position = destination;
		transform.rotation = finalRotation;
		m_IsDashing = false;

		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;
	}

}