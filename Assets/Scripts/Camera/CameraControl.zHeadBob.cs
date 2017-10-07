using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  BEST FONFIGS FOUND
 * - WALKING
 * 0.5, 30, 30
 * - RUNNING
 * 0.7, 60, 20
 * - CROUCHED
 * 0.15, 40, 20
 */


[System.Serializable]
public class HeadBob : CameraEffectBase {

	[SerializeField]
	private float	m_Amplitude					= 3.0f;

	[SerializeField]
	private float	m_Speed						= 1.0f;

	const	float	STEP_VALUE					= 0.8f;
	private	bool	m_StepDone					= false;
	
	private float	m_ThetaUpdateX				= 5f;
	private float	m_ThetaUpdateY				= 2.5f;

	private float	m_ThetaX					= 0f;
	private float	m_ThetaY					= 0f;

	private Vector3	m_Direction					= Vector3.zero;
	public	Vector3 Direction {
		get { return m_Direction; }
	}

	public void _Update( LiveEntity pLiveEntity ) {

		if ( m_IsActive == false ) return;

		float	fStamina	= pLiveEntity.Stamina;
		bool	bRunning	= pLiveEntity.IsRunning;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
		fSpeed		*= ( ( bRunning )	?	1.70f : 1.00f );
	//	fSpeed		*= ( ( bCrouched )	?	0.50f : 1.00f );
	//	fSpeed		*= ( ( bZoomed )	?	0.50f : 1.00f );

		float fAmplitude = m_Amplitude * m_AmplitudeMult;
		fAmplitude		*= ( ( bRunning )	?	2.00f : 1.00f );
//		fAmplitude		*= ( ( bCrouched )	?	0.50f : 1.00f );
	//	fAmplitude		*= ( ( bZoomed )	?	0.80f : 1.00f );
//		fAmplitude		*= ( 3.0f - fStamina * 2.0f );

		m_ThetaX += m_ThetaUpdateX * fSpeed;
		m_ThetaY += ( m_ThetaUpdateY + Random.Range( 0.0f, 0.03f ) ) * fSpeed;

		m_Direction.x = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		m_Direction.y =  Mathf.Cos( m_ThetaY ) * fAmplitude;

		// Steps
		if ( Mathf.Abs( Mathf.Cos( m_ThetaY ) ) > ( STEP_VALUE ) ) {
			if ( !m_StepDone ) {
				( pLiveEntity.Foots as IFoots ).PlayStep();
				m_StepDone = true;
			}
		}
		else {
			m_StepDone = false;
		}


	}

	public void _Reset( bool bInstantly = false ) {

		if ( bInstantly )
			m_Direction = Vector3.zero;
		else
			m_Direction = Vector3.Lerp ( m_Direction, Vector3.zero, Time.deltaTime );

	}

}
