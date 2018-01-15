using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl {

	const float	MIN_DISTANCE = 5.0f;

	[Header("Camera Target Switch")]
	[Tooltip("Heigth at witch camera must rise before traslating")]
	[SerializeField][Range(2f, 100f)]
	private float			m_TargetSwitchHeight			= 0.0f;
	[SerializeField][Tooltip("Speed of camera elevation")]
	private  float			m_TargetSwitchElevationSpeed	= 0.0f;
	[SerializeField][Tooltip("Speed of camera traslation")]
	private  float			m_TargetSwitchTraslationSpeed	= 0.0f;
	[SerializeField][Tooltip("Speed of camera focus")]
	private  float			m_TargetSwitchFocusSpeed		= 0.0f;
	[SerializeField][Tooltip("Check if you want camera must face down while rising")]
	private	bool			m_TargetSwitchElevationFaceDown	= true;

	[SerializeField]
	private AnimationCurve	m_CameraPositionElevationCurve	= null;
	[SerializeField]
	private AnimationCurve	m_CameraPositionTraslationCurve	= null;
	[SerializeField]
	private AnimationCurve	m_CameraPositionTargetingCurve	= null;

	[SerializeField]
	private AnimationCurve	m_CameraRotationElevationCurve	= null;
	[SerializeField]
	private AnimationCurve	m_CameraRotationTraslationCurve	= null;
	[SerializeField]
	private AnimationCurve	m_CameraRotationTargetingCurve	= null;

	private	GameObject		m_TargetSwitchTarget			= null;

	/// <summary> Switch camera target </summary>
	public	void SwitchToTarget( GameObject pNextTarget )
	{
		///// CHECKS

		// Return if no valid target
		if ( pNextTarget == null )
			return;

		// If has not target, set this as first camera target
		Transform pViewPivot = pNextTarget.transform.Find( "ViewPivot" );
		if ( m_Target == null )
		{
			if ( pViewPivot )
				m_Target = pViewPivot.gameObject;
			else
				m_Target = pNextTarget;

			return;
		}
		else
		{
			// Set previous player ( if was ) as inactive
			Player pPreviousPlayer = m_Target.GetComponentInParent<Player>();
			if ( pPreviousPlayer )
			{
				Player.CurrentActivePlayer = null;
				pPreviousPlayer.IsActive = false;
			}
		}

		// Return if target is already set
		if ( m_Target.GetInstanceID() == pNextTarget.GetInstanceID() )
			return;

		// Return if no valid speed
		if ( m_TargetSwitchElevationSpeed < 0.001f || m_TargetSwitchTraslationSpeed < 0.001f || m_TargetSwitchFocusSpeed < 0.001f )
			return;

		// if player distance is not enough than simply camera switch for target
		if ( Vector3.Distance( m_Target.transform.position, pNextTarget.transform.position ) <= MIN_DISTANCE )
		{
			m_Target = pNextTarget;
			return;
		}

		// save next target reference
		m_TargetSwitchTarget = ( pViewPivot ) ? pViewPivot.gameObject : pNextTarget;

		// Disable update and LateUpdate Callbacks
		this.enabled = false;

		// Reset head movements
		m_HeadBob.Reset( true );
		m_HeadMove.Reset( true );

		// Start magic things
		StartCoroutine( CameraElevation() );

	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private	IEnumerator CameraElevation()
	{
		print( "CameraElevation" );

		float destinationHeight = transform.position.y + m_TargetSwitchHeight;
		float startHeight = transform.position.y;
		float interpolant = 0;

		// While camera heigth is less than next target's one plus target switch heigth
		while ( interpolant < 1.0f )
		{
			// Update Phase Factor
			interpolant += m_TargetSwitchElevationSpeed * Time.deltaTime;

			// Update Camera
			{
				{	// Position
					float positionInterpolant = m_CameraPositionElevationCurve.Evaluate( interpolant );
					transform.position = new Vector3
					(
						transform.position.x, 
						Mathf.Lerp( startHeight, destinationHeight, positionInterpolant ), 
						transform.position.z
					);
				}

				{	// Rotation
					float rotationInterpolant = m_CameraRotationElevationCurve.Evaluate( interpolant );
					transform.rotation = Quaternion.Lerp
					(
						transform.rotation,
						Quaternion.LookRotation( ( m_TargetSwitchElevationFaceDown == true ) ? Vector3.down : Vector3.up, transform.up ),
						rotationInterpolant
					);
				}
			}

			yield return null;
		}

		transform.position = new Vector3( transform.position.x, destinationHeight, transform.position.z );
		transform.rotation = Quaternion.LookRotation( ( m_TargetSwitchElevationFaceDown == true ) ? Vector3.down : Vector3.up, transform.up );

		StartCoroutine( CameraTraslation() );

	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private	IEnumerator CameraTraslation()
	{
		print( "CameraTraslation" );

		Player pCurrentPlayer = m_TargetSwitchTarget.GetComponentInParent<Player>();

		Vector3 startPosition = transform.position;
		float interpolant = 0f;

		while( interpolant < 1.0f )
		{
			// Update Phase Factor
			interpolant += m_TargetSwitchTraslationSpeed * Time.deltaTime;

			// Update camera
			{

				{	// Position
					float positionInterpolant = m_CameraPositionTraslationCurve.Evaluate( interpolant );
					transform.position = Vector3.Lerp
					( 
						startPosition,
						new Vector3 ( m_TargetSwitchTarget.transform.position.x, transform.position.y, m_TargetSwitchTarget.transform.position.z ),
						positionInterpolant * 0.99f
					);
				}

				{	// Rotation
					float rotationInterpolant = m_CameraRotationTraslationCurve.Evaluate( interpolant );
					transform.rotation = Quaternion.Slerp
					( 
						transform.rotation, 
						Quaternion.LookRotation( m_TargetSwitchTarget.transform.position - transform.position ),
						rotationInterpolant
					);
				}
			}

			yield return null;
		}

		transform.rotation = Quaternion.LookRotation( m_TargetSwitchTarget.transform.position - transform.position );

		StartCoroutine( CameraTargetFocus() );
	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private IEnumerator CameraTargetFocus()
	{
		print( "CameraTargetFocus" );

		float startCamHeight = transform.position.y;
		Player pCurrentPlayer = m_TargetSwitchTarget.GetComponentInParent<Player>();

		Vector3 destinationPosition = m_TargetSwitchTarget.transform.parent.transform.TransformPoint( m_TargetSwitchTarget.transform.localPosition );

		float interpolant = 0f;

		// while camera is over the target
		while ( interpolant < 1.0f )
		{
			// Update Phase Factor
			interpolant += m_TargetSwitchFocusSpeed * Time.deltaTime;

			// Update Camera
			{
				{	// Position
					float positionInterpolant = m_CameraPositionTargetingCurve.Evaluate( interpolant );
					transform.position = Vector3.Lerp
					(
						transform.position, 
						destinationPosition,
						positionInterpolant
					);
				}

				{	// Rotation
					float rotationInterpolant = m_CameraRotationTargetingCurve.Evaluate( interpolant );
					transform.rotation = Quaternion.Slerp
					(
						transform.rotation,
						( pCurrentPlayer ) ? pCurrentPlayer.FaceDirection : m_TargetSwitchTarget.transform.rotation, 
						rotationInterpolant
					);
				}
			}

			yield return null;
		}

		transform.position = m_TargetSwitchTarget.transform.parent.transform.TransformPoint( m_TargetSwitchTarget.transform.localPosition );

//		transform.position = new Vector3( transform.position.x, m_TargetSwitchTarget.transform.position.y, transform.position.z );
		transform.rotation = ( pCurrentPlayer ) ? pCurrentPlayer.FaceDirection : m_TargetSwitchTarget.transform.rotation;

		if ( pCurrentPlayer )
		{
		// Set current player ( if is ) as active
			pCurrentPlayer.IsActive = true;
			Player.CurrentActivePlayer = pCurrentPlayer;
			m_CurrentDirection = pCurrentPlayer.FaceDirection.eulerAngles;
		}
		else
		{
			m_CurrentDirection = m_TargetSwitchTarget.transform.rotation.eulerAngles;
		}


		// set as current target
		m_Target = m_TargetSwitchTarget;

		// clear the target of switch ref
		m_TargetSwitchTarget = null;

		// Reset head movements
		m_HeadBob.Reset( true );
		m_HeadMove.Reset( true );

		// If camera is in third person mode remove offset to create an effect
		if ( m_TPSMode )
			m_CurrentCameraOffset = 0.0f;

		// re-enable script
		this.enabled = true;

		StopAllCoroutines();
	}


	private	float	PlaneDistance( Vector3 a, Vector3 b )
	{
		return Vector3.Distance( new Vector3( a.x, 0.0f, a.z ), new Vector3( b.x, 0.0f, b.z ) );
	}




}
