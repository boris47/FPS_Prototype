using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadMove : CameraEffectBase {

	[SerializeField]
	private float	m_Amplitude					= 0.2f;

	[SerializeField]
	private float	m_Speed						= 5.0f;

	private float	m_ThetaUpdateX				= 0.4f;
	private float	m_ThetaUpdateY				= 0.8f;

	private float	m_ThetaX					= 0f;
	private float	m_ThetaY					= 0f;

	private Vector3	m_Direction					= Vector3.zero;
	public	Vector3 Direction {
		get { return m_Direction; }
	}

	public void Update( LiveEntity pLiveEntity ) {

		if ( m_IsActive == false ) return;

		float fStamina = pLiveEntity.Stamina;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
//		fSpeed		*= ( bCrouched )	?	0.80f : 1.00f;
//		fSpeed		*= ( bIsUnderwater )?	0.50f : 1.00f;
//		fSpeed		*= ( bZoomed )		?	0.85f : 1.00f;
		fSpeed		*= ( 4.0f - ( fStamina * 2.0f ) );


		float fAmplitude = m_Amplitude * m_AmplitudeMult;
//		fAmplitude		*= ( ( bCrouched )	? 0.08f : 1.00f );
//		fAmplitude		*= ( ( bZoomed )	? 0.85f : 1.00f );
		fAmplitude		*= ( 5.0f - ( fStamina * 4.0f ) );


		m_ThetaX += m_ThetaUpdateY * fSpeed;
		m_ThetaY += ( m_ThetaUpdateX + Random.Range( 0.0f, 0.03f ) ) * fSpeed;


		m_Direction.x = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		m_Direction.y =  Mathf.Cos( m_ThetaY ) * fAmplitude * 0.2f;

//		pCamera.rotation = Quaternion.Euler( pCamera.rotation.eulerAngles + m_Direction );

	}

	public void Reset( bool bInstantly = false ) {

		if ( bInstantly )
			m_Direction = Vector3.zero;
		else
			m_Direction = Vector3.Lerp ( m_Direction, Vector3.zero, Time.deltaTime * 2f );

	}

}
