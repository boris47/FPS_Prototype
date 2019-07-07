using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadMove : CameraEffectBase {

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
	private class EffectSectionData {
		public	float	WpnInfluence			= 0.1f;
		public	float	Amplitude				= 0.80f;
		public	float	Speed					= 1.25f;
		public	float	Theta_Upd_Vert			= 0.80f;
		public	float	Theta_Upd_Oriz			= 0.40f;
	}

	[SerializeField]
	private		EffectSectionData			m_EffectSectionData = new EffectSectionData();


	//////////////////////////////////////////////////////////////////////////
	public	void Setup()
	{
		if ( GlobalManager.Configs.bGetSection( "HeadMove", m_EffectSectionData ) == false )
		{
			Debug.Log( "HeadMove::Setup:Cannot load m_HeadMoveSectionData" );
		}
		else
		{
			m_WpnInfluence		= m_EffectSectionData.WpnInfluence;
			m_Amplitude			= m_EffectSectionData.Amplitude;
			m_Speed				= m_EffectSectionData.Speed;
			m_Theta_Upd_Vert	= m_EffectSectionData.Theta_Upd_Vert;
			m_Theta_Upd_Oriz	= m_EffectSectionData.Theta_Upd_Oriz;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public void Update( float weight )
	{
		m_InternalWeight = Mathf.Lerp( m_InternalWeight, weight, Time.deltaTime * 5f );

		if ( m_IsActive == false )
			return;

		float	fStamina	= Player.Instance.Stamina;
		bool	bCrouched	= Player.Instance.IsCrouched;
//		bool	bZoomed		= WeaponManager.Instance.IsZoomed;

		float fSpeed = m_Speed * m_SpeedMul * Time.deltaTime;
		fSpeed		*= ( bCrouched )	?	0.80f : 1.00f;
//		fSpeed		*= ( bIsUnderwater )?	0.50f : 1.00f;
//		fSpeed		*= ( bZoomed )		?	0.85f : 1.00f;
		fSpeed		*= ( 4.0f - ( fStamina * 2.0f ) );

		float fAmplitude = m_Amplitude * m_AmplitudeMult;
		fAmplitude		*= ( ( bCrouched )	? 0.80f : 1.00f );
//		fAmplitude		*= ( ( bZoomed )	? 0.85f : 1.00f );
		fAmplitude		*= ( 5.0f - ( fStamina * 4.0f ) );

		m_ThetaX += fSpeed * Random.Range( 0.0f, 6f );
		m_ThetaY += fSpeed * Random.Range( 0.0f, 6f );

		float deltaX = Mathf.Sin( m_ThetaX ) * m_Theta_Upd_Vert * fAmplitude * m_InternalWeight;
		float deltaY = Mathf.Cos( m_ThetaY ) * m_Theta_Upd_Oriz * fAmplitude * m_InternalWeight;
		m_Direction.Set ( deltaX, deltaY, 0.0f );
//		m_Direction *= m_InternalWeight;

//		m_WeaponPositionDelta.x = deltaX * m_WpnInfluence;
		m_WeaponPositionDelta.y = -deltaY * m_WpnInfluence;
		m_WeaponRotationDelta.x = deltaX * m_WpnInfluence;
		m_WeaponRotationDelta.y = deltaY * m_WpnInfluence;
	}


	//////////////////////////////////////////////////////////////////////////
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
