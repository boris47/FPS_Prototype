using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraEffectBase {
	
	protected	float	m_SpeedMul			= 1.0f;
	protected	float	m_AmplitudeMult		= 1.0f;

	protected	bool	m_IsActive			= true;


	public void				_Enable()					{ m_IsActive = true; }
	public void				_Disable()					{ m_IsActive = false; }
	public bool				_IsActive()					{ return m_IsActive; }

	public void				_SetSpeedMul( float Val )	{ m_SpeedMul = Val; }
	public float			_GetSpeedMul()				{ return m_SpeedMul; }

	public void				_SetAmplitudeMul( float Val ){ m_AmplitudeMult = Val; }
	public float			_GetAmplitudeMul()			{ return m_AmplitudeMult; }

}
