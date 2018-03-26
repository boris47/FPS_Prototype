
using System.Collections;
using UnityEngine;

public partial class Player {

	private	const	float	DASH_SPEEED_FACTOR	= 0.5f;

	[SerializeField]
	private	AnimationCurve	m_DashTimeScaleCurve = AnimationCurve.Linear( 0f, 1f, 1f, 1f );


	public override void OnHit( ref Entity who, float damage )
	{
		Health -= damage;
		UI_InGame.Instance.UpdateUI();

		if ( Health < 0f )
			OnKill();
	}


	public override void OnHurt( ref Entity who, float damage )
	{
		Health -= damage;
		UI_InGame.Instance.UpdateUI();

		if ( Health < 0f )
			OnKill();
	}


	public override void OnKill()
	{
		print( "U r dead" );
		enabled = false;

		// to stop movements and camera effects
	}


	private	void	OnDashTargetUsed( ref DashTarget target )
	{
		if ( m_IsDashing )
			return;

		m_IsDashing = true;

		if ( m_PreviousDashTarget != null && m_PreviousDashTarget != target )
		{
			m_PreviousDashTarget.OnReset();
		}
		m_PreviousDashTarget = target;

		target.Disable();
		target.HideText();
		StartCoroutine( DashMoving( target ) );
	}


	private IEnumerator DashMoving( DashTarget target )
	{
		Vector3	startPosition		= transform.position;
		Vector3 targetPosition		= target.transform.position;
		float	currentTime			= 0f;
		float	interpolant			= 0f;
		
		UnityEngine.PostProcessing.MotionBlurModel.Settings settings = CameraControl.Instance.GetPP_Profile.motionBlur.settings;

		AnimationCurve animatioCurve = ( ( target.HasTimeScaleCurveOverride ) ? target.DashTimeScaleCurve : m_DashTimeScaleCurve );
		while ( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime * DASH_SPEEED_FACTOR;

			Time.timeScale =  animatioCurve.Evaluate( interpolant );
			SoundEffectManager.Instance.Pitch = Time.timeScale;
			
			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;

			transform.position = Vector3.Lerp( startPosition, target.transform.position, interpolant );
			yield return null;
		}

		SoundEffectManager.Instance.Pitch = 1f;
		Time.timeScale = 1f;

		target.OnTargetReached();
		transform.position = targetPosition;
		m_IsDashing = false;
	}


}