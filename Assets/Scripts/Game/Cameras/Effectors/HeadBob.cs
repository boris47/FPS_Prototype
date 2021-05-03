using UnityEngine;

public class HeadBob : CameraEffectBase
{
	private	float						m_StepValue					= 0.8f;

	private	bool						m_StepDone					= false;

	// SECTION DATA
	[System.Serializable]
	private class EffectSectionData
	{
		public	float	ToWeaponConvFactor		= 0.0025f;
		public	float	AmplitudeBase			= 0.005f;
		public	float	AmplitudeHoriz			= 0.003f;
		public	float	AmplitudeVert			= 0.003f;
		public	float	SpeedBase				= 5.40f;
		public	float	Step					= 0.80f;
		public	float	Theta_Upd_Vert			= 1.00f;
		public	float	Theta_Upd_Oriz			= 0.50f;
	}

	private		EffectSectionData			m_EffectSectionData = new EffectSectionData();


	//////////////////////////////////////////////////////////////////////////
	public override	void Setup( in EffectorActiveCondition condition )
	{
		m_EffectActiveCondition =  condition;

		if (!(GlobalManager.Configs.TryGetSection("HeadBob", out Database.Section headbobSection) && GlobalManager.Configs.TrySectionToOuter(headbobSection, m_EffectSectionData)))
		{
			Debug.Log( "HeadBob::Setup:Cannot load m_HeadBobSectionData" );
		}
		else
		{
			m_ToWeaponConvFactor	= m_EffectSectionData.ToWeaponConvFactor;
			m_AmplitudeBase			= m_EffectSectionData.AmplitudeBase;
			m_AmplitudeHoriz		= m_EffectSectionData.AmplitudeHoriz;
			m_AmplitudeVert			= m_EffectSectionData.AmplitudeVert;
			m_SpeedBase				= m_EffectSectionData.SpeedBase;
			m_Theta_Upd_Vert		= m_EffectSectionData.Theta_Upd_Vert;
			m_Theta_Upd_Oriz		= m_EffectSectionData.Theta_Upd_Oriz;
			m_StepValue				= m_EffectSectionData.Step;
			m_ThetaX				= Random.Range( 0f, 360f );
			m_ThetaY				= Random.Range( 0f, 360f );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public override void OnUpdate(float deltaTime, CameraEffectorData data)
	{
		if (IsActive)
		{
			if (!m_EffectActiveCondition())
			{
				float recoverInterpolant = deltaTime * 3f;
				m_CameraEffectsDirection.LerpTo(Vector3.zero, recoverInterpolant);
				m_WeaponPositionDelta.LerpTo(Vector3.zero, recoverInterpolant);
				m_WeaponDirectionDelta.LerpTo(Vector3.zero, recoverInterpolant);
				m_CurrentWeight = Mathf.Lerp(m_CurrentWeight, 0f, recoverInterpolant);
			}
			else
			{
				m_CurrentWeight = Mathf.MoveTowards(m_CurrentWeight, 1f, deltaTime * 5f);
				float	fStamina	= 1f; // Player.Instance.Stamina; TODO ASAP
				bool	bRunning	= Player.Instance.Motion.MotionStrategy.States.IsRunning;
				bool	bCrouched	= Player.Instance.Motion.MotionStrategy.States.IsCrouched;
				bool	bZoomed		= WeaponManager.Instance.IsChangingZoom || WeaponManager.Instance.IsZoomed;

				float fSpeed = m_SpeedBase * SpeedMul * deltaTime;
				fSpeed		*= ((bRunning)	?	1.70f : 1.00f);
				fSpeed		*= ((bCrouched)	?	0.80f : 1.00f);
				fSpeed		*= ((bZoomed)	?	0.50f : 1.00f);

				float fAmplitude = m_AmplitudeBase * AmplitudeMult * m_CurrentWeight;
				fAmplitude		*= ((bRunning)	?	2.00f : 1.00f);
				fAmplitude		*= ((bCrouched)	?	0.70f : 1.00f);
				fAmplitude		*= ((bZoomed)	?	0.20f : 1.00f);
				fAmplitude		*= (3.0f - (fStamina * 2.0f));

				m_ThetaX += fSpeed * m_Theta_Upd_Vert;
				m_ThetaY += fSpeed * m_Theta_Upd_Oriz;

				float deltaX = Mathf.Sin(m_ThetaX) * fAmplitude * m_AmplitudeVert;
				float deltaY = Mathf.Cos(m_ThetaY) * fAmplitude * m_AmplitudeHoriz;

				m_CameraEffectsDirection.Set(deltaX, deltaY, 0.0f);

				m_WeaponPositionDelta.z = deltaY;
				m_WeaponPositionDelta.y = deltaX;

				m_WeaponDirectionDelta.x = deltaX;
				m_WeaponDirectionDelta.y = deltaY;

				// Steps
				if (Mathf.Abs(Mathf.Sin(m_ThetaY)) > m_StepValue)
				{
					if (m_StepDone == false)
					{
						//Player.Instance.Foots.PlayStep();  TODO ASAP
						m_StepDone = true;
					}
				}
				else
				{
					m_StepDone = false;
				}
			}
			SetData(data);
		}
	}


	protected void SetData(CameraEffectorData data)
	{
		data.CameraEffectsDirection += m_CameraEffectsDirection;
		data.WeaponPositionDelta += m_WeaponPositionDelta;
		data.WeaponDirectionDelta += m_WeaponDirectionDelta;
	}
}
