using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadMove : CameraEffectBase
{
	// SECTION DATA
	[System.Serializable]
	private class EffectSectionData
	{
		public	float	WpnInfluence			= 1.0f;
		public	float	AmplitudeBase			= 0.003f;
		public	float	AmplitudeHoriz			= 0.003f;
		public	float	AmplitudeVert			= 0.003f;
		public	float	SpeedBase				= 0.4f;
		public	float	Theta_Upd_Vert			= 0.80f;
		public	float	Theta_Upd_Oriz			= 0.40f;
	}

	[SerializeField]
	private		EffectSectionData			m_EffectSectionData = new EffectSectionData();



	//////////////////////////////////////////////////////////////////////////
	public override	void Setup( in EffectActiveCondition condition )
	{
		m_EffectActiveCondition =  condition;

		Database.Section headmoveSection = null;
		if ( !( GlobalManager.Configs.GetSection( "HeadMove", ref headmoveSection ) && GlobalManager.Configs.bSectionToOuter( headmoveSection, m_EffectSectionData ) ) )
		{
			Debug.Log( "HeadMove::Setup:Cannot load m_HeadMoveSectionData" );
		}
		else
		{
			m_WpnInfluence		= m_EffectSectionData.WpnInfluence;
			m_AmplitudeBase		= m_EffectSectionData.AmplitudeBase;
			m_AmplitudeHoriz	= m_EffectSectionData.AmplitudeHoriz;
			m_AmplitudeVert		= m_EffectSectionData.AmplitudeVert;
			m_SpeedBase			= m_EffectSectionData.SpeedBase;
			m_Theta_Upd_Vert	= m_EffectSectionData.Theta_Upd_Vert;
			m_Theta_Upd_Oriz	= m_EffectSectionData.Theta_Upd_Oriz;
			m_ThetaX			= Random.Range( 0f, 360f );
			m_ThetaY			= Random.Range( 0f, 360f );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void Update( float deltaTime, ref CameraEffectorData data )
	{
		if (IsActive == false )
			return;

		if (m_EffectActiveCondition() == false )
		{
			SetData( ref data );
			return;
		}

		float	fStamina	= Player.Instance.Stamina;
		bool	bCrouched	= Player.Instance.IsCrouched;
		bool	bZoomed		= WeaponManager.Instance.IsZoomed;

		float fSpeed = m_SpeedBase * SpeedMul * deltaTime;
		fSpeed		*= ( bCrouched )	?	0.80f : 1.00f;
//		fSpeed		*= ( bIsUnderwater )?	0.50f : 1.00f;
		fSpeed		*= ( bZoomed )		?	0.85f : 1.00f;
		fSpeed		*= ( 4.0f - ( fStamina * 2.0f ) );

		float fAmplitude = m_AmplitudeBase * AmplitudeMult;
		fAmplitude		*= ( ( bCrouched )	? 0.80f : 1.00f );
		fAmplitude		*= ( ( bZoomed )	? 0.55f : 1.00f );
		fAmplitude		*= ( 5.0f - ( fStamina * 4.0f ) );

		m_ThetaX += fSpeed * m_Theta_Upd_Vert;
		m_ThetaY += fSpeed * m_Theta_Upd_Oriz;

		float deltaXBase = Mathf.Sin(m_ThetaX ) * fAmplitude * m_AmplitudeVert;
		float deltaYBase = Mathf.Cos(m_ThetaY ) * fAmplitude * m_AmplitudeHoriz;

		float deltaX = deltaXBase;
		float deltaY = deltaYBase;
		m_Direction.Set ( deltaX, deltaY, 0.0f );

		m_WeaponPositionDelta.y = -deltaX * m_WpnInfluence;
		SetData( ref data );
	}


	protected void SetData(ref CameraEffectorData data)
	{
		data.CameraEffectsDirection += m_Direction;
		data.WeaponPositionDelta += m_WeaponPositionDelta;
		data.WeaponRotationDelta += m_WeaponRotationDelta;
	}
}
