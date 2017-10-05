using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CameraControl {

	const float	MIN_DISTANCE = 5.0f;

	[Header("Camera Target Switch")]
	[Tooltip("Heigth at witch camera must rise before traslating")]
	public  float			m_TargetSwitchHeigth			= 0.0f;
	[Tooltip("Speed of camera elevation")]
	public  float			m_TargetSwitchElevationSpeed	= 0.0f;
	[Tooltip("Speed of camera traslation")]
	public  float			m_TargetSwitchTraslationSpeed	= 0.0f;
	[Tooltip("Speed of camera focus")]
	public  float			m_TargetSwitchFocusSpeed		= 0.0f;
	[Tooltip("Check if you want camera must face down while rising")]
	public	bool			m_TargetSwitchElevationFaceDown	= true;


	private	GameObject		m_TargetSwitchTarget			= null;

	private IEnumerator		m_CurrentElevationCoroutine		= null;
	private	float			m_CurrentElevationFactor		= 0.0f;
	public	float			CurrentElevationFactor {
		get { return m_CurrentElevationFactor; }
	}

	private IEnumerator		m_CurrentTraslationCoroutine	= null;
	private	float			m_CurrentTraslationFactor		= 0.0f;
	public	float			CurrentTraslationFactor {
		get { return m_CurrentTraslationFactor; }
	}

	private IEnumerator		m_CurrentFocusCoroutine			= null;
	private	float			m_CurrentFocusFactor			= 0.0f;
	public	float			CurrentFocusFactor {
		get { return m_CurrentFocusFactor; }
	}


	public	void SwitchToTarget( GameObject pNextTarget ) {

		///// CHECKS

		// Return if no valid target
		if ( pNextTarget == null ) return;

		// Return if target is already set
		if ( m_Target.GetInstanceID() == pNextTarget.GetInstanceID() ) return;

		// Return if no valid speed
		if ( m_TargetSwitchElevationSpeed < 0.001f || m_TargetSwitchTraslationSpeed < 0.001f || m_TargetSwitchFocusSpeed < 0.001f ) return;

		// Switch heigth must at last 2 meter over current target
		if ( m_TargetSwitchHeigth < ( m_Target.transform.position.y + 2.0f ) ) return;

		// if player distance is not enough than simply camera switch for target
		if ( Vector3.Distance( m_Target.transform.position, pNextTarget.transform.position ) <= MIN_DISTANCE ) {
			m_Target = pNextTarget;
			return;
		}

		// save next target reference
		m_TargetSwitchTarget = pNextTarget;

		// Disable update and LateUpdate Callbacks
		this.enabled = false;

		// Start magic things
		StartCoroutine( m_CurrentElevationCoroutine = CameraElevation() );

	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private	IEnumerator CameraElevation() {

		print( "CameraElevation" );

		float fNewCamHeigth = transform.position.y + m_TargetSwitchHeigth;
		float fOldCamHeigth = transform.position.y;

		// While camera heigth is less than next target's one plus target switch heigth
		while ( m_CurrentElevationFactor < 1.0f ) {

			// Update Phase Factor
			m_CurrentElevationFactor += m_TargetSwitchElevationSpeed * Time.deltaTime;


			// Update Camera
			{
				float fNewHeigth = Mathf.Lerp( fOldCamHeigth, fNewCamHeigth, m_CurrentElevationFactor );
				transform.position = new Vector3( transform.position.x, fNewHeigth, transform.position.z );

				if ( m_CurrentTraslationCoroutine == null )
					if ( m_TargetSwitchElevationFaceDown )
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( Vector3.down ), m_CurrentElevationFactor );
					else {
						transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( Vector3.up ), m_CurrentElevationFactor );
					}
			}
			
			// Next Coroutine
			if ( m_CurrentElevationFactor > 0.8f && m_CurrentTraslationCoroutine == null ) {
				StartCoroutine( m_CurrentTraslationCoroutine = CameraTraslation() );
			}


			yield return null;
		}

		m_CurrentElevationCoroutine = null;
		m_CurrentElevationFactor = 0.0f;
		

	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private	IEnumerator CameraTraslation() {

		print( "CameraTraslation" );

		float	fDistance;
		float	fStartDistance = fDistance = PlaneDistance( transform.position, m_TargetSwitchTarget.transform.position );

		m_CurrentTraslationFactor = 0.001f;

		while( fDistance > 0.1f ) {

			// Update camera
			{
				Vector3 vNewPosition = Vector3.Lerp( transform.position, 
					new Vector3 ( m_TargetSwitchTarget.transform.position.x, transform.position.y, m_TargetSwitchTarget.transform.position.z ),
					m_TargetSwitchTraslationSpeed * Time.deltaTime * ( m_CurrentTraslationFactor * 3 )
				);
				transform.position = new Vector3( vNewPosition.x, transform.position.y, vNewPosition.z );

				if ( m_CurrentFocusCoroutine == null ) {
					transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( m_TargetSwitchTarget.transform.position - transform.position ), 
						m_TargetSwitchTraslationSpeed * Time.deltaTime * ( m_CurrentTraslationFactor * 50f )
					);
				}
			}

			// Update Phase Factor
			fDistance	= PlaneDistance( transform.position, m_TargetSwitchTarget.transform.position );
			m_CurrentTraslationFactor = Mathf.Abs( 1.0f - ( fDistance / fStartDistance ) );

			// Next Coroutine
			if ( m_CurrentTraslationFactor > 0.9f && m_CurrentFocusCoroutine == null )
				StartCoroutine( m_CurrentFocusCoroutine = CameraTargetFocus() );

			yield return null;

		}

		m_CurrentTraslationCoroutine = null;
		m_CurrentTraslationFactor = 0.0f;
	}

	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////

	private IEnumerator CameraTargetFocus() {

		print( "CameraTargetFocus" );

		float fOldCamHeigth = transform.position.y;

		// while camera is over the target
		while ( m_CurrentFocusFactor < 1.0f ) {

			// Update Camera
			{
				float fNewHeigth = Mathf.Lerp( fOldCamHeigth, m_TargetSwitchTarget.transform.position.y, m_CurrentFocusFactor );
				transform.position = new Vector3( transform.position.x, fNewHeigth, transform.position.z );

				transform.rotation = Quaternion.Slerp( transform.rotation, m_TargetSwitchTarget.transform.rotation, m_CurrentFocusFactor / 10f );
			}

			// Update Phase Factor
			m_CurrentFocusFactor += m_TargetSwitchFocusSpeed * Time.deltaTime;

			yield return null;

		}

		// Set previous player ( if was ) as inactive
		Player pPreviousPlayer = m_Target.GetComponentInParent<Player>();
		if ( pPreviousPlayer ) pPreviousPlayer.IsActive = false;

		// Set current player ( if is ) as active
		Player pCurrentPlayer = m_TargetSwitchTarget.GetComponent<Player>();
		if ( pCurrentPlayer ) {
			pCurrentPlayer.IsActive = true;
			transform.rotation = Quaternion.identity;
			m_CurrentDirection = pCurrentPlayer.transform.rotation.eulerAngles;
		}

		// choose camera target
		Transform pViewPivot = m_TargetSwitchTarget.transform.Find( "ViewPivot" );
		m_Target = ( pViewPivot != null ) ? pViewPivot.gameObject : m_TargetSwitchTarget;

		// clear the target of switch ref
		m_TargetSwitchTarget = null;

		// Reset head movements
		m_HeadBob._Reset( true );
		m_HeadMove._Reset( true );

		// re-enable script
		this.enabled = true;

		// Reset all vars
		m_CurrentElevationCoroutine = m_CurrentTraslationCoroutine = m_CurrentFocusCoroutine = null;
		m_CurrentElevationFactor = m_CurrentTraslationFactor = m_CurrentFocusFactor = 0.0f;
		StopAllCoroutines();

	}


	private	float	PlaneDistance( Vector3 a, Vector3 b ) {

		return Vector3.Distance( new Vector3( a.x, 0.0f, a.z ), new Vector3( b.x, 0.0f, b.z ) );

	}




}
