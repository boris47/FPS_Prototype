
using System.Collections;
using UnityEngine;

public partial class Player {

	[SerializeField]
	private	AnimationCurve	DashTimeScaleCurve = AnimationCurve.Linear( 0f, 1f, 1f, 1f );

	public override void OnHit( HitInfo info )
	{
		
	}

	public override void OnHurt( HurtInfo info )
	{
		Health -= info.Damage;

		if ( Health < 0f )
			OnKill();

	}

	public override void OnKill( HitInfo info = null )
	{
		print( "U r dead" );
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

		StartCoroutine( DashMoving( target ) );
	}


	private IEnumerator DashMoving( DashTarget target )
	{
		Vector3	startPosition		= transform.position;
		Vector3 targetPosition		= target.transform.position;
//		Vector3 direction			= ( targetPosition - transform.position ).normalized;
//		float	distanceToTravel	= ( targetPosition - transform.position ).sqrMagnitude;
//		float	travelledDistance	= 0f;
		float	currentTime			= 0f;
		float	interpolant			= 0f;
		/*
		while( travelledDistance < distanceToTravel )
		{
			Time.timeScale = DashTimeScaleCurve.Evaluate( interpolant );
			interpolant = travelledDistance / distanceToTravel;

			transform.position += ( direction * m_DashSpeed ) * Time.deltaTime;
			travelledDistance = ( transform.position - startPosition ).sqrMagnitude;
			yield return null;
		}
		 */
		
		UnityEngine.PostProcessing.MotionBlurModel.Settings settings = CameraControl.Instance.GetPP_Profile.motionBlur.settings;
		while ( interpolant < 1f )
		{
			currentTime += Time.deltaTime;
			interpolant = currentTime / m_DashSpeed;

			Time.timeScale = DashTimeScaleCurve.Evaluate( interpolant );
			SoundEffectManager.Instance.Pitch = Time.timeScale;

			settings.frameBlending = ( 1f - Time.timeScale );
			CameraControl.Instance.GetPP_Profile.motionBlur.settings = settings;


			transform.position = Vector3.Lerp( startPosition, target.transform.position, interpolant );
			yield return null;
		}


		Time.timeScale = 1f;

		target.OnTargetReached();
		transform.position = targetPosition;
		m_IsDashing = false;
	}


}