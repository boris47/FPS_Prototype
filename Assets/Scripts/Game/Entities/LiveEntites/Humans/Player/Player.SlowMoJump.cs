using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class Player {
	

	private	const	float	SLOWMO_EFFECT_SPEED			= 1.0f;
	
	[SerializeField, Range( 0.1f, 2f )]
	private	float			m_SlowMoJumpSpeed			= 0.2f;

	[SerializeField]
	private	AnimationCurve	m_SlowMoJumpTimeScaleCurve	= AnimationCurve.Linear( 0f, 1f, 1f, 1f );



	private	IEnumerator	SlowMoJumpStartEffect()
	{
		UnityEngine.UI.Image effectFrame	= UI.Instance.InGame.GetEffectFrame();
		
		float	interpolant					= 0f;
		while ( interpolant < 1f )
		{
//			camera.fieldOfView	= Mathf.Lerp( fovStartVal, fovEndVal, interpolant );
			effectFrame.color	= Color.Lerp ( Color.clear, Color.white, interpolant * 0.7f );
			interpolant += Time.unscaledDeltaTime * SLOWMO_EFFECT_SPEED;
			yield return null;
		}

		effectFrame.color	= Color.clear;
		m_States.Reset();
	}


	private IEnumerator SlowMoJump( Vector3[] curvePoints, AnimationCurve slowMoJumpTimeScaleCurve )
	{
		Vector3	startPosition				= transform.position;
		float	interpolant					= 0f;
		var		settings					= CameraControl.Instance.GetPP_Profile.motionBlur.settings;


		CameraControl.Instance.HeadBob.IsActive		= false;
		CameraControl.Instance.HeadMove.IsActive	= false;

		Time.timeScale = 0.0f;
		yield return StartCoroutine( SlowMoJumpStartEffect() );
		Time.timeScale = 1.0f;


		AnimationCurve animationCurve = ( ( slowMoJumpTimeScaleCurve != null ) ? slowMoJumpTimeScaleCurve : m_SlowMoJumpTimeScaleCurve );
		while ( interpolant < 1f )
		{
			interpolant += Time.deltaTime * m_SlowMoJumpSpeed;
			float curveValue = animationCurve.Evaluate( interpolant );
			Time.timeScale = curveValue;

			SoundEffectManager.Instance.Pitch = Time.timeScale;
			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			Vector3 position = Utils.Math.GetPoint( curvePoints[0], curvePoints[1], curvePoints[2], curvePoints[3], curvePoints[4],
				interpolant * ( ( 1f - curveValue ) * 2f )
				);
			transform.position = position;

			yield return null;
		}

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;
		transform.position = curvePoints[4];

		CameraControl.Instance.HeadBob.IsActive		= true;
		CameraControl.Instance.HeadMove.IsActive	= true;

		m_RigidBody.velocity	= Vector3.zero;
		m_Move					= Vector3.zero;
		m_IsDashing = false;
	}

	public	void	StartSlowMoJump( ref Vector3[] curvePoints, AnimationCurve slowMoJumpTimeScaleCurve = null )
	{
		if ( m_IsDashing )
			return;

		m_IsDashing = true;

		StartCoroutine( SlowMoJump( curvePoints, slowMoJumpTimeScaleCurve ) );
	}
	
}