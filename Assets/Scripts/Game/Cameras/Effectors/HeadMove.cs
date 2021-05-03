using UnityEngine;

public class HeadMove : CameraEffectBase
{
	// SECTION DATA
	[System.Serializable]
	private class EffectSectionData
	{
		public	float	ToWeaponConvFactor		= 0.0025f;
		public	float	AmplitudeBase			= 0.003f;
		public	float	AmplitudeHoriz			= 0.003f;
		public	float	AmplitudeVert			= 0.003f;
		public	float	SpeedBase				= 0.4f;
		public	float	Theta_Upd_Vert			= 0.80f;
		public	float	Theta_Upd_Oriz			= 0.40f;
	}
	private		EffectSectionData			m_EffectSectionData = new EffectSectionData();

	//////////////////////////////////////////////////////////////////////////
	public override	void Setup(in EffectorActiveCondition condition)
	{
		m_EffectActiveCondition =  condition;

		if (!(GlobalManager.Configs.TryGetSection("HeadMove", out Database.Section headmoveSection) && GlobalManager.Configs.TrySectionToOuter(headmoveSection, m_EffectSectionData)))
		{
			Debug.Log("HeadMove::Setup:Cannot load m_HeadMoveSectionData");
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
			m_ThetaX				= Random.Range(0f, 360f);
			m_ThetaY				= Random.Range(0f, 360f);
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
				float	fStamina	= 1f; //Player.Instance.Stamina; TODO Restore asap
				bool	bCrouched	= Player.Instance.Motion.MotionStrategy.States.IsCrouched;
				bool	bZoomed		= WeaponManager.Instance.IsChangingZoom || WeaponManager.Instance.IsZoomed;

				float fSpeed = m_SpeedBase * SpeedMul * deltaTime;
				fSpeed		*= (bCrouched)		?	0.80f : 1.00f;
		//		fSpeed		*= (bIsUnderwater)	?	0.50f : 1.00f;
				fSpeed		*= (bZoomed)		?	0.85f : 1.00f;
				fSpeed		*= (4.0f - (fStamina * 2.0f));

				float fAmplitude = m_AmplitudeBase * AmplitudeMult * m_CurrentWeight;
				fAmplitude		*= ((bCrouched) ? 0.80f : 1.00f);
				fAmplitude		*= ((bZoomed)	? 0.50f : 1.00f);
				fAmplitude		*= (5.0f - (fStamina * 4.0f));

				m_ThetaX += fSpeed * m_Theta_Upd_Vert;
				m_ThetaY += fSpeed * m_Theta_Upd_Oriz;

				float deltaX = Mathf.Sin(m_ThetaX) * fAmplitude * m_AmplitudeVert;
				float deltaY = Mathf.Cos(m_ThetaY) * fAmplitude * m_AmplitudeHoriz;

				m_CameraEffectsDirection.Set(deltaX, deltaY, 0.0f);

				m_WeaponPositionDelta.y = -deltaX * m_ToWeaponConvFactor;

				//m_WeaponDirectionDelta.y = deltaX;
				//m_WeaponDirectionDelta.x = deltaY;
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
