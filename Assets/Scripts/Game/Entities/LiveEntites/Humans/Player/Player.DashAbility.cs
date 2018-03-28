using UnityEngine;
using System.Collections;


public partial class Player {

	private	IEnumerator	DashStartEffect( DashTarget target )
	{
		UnityEngine.UI.Image effectFrame	= UI_InGame.Instance.GetEffectFrame();
		Camera	camera						= Camera.main;
		float	fovStartVal					= 60f;
		float	fovEndVal					= 100f;
		float	interpolant					= 0f;


		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;
		CameraControl.Instance.CanParseInput		= false;
		CameraControl.Instance.transform.LookAt( target.transform.position );

		while ( interpolant < 1f )
		{
			camera.fieldOfView	= Mathf.Lerp( fovStartVal, fovEndVal, interpolant );
			effectFrame.color	= Color.Lerp ( Color.clear, Color.white, interpolant * 0.7f );
			interpolant += Time.deltaTime * DASH_EFFECT_SPEED;
			yield return null;
		}

		effectFrame.color	= Color.Lerp ( Color.clear, Color.white, 0.5f );

		CameraControl.Instance.CanParseInput		= true;
		camera.fieldOfView							= fovStartVal;
	}


	private IEnumerator DashMoving( DashTarget target )
	{
		Vector3	startPosition				= transform.position;
		Vector3 targetPosition				= target.transform.position;
		float	currentTime					= 0f;
		float	interpolant					= 0f;
		var		settings					= CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		UnityEngine.UI.Image effectFrame	= UI_InGame.Instance.GetEffectFrame();

		yield return StartCoroutine( DashStartEffect( target ) );

		AnimationCurve animatioCurve = ( ( target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			effectFrame.color	= Color.Lerp ( Color.white, Color.clear, interpolant * 6f );
			currentTime += Time.deltaTime;
			interpolant = currentTime * DASH_SPEEED_FACTOR;

			Time.timeScale = animatioCurve.Evaluate( interpolant );
			SoundEffectManager.Instance.Pitch = Time.timeScale;
			
			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			transform.position = Vector3.Lerp( startPosition, target.transform.position, interpolant );

			if ( interpolant > 0.6f )
			{
				DashEndEffect( interpolant - 0.6f );
			}

			yield return null;
		}

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;
		effectFrame.color = Color.clear;

		target.OnTargetReached();
		transform.position = targetPosition;
		m_IsDashing = false;

		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;

	}


	private	void	DashEndEffect( float interpolant )
	{
		UnityEngine.UI.Image effectFrame	= UI_InGame.Instance.GetEffectFrame();

		float dashFinalInterpolant = ( interpolant *= ( 0.3f * 10f ) );
		effectFrame.color	= ( dashFinalInterpolant < 0.98f ) ?
			Color.Lerp ( Color.clear, Color.white, Mathf.Clamp01( interpolant ) )
			:
			Color.clear;
	}


}