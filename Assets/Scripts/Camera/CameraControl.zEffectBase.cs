using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CameraEffectBase {
	
	[SerializeField]
	protected	float		m_WpnInfluence				= 0.01f;

	[SerializeField]
	protected float			m_AmplitudeOverall			= 3.0f;

	[SerializeField]
	protected	float		m_AmplitudeHoriz			= 1.00f;

	[SerializeField]
	protected	float		m_AmplitudeVert				= 1.00f;

	[SerializeField]
	protected float			m_Speed						= 1.0f;

	[SerializeField]
	protected	float		m_Theta_Upd_Vert			= 5.0f;

	[SerializeField]
	protected	float		m_Theta_Upd_Oriz			= 2.5f;

	[SerializeField]
	protected	float		m_InternalWeight			= 1.0f;


	[SerializeField]
	protected	Vector3		m_Direction					= Vector3.zero;
	public		Vector3		Direction					{ get { return m_Direction; } }

	protected	float		m_ThetaX					= 0f;
	protected	float		m_ThetaY					= 90f;

	protected	float		m_SpeedMul					= 1.0f;
	public		float		SpeedMul					{ get { return m_SpeedMul; } set { m_SpeedMul = value; } }

	protected	float		m_AmplitudeMult				= 1.0f;
	public		float		AmplitudeMult				{ get { return m_AmplitudeMult; } set { m_AmplitudeMult = value; } }

	[SerializeField]
	protected	bool		m_IsActive					= true;
	public		bool		IsActive					{ get { return m_IsActive; } set { m_IsActive = value; } }

}
