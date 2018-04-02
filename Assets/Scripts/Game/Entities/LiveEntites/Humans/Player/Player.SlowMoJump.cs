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
		UnityEngine.UI.Image effectFrame	= UI_InGame.Instance.GetEffectFrame();
		
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

	public float interG;
	public float interP;
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
			interG = interpolant += Time.deltaTime * m_SlowMoJumpSpeed;
			float curveValue = animationCurve.Evaluate( interpolant );
			Time.timeScale = curveValue;

			SoundEffectManager.Instance.Pitch = Time.timeScale;
			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			Vector3 position = GetPoint( curvePoints[0], curvePoints[1], curvePoints[2], curvePoints[3], curvePoints[4],
				interP = interpolant * ( ( 1f - curveValue ) * 2f )
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




	private Vector3 GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, float t )
    {
        t = Mathf.Clamp01( t );
        float oneMinusT = 1f - t;
        return
            oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }

    private Vector3 GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
    {
        t = Mathf.Clamp01( t );
        float OneMinusT = 1f - t;
        return
            OneMinusT * OneMinusT * OneMinusT * p0 +
            3f * OneMinusT * OneMinusT * t * p1 +
            3f * OneMinusT * t * t * p2 +
            t * t * t * p3;
    }

	private Vector3 GetPoint( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t )
    {
        t = Mathf.Clamp01( t );
        float OneMinusT = 1f - t;
        return
					OneMinusT	*	OneMinusT	*	OneMinusT	*	OneMinusT	*	p0 +
			4f *	OneMinusT	*	OneMinusT	*	OneMinusT	*		t		*	p1 +
            5f *	OneMinusT	*	OneMinusT	*		t		*		t		*	p2 +
			4f *	OneMinusT	*		t		*		t		*		t		*	p3 +
						t		*		t		*		t		*		t		*	p4;
    }


	private Vector3 GetPoint( ref Vector3[] wayPoints, float t )
	{
		int numSections = wayPoints.Length - 3;
		int currPt = Mathf.Min(Mathf.FloorToInt(t * (float) numSections), numSections - 1);
		float u = t * (float) numSections - (float) currPt;
		
		Vector3 a = wayPoints[ currPt + 0 ];
		Vector3 b = wayPoints[ currPt + 1 ];
		Vector3 c = wayPoints[ currPt + 2 ];
		Vector3 d = wayPoints[ currPt + 3 ];
		
		return .5f * 
		(
			( -a + 3f * b - 3f * c + d )		* ( u * u * u ) +
			( 2f * a - 5f * b + 4f * c - d )	* ( u * u ) +
			( -a + c )							* u +
			2f * b
		);
	}

}