using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadBob : CameraEffectBase {

	[SerializeField]
	private	float						m_StepValue					= 0.8f;

	private	bool						m_StepDone					= false;

	private	static	Vector3				m_WeaponPositionDelta		= Vector3.zero;
	public	static	Vector3				WeaponPositionDelta
	{
		get { return m_WeaponPositionDelta; }
	}

	private	static	Vector3				m_WeaponRotationDelta		= Vector3.zero;
	public	static	Vector3				WeaponRotationDelta
	{
		get { return m_WeaponRotationDelta; }
	}


	// SECTION DATA
	[System.Serializable]
	private class HeadBobSectionData {
		public	float	Amplitude				= 0.002f;
		public	float	Speed					= 0.5f;
		public	float	Step					= 0.80f;
		public	float	Theta_Upd_Vert			= 0.80f;
		public	float	Theta_Upd_Oriz			= 0.40f;
	}

	[SerializeField]
	private		HeadBobSectionData			m_HeadBobSectionData = new HeadBobSectionData();


	public	void Setup()
	{
		if ( GlobalManager.Configs.bGetSection( "HeadBob", m_HeadBobSectionData ) == false )
		{
			Debug.Log( "HeadBob::Setup:Cannot load m_HeadBobSectionData" );
		}
		else
		{
			m_Amplitude			= m_HeadBobSectionData.Amplitude;
			m_Speed				= m_HeadBobSectionData.Speed;
			m_StepValue			= m_HeadBobSectionData.Step;
			m_Theta_Upd_Vert	= m_HeadBobSectionData.Theta_Upd_Vert;
			m_Theta_Upd_Oriz	= m_HeadBobSectionData.Theta_Upd_Oriz;
		}
	}


	public void Update( float weight )
	{
		m_InternalWeight = Mathf.Lerp( m_InternalWeight, weight, Time.deltaTime * 5f );

		if ( m_IsActive == false )
			return;

		float	fStamina	= Player.Instance.Stamina;
		bool	bRunning	= Player.Instance.IsRunning;
		bool	bCrouched	= Player.Instance.IsCrouched;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
		fSpeed		*= ( ( bRunning )	?	1.70f : 1.00f );
		fSpeed		*= ( ( bCrouched )	?	0.80f : 1.00f );
	//	fSpeed		*= ( ( bZoomed )	?	0.50f : 1.00f );

		float fAmplitude = m_Amplitude * m_AmplitudeMult;
		fAmplitude		*= ( ( bRunning )	?	2.00f : 1.00f );
		fAmplitude		*= ( ( bCrouched )	?	0.70f : 1.00f );
	//	fAmplitude		*= ( ( bZoomed )	?	0.80f : 1.00f );
//		fAmplitude		*= ( 3.0f - fStamina * 2.0f );


		m_ThetaX +=   m_Theta_Upd_Vert * fSpeed * m_InternalWeight;
		m_ThetaY += ( m_Theta_Upd_Oriz + Random.Range( 0.0f, 0.03f ) ) * fSpeed * m_InternalWeight;


		float	deltaX = -Mathf.Cos( m_ThetaX ) * fAmplitude;
		float	deltaY =  Mathf.Cos( m_ThetaY ) * fAmplitude;
		m_Direction.Set ( deltaX * m_InternalWeight, deltaY * m_InternalWeight, 0.0f );

		m_WeaponPositionDelta.x = deltaY * m_WpnInfluence * m_InternalWeight;
		m_WeaponPositionDelta.y = deltaX * m_WpnInfluence * m_InternalWeight;

		m_WeaponRotationDelta.x = deltaY * m_WpnInfluence * m_InternalWeight;
		m_WeaponRotationDelta.y = deltaX * m_WpnInfluence * m_InternalWeight;

		// Steps
		if ( Mathf.Abs( Mathf.Cos( m_ThetaY ) ) > m_StepValue )
		{
			if ( m_StepDone == false )
			{
				Player.Instance.Foots.PlayStep();
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
			m_Direction = Vector3.MoveTowards( m_Direction, Vector3.zero, Time.deltaTime * 5f );
			m_WeaponPositionDelta = Vector3.MoveTowards( m_WeaponPositionDelta, Vector3.zero, Time.deltaTime * 8f );
		}
	}

}
