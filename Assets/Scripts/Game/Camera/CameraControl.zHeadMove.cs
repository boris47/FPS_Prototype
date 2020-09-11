using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadMove : CameraEffectBase {



	// SECTION DATA
	[System.Serializable]
	private class EffectSectionData {
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
	public override	void Setup( EffectActiveCondition condition )
	{
		this.m_EffectActiveCondition =  condition;

		Database.Section headmoveSection = null;
		if (!(GlobalManager.Configs.GetSection("HeadMove", ref headmoveSection) && GlobalManager.Configs.bSectionToOuter(headmoveSection, this.m_EffectSectionData)))
		{
			Debug.Log( "HeadMove::Setup:Cannot load m_HeadMoveSectionData" );
		}
		else
		{
			this.m_WpnInfluence		= this.m_EffectSectionData.WpnInfluence;
			this.m_AmplitudeBase		= this.m_EffectSectionData.AmplitudeBase;
			this.m_AmplitudeHoriz	= this.m_EffectSectionData.AmplitudeHoriz;
			this.m_AmplitudeVert		= this.m_EffectSectionData.AmplitudeVert;
			this.m_SpeedBase			= this.m_EffectSectionData.SpeedBase;
			this.m_Theta_Upd_Vert	= this.m_EffectSectionData.Theta_Upd_Vert;
			this.m_Theta_Upd_Oriz	= this.m_EffectSectionData.Theta_Upd_Oriz;
			this.m_ThetaX			= Random.Range( 0f, 360f );
			this.m_ThetaY			= Random.Range( 0f, 360f );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	public override void Update( float deltaTime, ref CameraEffectorsManager.CameraEffectorData data )
	{
		if (this.IsActive == false )
			return;

		if (this.m_EffectActiveCondition() == false )
		{
//			m_Direction = Vector3.MoveTowards( m_Direction, Vector3.zero, deltaTime * RETURN_FACTOR );
//			m_WeaponPositionDelta = Vector3.MoveTowards( m_WeaponPositionDelta, Vector3.zero, deltaTime * RETURN_FACTOR );
//			m_WeaponRotationDelta = Vector3.MoveTowards( m_WeaponRotationDelta, Vector3.zero, deltaTime * RETURN_FACTOR );
			this.SetData( ref data );
			return;
		}

		float	fStamina	= Player.Instance.Stamina;
		bool	bCrouched	= Player.Instance.IsCrouched;
		bool	bZoomed		= WeaponManager.Instance.IsZoomed;

		float fSpeed = this.m_SpeedBase * this.SpeedMul * deltaTime;
		fSpeed		*= ( bCrouched )	?	0.80f : 1.00f;
//		fSpeed		*= ( bIsUnderwater )?	0.50f : 1.00f;
		fSpeed		*= ( bZoomed )		?	0.85f : 1.00f;
		fSpeed		*= ( 4.0f - ( fStamina * 2.0f ) );

		float fAmplitude = this.m_AmplitudeBase * this.AmplitudeMult;
		fAmplitude		*= ( ( bCrouched )	? 0.80f : 1.00f );
		fAmplitude		*= ( ( bZoomed )	? 0.55f : 1.00f );
		fAmplitude		*= ( 5.0f - ( fStamina * 4.0f ) );

		this.m_ThetaX += fSpeed * this.m_Theta_Upd_Vert;
		this.m_ThetaY += fSpeed * this.m_Theta_Upd_Oriz;

		float deltaXBase = Mathf.Sin(this.m_ThetaX ) * fAmplitude * this.m_AmplitudeVert;
		float deltaYBase = Mathf.Cos(this.m_ThetaY ) * fAmplitude * this.m_AmplitudeHoriz;

		float deltaX = deltaXBase;
		float deltaY = deltaYBase;
		this.m_Direction.Set ( deltaX, deltaY, 0.0f );

		//		m_WeaponPositionDelta.x = deltaX;
		this.m_WeaponPositionDelta.y = -deltaX * this.m_WpnInfluence;

//		m_WeaponPositionDelta.x = deltaXBase * m_WpnInfluence;
//		m_WeaponPositionDelta.y = -deltaYBase * m_WpnInfluence;
//		m_WeaponRotationDelta.x = deltaXBase * m_WpnInfluence;
//		m_WeaponRotationDelta.y = deltaYBase * m_WpnInfluence;

		this.SetData( ref data );
	}


	public	override void SetData( ref CameraEffectorsManager.CameraEffectorData data )
	{
		data.CameraEffectsDirection += this.m_Direction;
		data.WeaponPositionDelta += this.m_WeaponPositionDelta;
		data.WeaponRotationDelta += this.m_WeaponRotationDelta;
	}
}
