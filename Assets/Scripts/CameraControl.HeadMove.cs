using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadMove {

	[SerializeField]
	private float	m_Amplitude					= 0.2f;

	[SerializeField]
	private float	m_Speed						= 5.0f;

//	[SerializeField]
	private float	m_ThetaUpdateX				= 0.4f;
//	[SerializeField]
	private float	m_ThetaUpdateY				= 0.8f;

	private float	m_ThetaX					= 0f;
	private float	m_ThetaY					= 0f;

	private Vector3	m_Direction					= Vector3.zero;

	public	bool	m_IsActive					= true;


	public void Update( Transform pCamera ) {

		if ( m_IsActive == false ) return;

		float fSpeed = m_Speed; /* * fSpeedMul*/;
//		fSpeed *= ( bCrouched )		? 0.8f  : 1.0f;
//		fSpeed *= ( bIsUnderwater )	? 0.5f  : 1.0f;
//		fSpeed *= ( bZoomed		  ) ? 0.85f : 1.0f;
//		fSpeed *= ( 4.f - fStamina*2.f );


		float fAmplitude = m_Amplitude /* * fAmplitudeMul*/;
//		fAmplitude *= ( ( bCrouched )	? 0.8f  : 1.f );
//		fAmplitude *= ( ( bZoomed )		? 0.85f : 1.f );
//		fAmplitude *= ( 5.f - fStamina*4.f );


		m_ThetaX += m_ThetaUpdateY * fSpeed * Time.deltaTime;
		m_ThetaY += ( m_ThetaUpdateX + Random.Range( 0.0f, 0.03f ) ) * fSpeed * Time.deltaTime;


		m_Direction.x = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		m_Direction.y =  Mathf.Cos( m_ThetaY ) * fAmplitude * 0.2f;

		pCamera.rotation = Quaternion.Euler( pCamera.rotation.eulerAngles + m_Direction );

	}

	public void Reset( bool bInstantly = false ) {

		if ( bInstantly )
			m_Direction = Vector3.zero;
		else
			m_Direction = Vector3.Lerp ( m_Direction, Vector3.zero, Time.deltaTime * 2f );

	}

}
