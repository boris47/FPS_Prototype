using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeadBob : CameraEffectBase {

	[SerializeField]
	private	float						m_StepValue					= 0.8f;

	private	bool						m_StepDone					= false;



	// SECTION DATA
	[System.Serializable]
	private class EffectSectionData {
		public	float	WpnInfluence			= 1.00f;
		public	float	AmplitudeBase			= 0.005f;
		public	float	AmplitudeHoriz			= 0.003f;
		public	float	AmplitudeVert			= 0.003f;
		public	float	SpeedBase				= 5.40f;
		public	float	Step					= 0.80f;
		public	float	Theta_Upd_Vert			= 1.00f;
		public	float	Theta_Upd_Oriz			= 0.50f;
	}

	[SerializeField]
	private		EffectSectionData			m_EffectSectionData = new EffectSectionData();


	//////////////////////////////////////////////////////////////////////////
	public override	void Setup( EffectActiveCondition condition )
	{
		this.m_EffectActiveCondition =  condition;

		if ( GlobalManager.Configs.GetSection( "HeadBob", this.m_EffectSectionData ) == false )
		{
			Debug.Log( "HeadBob::Setup:Cannot load m_HeadBobSectionData" );
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
			this.m_StepValue			= this.m_EffectSectionData.Step;
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
		bool	bRunning	= Player.Instance.IsRunning;
		bool	bCrouched	= Player.Instance.IsCrouched;
		bool	bZoomed		= WeaponManager.Instance.IsZoomed;

		float fSpeed = this.m_SpeedBase * this.SpeedMul * deltaTime;
		fSpeed		*= ( ( bRunning )	?	1.70f : 1.00f );
		fSpeed		*= ( ( bCrouched )	?	0.80f : 1.00f );
		fSpeed		*= ( ( bZoomed )	?	0.50f : 1.00f );

		float fAmplitude = this.m_AmplitudeBase * this.AmplitudeMult;
		fAmplitude		*= ( ( bRunning )	?	2.00f : 1.00f );
		fAmplitude		*= ( ( bCrouched )	?	0.70f : 1.00f );
		fAmplitude		*= ( ( bZoomed )	?	0.80f : 1.00f );
		fAmplitude		*= ( 3.0f - fStamina * 2.0f );

		this.m_ThetaX += fSpeed * this.m_Theta_Upd_Vert;
		this.m_ThetaY += fSpeed * this.m_Theta_Upd_Oriz;

		float deltaXBase = Mathf.Sin(this.m_ThetaX ) * fAmplitude * this.m_AmplitudeVert;
		float deltaYBase = Mathf.Cos(this.m_ThetaY ) * fAmplitude * this.m_AmplitudeHoriz;

		float deltaX = deltaXBase;
		float deltaY = deltaYBase;
		this.m_Direction.Set ( deltaX, deltaY, 0.0f );

		this.m_WeaponPositionDelta.z = deltaY * this.m_WpnInfluence;
		this.m_WeaponPositionDelta.y = deltaX * this.m_WpnInfluence;

		this.m_WeaponRotationDelta.x = deltaX * this.m_WpnInfluence;
		this.m_WeaponRotationDelta.y = deltaY * this.m_WpnInfluence;

		this.SetData( ref data );

		// Steps
		if ( Mathf.Abs( Mathf.Sin(this.m_ThetaY ) ) > this.m_StepValue )
		{
			if (this.m_StepDone == false )
			{
				Player.Instance.Foots.PlayStep();
				this.m_StepDone = true;
			}
		}
		else
		{
			this.m_StepDone = false;
		}
	}

	public	override void SetData( ref CameraEffectorsManager.CameraEffectorData data )
	{
		data.CameraEffectsDirection += this.m_Direction;
		data.WeaponPositionDelta += this.m_WeaponPositionDelta;
		data.WeaponRotationDelta += this.m_WeaponRotationDelta;
	}
	
}
