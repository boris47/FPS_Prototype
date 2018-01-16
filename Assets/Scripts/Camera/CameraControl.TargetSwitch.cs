using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl {

	const float	MIN_DISTANCE = 5.0f;

	[Header("Camera Target Switch")]
	[Tooltip("Heigth at witch camera must rise before traslating")]
	[SerializeField][Range(2f, 100f)]
	private float			m_TargetSwitchHeight			= 100.0f;
	[SerializeField][Tooltip("Speed of camera focus")]
	private  float			m_TransitionDuration			= 6.0f;

	public	AnimationCurve	m_TransitionPositionCurve = new AnimationCurve();
	public	AnimationCurve	m_TransitionRotationCurve = new AnimationCurve();

	public bool	InTransition
	{
		get { return m_CameraTransition != null; }
	}

	private	Player			m_TargetSwitchTarget			= null;
	private	IEnumerator		m_CameraTransition				= null;


	/// //////////////////////////////////////////////////////////////////////////
	/// SwitchToTarget ( Abstract )
	/// <summary> Switch camera target </summary>
	public	void SwitchToTarget( Player pNextTarget )
	{

		///// CHECKS
		// if already intransition return
		if ( InTransition )
			return;

		// Return if no valid target
		if ( pNextTarget == null )
			return;

		// Return if target is already set
		if ( m_Target == pNextTarget )
			return;

		// Return if no valid speed
		if ( m_TransitionDuration < 0.001f )
			return;

		// save next target reference
		m_TargetSwitchTarget = pNextTarget;

		// if player distance is not enough than simply camera switch for target
		if ( Vector3.Distance( pNextTarget.transform.position, Player.CurrentActivePlayer.transform.position ) <= MIN_DISTANCE )
		{
			// Skip bezier jump and instantly swith to next target
			OnEndTransition( pNextTarget );
			return;
		}

		m_TargetSwitchHeight = Vector3.Distance( pNextTarget.transform.position, Player.CurrentActivePlayer.transform.position ) / 5f;

		// Disable update and LateUpdate Callbacks
		this.enabled = false;

		// Reset head movements
		m_HeadBob.Reset( true );
		m_HeadMove.Reset( true );

		// Start magic things
		StartCoroutine( m_CameraTransition = CameraTransition() );
	}



	/// //////////////////////////////////////////////////////////////////////////
	/// CameraTransition ( Coroutine )
	private	IEnumerator	CameraTransition()
	{
		Vector3		startPosition	= transform.position;
		Quaternion	startRotation	= transform.rotation;
		Transform	finalTransform	= m_TargetSwitchTarget.transform.GetChild(0);

		float	currentTime = 0f;
		float	interpolant = 0f;

		// Transition
		while ( interpolant < 1f )
		{
			currentTime += Time.unscaledDeltaTime;
			interpolant = currentTime / m_TransitionDuration;

			/// POSITION
			float positionInterpolant = m_TransitionPositionCurve.Evaluate( interpolant );
			transform.position = Vector3.Lerp
			(				
				GetPosition
				( 
					startPosition,
					startPosition + ( Vector3.up * m_TargetSwitchHeight ),
					startPosition + ( Vector3.up * m_TargetSwitchHeight * 2f ),
					finalTransform.position,
					positionInterpolant
				),
				GetPosition
				(
					startPosition,
					finalTransform.position + ( Vector3.up * m_TargetSwitchHeight * 2f ),
					finalTransform.position + ( Vector3.up * m_TargetSwitchHeight ),
					finalTransform.position,
					positionInterpolant
				),
				positionInterpolant
			);

			/// ROTATION
			float  rotationInterpolant = m_TransitionRotationCurve.Evaluate( interpolant );
			transform.rotation = GetRotation
			( 
				startRotation,
				startRotation * Quaternion.LookRotation( Vector3.right, Vector3.up ),
				m_TargetSwitchTarget.FaceDirection * Quaternion.LookRotation( Vector3.right, Vector3.up ),
				m_TargetSwitchTarget.FaceDirection,
				rotationInterpolant
			);

			yield return null;
		}

		transform.position = finalTransform.position;
		transform.rotation = m_TargetSwitchTarget.FaceDirection;
		
		// Reset head movements
		m_HeadBob.Reset( true );
		m_HeadMove.Reset( true );

		// re-enable script
		this.enabled = true;

		OnEndTransition( m_TargetSwitchTarget );
	}


	/// //////////////////////////////////////////////////////////////////////////
	/// OnEndTransition
	private	void	OnEndTransition( Player pNextTarget )
	{
		
		// Disable previous player
		Player.CurrentActivePlayer.IsActive = false;;
		
		// Set this player as active and current target
		Transform finalTransform = pNextTarget.transform.GetChild(0);
		m_Target = finalTransform;
		Player.CurrentActivePlayer = m_TargetSwitchTarget;
		Player.CurrentActivePlayer.IsActive = true;
		
		// clear the target of switch ref
		m_TargetSwitchTarget = null;

		// If camera is in third person mode remove offset to create an effect
		if ( m_TPSMode )
			m_CurrentCameraOffset = 0.0f;

		m_CameraTransition = null;
	}

	
	// BEZIER CURVE
	private Vector3		GetPosition( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
	{
		t = Mathf.Clamp01(t);
		float OneMinusT = 1f - t;
		return
			OneMinusT * OneMinusT * OneMinusT * p0 +
			3f * OneMinusT * OneMinusT * t * p1 +
			3f * OneMinusT * t * t * p2 +
			t * t * t * p3;
	}
	private Quaternion	GetRotation( Quaternion p0, Quaternion p1, Quaternion p2, Quaternion p3, float t )
	{
		t = Mathf.Clamp01( t );
		float OneMinusT = 1f - t;
		return Quaternion.Euler
			(
				OneMinusT * OneMinusT * OneMinusT * p0.eulerAngles +
				OneMinusT * OneMinusT * t * 3f *	p1.eulerAngles +
				OneMinusT *         t * t * 3f *	p2.eulerAngles +
						                t * t * t * p3.eulerAngles
			);
	}

}
