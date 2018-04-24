using UnityEngine;
using System.Collections;


public partial class Player {

	private	const	float	DASH_EFFECT_SPEED	= 1.0f;
	private	const	float	DASH_SPEED_FACTOR	= 0.5f;
	private	const	float	DASH_START_EXIT_EFFET_THRESOLD = 0.85f;

	[Header("Player Properties")]
	[SerializeField]
	private	AnimationCurve	m_DashTimeScaleCurve = AnimationCurve.Linear( 0f, 1f, 1f, 1f );


	private	Coroutine	m_NormalDashCoroutine	= null;
	private	Coroutine	m_RotorDashCoroutine	= null;

	/*
	private	IEnumerator	DashStartEffect( DashTarget target )
	{
		UnityEngine.UI.Image effectFrame	= UI.Instance.InGame.GetEffectFrame();
		Camera	camera						= Camera.main;
		float	fovStartVal					= 60f;
		float	fovEndVal					= 100f;
		float	interpolant					= 0f;

		InputManager.IsEnabled						= false;
		CameraControl.Instance.CanParseInput		= false;
		CameraControl.Instance.transform.LookAt( target.transform.position );

		while ( interpolant < 1f )
		{
			camera.fieldOfView	= Mathf.Lerp( fovStartVal, fovEndVal, interpolant );
			effectFrame.color	= Color.Lerp ( Color.clear, Color.white, interpolant * 0.7f );
			interpolant += Time.unscaledDeltaTime * DASH_EFFECT_SPEED;
			yield return null;
		}

		effectFrame.color	= Color.Lerp ( Color.clear, Color.white, 0.5f );

		InputManager.IsEnabled						= true;
		CameraControl.Instance.CanParseInput		= true;
		camera.fieldOfView							= fovStartVal;
	}
	*/
	/*
	private IEnumerator DashMoving( DashTarget target )
	{
		Vector3	startPosition				= transform.position;
		Vector3 targetPosition				= target.transform.position;
		float	currentTime					= 0f;
		float	interpolant					= 0f;
		var		settings					= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		UnityEngine.UI.Image effectFrame	= UI.Instance.InGame.GetEffectFrame();

		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;

		Time.timeScale = 0.1f;
		yield return StartCoroutine( DashStartEffect( target ) );
		Time.timeScale = 1.0f;

		float slowMotionCoeff = WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;

		AnimationCurve animationCurve = ( ( target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			effectFrame.color	= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime += Time.deltaTime;
			interpolant = currentTime * DASH_SPEED_FACTOR;

			Time.timeScale = animationCurve.Evaluate( interpolant ) * slowMotionCoeff;
			SoundEffectManager.Instance.Pitch = Time.timeScale;
			
			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			transform.position = Vector3.Lerp( startPosition, target.transform.position, interpolant );
			
			yield return null;
		}

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;
		effectFrame.color = Color.clear;

		m_RigidBody.velocity =Vector3.zero;

		target.OnTargetReached();
		transform.position = targetPosition;
		m_IsDashing = false;

		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;
//		InputManager.IsEnabled						= true;

	}
	*/

	private	IEnumerator	DashRotator( Vector3 destination, Vector3 destinationUp, DashTarget target = null )
	{
		Vector3		startPosition			= transform.position;
		Quaternion	startRotation			= transform.rotation;
		Vector3		alignedPoint			= ProjectPointOnPlane( -transform.up, destination, transform.position );
		Quaternion	finalRotation			= Quaternion.LookRotation( alignedPoint - startPosition, destinationUp );
		float	currentTime					= 0f;
		float	interpolant					= 0f;
		var		settings					= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		UnityEngine.UI.Image effectFrame	= UI.Instance.InGame.GetEffectFrame();

		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;

		m_RigidBody.constraints = RigidbodyConstraints.None;

		float slowMotionCoeff = WeaponManager.Instance.CurrentWeapon.SlowMotionCoeff;

		AnimationCurve animationCurve = ( ( target != null && target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			effectFrame.color		= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime				+= Time.deltaTime;
			interpolant				= currentTime * DASH_SPEED_FACTOR;

			Time.timeScale			= animationCurve.Evaluate( interpolant ) * slowMotionCoeff;
			
			transform.position		= Vector3.Lerp( startPosition, destination, interpolant );
			transform.rotation		= Quaternion.Lerp( startRotation, finalRotation, interpolant );

//			transform.up			= Vector3.Slerp( startUpVector, destinationUp, interpolant );

			settings.frameBlending	= ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			SoundEffectManager.Instance.Pitch = Time.timeScale;
			yield return null;
		}

		m_RigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		m_RigidBody.velocity = Vector3.zero;

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;
		effectFrame.color = Color.clear;

		CameraControl.Instance.OnCutsceneEnd();

		transform.position = destination;
		transform.rotation = finalRotation;
//		transform.up = destinationUp;
		m_IsDashing = false;

		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;
	}











	//create a vector of direction "vector" with length "size"
	private Vector3 SetVectorLength( Vector3 vector, float size )
	{
		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);

		//scale the vector
		return vectorNormalized *= size;
	}

	//This function returns a point which is a projection from a point to a plane.
	private Vector3 ProjectPointOnPlane( Vector3 planeNormal, Vector3 planePoint, Vector3 point )
	{
		float distance;
		Vector3 translationVector;

		//First calculate the distance from the point to the plane:
		distance = Vector3.Dot( planeNormal, ( point - planePoint ) );

		//Reverse the sign of the distance
		distance *= -1;

		//Get a translation vector
		translationVector = SetVectorLength(planeNormal, distance);

		//Translate the point to form a projection
		return point + translationVector;
	}

}