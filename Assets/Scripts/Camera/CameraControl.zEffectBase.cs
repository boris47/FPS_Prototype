using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CameraEffectBase {
	
	[SerializeField]
	protected	float		m_WpnInfluence				= 0.01f;


	protected	float		m_CurrentWeight				= 1.0f;
	protected	float		m_TargetWeight				= 1.0f;
	protected	float		m_InternalWeight			= 1.0f;
	protected	float		m_Interpolant				= 0.0f;


	protected	Vector3		m_Direction					= Vector3.zero;
	public		Vector3		Direction					{ get { return m_Direction; } }

	protected	float		m_ThetaX					= 0f;
	protected	float		m_ThetaY					= 0f;

	protected	float		m_SpeedMul					= 1.0f;
	public		float		SpeedMul					{ get { return m_SpeedMul; } set { m_SpeedMul = value; } }

	protected	float		m_AmplitudeMult				= 1.0f;
	public		float		AmplitudeMult				{ get { return m_AmplitudeMult; } set { m_AmplitudeMult = value; } }

	protected	bool		m_IsActive					= true;
	public		bool		IsActive					{ get { return m_IsActive; } set { m_IsActive = value; } }

	public		void SetWeight( float newWeight )
	{
		m_TargetWeight = newWeight;
		m_Interpolant = 1.0f;
	}
}
