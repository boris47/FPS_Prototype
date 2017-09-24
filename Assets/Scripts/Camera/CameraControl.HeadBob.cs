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
	
	private float	m_ThetaUpdateX				= 5f;
	private float	m_ThetaUpdateY				= 2.5f;

	private float	m_ThetaX					= 0f;
	private float	m_ThetaY					= 0f;

	private Vector3	m_Direction					= Vector3.zero;
	public	Vector3 Direction {
		get { return m_Direction; }
	}

	public void _Update() {

		if ( m_IsActive == false ) return;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
//		fSpeed *= ( ( bRunning )	?1.7:1.0 );
	//	fSpeed *= ( ( bCrouched )	?0.5:1.0 );
	//	fSpeed *= ( ( bZoomed )		?0.5:1.0 );

		float fAmplitude = m_Amplitude * m_AmplitudeMult;
	//	fAmplitude *= ( ( bRunning )	? 2.0:1.0 );
//		fAmplitude *= ( ( bCrouched )	? 0.5 : 1.0 );
	//	fAmplitude *= ( ( bZoomed )		? 0.8:1.0 );
//		fAmplitude *= ( 3.f - fStamina * 2 );

		m_ThetaX += m_ThetaUpdateX * fSpeed;
		m_ThetaY += ( m_ThetaUpdateY + Random.Range( 0.0f, 0.03f ) ) * fSpeed;

		m_Direction.x = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		m_Direction.y =  Mathf.Cos( m_ThetaY ) * fAmplitude;

//		pCamera.rotation = Quaternion.Euler( pCamera.rotation.eulerAngles + m_Direction );

	}

	public void _Reset( bool bInstantly = false ) {

		if ( bInstantly )
			m_Direction = Vector3.zero;
		else
			m_Direction = Vector3.Lerp ( m_Direction, Vector3.zero, Time.deltaTime * 2f );

	}

}
