using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadBob : CameraEffectBase {

	const	float				STEP_VALUE					= 0.8f;
	const	float				THETA_UPDATE_X				= 5.0f;
	const	float				THETA_UPDATE_Y				= 2.5f;


	[SerializeField]
	private float				m_Amplitude					= 3.0f;

	[SerializeField]
	private float				m_Speed						= 1.0f;


	private Vector3				m_Direction					= Vector3.zero;
	public	Vector3				Direction
	{
		get { return m_Direction; }
	}

	private	Vector3				m_WeaponPositionDelta		= Vector3.zero;
	public	Vector3				WeaponPositionDelta
	{
		get { return m_WeaponPositionDelta; }
	}
	private	Vector3				m_WeaponRotationDelta		= Vector3.zero;
	public	Vector3				WeaponRotationDelta
	{
		get { return m_WeaponRotationDelta; }
	}


	private	bool				m_StepDone					= false;
	private float				m_ThetaX					= 0f;
	private float				m_ThetaY					= 0f;



	public void Update( ref LiveEntity liveEntity, float weight )
	{
		if ( m_IsActive == false )
			return;

		m_InternalWeight = Mathf.Lerp( m_InternalWeight, weight, Time.deltaTime * 5f );

		float	fStamina	= liveEntity.Stamina;
		bool	bRunning	= liveEntity.IsRunning;
		bool	bCrouched	= liveEntity.IsCrouched;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
		fSpeed		*= ( ( bRunning )	?	1.70f : 1.00f );
		fSpeed		*= ( ( bCrouched )	?	0.80f : 1.00f );
	//	fSpeed		*= ( ( bZoomed )	?	0.50f : 1.00f );

		float fAmplitude = m_Amplitude * m_AmplitudeMult;
		fAmplitude		*= ( ( bRunning )	?	2.00f : 1.00f );
		fAmplitude		*= ( ( bCrouched )	?	0.70f : 1.00f );
	//	fAmplitude		*= ( ( bZoomed )	?	0.80f : 1.00f );
//		fAmplitude		*= ( 3.0f - fStamina * 2.0f );


		m_ThetaX +=   THETA_UPDATE_X * fSpeed * m_InternalWeight;
		m_ThetaY += ( THETA_UPDATE_Y + Random.Range( 0.0f, 0.03f ) ) * fSpeed * m_InternalWeight;


		float	deltaX = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		float	deltaY =  Mathf.Cos( m_ThetaY ) * fAmplitude;
		m_Direction.Set ( deltaX, deltaY, 0.0f );

		m_WeaponPositionDelta.x = deltaY;
		m_WeaponPositionDelta.y = deltaX;
		m_WeaponRotationDelta.x = deltaY;
		m_WeaponRotationDelta.y = deltaX;
		m_WeaponPositionDelta *= 0.007f * m_InternalWeight;
		m_WeaponRotationDelta *= 0.007f * m_InternalWeight;

		// Steps
		if ( Mathf.Abs( Mathf.Cos( m_ThetaY ) ) > STEP_VALUE )
		{
			if ( m_StepDone == false )
			{
				liveEntity.Foots.PlayStep();
				m_StepDone = true;
			}
		}
		else
		{
			m_StepDone = false;
		}
	}



	public void Reset( bool bInstantly = false )
	{
		if ( bInstantly )
		{
			m_Direction				= Vector3.zero;
			m_WeaponPositionDelta	= Vector3.zero;
		}
		else
		{
			m_Direction = Vector3.Lerp ( m_Direction, Vector3.zero, Time.deltaTime * 5f );
			m_WeaponPositionDelta = Vector3.Lerp( m_WeaponPositionDelta, Vector3.zero, Time.deltaTime * 8f );
		}
	}

}
